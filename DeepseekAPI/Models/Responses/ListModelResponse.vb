Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson
Imports System.IO

Namespace Models

    ''' <summary>
    ''' Represents <c>ListModelResponse</c> in JSON.
    ''' </summary>
    Public Class ListModelResponse
        ''' <summary>
        ''' Reads or writes <c>object</c> in json.
        ''' </summary>
        ''' <value>
        ''' Can be value of <c>"list"</c>
        ''' </value>
        Public Property TypeName As String
        ''' <summary>
        ''' Reads or writes <c>data</c> in json.
        ''' </summary>
        Public Property Data As IReadOnlyList(Of ModelBasicInformation)

        Public Shared Function FromJson(json As Stream) As ListModelResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return ListModelsReader.ReadListModelResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As ListModelResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return ListModelsReader.ReadListModelResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' ListModelResponse

    ''' <summary>
    ''' The basic information of a model.
    ''' </summary>
    Public Class ModelBasicInformation
        ''' <summary>
        ''' 模型的标识符
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>id</c> in json.
        ''' </remarks>
        Public Property Id As String
        ''' <summary>
        ''' 对象的类型，其值为 model。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>object</c> in json.
        ''' </remarks>
        ''' <value>
        ''' Can be value of <c>"list"</c>
        ''' </value>
        Public Property TypeName As String
        ''' <summary>
        ''' 拥有该模型的组织。
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>owned_by</c> in json.
        ''' </remarks>
        Public Property OwnedBy As String
    End Class ' ModelBasicInformation
End Namespace