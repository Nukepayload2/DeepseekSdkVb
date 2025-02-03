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

    <TestMethod>
    Async Function TestToolCallAsync() As Task
        Dim clientV4 As New DeepSeekClient(ApiKey)
        Dim response = Await clientV4.Chat.CompleteAsync(
            New ChatRequest With {
                .Model = ModelNames.ChatModel,
                .Messages = {
                    New ChatMessage(ChatRoles.System, "不要假设或猜测传入函数的参数值。如果用户的描述不明确，请要求用户提供必要信息"),
                    New ChatMessage(ChatRoles.User, "能帮我查天气吗？"),
                    New ChatMessage(ChatRoles.Assistant, "好的，请告诉我您所在的城市名称。"),
                    New ChatMessage(ChatRoles.User, "北京"),
                    New ChatMessage(ChatRoles.Assistant, "您需要查询未来几天的天气呢？"),
                    New ChatMessage(ChatRoles.User, "就今天一天的")
                },
                .Tools = {
                    New AICallableTool With {
                        .FunctionMetadata = New FunctionMetadata With {
                            .Name = "get_weather",
                            .Description = "根据提供的城市名称，提供未来的天气数据",
                            .Parameters = New FunctionParameters With {
                                .Required = {"city"},
                                .Properties = New Dictionary(Of String, FunctionParameterDescriptor) From {
                                    {"city", New FunctionParameterDescriptor With {.Type = JsonSchemaBasicTypes.String, .Description = "搜索的城市名称"}},
                                    {"days", New FunctionParameterDescriptor With {.Type = JsonSchemaBasicTypes.Integer, .Description = "要查询的未来的天数，默认为0"}}
                                }
                            }
                        }
                    }
                },
                .ToolChoice = "auto",
                .Temperature = 0.7
            }
        )
        ' 有时候不会返回工具调用信息，得手动多试几次。
        Dim respMessage = response.Choices?.FirstOrDefault?.Message?.ToolCalls?.FirstOrDefault.FunctionCall
        Assert.AreEqual("get_weather", respMessage?.Name)
        Assert.AreEqual("{""city"":""北京"",""days"":0}", respMessage?.Arguments)
    End Function

End Class
