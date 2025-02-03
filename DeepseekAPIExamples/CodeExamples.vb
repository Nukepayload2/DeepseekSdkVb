Imports System.Text
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
            .Messages = {New ChatMessage(ChatRoles.User, "你好，你是谁？")},
            .TopP = 0.9
        }
        Dim response = Await clientV4.Chat.CompleteAsync(request)

        Dim respMessage = response.Choices?.FirstOrDefault?.Message?.Content
        Console.WriteLine(respMessage)
        Assert.IsNotNull(respMessage)
    End Function

    <TestMethod>
    Async Function TestStreamAsync() As Task
        Dim clientV4 As New DeepSeekClient(ApiKey)
        Dim sb As New StringBuilder
        Await clientV4.Chat.StreamAsync(
            New ChatRequest With {
                .Model = ModelNames.ChatModel,
                .Messages = {New ChatMessage(ChatRoles.User, "1+1等于多少"),
                              New ChatMessage(ChatRoles.Assistant, "1+1等于2。"),
                              New ChatMessage(ChatRoles.User, "再加2呢？")},
                .Temperature = 0.3,
                .Stream = True
            },
            Sub(resp)
                Dim respMessage = resp.Choices?.FirstOrDefault?.Delta?.Content
                If respMessage <> Nothing Then
                    sb.AppendLine($"{Environment.TickCount}: {respMessage}")
                End If
            End Sub)

        Console.WriteLine(sb.ToString)
        Assert.AreNotEqual(0, sb.Length)
    End Function

End Class
