Imports System.Net.Http

''' <summary>
''' The client of DeepSeek API.
''' </summary>
Public Class DeepSeekClient
    Friend Shared ReadOnly Property DefaultHttpClient As New HttpClient
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
    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Chat = New ChatClient(apiKey, client)
        Model = New ModelClient(apiKey, client)
        User = New UserClient(apiKey, client)
        Beta = New BetaClient(apiKey, client)
    End Sub
End Class
