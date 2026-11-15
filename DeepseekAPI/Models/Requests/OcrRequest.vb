Imports System.IO

Namespace Models
    ''' <summary>
    ''' OCR 请求参数，对应 <c>POST /v1/tools/ocr</c> 的 multipart 表单。
    ''' </summary>
    ''' <remarks>
    ''' 完整说明见 <see href="https://api-docs.deepseek.com/zh-cn/api/ocr">OCR API</see>。
    ''' </remarks>
    Public Class OcrRequest
        ''' <summary>
        ''' 识别方式，取值见 <see cref="OcrPurposes"/>。
        ''' </summary>
        Public Property Purpose As String

        ''' <summary>
        ''' 要识别的文件内容。调用方保留流的所有权。
        ''' </summary>
        Public Property File As Stream

        ''' <summary>
        ''' 文件名，必须带扩展名，供服务端判断格式。
        ''' </summary>
        Public Property FileName As String

        Sub New()
        End Sub

        Sub New(file As Stream, fileName As String, purpose As String)
            Me.File = file
            Me.FileName = fileName
            Me.Purpose = purpose
        End Sub
    End Class ' OcrRequest

    ''' <summary>
    ''' OCR 识别方式的常量。
    ''' </summary>
    Public Class OcrPurposes
        ''' <summary>
        ''' 只识别文字：不含图片的 Word、PowerPoint 直接提取，其他文件逐页识别。
        ''' </summary>
        Public Shared ReadOnly Property TextRecognition As String = "text_recognition"

        ''' <summary>
        ''' 图文混排：所有文件都逐页识别，保留文档中的图片。
        ''' </summary>
        Public Shared ReadOnly Property RichLayoutRecognition As String = "rich_layout_recognition"
    End Class
End Namespace
