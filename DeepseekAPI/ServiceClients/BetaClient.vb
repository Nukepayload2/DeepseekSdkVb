Imports System.Net.Http

''' <summary>
''' Provides beta API client. The beta API is still in development and may change in stable releases.
''' </summary>
Public Class BetaClient

    ''' <summary>
    ''' Fill-In-the-Middle completion
    ''' </summary>
    Public ReadOnly Property Fim As FimBetaClient

    Sub New(apiKey As String, client As HttpClient)
        Fim = New FimBetaClient(apiKey, client)
    End Sub

End Class
