Imports System.Net.Http

Public Class DeepSeekClient
    Friend Shared ReadOnly Property DefaultHttpClient As New HttpClient
    Public ReadOnly Property Chat As ChatClient
    Public ReadOnly Property Model As ModelClient
    Public ReadOnly Property User As UserClient
    Public ReadOnly Property Beta As BetaClient
    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Chat = New ChatClient(apiKey, client)
        Model = New ModelClient(apiKey, client)
        User = New UserClient(apiKey, client)
        Beta = New BetaClient(apiKey, client)
    End Sub
End Class
