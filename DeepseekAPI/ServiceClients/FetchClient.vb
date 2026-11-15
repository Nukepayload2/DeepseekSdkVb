Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Provides the page fetching API.
''' </summary>
''' <remarks>
''' See <see href="https://api-docs.deepseek.com/zh-cn/api/fetch">Fetch API</see>.
''' </remarks>
Public Class FetchClient
    Inherits ClientFeatureBase

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/v1/tools/fetch"

    ''' <summary>
    ''' Fetches a web page and returns its title and Markdown content.
    ''' </summary>
    ''' <param name="request">The fetch request.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The fetched page.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON error.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    Public Async Function FetchAsync(request As FetchRequest,
                                     Optional cancellationToken As CancellationToken = Nothing) As Task(Of FetchResponse)
        ValidateRequest(request)
        Dim json = request.ToJsonUtf8()
        Dim response = Await PostAsync(RequestUrl, json, cancellationToken)
        Return FetchResponse.FromJson(response)
    End Function

    Private Shared Sub ValidateRequest(request As FetchRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If String.IsNullOrWhiteSpace(request.Url) Then
            Throw New ArgumentException("Url is required and cannot be blank.", NameOf(request))
        End If
        If request.Url.Length > 8192 Then
            Throw New ArgumentException("Url must be at most 8192 characters.", NameOf(request))
        End If
        Dim uri As Uri = Nothing
        If Not Uri.TryCreate(request.Url, UriKind.Absolute, uri) OrElse
           (uri.Scheme <> Uri.UriSchemeHttp AndAlso uri.Scheme <> Uri.UriSchemeHttps) Then
            Throw New ArgumentException("Url must be an absolute HTTP or HTTPS URL.", NameOf(request))
        End If
    End Sub

End Class
