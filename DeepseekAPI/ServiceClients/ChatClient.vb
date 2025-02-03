Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Public Class ChatClient
    Inherits ClientFeatureBase

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Public Async Function CompleteAsync(textRequestBody As ChatRequest,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of ChatResponse)
        If textRequestBody.Stream Then Throw New ArgumentException("You must set Stream to False.", NameOf(textRequestBody))
        Return ChatResponse.FromJson(Await CompleteRawAsync(textRequestBody, cancellationToken))
    End Function

    Public Async Function StreamAsync(textRequestBody As ChatRequest,
                                      yieldCallback As Action(Of ChatResponse),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await StreamAsync(textRequestBody,
                          Function(resp)
                              yieldCallback(resp)
                              Return Task.CompletedTask
                          End Function, cancellationToken)
    End Function

    Public Async Function StreamAsync(textRequestBody As ChatRequest,
                                      yieldCallback As Func(Of ChatResponse, Task),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        If Not textRequestBody.Stream Then Throw New ArgumentException("You must set Stream to True.", NameOf(textRequestBody))

        Dim reader As New ServerSentEventReader(
            Async Function(jsonStream) As Task
                Dim response = ChatResponse.FromJson(jsonStream)
                Debug.WriteLine("Server stream delta: " & IoUtils.UTF8NoBOM.GetString(jsonStream.ToArray))
                If response IsNot Nothing Then
                    Await yieldCallback(response)
                End If
            End Function)

        Await StreamUtf8Async(textRequestBody, AddressOf reader.OnChunkAsync, cancellationToken)
    End Function

    Private Async Function CompleteRawAsync(textRequestBody As ChatRequest, cancellation As CancellationToken) As Task(Of MemoryStream)
        Dim json = textRequestBody?.ToJsonUtf8
        Const requestUrl = "https://api.deepseek.com/chat/completions"
#If DEBUG Then
        Debug.WriteLine("Sending chat request: ")
        Debug.WriteLine(IoUtils.UTF8NoBOM.GetString(json.ToArray()))
#End If
        Return Await PostAsync(requestUrl, json, cancellation)
    End Function

    Private Async Function StreamUtf8Async(textRequestBody As ChatRequest,
                                           yieldCallback As Func(Of ReadOnlyMemory(Of Byte), Task),
                                           cancellationToken As CancellationToken) As Task
        Dim json = textRequestBody?.ToJsonUtf8
        Const requestUrl = "https://api.deepseek.com/chat/completions"

        Dim response = Await PostRawAsync(requestUrl, json, cancellationToken)
#If NET6_0_OR_GREATER Then
        Dim stream = Await response.Content.ReadAsStreamAsync(cancellationToken)
#Else
        Dim stream = Await response.Content.ReadAsStreamAsync()
#End If

        Dim buffer(8192) As Byte
        While True
#If NET6_0_OR_GREATER Then
            Dim bytesRead = Await stream.ReadAsync(buffer, cancellationToken)
#Else
            Dim bytesRead = Await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)
#End If

            If bytesRead <= 0 Then
                Exit While
            End If
            cancellationToken.ThrowIfCancellationRequested()
            Await yieldCallback(buffer.AsMemory(0, bytesRead))
        End While
    End Function

End Class
