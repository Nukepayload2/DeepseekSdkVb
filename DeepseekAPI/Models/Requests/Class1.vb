Imports System.Text.Json.Serialization
Imports Newtonsoft.Json.Linq

Namespace Models
    ''' <summary>
    ''' 根据输入的上下文，来让模型补全对话内容。
    ''' </summary>
    Public Class ChatRequest
        ''' <summary>
        ''' 对话的消息列表。
        ''' </summary>
        Public Property Messages As IReadOnlyList(Of ChatMessage)

        ''' <summary>
        ''' 使用的模型的 ID，详见 <see cref="ModelNames"/>。必填，否则服务端会报错。
        ''' </summary>
        Public Property Model As String

        ''' <summary>
        ''' 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其在已有文本中的出现频率受到相应的惩罚，降低模型重复相同内容的可能性。
        ''' </summary>
        <JsonPropertyName("frequency_penalty")>
        Public Property FrequencyPenalty As Double?

        ''' <summary>
        ''' 限制一次请求中模型生成 completion 的最大 token 数。输入 token 和输出 token 的总长度受模型的上下文长度的限制。
        ''' default:4096
        ''' </summary>
        <JsonPropertyName("max_tokens")>
        Public Property MaxTokens As Integer?

        ''' <summary>
        ''' 介于 -2.0 和 2.0 之间的数字。如果该值为正，那么新 token 会根据其是否已在已有文本中出现受到相应的惩罚，从而增加模型谈论新主题的可能性。
        ''' </summary>
        <JsonPropertyName("presence_penalty")>
        Public Property PresencePenalty As Double?

        ''' <summary>
        ''' 一个 object，指定模型必须输出的格式。
        ''' </summary>
        <JsonPropertyName("response_format")>
        Public Property ResponseFormat As ResponseFormat

        ''' <summary>
        ''' Up to 16 sequences where the API will stop generating further tokens.
        ''' </summary>
        <JsonPropertyName("stop")>
        Public Property StopWords As IReadOnlyList(Of String)

        ''' <summary>
        ''' 如果设置为 True，将会以 SSE（server-sent events）的形式以流式发送消息增量。消息流以 data: [DONE] 结尾。
        ''' </summary>
        Public Property Stream As Boolean?

        ''' <summary>
        ''' 模型可能会调用的 tool 的列表。目前，仅支持 function 作为工具。使用此参数来提供以 JSON 作为输入参数的 function 列表。最多支持 128 个 function。
        ''' </summary>
        Public Property Tools As IReadOnlyList(Of AICallableTool)

        ''' <summary>
        ''' tool choice<br/>
        ''' String: [none, auto, required]
        ''' Object: <see cref="NamedToolChoice"/>
        ''' </summary>
        <JsonPropertyName("tool_choice")>
        Public Property ToolChoice As StringOrObject(Of NamedToolChoice)

        <JsonPropertyName("stream_options")>
        Public Property StreamOptions As StreamOptions

        ''' <summary>
        ''' 采样温度，介于 0 和 2 之间。更高的值，如 0.8，会使输出更随机，而更低的值，如 0.2，会使其更加集中和确定。 我们通常建议可以更改这个值或者更改 top_p，但不建议同时对两者进行修改。
        ''' </summary>
        Public Property Temperature As Double?

        ''' <summary>
        ''' 作为调节采样温度的替代方案，模型会考虑前 top_p 概率的 token 的结果。所以 0.1 就意味着只有包括在最高 10% 概率中的 token 会被考虑。 我们通常建议修改这个值或者更改 temperature，但不建议同时对两者进行修改。
        ''' </summary>
        <JsonPropertyName("top_p")>
        Public Property TopP As Double?

        ''' <summary>
        ''' 是否返回所输出 token 的对数概率。如果为 true，则在 message 的 content 中返回每个输出 token 的对数概率
        ''' </summary>
        <JsonPropertyName("logprobs")>
        Public Property Logprobs As Boolean?

        ''' <summary>
        ''' 一个介于 0 到 20 之间的整数 N，指定每个输出位置返回输出概率 top N 的 token，且返回这些 token 的对数概率。指定此参数时，logprobs 必须为 true。
        ''' </summary>
        <JsonPropertyName("top_logprobs")>
        Public Property TopLogprobs As Integer?
    End Class

    ''' <summary>
    ''' 代表这个位置在 JSON 里面要么是字符串要么是对象
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class StringOrObject(Of T)
        Public Property StringValue As String
        Public Property ObjectValue As T

        Public Shared Widening Operator CType(value As String) As StringOrObject(Of T)
            Return New StringOrObject(Of T) With {.StringValue = value}
        End Operator
    End Class

    ''' <summary>
    ''' 指定选择的工具。
    ''' </summary>
    Public Class NamedToolChoice
        ''' <summary>
        ''' tool 的类型。目前，仅支持 function
        ''' </summary>
        Public Property Type As String
        ''' <summary>
        ''' 指定选择的工具。
        ''' </summary>
        <JsonPropertyName("function")>
        Public Property FunctionChoice As NamedToolChoiceFunction
    End Class

    ''' <summary>
    ''' 指定选择的函数。
    ''' </summary>
    Public Class NamedToolChoiceFunction
        ''' <summary>
        ''' 要调用的函数名称。
        ''' </summary>
        Public Property Name As String
    End Class

    Public Class StreamOptions
        ''' <summary>
        ''' 如果设置为 true，在流式消息最后的 data: [DONE] 之前将会传输一个额外的块。
        ''' 此块上的 usage 字段显示整个请求的 token 使用统计信息，而 choices 字段将始终是一个空数组。
        ''' 所有其他块也将包含一个 usage 字段，但其值为 null。
        ''' </summary>
        <JsonPropertyName("include_usage")>
        Public Property IncludeUsage As Boolean?
    End Class

    Public Class AICallableTool
        ''' <summary>
        ''' 固定值 "function"
        ''' </summary>
        Public Property Type As String = "function"
        ''' <summary>
        ''' 要调用的函数
        ''' </summary>
        Public Property [Function] As FunctionMetadata
    End Class

    ''' <summary>
    ''' 函数的元数据定义
    ''' </summary>
    Public Class FunctionMetadata

        ''' <summary>
        ''' 函数名
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' 函数的描述，什么时候需要用这个函数
        ''' </summary>
        Public Property Description As String

        ''' <summary>
        ''' 参数列表。如果不写就是没有参数。
        ''' </summary>
        Public Property Parameters As FunctionParameters

    End Class

    ''' <summary>
    ''' 参数列表的 JSON schema
    ''' </summary>
    Public Class FunctionParameters
        Public Property Type As String = "object"

        Public ReadOnly Property Properties As New Dictionary(Of String, FunctionParameterDescriptor)

        Public Property Required As IReadOnlyList(Of String)

    End Class

    ''' <summary>
    ''' 参数的 JSON schema
    ''' </summary>
    Public Class FunctionParameterDescriptor
        Public Property Type As String

        Public Property Description As String

        Public Property [Default] As Object
            Get
                Return ConvertJTokenToObject(DefaultJson)
            End Get
            Set(value As Object)
                If value Is Nothing Then
                    DefaultJson = Nothing
                    Return
                End If

                DefaultJson = New JValue(value)
            End Set
        End Property

        ' 避免导出具体 JSON 库的类型
        Friend Property DefaultJson As JValue

        Public Property [Enum] As IReadOnlyList(Of Object)
            Get
                If EnumJson Is Nothing Then
                    Return Nothing
                End If
                Return Aggregate item In EnumJson Select ConvertJTokenToObject(item) Into ToArray
            End Get
            Set(value As IReadOnlyList(Of Object))
                If value Is Nothing Then
                    EnumJson = Nothing
                    Return
                End If

                EnumJson = Aggregate item In value Select New JValue(item) Into ToArray
            End Set
        End Property

        ' 避免导出具体 JSON 库的类型
        Friend Property EnumJson As IReadOnlyList(Of JValue)

    End Class

    ''' <summary>
    ''' 指定模型必须输出的格式。
    ''' </summary>
    Public Class ResponseFormat
        ''' <summary>
        ''' 返回消息的格式，详见 <see cref="ResponseFormatTypes"/>
        ''' </summary>
        ''' <remarks>
        ''' 注意: 使用 JSON 模式时，你还必须通过系统或用户消息指示模型生成 JSON。
        ''' 否则，模型可能会生成不断的空白字符，直到生成达到令牌限制，从而导致请求长时间运行并显得“卡住”。
        ''' 此外，如果 finish_reason="length"，这表示生成超过了 max_tokens 或对话超过了最大上下文长度，消息内容可能会被部分截断。
        ''' </remarks>
        Public Property Type As String
    End Class

    ''' <summary>
    ''' 模型的名字
    ''' </summary>
    Public Class ModelNames
        ''' <summary>
        ''' 聊天模型 ID "deepseek-chat"
        ''' </summary>
        Public Shared ReadOnly Property ChatModel As String = "deepseek-chat"

        ''' <summary>
        ''' 编码器模型 ID "deepseek-coder"
        ''' </summary>
        Public Shared ReadOnly Property CoderModel As String = "deepseek-coder"

        ''' <summary>
        ''' 推理模型 ID "deepseek-reasoner"
        ''' </summary>
        Public Shared ReadOnly Property ReasonerModel As String = "deepseek-reasoner"
    End Class

    ''' <summary>
    ''' 响应格式类型常量类
    ''' </summary>
    Public Class ResponseFormatTypes
        ''' <summary>
        ''' 文本响应类型 "text"
        ''' </summary>
        Public Shared ReadOnly Property Text As String = "text"

        ''' <summary>
        ''' JSON 对象响应类型 "json_object"
        ''' </summary>
        Public Shared ReadOnly Property JsonObject As String = "json_object"
    End Class

    ''' <summary>
    ''' 聊天消息
    ''' </summary>
    Public Class ChatMessage
        ''' <summary>
        ''' 消息的内容
        ''' </summary>
        Public Property Content As String

        ''' <summary>
        ''' 消息角色，详见 <see cref="ChatRoles"/>。必填，否则服务端会报错。
        ''' </summary>
        Public Property Role As String

        ''' <summary>
        ''' 可以选填的参与者的名称，为模型提供信息以区分相同角色的参与者。
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' (Beta) 设置此参数为 true，来强制模型在其回答中以此 assistant 消息中提供的前缀内容开始。
        ''' 您必须设置 base_url = "https://api.deepseek.com/beta" 来使用此功能。
        ''' </summary>
        Public Property Prefix As Boolean?

        ''' <summary>
        ''' 推理内容，也就是 &lt;think&gt; 里面的内容。
        ''' </summary>
        ''' <remarks>用于存储与推理相关的额外信息</remarks>
        <JsonPropertyName("reasoning_content")>
        Public Property ReasoningContent As String

        ''' <summary>
        ''' 此消息所响应的 tool call 的 ID。
        ''' </summary>
        <JsonPropertyName("tool_call_id")>
        Public Property ToolCallId As String

    End Class

    ''' <summary>
    ''' 聊天的角色
    ''' </summary>
    Public Class ChatRoles
        Public Shared ReadOnly Property System As String = "system"
        Public Shared ReadOnly Property User As String = "user"
        Public Shared ReadOnly Property Assistant As String = "assistant"
        Public Shared ReadOnly Property Tool As String = "tool"
    End Class
End Namespace
