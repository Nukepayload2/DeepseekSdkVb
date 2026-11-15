Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' Files API 删除响应，对应 <c>DELETE /files/{file_id}</c>。
    ''' </summary>
    Public Class FileDeleteResponse
        ''' <summary>
        ''' 被删除文件的 ID。
        ''' </summary>
        Public Property Id As String

        ''' <summary>
        ''' 对象类型，固定为 <c>file</c>。
        ''' </summary>
        Public Property TypeName As String

        ''' <summary>
        ''' 文件是否被成功删除。
        ''' </summary>
        Public Property Deleted As Boolean?

        Public Shared Function FromJson(json As Stream) As FileDeleteResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FileDeleteReader.ReadFileDeleteResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As FileDeleteResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FileDeleteReader.ReadFileDeleteResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' FileDeleteResponse
End Namespace
