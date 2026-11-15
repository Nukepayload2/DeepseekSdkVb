Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Nukepayload2.AI.Providers.Deepseek
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Offline tests for the tool APIs. They use a stub <see cref="HttpMessageHandler"/>
''' and never touch the network.
''' </summary>
<TestClass>
Public Class ToolsHttpTests

    Private Const SearchJson As String =
        "{""results"":[{""rerank_score"":2.4,""title"":""DeepSeek V4"",""url"":""https://example.com/v4"",""snippet"":""short"",""summary"":""long summary"",""content"":"""",""site_name"":""Example"",""site_icon"":null,""date"":null,""authority"":3}]}"

    Private Const FetchJson As String =
        "{""title"":""Example article"",""content"":""# Example article\n\nBody.""}"

    Private Const OcrJson As String =
        "{""id"":""ocr-1"",""object"":""ocr.result"",""created"":1762790000,""purpose"":""text_recognition"",""page_count"":2,""pages"":[{""index"":0,""text"":""page one"",""finish_reason"":""stop""},{""index"":1,""text"":""page two"",""finish_reason"":""length""}],""usage"":{""input_tokens"":10,""output_tokens"":5,""total_tokens"":15}}"

    Private Const OcrErrorJson As String =
        "{""error"":{""message"":""OCR did not finish in time"",""type"":""gateway_timeout_error"",""param"":null,""code"":""gateway_timeout_error""}}"

    Private Const AsrJson As String =
        "{""request_id"":""req-1"",""text"":""上海今天天气怎么样？"",""duration_ms"":3200}"

    Private Const ErrorJson As String =
        "{""error"":{""message"":""bad request"",""type"":""invalid_request_error"",""param"":null,""code"":""invalid_request_error""}}"

    <TestMethod>
    Public Sub SearchRequest_SerializesOptionalFields()
        Dim json = New SearchRequest With {
            .Query = "DeepSeek",
            .Count = 5,
            .NeedContent = True
        }.ToJson()

        StringAssert.Contains(json, """query"":""DeepSeek""")
        StringAssert.Contains(json, """count"":5")
        StringAssert.Contains(json, """need_content"":true")

        Dim minimal = New SearchRequest With {.Query = "x"}.ToJson()
        Assert.IsFalse(minimal.Contains("count"))
        Assert.IsFalse(minimal.Contains("need_content"))
    End Sub

    <TestMethod>
    Public Sub FetchRequest_SerializesUrl()
        Dim json = New FetchRequest With {.Url = "https://example.com"}.ToJson()
        Assert.AreEqual("{""url"":""https://example.com""}", json)
    End Sub

    <TestMethod>
    Public Sub TtsRequest_SerializesFormatAndVoice()
        Dim json = New TtsRequest With {
            .Text = "hello",
            .VoiceId = TtsVoices.Tide,
            .Format = TtsFormats.Pcm,
            .FilterMarkdown = False
        }.ToJson()

        StringAssert.Contains(json, """text"":""hello""")
        StringAssert.Contains(json, """voice_id"":""tide""")
        StringAssert.Contains(json, """format"":""pcm""")
        StringAssert.Contains(json, """filter_markdown"":false")
    End Sub

    <TestMethod>
    Public Async Function Search_ReturnsResultsAsync() As Task
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                Dim captured = Await CaptureAsync(request)
                Assert.AreEqual(HttpMethod.Post, captured.Method)
                Assert.AreEqual("https://api.deepseek.com/v1/tools/search", captured.Uri.AbsoluteUri)
                StringAssert.Contains(captured.Body, """query"":""DeepSeek""")
                Return CreateJsonResponse(SearchJson)
            End Function)

        Dim client As New SearchClient("test-key", New HttpClient(handler))
        Dim response = Await client.SearchAsync(New SearchRequest With {.Query = "DeepSeek"})

        Assert.AreEqual(1, response.Results.Count)
        Dim result = response.Results(0)
        Assert.AreEqual(2.4R, result.RerankScore)
        Assert.AreEqual("DeepSeek V4", result.Title)
        Assert.AreEqual("Example", result.SiteName)
        Assert.IsNull(result.SiteIcon)
        Assert.IsNull(result.PublishedAt)
        Assert.AreEqual(3, result.Authority)
    End Function

    <TestMethod>
    Public Async Function Fetch_ReturnsContentAsync() As Task
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                Dim captured = Await CaptureAsync(request)
                Assert.AreEqual("https://api.deepseek.com/v1/tools/fetch", captured.Uri.AbsoluteUri)
                StringAssert.Contains(captured.Body, """url"":""https://example.com""")
                Return CreateJsonResponse(FetchJson)
            End Function)

        Dim client As New FetchClient("test-key", New HttpClient(handler))
        Dim response = Await client.FetchAsync(New FetchRequest With {.Url = "https://example.com"})

        Assert.AreEqual("Example article", response.Title)
        StringAssert.Contains(response.Content, "# Example article")
    End Function

    <TestMethod>
    Public Async Function Asr_SendsMultipartAndParsesResponseAsync() As Task
        Dim fileBytes As Byte() = {1, 2, 3, 4}
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(AsrJson)
            End Function)

        Using stream As New MemoryStream(fileBytes)
            Dim client As New AsrClient("test-key", New HttpClient(handler))
            Dim response = Await client.TranscribeAsync(
                New AsrRequest With {.File = stream, .FileName = "speech.wav"})

            Assert.AreEqual("req-1", response.RequestId)
            Assert.AreEqual("上海今天天气怎么样？", response.Text)
            Assert.AreEqual(3200L, response.DurationMs)
        End Using

        Assert.AreEqual("https://api.deepseek.com/v1/tools/asr", capturedRequest.Uri.AbsoluteUri)
        Assert.AreEqual(1, capturedRequest.Parts.Count)
        Assert.AreEqual("file", capturedRequest.Parts(0).Name)
        Assert.AreEqual("speech.wav", capturedRequest.Parts(0).FileName)
        CollectionAssert.AreEqual(fileBytes, capturedRequest.Parts(0).Bytes)
    End Function

    <TestMethod>
    Public Async Function Ocr_SendsPurposeBeforeFileAndParsesResponseAsync() As Task
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(OcrJson)
            End Function)

        Using stream As New MemoryStream(New Byte() {9, 8, 7})
            Dim client As New OcrClient("test-key", New HttpClient(handler))
            Dim response = Await client.RecognizeAsync(
                New OcrRequest(stream, "document.pdf", OcrPurposes.TextRecognition))

            Assert.AreEqual("ocr-1", response.Id)
            Assert.AreEqual(2, response.PageCount)
            Assert.AreEqual(2, response.Pages.Count)
            Assert.AreEqual("page one", response.Pages(0).Text)
            Assert.AreEqual("length", response.Pages(1).FinishReason)
            Assert.AreEqual(15, response.Usage.TotalTokens)
            Assert.IsNull(response.Error)
        End Using

        Assert.AreEqual(2, capturedRequest.Parts.Count)
        Assert.AreEqual("purpose", capturedRequest.Parts(0).Name)
        Assert.AreEqual("text_recognition", Encoding.UTF8.GetString(capturedRequest.Parts(0).Bytes))
        Assert.AreEqual("file", capturedRequest.Parts(1).Name)
        Assert.AreEqual("document.pdf", capturedRequest.Parts(1).FileName)
    End Function

    <TestMethod>
    Public Async Function Ocr_ThrowsOnErrorBodyAsync() As Task
        Dim handler As New StubHttpMessageHandler(
            Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                Return Task.FromResult(CreateJsonResponse(OcrErrorJson))
            End Function)

        Dim client As New OcrClient("test-key", New HttpClient(handler))
        Try
            Await client.RecognizeAsync(New OcrRequest(New MemoryStream(New Byte() {1}), "x.pdf", OcrPurposes.TextRecognition))
            Assert.Fail("Exception expected.")
        Catch ex As DeepSeekHttpRequestException
            Assert.AreEqual("gateway_timeout_error", ex.Details.Code)
            StringAssert.Contains(ex.Message, "OCR failed")
        End Try
    End Function

    <TestMethod>
    Public Async Function Tts_WritesAudioToDestinationAsync() As Task
        Dim audioBytes As Byte() = {82, 73, 70, 70, 1, 2, 3, 4}
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                Dim captured = Await CaptureAsync(request)
                Assert.AreEqual("audio/wav", request.Headers.Accept.ToString())
                StringAssert.Contains(captured.Body, """text"":""hello""")
                Dim content As New ByteArrayContent(audioBytes)
                content.Headers.ContentType = New MediaTypeHeaderValue("audio/wav")
                Return New HttpResponseMessage(HttpStatusCode.OK) With {.Content = content}
            End Function)

        Using destination As New MemoryStream
            Dim client As New TtsClient("test-key", New HttpClient(handler))
            Dim info = Await client.SynthesizeAsync(New TtsRequest With {.Text = "hello"}, destination)

            CollectionAssert.AreEqual(audioBytes, destination.ToArray())
            Assert.AreEqual("audio/wav", info.ContentType)
            Assert.AreEqual(TtsFormats.Wav, info.Format)
        End Using
    End Function

    <TestMethod>
    Public Async Function Tts_ThrowsOnErrorResponseAsync() As Task
        Dim handler As New StubHttpMessageHandler(
            Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                Return Task.FromResult(CreateJsonResponse(ErrorJson, HttpStatusCode.BadRequest))
            End Function)

        Dim client As New TtsClient("test-key", New HttpClient(handler))
        Try
            Await client.SynthesizeAsync(New TtsRequest With {.Text = "hello"}, New MemoryStream)
            Assert.Fail("Exception expected.")
        Catch ex As DeepSeekHttpRequestException
            Assert.AreEqual("invalid_request_error", ex.Details.Code)
        End Try
    End Function

    Private Shared Function CreateJsonResponse(json As String,
                                              Optional status As HttpStatusCode = HttpStatusCode.OK) As HttpResponseMessage
        Return New HttpResponseMessage(status) With {
            .Content = New StringContent(json, Encoding.UTF8, "application/json")
        }
    End Function

    Private Shared Async Function CaptureAsync(request As HttpRequestMessage) As Task(Of CapturedRequest)
        Dim captured As New CapturedRequest With {
            .Method = request.Method,
            .Uri = request.RequestUri
        }
        If request.Content Is Nothing Then Return captured

        Dim form = TryCast(request.Content, MultipartFormDataContent)
        If form Is Nothing Then
            captured.Body = Await request.Content.ReadAsStringAsync()
            Return captured
        End If

        For Each part As HttpContent In form
            Dim disposition = part.Headers.ContentDisposition
            captured.Parts.Add(New CapturedPart With {
                .Name = If(disposition Is Nothing, Nothing, disposition.Name),
                .FileName = If(disposition Is Nothing, Nothing, disposition.FileName?.Trim(""""c)),
                .Bytes = Await part.ReadAsByteArrayAsync()
            })
        Next
        Return captured
    End Function

    Private Class CapturedRequest
        Public Property Method As HttpMethod
        Public Property Uri As Uri
        Public Property Body As String
        Public ReadOnly Property Parts As New List(Of CapturedPart)
    End Class

    Private Class CapturedPart
        Public Property Name As String
        Public Property FileName As String
        Public Property Bytes As Byte()
    End Class

End Class

''' <summary>
''' A stub <see cref="HttpMessageHandler"/> that returns canned responses.
''' </summary>
Friend Class StubHttpMessageHandler
    Inherits HttpMessageHandler

    Private ReadOnly _handler As Func(Of HttpRequestMessage, Task(Of HttpResponseMessage))

    Sub New(handler As Func(Of HttpRequestMessage, Task(Of HttpResponseMessage)))
        _handler = handler
    End Sub

    Protected Overrides Function SendAsync(request As HttpRequestMessage,
                                           cancellationToken As CancellationToken) As Task(Of HttpResponseMessage)
        Return _handler(request)
    End Function
End Class
