Imports System.Net.Http

Public Class BetaClient

    ''' <summary>
    ''' Fill-In-the-Middle
    ''' </summary>
    Public ReadOnly Property Fim As FimClient

    Sub New(apiKey As String, client As HttpClient)
        Fim = New FimClient(apiKey, client)
    End Sub

End Class
