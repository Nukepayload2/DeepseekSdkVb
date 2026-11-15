Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models
Imports Nukepayload2.AI.Providers.Deepseek.Utils

''' <summary>
''' Provides the speech recognition API.
''' </summary>
''' <remarks>
''' Sends a whole audio file in one request and returns the complete transcription.
''' See <see href="https://api-docs.deepseek.com/zh-cn/api/asr">ASR API</see>.
''' </remarks>
Public Class AsrClient
    Inherits ClientFeatureBase

    ''' <summary>
    ''' Timeout for upload and recognition. Defaults to 10 minutes.
    ''' </summary>
    Public Property RequestTimeout As TimeSpan = TimeSpan.FromMinutes(10)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client, DeepSeekClient.DefaultLongRunningHttpClient)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/v1/tools/asr"

    ''' <summary>
    ''' Transcribes an audio file.
    ''' </summary>
    ''' <param name="request">The ASR request.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The transcription result.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON error.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    Public Async Function TranscribeAsync(request As AsrRequest,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of AsrResponse)
        ValidateRequest(request)
        Using cts = CreateTimeout(cancellationToken, RequestTimeout)
            Using form = CreateForm(request)
                Dim json = Await PostMultipartAsync(RequestUrl, form, cts.Token)
                Dim memory = Await IoUtils.ToMemoryStreamAsync(json, cts.Token)
                Return AsrResponse.FromJson(memory)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Transcribes a local audio file.
    ''' </summary>
    ''' <param name="filePath">Path of a 16-bit PCM WAV or Ogg Opus file.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The transcription result.</returns>
    Public Async Function TranscribeAsync(filePath As String,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of AsrResponse)
        If filePath Is Nothing Then Throw New ArgumentNullException(NameOf(filePath))
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim request As New AsrRequest With {
                .File = fs,
                .FileName = Path.GetFileName(filePath)
            }
            Return Await TranscribeAsync(request, cancellationToken)
        End Using
    End Function

    Private Shared Function CreateForm(request As AsrRequest) As MultipartFormDataContent
        Dim form As New MultipartFormDataContent()
        Dim fileContent As New LeaveOpenStreamContent(request.File)
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream")
        form.Add(fileContent, "file", request.FileName)
        Return form
    End Function

    Private Shared Sub ValidateRequest(request As AsrRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If request.File Is Nothing Then
            Throw New ArgumentException("File is required.", NameOf(request))
        End If
        If String.IsNullOrEmpty(request.FileName) Then
            Throw New ArgumentException("FileName is required.", NameOf(request))
        End If
        If request.File.CanSeek AndAlso request.File.Length - request.File.Position > 8L * 1024 * 1024 Then
            Throw New ArgumentException("File must be at most 8 MiB.", NameOf(request))
        End If
    End Sub

End Class
