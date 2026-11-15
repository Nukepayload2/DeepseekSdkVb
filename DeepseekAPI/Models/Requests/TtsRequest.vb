Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Namespace Models
    ''' <summary>
    ''' 语音合成请求体（整段合成），对应 <c>POST /v1/tools/tts</c>。
    ''' </summary>
    ''' <remarks>
    ''' 完整说明见 <see href="https://api-docs.deepseek.com/zh-cn/api/tts">TTS API</see>。
    ''' </remarks>
    Public Class TtsRequest
        ''' <summary>
        ''' 要朗读的文本，不能为空，最多 10,000 个计费字符。
        ''' </summary>
        Public Property Text As String

        ''' <summary>
        ''' 音色，取值见 <see cref="TtsVoices"/>。默认 <c>stella</c>。
        ''' </summary>
        Public Property VoiceId As String

        ''' <summary>
        ''' 音频格式，取值见 <see cref="TtsFormats"/>。默认 <c>wav</c>。
        ''' </summary>
        Public Property Format As String

        ''' <summary>
        ''' 朗读前是否去除 Markdown 格式。默认 <see langword="True"/>。
        ''' </summary>
        Public Property FilterMarkdown As Boolean?

        Public Function ToJsonUtf8() As MemoryStream
            Dim ms As New MemoryStream
            Using sw As New StreamWriter(ms, IoUtils.UTF8NoBOM, 8192, True), jsonWriter As New JsonTextWriter(sw)
                TtsRequestWriter.WriteTtsRequest(jsonWriter, Me)
            End Using
            ms.Position = 0
            Return ms
        End Function

        Public Function ToJson() As String
            Using stringWriter = New StringWriter, jsonWriter = New JsonTextWriter(stringWriter)
                TtsRequestWriter.WriteTtsRequest(jsonWriter, Me)
                Return stringWriter.ToString()
            End Using
        End Function

    End Class ' TtsRequest

    ''' <summary>
    ''' 语音合成音色的常量。
    ''' </summary>
    Public Class TtsVoices
        ''' <summary>
        ''' 海星，女声，默认。
        ''' </summary>
        Public Shared ReadOnly Property Stella As String = "stella"

        ''' <summary>
        ''' 暗潮，男声。
        ''' </summary>
        Public Shared ReadOnly Property Tide As String = "tide"
    End Class

    ''' <summary>
    ''' 语音合成音频格式的常量。
    ''' </summary>
    Public Class TtsFormats
        ''' <summary>
        ''' WAV，响应 Content-Type 为 <c>audio/wav</c>。
        ''' </summary>
        Public Shared ReadOnly Property Wav As String = "wav"

        ''' <summary>
        ''' 24 kHz、16 位有符号、单声道的原始 PCM，响应 Content-Type 为 <c>audio/pcm</c>。
        ''' </summary>
        Public Shared ReadOnly Property Pcm As String = "pcm"
    End Class
End Namespace
