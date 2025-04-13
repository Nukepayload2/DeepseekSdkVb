Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

Public Class ChatClient
    Inherits CompletionClientBase(Of ChatRequest, ChatResponse)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overrides ReadOnly Property RequestUrl As String = "https://api.deepseek.com/chat/completions"

    Protected Overrides Function FromJson(memoryStream As MemoryStream) As ChatResponse
        Return ChatResponse.FromJson(memoryStream)
    End Function

    Protected Overrides Function IsStream(textRequestBody As ChatRequest) As Boolean
        Return textRequestBody.Stream.GetValueOrDefault
    End Function

    Protected Overrides Function ToUtf8Json(textRequestBody As ChatRequest) As MemoryStream
        Return textRequestBody.ToJsonUtf8
    End Function

    ''' <summary>
    ''' Chat completion without streaming.
    ''' </summary>
    ''' <param name="textRequestBody">The request message.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The response message.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-chat-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function CompleteAsync(textRequestBody As ChatRequest,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of ChatResponse)
        Return Await MyBase.CompleteAsync(textRequestBody, cancellationToken)
    End Function

    ''' <summary>
    ''' Chat completion with streaming.
    ''' </summary>
    ''' <param name="textRequestBody">The request message.</param>
    ''' <param name="yieldCallback">The body of async for each loop.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The response message.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-chat-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function StreamAsync(textRequestBody As ChatRequest,
                                      yieldCallback As Action(Of ChatResponse),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await MyBase.StreamAsync(textRequestBody, yieldCallback, cancellationToken)
    End Function

    ''' <summary>
    ''' Chat completion with streaming.
    ''' </summary>
    ''' <param name="textRequestBody">The request message.</param>
    ''' <param name="yieldCallback">The body of async for each loop.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The response message.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-chat-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function StreamAsync(textRequestBody As ChatRequest,
                                      yieldCallback As Func(Of ChatResponse, Task),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await MyBase.StreamAsync(textRequestBody, yieldCallback, cancellationToken)
    End Function

End Class
