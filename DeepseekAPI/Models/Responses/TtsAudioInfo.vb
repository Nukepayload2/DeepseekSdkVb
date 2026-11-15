Namespace Models
    ''' <summary>
    ''' 语音合成结果元信息。音频内容写入调用方提供的流，本类型只描述格式信息。
    ''' </summary>
    Public Class TtsAudioInfo
        ''' <summary>
        ''' 响应 Content-Type，例如 <c>audio/wav</c> 或 <c>audio/pcm</c>。
        ''' </summary>
        Public ReadOnly Property ContentType As String

        ''' <summary>
        ''' 请求的音频格式。
        ''' </summary>
        Public ReadOnly Property Format As String

        ''' <summary>
        ''' 响应声明的音频字节数；未知时为 <see langword="Nothing"/>。
        ''' </summary>
        Public ReadOnly Property ContentLength As Long?

        Sub New(contentType As String, format As String, Optional contentLength As Long? = Nothing)
            Me.ContentType = contentType
            Me.Format = format
            Me.ContentLength = contentLength
        End Sub
    End Class ' TtsAudioInfo
End Namespace
