Imports System.Net.Http

''' <summary>
''' The client of DeepSeek API.
''' </summary>
Public Class DeepSeekClient
    Friend Shared ReadOnly Property DefaultHttpClient As New HttpClient
    ''' <summary>
    ''' Shared client for long-running requests (OCR, ASR, TTS).
    ''' </summary>
    ''' <remarks>
    ''' The default <see cref="HttpClient.Timeout"/> of 100 seconds is too short for large uploads
    ''' and long audio responses, so this client disables the built-in timeout and relies on
    ''' per-request cancellation tokens instead.
    ''' </remarks>
    Friend Shared ReadOnly Property DefaultLongRunningHttpClient As New HttpClient With {
        .Timeout = Threading.Timeout.InfiniteTimeSpan
    }
    ''' <summary>
    ''' Provides <see href="https://api-docs.deepseek.com/api/create-chat-completion">chat completion</see> APIs.
    ''' </summary>
    Public ReadOnly Property Chat As ChatClient
    ''' <summary>
    ''' Provides <see href="https://api-docs.deepseek.com/api/list-models">model list</see> APIs.
    ''' </summary>
    Public ReadOnly Property Model As ModelClient
    ''' <summary>
    ''' Provides <see href="https://api-docs.deepseek.com/api/get-user-balance">user balance</see> APIs.
    ''' </summary>
    Public ReadOnly Property User As UserClient
    ''' <summary>
    ''' Provides beta APIs, such as <see href="https://api-docs.deepseek.com/api/create-completion">FIM completion</see>.
    ''' </summary>
    Public ReadOnly Property Beta As BetaClient
    ''' <summary>
    ''' Provides tool APIs: web search, fetch, OCR, ASR and text-to-speech.
    ''' </summary>
    Public ReadOnly Property Tools As ToolsClient
    ''' <summary>
    ''' Provides the Files API.
    ''' </summary>
    Public ReadOnly Property Files As FilesClient
    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Chat = New ChatClient(apiKey, client)
        Model = New ModelClient(apiKey, client)
        User = New UserClient(apiKey, client)
        Beta = New BetaClient(apiKey, client)
        Tools = New ToolsClient(apiKey, client)
        Files = New FilesClient(apiKey, client)
    End Sub
End Class
