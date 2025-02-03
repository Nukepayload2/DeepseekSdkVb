Imports System.Net.Http

Public Class DeepSeekClient
    Friend Shared ReadOnly Property DefaultHttpClient As New HttpClient
    Public ReadOnly Property Chat As ChatClient

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Chat = New ChatClient(apiKey, client)
    End Sub
End Class
