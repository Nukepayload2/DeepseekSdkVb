Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson
Imports System.IO

Namespace Models
    ''' <summary>
    ''' API 响应的 JSON Schema 结构
    ''' </summary>
    ''' <remarks>
    ''' Represents <c>FimResponse</c> in JSON.
    ''' </remarks>
    Public Class FimResponse
        ''' <summary>
        ''' 补全响应的 ID。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>id</c> in json.
        ''' </remarks>
        Public Property Id As String
        ''' <summary>
        ''' 模型生成的补全内容选择列表。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>choices</c> in json.
        ''' </remarks>
        Public Property Choices As IReadOnlyList(Of FimResponseMessage)
        ''' <summary>
        ''' 补全请求开始时间的 Unix 时间戳（秒）。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>created</c> in json.
        ''' </remarks>
        Public Property Created As Long?
        ''' <summary>
        ''' 执行补全请求所用的模型名称。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>model</c> in json.
        ''' </remarks>
        Public Property Model As String
        ''' <summary>
        ''' 后端配置的指纹信息。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>system_fingerprint</c> in json.
        ''' </remarks>
        Public Property SystemFingerprint As String
        ''' <summary>
        ''' 对象类型，固定为 text_completion。
        ''' </summary>
        ''' <value>
        ''' Can be value of <c>"text_completion"</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>object</c> in json.
        ''' </remarks>
        Public Property TypeName As String
        ''' <summary>
        ''' API 请求的 token 使用量统计。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>usage</c> in json.
        ''' </remarks>
        Public Property Usage As Usage

        Private Shared ReadOnly s_defaultErrorHandler As New JsonReadErrorHandler

        Public Shared Function FromJson(json As Stream) As FimResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FimResponseReader.ReadFimResponse(jsonReader, s_defaultErrorHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As FimResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FimResponseReader.ReadFimResponse(jsonReader, s_defaultErrorHandler)
            End Using
        End Function
    End Class ' FimResponse
    ''' <summary>
    ''' 模型生成的补全内容的选择列表项。
    ''' </summary>
    Public Class FimResponseMessage
        ''' <summary>
        ''' 模型停止生成 token 的原因。
        ''' </summary>
        ''' <value>
        ''' Can be value of <c>"stop"</c>, <c>"length"</c>, <c>"content_filter"</c>, <c>"insufficient_system_resource"</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>finish_reason</c> in json.
        ''' </remarks>
        Public Property FinishReason As String
        ''' <summary>
        ''' 该选择项的索引。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>index</c> in json.
        ''' </remarks>
        Public Property Index As Long?
        ''' <summary>
        ''' Reads or writes <c>logprobs</c> in json.
        ''' </summary>
        Public Property Logprobs As FimResponseLogprobs
        ''' <summary>
        ''' 模型生成的补全文本。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>text</c> in json.
        ''' </remarks>
        Public Property Text As String
    End Class
    ''' <summary>
    ''' 与生成 token 相关的详细概率信息。
    ''' </summary>
    Public Class FimResponseLogprobs
        ''' <summary>
        ''' 文本偏移位置的数组。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>text_offset</c> in json.
        ''' </remarks>
        Public Property TextOffset As IReadOnlyList(Of Long?)
        ''' <summary>
        ''' 每个 token 的对数概率。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>token_logprobs</c> in json.
        ''' </remarks>
        Public Property TokenLogprobs As IReadOnlyList(Of Double?)
        ''' <summary>
        ''' 生成的 token 列表。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>tokens</c> in json.
        ''' </remarks>
        Public Property Tokens As IReadOnlyList(Of String)
        ''' <summary>
        ''' 概率最高的 token 的对数概率信息。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>top_logprobs</c> in json.
        ''' </remarks>
        Public Property TopLogprobs As IReadOnlyList(Of TopLogprobs)
    End Class

End Namespace

