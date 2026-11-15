Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' OCR 响应，对应 <c>POST /v1/tools/ocr</c> 的响应体。
    ''' </summary>
    ''' <remarks>
    ''' 识别开始后 HTTP 状态码可能已经是 200，失败时响应体是一个 error 对象。
    ''' 调用方应先检查 <c>Error</c> 属性。
    ''' </remarks>
    Public Class OcrResponse
        ''' <summary>
        ''' 本次识别的 ID。
        ''' </summary>
        Public Property Id As String

        ''' <summary>
        ''' 对象类型，固定为 <c>ocr.result</c>。
        ''' </summary>
        Public Property TypeName As String

        ''' <summary>
        ''' 识别创建时的 Unix 时间戳（秒）。
        ''' </summary>
        Public Property Created As Long?

        ''' <summary>
        ''' 请求中的识别方式。
        ''' </summary>
        Public Property Purpose As String

        ''' <summary>
        ''' 页数，等于 <see cref="Pages"/> 的长度。
        ''' </summary>
        Public Property PageCount As Integer?

        ''' <summary>
        ''' 每一页的识别结果，按页序排列。
        ''' </summary>
        Public Property Pages As IReadOnlyList(Of OcrPage)

        ''' <summary>
        ''' 所有页的 Token 用量，仅供参考，不用于计费。
        ''' </summary>
        Public Property Usage As OcrUsage

        ''' <summary>
        ''' 识别开始后失败时的错误对象；成功时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property [Error] As RequestError

        Public Shared Function FromJson(json As Stream) As OcrResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return OcrResponseReader.ReadOcrResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As OcrResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return OcrResponseReader.ReadOcrResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' OcrResponse

    ''' <summary>
    ''' OCR 的单页识别结果。
    ''' </summary>
    Public Class OcrPage
        ''' <summary>
        ''' 页码，从 0 开始。
        ''' </summary>
        Public Property Index As Integer?

        ''' <summary>
        ''' 该页的模型原始输出，不做任何清理。
        ''' </summary>
        ''' <remarks>
        ''' 逐页识别时可能包含 <c>&lt;｜ref｜&gt;</c>、<c>&lt;｜box｜&gt;</c> 等特殊标记及坐标。
        ''' </remarks>
        Public Property Text As String

        ''' <summary>
        ''' <c>stop</c> 表示该页完整识别；<c>length</c> 表示输出达到上限，该页文本不完整。
        ''' </summary>
        Public Property FinishReason As String
    End Class ' OcrPage

    ''' <summary>
    ''' OCR 的 Token 用量。
    ''' </summary>
    Public Class OcrUsage
        ''' <summary>
        ''' 所有页的 OCR 输入 Token。
        ''' </summary>
        Public Property InputTokens As Integer?

        ''' <summary>
        ''' 所有页的 OCR 输出 Token。
        ''' </summary>
        Public Property OutputTokens As Integer?

        ''' <summary>
        ''' 输入与输出 Token 之和。
        ''' </summary>
        Public Property TotalTokens As Integer?
    End Class ' OcrUsage
End Namespace
