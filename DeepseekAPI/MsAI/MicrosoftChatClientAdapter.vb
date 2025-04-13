Imports System.Threading
Imports Microsoft.Extensions.AI
Imports Newtonsoft.Json.Linq
Imports Nukepayload2.AI.Providers.Deepseek.Models
Imports ChatMessage = Nukepayload2.AI.Providers.Deepseek.Models.ChatMessage
Imports ChatResponse = Nukepayload2.AI.Providers.Deepseek.Models.ChatResponse
Imports MSChatMessage = Microsoft.Extensions.AI.ChatMessage
Imports MSChatResponse = Microsoft.Extensions.AI.ChatResponse

Public Class MicrosoftChatClientAdapter
    Implements IChatClient

    Private disposedValue As Boolean

    Sub New(client As ChatClient, modelId As String)
        If client Is Nothing Then
            Throw New ArgumentNullException(NameOf(client))
        End If
        If String.IsNullOrEmpty(modelId) Then
            Throw New ArgumentException($"""{NameOf(modelId)}""不能为 null 或空。", NameOf(modelId))
        End If

        Me.Client = client
        Metadata = New ChatClientMetadata("DeepSeek",
                    New Uri("https://api.deepseek.com/"),
                    modelId)
    End Sub

    Public ReadOnly Property Client As ChatClient

    Public ReadOnly Property Metadata As ChatClientMetadata

    Public Property ToolCallMaxRetry As Integer = 10

    Public Async Function GetResponseAsync(chatMessages As IEnumerable(Of MSChatMessage),
                                        Optional options As ChatOptions = Nothing,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of MSChatResponse) Implements IChatClient.GetResponseAsync
        Dim request As New ChatRequest With {
            .Model = Metadata.ModelId,
            .Messages = (From msg In chatMessages
                         Select ConvertMessage(msg)).ToList,
            .Temperature = ToDoubleWithRounding((options?.Temperature)),
            .TopP = ToDoubleWithRounding((options?.TopP)),
            .ToolChoice = ConvertToolChoice((options?.Tools), (options?.ToolMode)),
            .Tools = ConvertTools((options?.Tools))
        }
        Return Await CompleteInternalAsync(chatMessages, options, request, 1, cancellationToken)
    End Function

    Private Async Function CompleteInternalAsync(chatMessages As IEnumerable(Of MSChatMessage),
                                                 options As ChatOptions,
                                                 request As ChatRequest, attemptCount As Integer,
                                                 cancellationToken As CancellationToken) As Task(Of MSChatResponse)
        Dim response = Await Client.CompleteAsync(request, cancellationToken)
        ThrowForNonSuccessResponse(response)

        If options?.Tools IsNot Nothing AndAlso options.Tools.Count > 0 Then
            Dim toolCalls = response.Choices?.FirstOrDefault?.Message?.ToolCalls
            If toolCalls IsNot Nothing AndAlso toolCalls.Count > 0 Then
                Dim messages = DirectCast(request.Messages, List(Of ChatMessage))
                Dim msgList = If(attemptCount > 1, DirectCast(chatMessages, List(Of MSChatMessage)), chatMessages.ToList)
                messages.Add(New ChatMessage(ChatRoles.Assistant, Nothing) With {.ToolCalls = toolCalls})
                Dim toolResponseAdded = Await TryAddToolCallMessages(msgList, messages, options, toolCalls, cancellationToken)
                If toolResponseAdded AndAlso attemptCount < ToolCallMaxRetry Then
                    Return Await CompleteInternalAsync(msgList, options, request, attemptCount + 1, cancellationToken)
                End If
            End If
        End If

        Return New MSChatResponse(
            (From choice In response.Choices
             Let msg = choice.Message
             Where msg IsNot Nothing
             Select New MSChatMessage(New ChatRole(msg.Role), msg.Content)
            ).ToArray) With {.FinishReason = ConvertFinishReason(response.Choices)}
    End Function

    Private Shared Function ConvertFinishReason(choices As IReadOnlyList(Of Choice)) As ChatFinishReason?
        Dim finishReasonText = choices.FirstOrDefault(Function(it) it.FinishReason IsNot Nothing)
        If finishReasonText IsNot Nothing Then
            Select Case finishReasonText.FinishReason
                Case "stop"
                    Return ChatFinishReason.Stop
                Case "tool_calls"
                    Return ChatFinishReason.ToolCalls
                Case "length"
                    Return ChatFinishReason.Length
                Case "sensitive"
                    Return ChatFinishReason.ContentFilter
                Case Else
                    Return Nothing
            End Select
        End If
    End Function

    Private Shared Async Function TryAddToolCallMessages(chatMessages As IList(Of MSChatMessage), messages As List(Of ChatMessage), options As ChatOptions, toolCalls As IReadOnlyList(Of ToolCall), cancellationToken As CancellationToken) As Task(Of Boolean)
        Dim toolResponseAdded = False
        Dim toolCache = options.Tools.OfType(Of AIFunction).ToDictionary(
            Function(it) it.JsonSchema.GetNullableString("title"),
            Function(it) it)
        For Each toolCall In toolCalls
            Dim func = toolCall?.FunctionCall
            If func Is Nothing Then Continue For
            Dim tool = toolCache(func.Name)
            If tool IsNot Nothing Then
                Dim args = TryParseJObjectArgs(func.Arguments)
                Dim retVal = Await tool.InvokeAsync(args, cancellationToken)
                Dim toolRetString = retVal?.ToString
                chatMessages.Add(
                    New MSChatMessage(ChatRole.Tool, toolRetString) With {
                        .AdditionalProperties = New AdditionalPropertiesDictionary From {
                           {"tool_call_id", toolCall.Id}
                        }
                    })
                messages.Add(New ChatMessage("tool", toolRetString) With {.ToolCallId = toolCall.Id})
                toolResponseAdded = True
            End If
        Next

        Return toolResponseAdded
    End Function

    Private Shared Function ConvertMessage(msg As MSChatMessage) As ChatMessage
        Return New ChatMessage(msg.Role.Value, msg.Text) With {
            .ToolCallId = TryCast(msg.AdditionalProperties?!tool_call_id, String)
        }
    End Function

    Private Shared Function TryParseJObjectArgs(arguments As String) As IEnumerable(Of KeyValuePair(Of String, Object))
        If arguments Is Nothing Then Return Nothing
        Dim argList = TryCast(JToken.Parse(arguments), JObject)
        If argList Is Nothing Then Return Nothing
        Return From prop In argList.Properties Select New KeyValuePair(Of String, Object)(prop.Name, ConvertJTokenToObject(prop.Value))
    End Function

    Private Shared Sub ThrowForNonSuccessResponse(response As ChatResponse)
        If response.LastError IsNot Nothing Then
            Throw New InvalidOperationException($"错误 {If(response.LastError.Code, "???")}: {If(response.LastError.Message, "未指定的错误")}")
        End If
    End Sub

    Private Function ToDoubleWithRounding(value As Single?) As Double?
        If value Is Nothing Then
            Return Nothing
        End If
        Return Math.Round(CDbl(value.Value), 2)
    End Function

    ' https://www.bigmodel.cn/dev/howuse/functioncall
    Private Shared Function ConvertTools(tools As IList(Of AITool)) As AICallableTool()
        If tools Is Nothing Then Return Nothing
        Return tools.OfType(Of AIFunction).Select(
            Function(tool)
                Dim functionParams As New FunctionParameters
                Dim metadata = tool.JsonSchema
                ' {
                '   "title" : "addNumbers",
                '   "description": "A simple function that adds two numbers together.",
                '   "type": "object",
                '   "properties": {
                '     "a" : { "type": "number" },
                '     "b" : { "type": "number", "default": 1 }
                '   },
                '   "required" : ["a"]
                ' }
                Dim type = metadata.GetNullableString("type")
                Dim title = metadata.GetNullableString("title")
                Dim description = metadata.GetNullableString("description")
                Dim properties = metadata.GetNullableObject("properties")
                Dim required = metadata.GetNullableArray("required")

                If properties IsNot Nothing Then
                    For Each param In properties.Value.EnumerateObject
                        Dim paraName = param.Name
                        Dim paraVal = param.Value
                        Dim paraType = paraVal.GetNullableString("type")
                        Dim paraDesc = paraVal.GetNullableString("description")
                        Dim paraDef = paraVal.GetNullableValue("default")
                        Dim paramEnum = paraVal.GetNullableArray("enum")
                        Dim paramSchema As New FunctionParameterDescriptor With {
                            .Default = paraDef, .Type = paraType, .Description = paraDesc
                        }
                        If paramEnum IsNot Nothing Then
                            paramSchema.Enum = Aggregate v In paramEnum.Value.EnumerateArray
                                               Let rawValue = v.GetNullableValue
                                               Where rawValue IsNot Nothing
                                               Select rawValue Into ToArray
                        End If
                        If functionParams.Properties Is Nothing Then functionParams.Properties = New Dictionary(Of String, FunctionParameterDescriptor)
                        functionParams.Properties(param.Name) = paramSchema
                    Next

                    If required IsNot Nothing Then
                        functionParams.Required = Aggregate req In required.Value.EnumerateArray
                                                  Where req.ValueKind = Text.Json.JsonValueKind.String
                                                  Select req.GetString Into ToArray
                    End If
                Else
                    ' 如果调用函数时不需要参数，则可以省略此参数。
                    functionParams = Nothing
                End If

                ' ReturnParameter 目前映射不了

                Return New AICallableTool With {
                    .FunctionMetadata = New FunctionMetadata With {
                        .Name = title,
                        .Description = description,
                        .Parameters = functionParams
                    }
                }
            End Function).ToArray()
    End Function

    Private Shared Function ConvertToolChoice(tools As IList(Of AITool), toolMode As ChatToolMode) As String
        If tools Is Nothing Then Return Nothing
        If toolMode Is ChatToolMode.Auto Then Return "auto"
        Return Nothing
    End Function

    Public Function GetStreamingResponseAsync(chatMessages As IEnumerable(Of MSChatMessage),
                                           Optional options As ChatOptions = Nothing,
                                           Optional cancellationToken As CancellationToken = Nothing
                                           ) As IAsyncEnumerable(Of ChatResponseUpdate) Implements IChatClient.GetStreamingResponseAsync
        Dim messages = (From msg In chatMessages
                        Select ConvertMessage(msg)).ToList
        Dim requestParams As New ChatRequest With {
            .Model = Metadata.ModelId,
            .Messages = messages,
            .Temperature = ToDoubleWithRounding(options?.Temperature),
            .TopP = ToDoubleWithRounding(options?.TopP),
            .ToolChoice = ConvertToolChoice(options?.Tools, options?.ToolMode),
            .Tools = ConvertTools(options?.Tools),
            .Stream = True
        }
        Dim builder As New AsyncEnumerableAdapter(Of ChatResponseUpdate).Builder With {
            .ReturnAsync =
            Async Function(enumerator)
                Dim funcCallByName As Dictionary(Of String, ToolCall) = Nothing
                Dim funcCalls As List(Of ToolCall) = Nothing
                Dim onResponse =
                Sub(resp As ChatResponse)
                    ThrowForNonSuccessResponse(resp)
                    Dim delta = resp.Choices?.FirstOrDefault?.Delta
                    Dim toolCalls = delta?.ToolCalls
                    If toolCalls IsNot Nothing AndAlso toolCalls.Count > 0 Then
                        If funcCallByName Is Nothing Then funcCallByName = New Dictionary(Of String, ToolCall)
                        If funcCalls Is Nothing Then funcCalls = New List(Of ToolCall)
                        For Each toolCall In toolCalls
                            If toolCall?.Index Is Nothing Then Continue For
                            Dim existingCall As ToolCall = Nothing
                            If funcCallByName.TryGetValue(toolCall.Index, existingCall) Then
                                If existingCall.FunctionCall IsNot Nothing Then
                                    existingCall.FunctionCall.Arguments &= toolCall.FunctionCall?.Arguments
                                End If
                            Else
                                funcCallByName(toolCall.Index) = toolCall
                                funcCalls.Add(toolCall)
                            End If
                        Next
                        Return
                    End If
                    Dim respMessage = delta?.Content
                    If respMessage <> Nothing Then
                        Dim role = If(delta.Role Is Nothing, ChatRole.Assistant, New ChatRole(delta.Role))
                        Dim converted As New ChatResponseUpdate(role, respMessage) With {
                            .FinishReason = ConvertFinishReason(resp.Choices)
                        }
                        enumerator.YieldValue(converted)
                    End If
                End Sub
                ' 这个模型有时候会需要多次工具调用才给你回答
                Await Client.StreamAsync(requestParams, onResponse, cancellationToken)
                Dim retry = 0
                Dim msgList = chatMessages.ToList
                Do While funcCalls IsNot Nothing AndAlso funcCalls.Count > 0 AndAlso Interlocked.Increment(retry) <= ToolCallMaxRetry
                    messages.Add(New ChatMessage(ChatRoles.Assistant, Nothing) With {.ToolCalls = funcCalls})
                    Dim toolResponseAdded = Await TryAddToolCallMessages(msgList, messages, options, funcCalls, cancellationToken)
                    If Not toolResponseAdded Then Exit Do
                    funcCallByName.Clear()
                    funcCalls = Nothing
                    Await Client.StreamAsync(requestParams, onResponse, cancellationToken)
                Loop
            End Function
        }
        Return builder.Build()
    End Function

    Public Function GetService(serviceType As Type, Optional serviceKey As Object = Nothing) As Object Implements IChatClient.GetService
        Return Nothing
    End Function

    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: 释放托管状态(托管对象)
            End If

            ' TODO: 释放未托管的资源(未托管的对象)并重写终结器
            ' TODO: 将大型字段设置为 null
            disposedValue = True
        End If
    End Sub

    ' ' TODO: 仅当“Dispose(disposing As Boolean)”拥有用于释放未托管资源的代码时才替代终结器
    ' Protected Overrides Sub Finalize()
    '     ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
    '     Dispose(disposing:=False)
    '     MyBase.Finalize()
    ' End Sub

    Public Sub Dispose() Implements IDisposable.Dispose
        ' 不要更改此代码。请将清理代码放入“Dispose(disposing As Boolean)”方法中
        Dispose(disposing:=True)
        GC.SuppressFinalize(Me)
    End Sub

End Class
