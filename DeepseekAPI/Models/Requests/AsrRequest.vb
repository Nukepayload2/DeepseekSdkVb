Imports System.IO

Namespace Models
    ''' <summary>
    ''' 语音识别请求参数，对应 <c>POST /v1/tools/asr</c> 的 multipart 表单。
    ''' </summary>
    ''' <remarks>
    ''' 语种由服务端根据音频自动识别。完整说明见
    ''' <see href="https://api-docs.deepseek.com/zh-cn/api/asr">ASR API</see>。
    ''' </remarks>
    Public Class AsrRequest
        ''' <summary>
        ''' 音频文件内容，支持 16 位 PCM WAV 或 Ogg Opus。调用方保留流的所有权。
        ''' </summary>
        Public Property File As Stream

        ''' <summary>
        ''' 文件名，必须带扩展名。
        ''' </summary>
        Public Property FileName As String

        Sub New()
        End Sub

        Sub New(file As Stream, fileName As String)
            Me.File = file
            Me.FileName = fileName
        End Sub
    End Class ' AsrRequest
End Namespace
