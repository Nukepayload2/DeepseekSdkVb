Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.AI.Providers.Deepseek.Utils
Imports System.IO

Namespace Models
    ''' <summary>
    ''' API 请求体参数
    ''' </summary>
    ''' <remarks>
    ''' Represents <c>FimRequest</c> in JSON.
    ''' </remarks>
    Public Class FimRequest
        ''' <summary>
        ''' 模型的 ID
        ''' </summary>
        ''' <value>
        ''' Can be value of <c>"deepseek-chat"</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>model</c> in json.
        ''' </remarks>
        Public Property Model As String
        ''' <summary>
        ''' 用于生成完成内容的提示
        ''' </summary>
        ''' <value>
        ''' The default value is <c>&quot;Once upon a time, &quot;</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>prompt</c> in json.
        ''' </remarks>
        Public Property Prompt As String
        ''' <summary>
        ''' 在输出中，把 prompt 的内容也输出出来
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>echo</c> in json.
        ''' </remarks>
        Public Property Echo As Boolean?
        ''' <summary>
        ''' 介于 -2.0 和 2.0 之间的数字。如果该值为正，新 token 会根据其在已有文本中的出现频率受到惩罚，降低重复内容的可能性。
        ''' </summary>
        ''' <value>
        ''' The default value is <c>0</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>frequency_penalty</c> in json.
        ''' </remarks>
        Public Property FrequencyPenalty As Double?
        ''' <summary>
        ''' 返回最多 logprobs 个最可能 token 的对数概率。例如，logprobs=20 会返回20个最可能 token，API 还会包含采样 token 的对数概率，最多返回 logprobs+1 个元素。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>logprobs</c> in json.
        ''' </remarks>
        Public Property Logprobs As Long?
        ''' <summary>
        ''' 生成内容的最大 token 数量。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>max_tokens</c> in json.
        ''' </remarks>
        Public Property MaxTokens As Long?
        ''' <summary>
        ''' 介于 -2.0 和 2.0 之间的数字。若为正，新 token 会因已在文本中出现而受到惩罚，增加模型讨论新主题的可能性。
        ''' </summary>
        ''' <value>
        ''' The default value is <c>0</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>presence_penalty</c> in json.
        ''' </remarks>
        Public Property PresencePenalty As Double?
        ''' <summary>
        ''' 停止生成的词或词列表（最多 16 个字符串）。当遇到这些词时，API 停止生成更多 token。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>stop</c> in json.
        ''' </remarks>
        Public Property StopWords As FimStopWords
        ''' <summary>
        ''' 若设为 true，以流式 SSE 返回响应，消息以 `data: [DONE]` 结尾。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>stream</c> in json.
        ''' </remarks>
        Public Property Stream As Boolean?
        ''' <summary>
        ''' 流式输出选项。仅当 `stream` 为 true 时可配置。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>stream_options</c> in json.
        ''' </remarks>
        Public Property StreamOptions As StreamOptions
        ''' <summary>
        ''' 生成内容的后缀字符串。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>suffix</c> in json.
        ''' </remarks>
        Public Property Suffix As String
        ''' <summary>
        ''' 采样温度（0 到 2）。更高值（如 0.8）使输出更随机，更低值（如 0.2）更集中。通常单独调整此值或 top_p。
        ''' </summary>
        ''' <value>
        ''' The default value is <c>1</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>temperature</c> in json.
        ''' </remarks>
        Public Property Temperature As Double?
        ''' <summary>
        ''' 取前 top_p 概率的 token 进行采样。例如 0.1 表示仅选择最高 10% 概率的 token。建议单独调整此值或 temperature。
        ''' </summary>
        ''' <value>
        ''' The default value is <c>1</c>
        ''' </value>
        ''' <remarks>
        ''' Reads or writes <c>top_p</c> in json.
        ''' </remarks>
        Public Property TopP As Double?

        Public Function ToJsonUtf8() As MemoryStream
            Dim ms As New MemoryStream
            Using sw As New StreamWriter(ms, IoUtils.UTF8NoBOM, 8192, True), jsonWriter As New JsonTextWriter(sw)
                FimRequestWriter.WriteFimRequest(jsonWriter, Me)
            End Using
            ms.Position = 0
            Return ms
        End Function

        Public Function ToJson() As String
            Using stringWriter = New StringWriter, jsonWriter = New JsonTextWriter(stringWriter)
                FimRequestWriter.WriteFimRequest(jsonWriter, Me)
                Return stringWriter.ToString()
            End Using
        End Function

    End Class ' FimRequest
    ''' <summary>
    ''' Stop words can be string or array of string. You can only use one of them.
    ''' </summary>
    Public Class FimStopWords
        ''' <summary>
        ''' Use this property only if JToken is <c>String</c>.
        ''' </summary>
        Public Property StringValue As String
        ''' <summary>
        ''' Use this property only if JToken is <c>Array</c>.
        ''' </summary>
        Public Property ArrayValue As IReadOnlyList(Of String)
    End Class

End Namespace
