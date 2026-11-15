Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' Files API 列表响应，对应 <c>GET /files</c>。
    ''' </summary>
    Public Class FilesListResponse
        ''' <summary>
        ''' 对象类型，固定为 <c>list</c>。
        ''' </summary>
        Public Property TypeName As String

        ''' <summary>
        ''' 文件对象列表。
        ''' </summary>
        Public Property Data As IReadOnlyList(Of FileItem)

        ''' <summary>
        ''' 列表中第一个文件的 ID，可用作分页游标。
        ''' </summary>
        Public Property FirstId As String

        ''' <summary>
        ''' 列表中最后一个文件的 ID，可用作分页游标。
        ''' </summary>
        Public Property LastId As String

        ''' <summary>
        ''' 是否还有更多文件。
        ''' </summary>
        Public Property HasMore As Boolean?

        Public Shared Function FromJson(json As Stream) As FilesListResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FilesListReader.ReadFilesListResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As FilesListResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FilesListReader.ReadFilesListResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' FilesListResponse
End Namespace
