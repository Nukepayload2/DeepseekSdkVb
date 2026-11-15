Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' 网页抓取响应，对应 <c>POST /v1/tools/fetch</c> 的响应体。
    ''' </summary>
    Public Class FetchResponse
        ''' <summary>
        ''' 页面标题；缺失时为空字符串。
        ''' </summary>
        Public Property Title As String

        ''' <summary>
        ''' 抓取后的 Markdown 正文。
        ''' </summary>
        Public Property Content As String

        Public Shared Function FromJson(json As Stream) As FetchResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FetchResponseReader.ReadFetchResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As FetchResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FetchResponseReader.ReadFetchResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' FetchResponse
End Namespace
