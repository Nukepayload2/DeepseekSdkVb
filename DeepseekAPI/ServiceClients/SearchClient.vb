Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Provides the web search API.
''' </summary>
''' <remarks>
''' See <see href="https://api-docs.deepseek.com/zh-cn/api/search">Search API</see>.
''' </remarks>
Public Class SearchClient
    Inherits ClientFeatureBase

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/v1/tools/search"

    ''' <summary>
    ''' Searches the web and returns a ranked list of structured results.
    ''' </summary>
    ''' <param name="request">The search request.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The search results.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON error.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    Public Async Function SearchAsync(request As SearchRequest,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of SearchResponse)
        ValidateRequest(request)
        Dim json = request.ToJsonUtf8()
        Dim response = Await PostAsync(RequestUrl, json, cancellationToken)
        Return SearchResponse.FromJson(response)
    End Function

    Private Shared Sub ValidateRequest(request As SearchRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If String.IsNullOrWhiteSpace(request.Query) Then
            Throw New ArgumentException("Query is required and cannot be blank.", NameOf(request))
        End If
        If request.Query.Length > 128 Then
            Throw New ArgumentException("Query must be at most 128 characters.", NameOf(request))
        End If
        If request.Count.HasValue AndAlso (request.Count.Value < 1 OrElse request.Count.Value > 50) Then
            Throw New ArgumentOutOfRangeException(NameOf(request), "Count must be between 1 and 50.")
        End If
    End Sub

End Class
