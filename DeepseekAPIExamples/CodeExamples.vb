Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Nukepayload2.AI.Providers.Deepseek
Imports Nukepayload2.AI.Providers.Deepseek.Models

<TestClass>
Public Class CodeExamples
    Private Shared ReadOnly Property ApiKey As String
        Get
            Dim key = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            If key = Nothing Then
                Throw New InvalidOperationException("Please set API key to %DEEPSEEK_API_KEY% and try again.")
            End If
            Return key
        End Get
    End Property

    <TestMethod>
    Async Function TestCompletionAsync() As Task
        Dim clientV4 As New DeepSeekClient(ApiKey)
        Dim request As New ChatRequest With {
            .Model = ModelNames.ChatModel,
            .Messages = {New ChatMessage With {.Role = ChatRoles.User, .Content = "你好，你是谁？"}},
            .TopP = 0.9
        }
        Dim response = Await clientV4.Chat.CompleteAsync(request)

        Dim respMessage = response.Choices?.FirstOrDefault?.Message?.Content
        Console.WriteLine(respMessage)
        Assert.IsNotNull(respMessage)
    End Function

End Class
