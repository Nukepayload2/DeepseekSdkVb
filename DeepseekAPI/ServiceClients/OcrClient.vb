Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models
Imports Nukepayload2.AI.Providers.Deepseek.Utils

''' <summary>
''' Provides the OCR API.
''' </summary>
''' <remarks>
''' Recognizes text in PDF, Word, PowerPoint documents and images, page by page.
''' The file is not stored in the Files API. See
''' <see href="https://api-docs.deepseek.com/zh-cn/api/ocr">OCR API</see>.
''' </remarks>
Public Class OcrClient
    Inherits ClientFeatureBase

    ''' <summary>
    ''' Timeout for upload and recognition. Defaults to 5 minutes.
    ''' </summary>
    Public Property RequestTimeout As TimeSpan = TimeSpan.FromMinutes(5)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client, DeepSeekClient.DefaultLongRunningHttpClient)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/v1/tools/ocr"

    ''' <summary>
    ''' Recognizes text page by page.
    ''' </summary>
    ''' <param name="request">The OCR request.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The recognition result.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request failed, either with a non-success status code or with an error object
    ''' embedded in an HTTP 200 response.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    Public Async Function RecognizeAsync(request As OcrRequest,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of OcrResponse)
        ValidateRequest(request)
        Using cts = CreateTimeout(cancellationToken, RequestTimeout)
            Using form = CreateForm(request)
                Dim json = Await PostMultipartAsync(RequestUrl, form, cts.Token)
                Dim memory = Await IoUtils.ToMemoryStreamAsync(json, cts.Token)
                Dim result = OcrResponse.FromJson(memory)
                If result.Error IsNot Nothing Then
                    Throw New DeepSeekHttpRequestException("OCR failed: " & result.Error.Message, result.Error)
                End If
                Return result
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Recognizes text in a local file.
    ''' </summary>
    ''' <param name="filePath">Path of the file to recognize.</param>
    ''' <param name="purpose">One of <see cref="OcrPurposes"/>.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The recognition result.</returns>
    Public Async Function RecognizeAsync(filePath As String, purpose As String,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of OcrResponse)
        If filePath Is Nothing Then Throw New ArgumentNullException(NameOf(filePath))
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim request As New OcrRequest With {
                .File = fs,
                .FileName = Path.GetFileName(filePath),
                .Purpose = purpose
            }
            Return Await RecognizeAsync(request, cancellationToken)
        End Using
    End Function

    Private Shared Function CreateForm(request As OcrRequest) As MultipartFormDataContent
        ' The API requires the purpose field before the file field.
        Dim form As New MultipartFormDataContent From {
            {New StringContent(request.Purpose), "purpose"}
        }
        Dim fileContent As New LeaveOpenStreamContent(request.File)
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream")
        form.Add(fileContent, "file", request.FileName)
        Return form
    End Function

    Private Shared Sub ValidateRequest(request As OcrRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If request.File Is Nothing Then
            Throw New ArgumentException("File is required.", NameOf(request))
        End If
        If String.IsNullOrEmpty(request.FileName) Then
            Throw New ArgumentException("FileName is required.", NameOf(request))
        End If
        If request.Purpose <> OcrPurposes.TextRecognition AndAlso
           request.Purpose <> OcrPurposes.RichLayoutRecognition Then
            Throw New ArgumentException("Purpose must be text_recognition or rich_layout_recognition.", NameOf(request))
        End If
        If request.File.CanSeek AndAlso request.File.Length - request.File.Position > 100L * 1024 * 1024 Then
            Throw New ArgumentException("File must be at most 100 MiB.", NameOf(request))
        End If
    End Sub

End Class
