Imports System.IO
Imports System.Net
Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Nukepayload2.AI.Providers.Deepseek
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Offline tests for the Files API and multimodal chat content.
''' </summary>
<TestClass>
Public Class FilesHttpTests

    Private Const FileJson As String =
        "{""id"":""file-api-1"",""object"":""file"",""bytes"":1024,""created_at"":1700000000,""filename"":""a.jpg"",""purpose"":""user_data"",""expires_at"":1700003600}"

    Private Const FileListJson As String =
        "{""object"":""list"",""data"":[{""id"":""file-api-1"",""object"":""file"",""bytes"":1024,""created_at"":1700000000,""filename"":""a.jpg"",""purpose"":""user_data"",""expires_at"":null}],""first_id"":""file-api-1"",""last_id"":""file-api-1"",""has_more"":false}"

    Private Const FileDeleteJson As String =
        "{""id"":""file-api-1"",""object"":""file"",""deleted"":true}"

    <TestMethod>
    Public Sub ChatMessage_StringContentStillSerializesAsString()
        Dim request As New ChatRequest With {
            .Model = ModelNames.V4Flash,
            .Messages = {New ChatMessage(ChatRoles.User, "hello")}
        }
        Dim json = request.ToJson()
        StringAssert.Contains(json, """content"":""hello""")
        Assert.IsFalse(json.Contains("""type"""))
    End Sub

    <TestMethod>
    Public Sub ChatMessage_PartsSerializeAsArray()
        Dim message As New ChatMessage(ChatRoles.User, Nothing) With {
            .Parts = {
                ChatContentPart.FromText("描述这张图"),
                ChatContentPart.FromImageUrl("https://example.com/a.jpg", ChatImageDetailLevels.Low),
                ChatContentPart.FromFileId("file-api-123")
            }
        }
        Dim request As New ChatRequest With {
            .Model = ModelNames.V4Flash,
            .Messages = {message}
        }
        Dim json = request.ToJson()

        StringAssert.Contains(json, """content"":[{""type"":""text"",""text"":""描述这张图""}")
        StringAssert.Contains(json, """type"":""image_url""")
        StringAssert.Contains(json, """detail"":""low""")
        StringAssert.Contains(json, """file_id"":""file-api-123""")
    End Sub

    <TestMethod>
    Public Sub ChatMessage_InlineFileSerializesDataUrl()
        Dim message As New ChatMessage(ChatRoles.User, Nothing) With {
            .Parts = {ChatContentPart.FromFileBytes("image/jpeg", New Byte() {1, 2}, "a.jpg")}
        }
        Dim json = New ChatRequest With {
            .Model = ModelNames.V4Flash,
            .Messages = {message}
        }.ToJson()

        StringAssert.Contains(json, """file_data"":""data:image/jpeg;base64,AQI=""")
        StringAssert.Contains(json, """filename"":""a.jpg""")
    End Sub

    <TestMethod>
    Public Async Function Upload_SendsOrderedMultipartAndParsesFileAsync() As Task
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(FileJson)
            End Function)

        Using stream As New MemoryStream(New Byte() {9, 8, 7})
            Dim client As New FilesClient("test-key", New HttpClient(handler))
            Dim response = Await client.UploadAsync(
                New FileUploadRequest With {
                    .File = stream,
                    .FileName = "a.jpg",
                    .Purpose = FilePurposes.UserData,
                    .ExpiresAfterAnchor = "created_at",
                    .ExpiresAfterSeconds = 3600
                })

            Assert.AreEqual("file-api-1", response.Id)
            Assert.AreEqual(1024L, response.Bytes)
            Assert.AreEqual(1700003600L, response.ExpiresAt)
        End Using

        Assert.AreEqual("https://api.deepseek.com/files", capturedRequest.Uri.AbsoluteUri)
        Assert.AreEqual(4, capturedRequest.Parts.Count)
        Assert.AreEqual("purpose", capturedRequest.Parts(0).Name)
        Assert.AreEqual("user_data", Encoding.UTF8.GetString(capturedRequest.Parts(0).Bytes))
        Assert.AreEqual("expires_after[anchor]", capturedRequest.Parts(1).Name)
        Assert.AreEqual("created_at", Encoding.UTF8.GetString(capturedRequest.Parts(1).Bytes))
        Assert.AreEqual("expires_after[seconds]", capturedRequest.Parts(2).Name)
        Assert.AreEqual("3600", Encoding.UTF8.GetString(capturedRequest.Parts(2).Bytes))
        Assert.AreEqual("file", capturedRequest.Parts(3).Name)
        Assert.AreEqual("a.jpg", capturedRequest.Parts(3).FileName)
        CollectionAssert.AreEqual(New Byte() {9, 8, 7}, capturedRequest.Parts(3).Bytes)
    End Function

    <TestMethod>
    Public Async Function List_BuildsQueryStringAsync() As Task
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(FileListJson)
            End Function)

        Dim client As New FilesClient("test-key", New HttpClient(handler))
        Dim response = Await client.ListAsync(
            New FilesListRequest With {
                .After = "file-api-1",
                .Limit = 2,
                .Order = "desc",
                .Purpose = FilePurposes.UserData
            })

        Assert.AreEqual("https://api.deepseek.com/files?after=file-api-1&limit=2&order=desc&purpose=user_data",
                        capturedRequest.Uri.AbsoluteUri)
        Assert.AreEqual(1, response.Data.Count)
        Assert.AreEqual("file-api-1", response.FirstId)
        Assert.AreEqual("file-api-1", response.LastId)
        Assert.AreEqual(False, response.HasMore)
    End Function

    <TestMethod>
    Public Async Function Retrieve_UsesFileIdPathAsync() As Task
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(FileJson)
            End Function)

        Dim client As New FilesClient("test-key", New HttpClient(handler))
        Dim response = Await client.RetrieveAsync("file-api-1")

        Assert.AreEqual("https://api.deepseek.com/files/file-api-1", capturedRequest.Uri.AbsoluteUri)
        Assert.AreEqual("a.jpg", response.Filename)
        Assert.AreEqual(FilePurposes.UserData, response.Purpose)
    End Function

    <TestMethod>
    Public Async Function Delete_UsesDeleteAndParsesResponseAsync() As Task
        Dim capturedRequest As CapturedRequest = Nothing
        Dim handler As New StubHttpMessageHandler(
            Async Function(request As HttpRequestMessage) As Task(Of HttpResponseMessage)
                capturedRequest = Await CaptureAsync(request)
                Return CreateJsonResponse(FileDeleteJson)
            End Function)

        Dim client As New FilesClient("test-key", New HttpClient(handler))
        Dim response = Await client.DeleteAsync("file-api-1")

        Assert.AreEqual(HttpMethod.Delete, capturedRequest.Method)
        Assert.AreEqual("https://api.deepseek.com/files/file-api-1", capturedRequest.Uri.AbsoluteUri)
        Assert.AreEqual(True, response.Deleted)
        Assert.AreEqual("file-api-1", response.Id)
    End Function

    Private Shared Function CreateJsonResponse(json As String) As HttpResponseMessage
        Return New HttpResponseMessage(HttpStatusCode.OK) With {
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
