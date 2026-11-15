Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Namespace Models
    ''' <summary>
    ''' 网页抓取请求体，对应 <c>POST /v1/tools/fetch</c>。
    ''' </summary>
    ''' <remarks>
    ''' 完整说明见 <see href="https://api-docs.deepseek.com/zh-cn/api/fetch">Fetch API</see>。
    ''' </remarks>
    Public Class FetchRequest
        ''' <summary>
        ''' 要抓取的网页地址，上限 8192 个字符，必须为含 host 的 HTTP/HTTPS URL。
        ''' </summary>
        Public Property Url As String

        Public Function ToJsonUtf8() As MemoryStream
            Dim ms As New MemoryStream
            Using sw As New StreamWriter(ms, IoUtils.UTF8NoBOM, 8192, True), jsonWriter As New JsonTextWriter(sw)
                FetchRequestWriter.WriteFetchRequest(jsonWriter, Me)
            End Using
            ms.Position = 0
            Return ms
        End Function

        Public Function ToJson() As String
            Using stringWriter = New StringWriter, jsonWriter = New JsonTextWriter(stringWriter)
                FetchRequestWriter.WriteFetchRequest(jsonWriter, Me)
                Return stringWriter.ToString()
            End Using
        End Function

    End Class ' FetchRequest
End Namespace
