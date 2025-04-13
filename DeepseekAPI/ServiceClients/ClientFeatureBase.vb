Imports System.IO
Imports System.Net.Http
Imports System.Threading

Public MustInherit Class ClientFeatureBase

    Private ReadOnly _apiKey As String
    Private ReadOnly _client As HttpClient

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        _apiKey = apiKey
        _client = If(client, DeepSeekClient.DefaultHttpClient)
    End Sub

    Protected Async Function PostAsync(requestUrl As String, json As Stream, cancellation As CancellationToken) As Task(Of MemoryStream)
        Dim response As HttpResponseMessage = Await PostRawAsync(requestUrl, json, cancellation)
#If NET6_0_OR_GREATER Then
        Dim stream = Await response.Content.ReadAsStreamAsync(cancellation)
#Else
        Dim stream = Await response.Content.ReadAsStreamAsync()
#End If
        Dim result As New MemoryStream
        Await stream.CopyToAsync(result, 8192, cancellation)
        result.Position = 0
        If result.Length = 0 Then
            ' 流里面没东西，请求又失败了，错误信息就从 HTTP 响应里面取
            response.EnsureSuccessStatusCode()
        End If
        Return result
    End Function

    Protected Async Function PostRawAsync(requestUrl As String, json As Stream, cancellation As CancellationToken) As Task(Of HttpResponseMessage)
        Dim data As New StreamContent(json)
        data.Headers.ContentType = New Headers.MediaTypeHeaderValue("application/json")
        Dim request As New HttpRequestMessage With {
            .Method = HttpMethod.Post,
            .RequestUri = New Uri(requestUrl),
            .Content = data
        }
        request.Headers.Add("Authorization", "Bearer " & _apiKey)
        Dim response = Await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellation)
        Return response
    End Function
End Class
