Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' 语音识别响应，对应 <c>POST /v1/tools/asr</c> 的响应体。
    ''' </summary>
    Public Class AsrResponse
        ''' <summary>
        ''' 请求 ID。
        ''' </summary>
        Public Property RequestId As String

        ''' <summary>
        ''' 识别出的文本。
        ''' </summary>
        Public Property Text As String

        ''' <summary>
        ''' 音频时长，单位为毫秒。
        ''' </summary>
        Public Property DurationMs As Long?

        Public Shared Function FromJson(json As Stream) As AsrResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return AsrResponseReader.ReadAsrResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As AsrResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return AsrResponseReader.ReadAsrResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' AsrResponse
End Namespace
