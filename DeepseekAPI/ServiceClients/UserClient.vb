Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

Public Class UserClient
    Inherits ClientFeatureBase

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overridable ReadOnly Property UserBalanceRequestUrl As String = "https://api.deepseek.com/user/balance"

    Public Async Function GetUserBalanceAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of UserBalanceResponse)
        Dim json = Await GetAsync(UserBalanceRequestUrl, cancellationToken)
        Return UserBalanceResponse.FromJson(json)
    End Function
End Class