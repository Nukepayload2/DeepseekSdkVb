Imports System.Text
Imports System.Threading
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

    <Ignore("2025-02-03 测试，会产生一个 assistant 空消息，原始 JSON 字符串没有工具调用消息。")>
    <TestMethod>
    Async Function TestToolCallStreamingAsync() As Task
        Dim clientV4 As New DeepSeekClient(ApiKey)
        Dim sb As New StringBuilder
        Dim messages As New List(Of ChatMessage) From {
            New ChatMessage(ChatRoles.System, "不要假设或猜测传入函数的参数值。如果用户的描述不明确，请要求用户提供必要信息"),
            New ChatMessage(ChatRoles.User, "能帮我查天气吗？"),
            New ChatMessage(ChatRoles.Assistant, "好的，请告诉我您所在的城市名称。"),
            New ChatMessage(ChatRoles.User, "北京"),
            New ChatMessage(ChatRoles.Assistant, "您需要查询未来几天的天气呢？"),
            New ChatMessage(ChatRoles.User, "就今天一天的")
        }
        Dim lastToolCall As ToolCall = Nothing
        Dim onResponse =
            Sub(resp As ChatResponse)
                Dim toolCall = resp.Choices?.FirstOrDefault?.Message?.ToolCalls?.FirstOrDefault
                If toolCall IsNot Nothing Then
                    lastToolCall = toolCall
                    Console.WriteLine("触发工具调用")
                    Return
                End If
                Dim respMessage = resp.Choices?.FirstOrDefault?.Delta?.Content
                If respMessage <> Nothing Then
                    sb.AppendLine($"{Environment.TickCount}: {respMessage}")
                End If
            End Sub
        Dim requestParams As New ChatRequest With {
            .Model = ModelNames.ChatModel,
            .Messages = messages,
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
            .Temperature = 0.7,
            .Stream = True
        }

        ' 这个模型有时候会需要多次工具调用才给你回答，这里我们重试最多十次。
        Await clientV4.Chat.StreamAsync(requestParams, onResponse)
        Dim retry = 0
        Do While lastToolCall IsNot Nothing AndAlso Interlocked.Increment(retry) <= 10
            Dim lastToolCallFunc = lastToolCall.FunctionCall
            If lastToolCallFunc IsNot Nothing Then
                ' 在这里返回了示例数据，实际应用中应当进行异步查询请求，并返回真实数据。
                If lastToolCallFunc.Name = "get_weather" AndAlso lastToolCallFunc.Arguments?.Contains("北京") Then
                    Dim callResult = "晴天，30 摄氏度。"
                    messages.Add(New ChatMessage(ChatRoles.Tool, callResult) With {.ToolCallId = lastToolCall.Id})
                    lastToolCall = Nothing
                    Await clientV4.Chat.StreamAsync(requestParams, onResponse)
                End If
            End If
        Loop

        Dim finalResult = sb.ToString
        Console.WriteLine(finalResult)
        Assert.IsTrue(finalResult.Contains("晴"c))
        Assert.IsTrue(finalResult.Contains("30"))
    End Function

End Class
