Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' Files API 的 file object。
    ''' </summary>
    Public Class FileItem
        ''' <summary>
        ''' 文件标识符，形如 <c>file-api-...</c>。
        ''' </summary>
        Public Property Id As String

        ''' <summary>
        ''' 对象类型，固定为 <c>file</c>。
        ''' </summary>
        Public Property TypeName As String

        ''' <summary>
        ''' 文件大小（字节）。
        ''' </summary>
        Public Property Bytes As Long?

        ''' <summary>
        ''' 文件创建时的 Unix 时间戳（秒）。
        ''' </summary>
        Public Property CreatedAt As Long?

        ''' <summary>
        ''' 文件名。
        ''' </summary>
        Public Property Filename As String

        ''' <summary>
        ''' 文件用途，取值见 <see cref="FilePurposes"/>。
        ''' </summary>
        Public Property Purpose As String

        ''' <summary>
        ''' 文件过期时的 Unix 时间戳（秒）；未设置有效期时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property ExpiresAt As Long?

        Public Shared Function FromJson(json As Stream) As FileItem
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FileItemReader.ReadFileItem(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As FileItem
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return FileItemReader.ReadFileItem(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' FileItem
End Namespace
