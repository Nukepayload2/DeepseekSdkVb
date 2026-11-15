Namespace Models
    ''' <summary>
    ''' 消息 <c>content</c> 字段的容器：纯字符串或内容块数组。
    ''' </summary>
    ''' <remarks>
    ''' 该类型只由 <see cref="ChatMessage.ContentValue"/> 使用，用于在 JSON 中表达
    ''' <c>content: "..."</c> 与 <c>content: [...]</c> 两种形态。
    ''' </remarks>
    Public Class ChatContent
        ''' <summary>
        ''' 纯文本内容。与 <see cref="Parts"/> 互斥。
        ''' </summary>
        Public Property Text As String

        ''' <summary>
        ''' 多模态内容块。与 <see cref="Text"/> 互斥。
        ''' </summary>
        Public Property Parts As IReadOnlyList(Of ChatContentPart)
    End Class

    ''' <summary>
    ''' Chat Completions 消息中的一个内容块。
    ''' </summary>
    ''' <remarks>
    ''' 使用 <see cref="Type"/> 区分块类型；未使用的字段保持 <see langword="Nothing"/>，不会被写入 JSON。
    ''' 可用工厂方法创建常见块。
    ''' </remarks>
    Public Class ChatContentPart
        ''' <summary>
        ''' 块类型，取值见 <see cref="ChatContentTypes"/>。
        ''' </summary>
        Public Property Type As String

        ''' <summary>
        ''' 文本块的内容。
        ''' </summary>
        Public Property Text As String

        ''' <summary>
        ''' 图片块的内容。
        ''' </summary>
        Public Property ImageUrl As ChatImageUrl

        ''' <summary>
        ''' 文件块引用的 Files API file_id。与 <see cref="FileData"/> 互斥。
        ''' </summary>
        Public Property FileId As String

        ''' <summary>
        ''' 内联文件块的 data URL。与 <see cref="FileId"/> 互斥。
        ''' </summary>
        Public Property FileData As String

        ''' <summary>
        ''' 内联文件块的文件名。
        ''' </summary>
        Public Property FileName As String

        ''' <summary>
        ''' 创建一个文本块。
        ''' </summary>
        Public Shared Function FromText(text As String) As ChatContentPart
            Return New ChatContentPart With {
                .Type = ChatContentTypes.Text,
                .Text = text
            }
        End Function

        ''' <summary>
        ''' 创建一个图片 URL 块。
        ''' </summary>
        ''' <param name="url">HTTP(S) URL 或 data:image/...;base64,... data URL。</param>
        ''' <param name="detail">细节级别，取值见 <see cref="ChatImageDetailLevels"/>。</param>
        Public Shared Function FromImageUrl(url As String, Optional detail As String = Nothing) As ChatContentPart
            Return New ChatContentPart With {
                .Type = ChatContentTypes.ImageUrl,
                .ImageUrl = New ChatImageUrl With {.Url = url, .Detail = detail}
            }
        End Function

        ''' <summary>
        ''' 把图片字节编码为 data URL，并创建图片块。
        ''' </summary>
        Public Shared Function FromImageBytes(mediaType As String, bytes As Byte(),
                                              Optional detail As String = Nothing) As ChatContentPart
            Return FromImageUrl("data:" & mediaType & ";base64," & Convert.ToBase64String(bytes), detail)
        End Function

        ''' <summary>
        ''' 创建一个引用 Files API 文件的块。
        ''' </summary>
        Public Shared Function FromFileId(fileId As String) As ChatContentPart
            Return New ChatContentPart With {
                .Type = ChatContentTypes.File,
                .FileId = fileId
            }
        End Function

        ''' <summary>
        ''' 创建一个内联文件块。
        ''' </summary>
        Public Shared Function FromFileBytes(mediaType As String, bytes As Byte(), fileName As String) As ChatContentPart
            Return New ChatContentPart With {
                .Type = ChatContentTypes.File,
                .FileData = "data:" & mediaType & ";base64," & Convert.ToBase64String(bytes),
                .FileName = fileName
            }
        End Function
    End Class

    ''' <summary>
    ''' 图片块的 <c>image_url</c> 对象。
    ''' </summary>
    Public Class ChatImageUrl
        ''' <summary>
        ''' HTTP(S) URL 或 data URL。
        ''' </summary>
        Public Property Url As String

        ''' <summary>
        ''' 细节级别，取值见 <see cref="ChatImageDetailLevels"/>。
        ''' </summary>
        Public Property Detail As String
    End Class

    ''' <summary>
    ''' 内容块类型的常量。
    ''' </summary>
    Public Class ChatContentTypes
        Public Shared ReadOnly Property Text As String = "text"
        Public Shared ReadOnly Property ImageUrl As String = "image_url"
        Public Shared ReadOnly Property File As String = "file"
    End Class

    ''' <summary>
    ''' 图片细节级别的常量。
    ''' </summary>
    Public Class ChatImageDetailLevels
        ''' <summary>
        ''' 缩放到 512×512，更快、更省 token。
        ''' </summary>
        Public Shared ReadOnly Property Low As String = "low"

        ''' <summary>
        ''' 保留原图（等同于 original）。
        ''' </summary>
        Public Shared ReadOnly Property High As String = "high"

        ''' <summary>
        ''' 保留原图。
        ''' </summary>
        Public Shared ReadOnly Property Original As String = "original"

        ''' <summary>
        ''' 自动选择，当前等同于 original。
        ''' </summary>
        Public Shared ReadOnly Property Auto As String = "auto"
    End Class
End Namespace
