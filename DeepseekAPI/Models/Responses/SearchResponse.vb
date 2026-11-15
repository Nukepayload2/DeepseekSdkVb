Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson

Namespace Models
    ''' <summary>
    ''' 网页搜索响应，对应 <c>POST /v1/tools/search</c> 的响应体。
    ''' </summary>
    Public Class SearchResponse
        ''' <summary>
        ''' 搜索结果列表；没有匹配结果时为空数组。
        ''' </summary>
        Public Property Results As IReadOnlyList(Of SearchResult)

        Public Shared Function FromJson(json As Stream) As SearchResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return SearchResponseReader.ReadSearchResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As SearchResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return SearchResponseReader.ReadSearchResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' SearchResponse

    ''' <summary>
    ''' 单条搜索结果。
    ''' </summary>
    Public Class SearchResult
        ''' <summary>
        ''' 排序分数，0～3 分；大于等于 1.5 分为优质结果。
        ''' </summary>
        Public Property RerankScore As Double?

        ''' <summary>
        ''' 搜索结果标题。
        ''' </summary>
        Public Property Title As String

        ''' <summary>
        ''' 搜索结果链接。
        ''' </summary>
        Public Property Url As String

        ''' <summary>
        ''' 短摘要，通常为 100～200 字符，适用于前端展现。
        ''' </summary>
        Public Property Snippet As String

        ''' <summary>
        ''' 长摘要，通常为 600～800 字符，适用于给模型阅读。
        ''' </summary>
        Public Property Summary As String

        ''' <summary>
        ''' 网页正文；请求未开启 <see cref="SearchRequest.NeedContent"/> 时为空字符串。
        ''' </summary>
        Public Property Content As String

        ''' <summary>
        ''' 站点名称；缺失时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property SiteName As String

        ''' <summary>
        ''' 站点图标 URL；缺失时为 <see langword="Nothing"/>。
        ''' </summary>
        Public Property SiteIcon As String

        ''' <summary>
        ''' 发布时间，UTC 的 RFC 3339 格式字符串；未知时为 <see langword="Nothing"/>。
        ''' </summary>
        ''' <remarks>
        ''' 对应 JSON 字段 <c>date</c>。保留字符串以避免时区解析差异。
        ''' </remarks>
        Public Property PublishedAt As String

        ''' <summary>
        ''' 网页权威性，0～4；-1 表示未知。
        ''' </summary>
        Public Property Authority As Integer?
    End Class ' SearchResult
End Namespace
