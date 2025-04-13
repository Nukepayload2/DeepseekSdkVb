Imports System.Net.Http
Imports System.Text
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

Public Class ModelClient
    Inherits ClientFeatureBase

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/models"

    Public Async Function ListModelsAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of ListModelResponse)
        Dim json = Await GetAsync(RequestUrl, cancellationToken)
        Dim tmp = Encoding.UTF8.GetString(json.ToArray)
        Return ListModelResponse.FromJson(json)
    End Function

End Class
