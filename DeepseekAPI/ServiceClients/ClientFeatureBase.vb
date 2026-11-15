Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Public MustInherit Class ClientFeatureBase

    Private ReadOnly _apiKey As String
    Private ReadOnly _client As HttpClient

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Me.New(apiKey, client, DeepSeekClient.DefaultHttpClient)
    End Sub

    ''' <summary>
    ''' Initializes the client with an explicit fallback <see cref="HttpClient"/>.
    ''' </summary>
    ''' <remarks>
    ''' Long-running APIs (OCR, ASR, TTS) pass <see cref="DeepSeekClient.DefaultLongRunningHttpClient"/>
    ''' so that the shared 100-second timeout does not abort uploads or audio streaming.
    ''' </remarks>
    Protected Sub New(apiKey As String, client As HttpClient, fallback As HttpClient)
        _apiKey = apiKey
        _client = If(client, fallback)
    End Sub

    Protected Async Function ReadAndCheckErrorAsync(response As HttpResponseMessage, cancellation As CancellationToken) As Task(Of Stream)
        If response.StatusCode = Net.HttpStatusCode.OK Then
#If NET5_0_OR_GREATER Then
            Return Await response.Content.ReadAsStreamAsync(cancellation)
#Else
            Return Await response.Content.ReadAsStreamAsync()
#End If
        End If
        Dim result = Await IoUtils.CopyToMemoryStreamAsync(response, cancellation)
        ErrorHandler.ThrowForNonSuccess(response, result)
        Return result
    End Function

    Protected Async Function PostAsync(requestUrl As String, json As Stream, cancellation As CancellationToken) As Task(Of Stream)
        Dim data As New StreamContent(json)
        data.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/json")
        Dim response As HttpResponseMessage = Await PostRawAsync(requestUrl, data, cancellation)
        Return Await ReadAndCheckErrorAsync(response, cancellation)
    End Function

    ''' <summary>
    ''' Sends a multipart form and returns the response body stream.
    ''' </summary>
    Protected Async Function PostMultipartAsync(requestUrl As String, form As MultipartFormDataContent,
                                                cancellation As CancellationToken) As Task(Of Stream)
        Dim response As HttpResponseMessage = Await PostRawAsync(requestUrl, form, cancellation)
        Return Await ReadAndCheckErrorAsync(response, cancellation)
    End Function

    ''' <summary>
    ''' Sends a JSON request and returns the raw <see cref="HttpResponseMessage"/> on success.
    ''' </summary>
    ''' <remarks>
    ''' Used by APIs whose response is a binary stream (for example TTS).
    ''' Non-success responses are parsed by <see cref="ErrorHandler"/> like the JSON APIs.
    ''' </remarks>
    Protected Async Function PostForStreamAsync(requestUrl As String, json As Stream, accept As String,
                                                cancellation As CancellationToken) As Task(Of HttpResponseMessage)
        Dim data As New StreamContent(json)
        data.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/json")
        Dim response As HttpResponseMessage = Await PostRawAsync(requestUrl, data, cancellation, accept)
        If response.StatusCode <> Net.HttpStatusCode.OK Then
            Dim result = Await IoUtils.CopyToMemoryStreamAsync(response, cancellation)
            ErrorHandler.ThrowForNonSuccess(response, result)
        End If
        Return response
    End Function

    ''' <summary>
    ''' Combines the caller's cancellation token with a client-specific timeout.
    ''' </summary>
    Protected Function CreateTimeout(cancellation As CancellationToken, timeout As TimeSpan) As CancellationTokenSource
        Dim cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation)
        If timeout <> Global.System.Threading.Timeout.InfiniteTimeSpan AndAlso timeout > TimeSpan.Zero Then
            cts.CancelAfter(timeout)
        End If
        Return cts
    End Function

    Protected Async Function PostRawAsync(requestUrl As String, data As HttpContent, cancellation As CancellationToken,
                                          Optional accept As String = "application/json") As Task(Of HttpResponseMessage)
        Dim request As New HttpRequestMessage With {
            .Method = HttpMethod.Post,
            .RequestUri = New Uri(requestUrl),
            .Content = data
        }
        With request.Headers
            If Not String.IsNullOrEmpty(accept) Then
                .Accept.TryParseAdd(accept)
            End If
            .Add("Authorization", "Bearer " & _apiKey)
        End With
        Dim response = Await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation)
        Return response
    End Function

    Protected Async Function GetAsync(requestUrl As String, cancellation As CancellationToken) As Task(Of Stream)
        Dim request As New HttpRequestMessage With {
            .Method = HttpMethod.Get,
            .RequestUri = New Uri(requestUrl)
        }
        With request.Headers
            .Accept.TryParseAdd("application/json")
            .Add("Authorization", "Bearer " & _apiKey)
        End With
        Dim response = Await _client.SendAsync(request, cancellation)
        Return Await ReadAndCheckErrorAsync(response, cancellation)
    End Function

    ''' <summary>
    ''' Sends a DELETE request and returns the response body stream.
    ''' </summary>
    Protected Async Function DeleteResourceAsync(requestUrl As String, cancellation As CancellationToken) As Task(Of Stream)
        Dim request As New HttpRequestMessage With {
            .Method = HttpMethod.Delete,
            .RequestUri = New Uri(requestUrl)
        }
        With request.Headers
            .Accept.TryParseAdd("application/json")
            .Add("Authorization", "Bearer " & _apiKey)
        End With
        Dim response = Await _client.SendAsync(request, cancellation)
        Return Await ReadAndCheckErrorAsync(response, cancellation)
    End Function

End Class
