Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Public MustInherit Class ClientFeatureBase

    Private ReadOnly _apiKey As String
    Private ReadOnly _client As HttpClient

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        _apiKey = apiKey
        _client = If(client, DeepSeekClient.DefaultHttpClient)
    End Sub

    Protected Async Function ReadAndCheckErrorAsync(response As HttpResponseMessage, cancellation As CancellationToken) As Task(Of MemoryStream)
        Dim result = Await IoUtils.CopyToMemoryStreamAsync(response, cancellation)
        ErrorHandler.ThrowForNonSuccess(response, result)
        Return result
    End Function

    Protected Async Function PostAsync(requestUrl As String, json As Stream, cancellation As CancellationToken) As Task(Of MemoryStream)
        Dim data As New StreamContent(json)
        data.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/json")
        Dim response As HttpResponseMessage = Await PostRawAsync(requestUrl, data, cancellation)
        Return Await ReadAndCheckErrorAsync(response, cancellation)
    End Function

    Protected Async Function PostRawAsync(requestUrl As String, data As HttpContent, cancellation As CancellationToken) As Task(Of HttpResponseMessage)
        Dim request As New HttpRequestMessage With {
            .Method = HttpMethod.Post,
            .RequestUri = New Uri(requestUrl),
            .Content = data
        }
        With request.Headers
            .Accept.TryParseAdd("application/json")
            .Add("Authorization", "Bearer " & _apiKey)
        End With
        Dim response = Await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation)
        Return response
    End Function

    Protected Async Function GetAsync(requestUrl As String, cancellation As CancellationToken) As Task(Of MemoryStream)
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

End Class
