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
        Dim client As New DeepSeekClient(ApiKey)
        Dim request As New ChatRequest With {
            .Model = ModelNames.ChatModel,
            .Messages = {New ChatMessage(ChatRoles.User, "你好，你是谁？")},
            .TopP = 0.9
        }
        Dim response = Await client.Chat.CompleteAsync(request)

        Dim respMessage = response.Choices?.FirstOrDefault?.Message?.Content
        Console.WriteLine(respMessage)
        Assert.IsNotNull(respMessage)
    End Function

    <TestMethod>
    Async Function TestStreamAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim sb As New StringBuilder
        Await client.Chat.StreamAsync(
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
    Async Function TestListModelsAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim result = Await client.Model.ListModelsAsync
        Assert.IsTrue(result.Data.Any(Function(it) it.Id = ModelNames.ChatModel))
    End Function

    <TestMethod>
    Async Function TestUserBalanceAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim result = Await client.User.GetUserBalanceAsync
        Assert.IsTrue(result.BalanceInfos.Any(Function(it) it.TotalBalance <> Nothing))
    End Function

    <TestMethod>
    Async Function TestStreamErrorHandlingAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim sb As New StringBuilder
        Await client.Chat.StreamAsync(
            New ChatRequest With {
                .Model = "wrong-model-name",
                .Messages = {New ChatMessage("user", "1+1等于多少"),
                              New ChatMessage("assistant", "1+1等于2。"),
                              New ChatMessage("user", "再加2呢？")},
                .Temperature = 0.7,
                .TopP = 0.7,
                .Stream = True
            },
            Sub(resp)
                If resp.LastError IsNot Nothing Then
                    Dim err = resp.LastError
                    sb.Append($"{err.Code}: {err.Message}")
                    Return
                End If
                Dim respMessage = resp.Choices?.FirstOrDefault?.Delta?.Content
                If respMessage <> Nothing Then
                    sb.AppendLine($"{Environment.TickCount}: {respMessage}")
                    Assert.Fail("Exception expected")
                End If
            End Sub)

        Dim errMsg = sb.ToString
        Console.WriteLine(errMsg)
        Assert.IsTrue(errMsg.Contains("Model Not Exist"))
    End Function

    <TestMethod>
    Async Function TestToolCallAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim response = Await client.Chat.CompleteAsync(
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

    <TestMethod>
    Async Function TestToolCallStreamingAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
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
                ' 文档没写这个时候工具调用信息在哪，也没写原生 HTTP 请求怎么工具调用，下面的代码是猜的。最后猜测时间是 2025-02-12 21:00。
                ' https://api-docs.deepseek.com/zh-cn/api/create-chat-completion/
                If resp.LastError IsNot Nothing Then
                    Assert.Fail(resp.LastError.Message)
                End If
                Dim toolCall = resp.Choices?.FirstOrDefault?.Delta?.ToolCalls?.FirstOrDefault
                If toolCall IsNot Nothing Then
                    If lastToolCall?.FunctionCall IsNot Nothing AndAlso toolCall?.FunctionCall IsNot Nothing Then
                        lastToolCall.FunctionCall.Arguments &= toolCall.FunctionCall.Arguments
                        Debug.WriteLine("触发增量工具调用，填补参数")
                    Else
                        lastToolCall = toolCall
                        Debug.WriteLine("触发新的工具调用")
                    End If
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

        ' 当前版本 deepseek-chat 模型 Function Calling 功能效果不稳定，会出现循环调用、空回复的情况，这里我们重试最多十次。
        Await client.Chat.StreamAsync(requestParams, onResponse)
        Dim retry = 0
        Do While lastToolCall IsNot Nothing AndAlso Interlocked.Increment(retry) <= 10
            Dim lastToolCallFunc = lastToolCall.FunctionCall
            If lastToolCallFunc IsNot Nothing Then
                ' 在这里返回了示例数据，实际应用中应当进行异步查询请求，并返回真实数据。
                If lastToolCallFunc.Name = "get_weather" AndAlso lastToolCallFunc.Arguments?.Contains("北京") Then
                    Dim callResult = "晴天，30 摄氏度。"
                    ' 这部分官方没写文档，照着 OpenAI 的文档来了。
                    ' https://community.openai.com/t/formatting-assistant-messages-after-tool-function-calls-in-gpt-conversations/535360/3
                    messages.Add(New ChatMessage(ChatRoles.Assistant, Nothing) With {.ToolCalls = {lastToolCall}})
                    messages.Add(New ChatMessage(ChatRoles.Tool, callResult) With {.ToolCallId = lastToolCall.Id, .Name = "get_weather"})
                    lastToolCall = Nothing
                    Await client.Chat.StreamAsync(requestParams, onResponse)
                End If
            End If
        Loop

        Dim finalResult = sb.ToString
        Console.WriteLine(finalResult)
        Assert.IsTrue(finalResult.Contains("晴"c))
        Assert.IsTrue(finalResult.Contains("30"))
    End Function

End Class
