Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' 回应聊天消息
    ''' </summary>
    Public Class ChatResponse
        ''' <summary>
        ''' 该对话的唯一标识符。
        ''' </summary>
        Public Property Id As String

        ''' <summary>
        ''' 模型生成的 completion 的选择列表
        ''' </summary>
        Public Property Choices As IReadOnlyList(Of Choice)

        ''' <summary>
        ''' 创建聊天完成时的 Unix 时间戳（以秒为单位）。
        ''' </summary>
        Public Property Created As Long?

        ''' <summary>
        ''' 生成该 completion 的模型名
        ''' </summary>
        Public Property Model As String

        ''' <summary>
        ''' 系统指纹
        ''' </summary>
        Public Property SystemFingerprint As String

        ''' <summary>
        ''' 对象的类型, 其值为 chat.completion
        ''' </summary>
        Public Property [Object] As String

        ''' <summary>
        ''' 该对话补全请求的用量信息
        ''' </summary>
        Public Property Usage As Usage

        ''' <summary>
        ''' 出错的情况下，这里面有错误信息。
        ''' </summary>
        Public Property LastError As ChatError

        Private Shared ReadOnly s_defaultErrorHandler As New JsonReadErrorHandler

        Public Shared Function FromJson(json As Stream) As ChatResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                Return ChatResponseReader.ReadChatResponse(jsonReader, s_defaultErrorHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As ChatResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                Return ChatResponseReader.ReadChatResponse(jsonReader, s_defaultErrorHandler)
            End Using
        End Function
    End Class

    Public Class ChatError
        ''' <summary>
        ''' 错误消息
        ''' </summary>
        Public Property Message As String
        ''' <summary>
        ''' 错误类型
        ''' </summary>
        Public Property Type As String
        ''' <summary>
        ''' 参数？
        ''' </summary>
        Public Property Param As String
        ''' <summary>
        ''' 错误码
        ''' </summary>
        Public Property Code As String

    End Class

    ''' <summary>
    ''' 模型生成的选择
    ''' </summary>
    Public Class Choice
        ''' <summary>
        ''' 模型停止生成 token 的原因。<br/>
        ''' Possible values: [stop, length, content_filter, tool_calls, insufficient_system_resource]<br/>
        ''' stop：模型自然停止生成，或遇到 stop 序列中列出的字符串。<br/>
        ''' length：输出长度达到了模型上下文长度限制，或达到了 max_tokens 的限制。<br/>
        ''' content_filter：输出内容因触发过滤策略而被过滤。<br/>
        ''' insufficient_system_resource: 由于后端推理资源受限，请求被打断。
        ''' </summary>
        Public Property FinishReason As String

        ''' <summary>
        ''' 该 completion 在模型生成的 completion 的选择列表中的索引。
        ''' </summary>
        Public Property Index As Integer?

        ''' <summary>
        ''' 模型生成的 completion 消息。
        ''' </summary>
        Public Property Message As ResponseMessage

        ''' <summary>
        ''' 该 choice 的对数概率信息。
        ''' </summary>
        Public Property Logprobs As Logprobs

        ''' <summary>
        ''' 流式返回的一个 completion 增量。
        ''' </summary>
        Public Property Delta As ResponseDelta

    End Class

    Public Class ResponseMessage
        ''' <summary>
        ''' 消息的内容部分。
        ''' </summary>
        Public Property Content As String

        ''' <summary>
        ''' 推理过程中的内容，用于详细说明推理步骤。
        ''' </summary>
        Public Property ReasoningContent As String

        ''' <summary>
        ''' 工具调用的列表，包含每个工具调用的详细信息。
        ''' </summary>
        Public Property ToolCalls As IReadOnlyList(Of ToolCall)

        ''' <summary>
        ''' 消息的角色，例如 "assistant" 表示由助手生成的消息。
        ''' </summary>
        Public Property Role As String
    End Class

    ''' <summary>
    ''' 表示工具调用的详细信息，包括工具的标识符、类型和相关函数调用。
    ''' </summary>
    Public Class ToolCall
        ''' <summary>
        ''' 工具调用的唯一标识符。
        ''' </summary>
        Public Property Id As String

        Public Property Index As String

        ''' <summary>
        ''' 工具调用的类型，例如 "function" 表示函数类型的工具调用。
        ''' </summary>
        Public Property Type As String

        ''' <summary>
        ''' 与该工具调用相关的函数详细信息，包括函数名称和参数。
        ''' </summary>
        Public Property FunctionCall As FunctionCall
    End Class

    ''' <summary>
    ''' 表示函数调用的详细信息，包含函数名称及其相关参数。
    ''' </summary>
    Public Class FunctionCall
        ''' <summary>
        ''' 函数的名称，用于标识具体的函数。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 函数调用时所需的参数，以字符串形式表示。
        ''' </summary>
        Public Property Arguments As String
    End Class

    Public Class ResponseDelta
        ''' <summary>
        ''' completion 增量的内容。
        ''' </summary>
        Public Property Content As String
        ''' <summary>
        ''' 仅适用于 deepseek-reasoner 模型。内容为 assistant 消息中在最终答案之前的推理内容。
        ''' </summary>
        Public Property ReasoningContent As String
        ''' <summary>
        ''' 工具调用的列表，包含每个工具调用的详细信息。
        ''' </summary>
        Public Property ToolCalls As IReadOnlyList(Of ToolCall)
        ''' <summary>
        ''' Possible values: [assistant] 产生这条消息的角色。
        ''' </summary>
        Public Property Role As String
    End Class

    ''' <summary>
    ''' 对数概率信息
    ''' </summary>
    Public Class Logprobs

        ''' <summary>
        ''' 包含输出 token 对数概率信息的列表
        ''' </summary>
        Public Property Content As IReadOnlyList(Of LogprobsContent)
    End Class

    ''' <summary>
    ''' 一个包含输出 token 对数概率信息的列表。
    ''' </summary>
    Public Class LogprobsContent
        ''' <summary>
        ''' 输出的 token。
        ''' </summary>
        Public Property Token As String
        ''' <summary>
        ''' 该 token 的对数概率。-9999.0 代表该 token 的输出概率极小，不在 top 20 最可能输出的 token 中。
        ''' </summary>
        Public Property Logprob As Double?
        ''' <summary>
        ''' 一个包含该 token UTF-8 字节表示的整数列表。一般在一个 UTF-8 字符被拆分成多个 token 来表示时有用。如果 token 没有对应的字节表示，则该值为 null。
        ''' </summary>
        ''' <remarks>
        ''' 此处的数据类型比较宽泛，用于处理可能不合规的 JSON。实际使用时，应进行校验，然后转换成 <see cref="Byte"/> 数组使用。
        ''' </remarks>
        Public Property Bytes As IReadOnlyList(Of Integer?)

        ''' <summary>
        ''' 该 choice 的对数概率信息。
        ''' </summary>
        Public Property TopLogprobs As IReadOnlyList(Of TopLogprobs)
    End Class

    ''' <summary>
    ''' 对数概率信息
    ''' </summary>
    Public Class TopLogprobs
        ''' <summary>
        ''' 输出的 token。
        ''' </summary>
        Public Property Token As String

        ''' <summary>
        ''' 该 token 的对数概率。-9999.0 代表该 token 的输出概率极小，不在 top 20 最可能输出的 token 中。
        ''' </summary>
        Public Property Logprob As Double?

        ''' <summary>
        ''' 一个包含该 token UTF-8 字节表示的整数列表。一般在一个 UTF-8 字符被拆分成多个 token 来表示时有用。如果 token 没有对应的字节表示，则该值为 null。
        ''' </summary>
        ''' <remarks>
        ''' 此处的数据类型比较宽泛，用于处理可能不合规的 JSON。实际使用时，应进行校验，然后转换成 <see cref="Byte"/> 数组使用。
        ''' </remarks>
        Public Property Bytes As IReadOnlyList(Of Integer?)
    End Class

    ''' <summary>
    ''' 用量信息
    ''' </summary>
    Public Class Usage
        ''' <summary>
        ''' 模型 completion 产生的 token 数。
        ''' </summary>
        Public Property CompletionTokens As Integer?
        ''' <summary>
        ''' 用户 prompt 所包含的 token 数。该值等于 prompt_cache_hit_tokens + prompt_cache_miss_tokens
        ''' </summary>
        Public Property PromptTokens As Integer?
        ''' <summary>
        ''' 用户 prompt 中，命中上下文缓存的 token 数。
        ''' </summary>
        Public Property PromptCacheHitTokens As Integer?
        ''' <summary>
        ''' 用户 prompt 中，未命中上下文缓存的 token 数。
        ''' </summary>
        Public Property PromptCacheMissTokens As Integer?
        ''' <summary>
        ''' 该请求中，所有 token 的数量（prompt + completion）。
        ''' </summary>
        Public Property TotalTokens As Integer?
        ''' <summary>
        ''' completion tokens 的详细信息。
        ''' </summary>
        Public Property Details As CompletionTokensDetails

    End Class

    Public Class CompletionTokensDetails
        ''' <summary>
        ''' 推理模型所产生的思维链 token 数量
        ''' </summary>
        Public Property ReasoningTokens As Integer?
        Public Property CachedTokens As Integer?

    End Class

End Namespace