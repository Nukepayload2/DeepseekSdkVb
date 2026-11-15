Imports System.IO

Namespace Models
    ''' <summary>
    ''' Files API 上传请求，对应 <c>POST /files</c> 的 multipart 表单。
    ''' </summary>
    ''' <remarks>
    ''' 表单中 <c>purpose</c> 必须位于 <c>file</c> 之前。完整说明见
    ''' <see href="https://api-docs.deepseek.com/zh-cn/api/create-file">Create File API</see>。
    ''' </remarks>
    Public Class FileUploadRequest
        ''' <summary>
        ''' 要上传的文件内容。调用方保留流的所有权。
        ''' </summary>
        Public Property File As Stream

        ''' <summary>
        ''' 文件名，必须带扩展名。
        ''' </summary>
        Public Property FileName As String

        ''' <summary>
        ''' 文件用途，取值见 <see cref="FilePurposes"/>。
        ''' </summary>
        Public Property Purpose As String

        ''' <summary>
        ''' 过期时间锚点。提供时必须为 <c>created_at</c>，且与 <see cref="ExpiresAfterSeconds"/> 一起使用。
        ''' </summary>
        Public Property ExpiresAfterAnchor As String

        ''' <summary>
        ''' 有效期（秒），3600～2592000。与 <see cref="ExpiresAfterAnchor"/> 一起使用。
        ''' </summary>
        Public Property ExpiresAfterSeconds As Long?

        Sub New()
        End Sub

        Sub New(file As Stream, fileName As String, purpose As String)
            Me.File = file
            Me.FileName = fileName
            Me.Purpose = purpose
        End Sub
    End Class ' FileUploadRequest

    ''' <summary>
    ''' 文件用途常量。
    ''' </summary>
    Public Class FilePurposes
        ''' <summary>
        ''' 图片，或纯文本/表格/电子书等不需要解析的文件。
        ''' </summary>
        Public Shared ReadOnly Property UserData As String = "user_data"

        ''' <summary>
        ''' 只解析文字，文档中的图不交给模型。
        ''' </summary>
        Public Shared ReadOnly Property TextRecognition As String = "text_recognition"

        ''' <summary>
        ''' 图文混排解析，文档中的图与文字一起交给模型。
        ''' </summary>
        Public Shared ReadOnly Property RichLayoutRecognition As String = "rich_layout_recognition"
    End Class

    ''' <summary>
    ''' Files API 列表查询参数，对应 <c>GET /files</c> 的查询字符串。
    ''' </summary>
    Public Class FilesListRequest
        ''' <summary>
        ''' 分页游标：返回排在该 file_id 之后的文件。
        ''' </summary>
        Public Property After As String

        ''' <summary>
        ''' 返回数量，1～1000。默认 1000。
        ''' </summary>
        Public Property Limit As Integer?

        ''' <summary>
        ''' 排序方式：<c>asc</c> 或 <c>desc</c>。默认 <c>asc</c>。
        ''' </summary>
        Public Property Order As String

        ''' <summary>
        ''' 按用途过滤，取值见 <see cref="FilePurposes"/>。
        ''' </summary>
        Public Property Purpose As String
    End Class ' FilesListRequest
End Namespace
