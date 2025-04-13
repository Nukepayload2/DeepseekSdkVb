Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Public MustInherit Class CompletionClientBase(Of TRequest, TResponse)
    Inherits ClientFeatureBase

    Sub New(apiKey As String, client As HttpClient)
        MyBase.New(apiKey, client)
    End Sub

    Protected MustOverride ReadOnly Property RequestUrl As String

    Protected MustOverride Function FromJson(memoryStream As MemoryStream) As TResponse

    Protected MustOverride Function IsStream(textRequestBody As TRequest) As Boolean

    Protected MustOverride Function ToUtf8Json(textRequestBody As TRequest) As MemoryStream

    Public Overridable Async Function CompleteAsync(
            textRequestBody As TRequest,
            Optional cancellationToken As CancellationToken = Nothing) As Task(Of TResponse)
        If IsStream(textRequestBody) Then Throw New ArgumentException("You must set Stream to False.", NameOf(textRequestBody))
        Return FromJson(Await CompleteRawAsync(textRequestBody, cancellationToken))
    End Function

    Public Overridable Async Function StreamAsync(textRequestBody As TRequest,
                                      yieldCallback As Action(Of TResponse),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await StreamAsync(textRequestBody,
                          Function(resp)
                              yieldCallback(resp)
                              Return Task.CompletedTask
                          End Function, cancellationToken)
    End Function

    Public Overridable Async Function StreamAsync(textRequestBody As TRequest,
                                      yieldCallback As Func(Of TResponse, Task),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        If Not IsStream(textRequestBody) Then Throw New ArgumentException("You must set Stream to True.", NameOf(textRequestBody))

        Dim reader As New ServerSentEventReader(
            Async Function(jsonStream) As Task
                Dim response = FromJson(jsonStream)
                Debug.WriteLine("Server stream delta: " & IoUtils.UTF8NoBOM.GetString(jsonStream.ToArray))
                If response IsNot Nothing Then
                    Await yieldCallback(response)
                End If
            End Function,
            Async Function(jsonStream) As Task
                Dim response = FromJson(jsonStream)
                Debug.WriteLine("Server non-streaming error: " & IoUtils.UTF8NoBOM.GetString(jsonStream.ToArray))
                If response IsNot Nothing Then
                    Await yieldCallback(response)
                End If
            End Function)

        Await StreamUtf8Async(textRequestBody, AddressOf reader.OnChunkAsync, cancellationToken)
    End Function

    Private Async Function CompleteRawAsync(textRequestBody As TRequest, cancellation As CancellationToken) As Task(Of MemoryStream)
        Dim json = ToUtf8Json(textRequestBody)
#If DEBUG Then
        Debug.WriteLine("Sending chat request: ")
        Debug.WriteLine(IoUtils.UTF8NoBOM.GetString(json.ToArray()))
#End If
        Return Await PostAsync(RequestUrl, json, cancellation)
    End Function

    Private Async Function StreamUtf8Async(textRequestBody As TRequest,
                                           yieldCallback As Func(Of ReadOnlyMemory(Of Byte), Task),
                                           cancellationToken As CancellationToken) As Task
        Dim json = ToUtf8Json(textRequestBody)

        Dim response = Await PostRawAsync(RequestUrl, json, cancellationToken)
#If NET6_0_OR_GREATER Then
        Dim stream = Await response.Content.ReadAsStreamAsync(cancellationToken)
#Else
        Dim stream = Await response.Content.ReadAsStreamAsync()
#End If

        Await ErrorHandler.ThrowForNonSuccessAsync(response, stream, cancellationToken)
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