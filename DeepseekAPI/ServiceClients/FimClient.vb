Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

Public Class FimClient
    Inherits CompletionClientBase(Of FimRequest, FimResponse)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overrides ReadOnly Property RequestUrl As String = "https://api.deepseek.com/beta/completions"

    ''' <summary>
    ''' Complete the fill-in-middlle request.
    ''' </summary>
    ''' <param name="request">The request message</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The response message or error message.</returns>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful and the response stream is empty.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function CompleteAsync(request As FimRequest,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of FimResponse)
        Return Await MyBase.CompleteAsync(request, cancellationToken)
    End Function

    ''' <summary>
    ''' Complete the fill-in-middlle request.
    ''' </summary>
    ''' <param name="request">The request message</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <param name="yieldCallback">The body of async for each loop.</param>
    ''' <returns>The response message or error message.</returns>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful and the response stream is empty.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function StreamAsync(request As FimRequest,
                                      yieldCallback As Action(Of FimResponse),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await MyBase.StreamAsync(request, yieldCallback, cancellationToken)
    End Function

    ''' <summary>
    ''' Complete the fill-in-middlle request.
    ''' </summary>
    ''' <param name="request">The request message</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <param name="yieldCallback">The body of async for each loop.</param>
    ''' <returns>The response message or error message.</returns>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful and the response stream is empty.
    ''' </exception>
    ''' <remarks>
    ''' <see href="https://api-docs.deepseek.com/api/create-completion">Online documentation</see>
    ''' </remarks>
    Public Overrides Async Function StreamAsync(request As FimRequest,
                                      yieldCallback As Func(Of FimResponse, Task),
                                      Optional cancellationToken As CancellationToken = Nothing) As Task
        Await MyBase.StreamAsync(request, yieldCallback, cancellationToken)
    End Function

    Protected Overrides Function FromJson(memoryStream As MemoryStream) As FimResponse
        Return FimResponse.FromJson(memoryStream)
    End Function

    Protected Overrides Function IsStream(textRequestBody As FimRequest) As Boolean
        Return textRequestBody.Stream.GetValueOrDefault()
    End Function

    Protected Overrides Function ToUtf8Json(textRequestBody As FimRequest) As MemoryStream
        Return textRequestBody.ToJsonUtf8()
    End Function

End Class
