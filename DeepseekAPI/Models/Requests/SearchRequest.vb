Imports System.IO
Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.AI.Providers.Deepseek.Utils

Namespace Models
    ''' <summary>
    ''' 网页搜索请求体，对应 <c>POST /v1/tools/search</c>。
    ''' </summary>
    ''' <remarks>
    ''' 支持 <c>site:</c>、<c>-site:</c>、引号短语等搜索语法。完整说明见
    ''' <see href="https://api-docs.deepseek.com/zh-cn/api/search">Search API</see>。
    ''' </remarks>
    Public Class SearchRequest
        ''' <summary>
        ''' 搜索词，1～128 个 Unicode 字符，不能全为空白。
        ''' </summary>
        Public Property Query As String

        ''' <summary>
        ''' 返回条数，取值 1～50。默认 10；为 <see langword="Nothing"/> 时不发送。
        ''' </summary>
        Public Property Count As Integer?

        ''' <summary>
        ''' 触发搜索的用户原始问题，1～128 个字符。有助于更好地理解搜索意图。
        ''' </summary>
        Public Property OriQuestion As String

        ''' <summary>
        ''' 用于优先返回本地化搜索结果的 IP 地址，默认为空字符串。
        ''' </summary>
        Public Property Localization As String

        ''' <summary>
        ''' 是否在 <see cref="SearchResult.Content"/> 中返回网页正文。默认 <see langword="False"/>。
        ''' </summary>
        Public Property NeedContent As Boolean?

        Public Function ToJsonUtf8() As MemoryStream
            Dim ms As New MemoryStream
            Using sw As New StreamWriter(ms, IoUtils.UTF8NoBOM, 8192, True), jsonWriter As New JsonTextWriter(sw)
                SearchRequestWriter.WriteSearchRequest(jsonWriter, Me)
            End Using
            ms.Position = 0
            Return ms
        End Function

        Public Function ToJson() As String
            Using stringWriter = New StringWriter, jsonWriter = New JsonTextWriter(stringWriter)
                SearchRequestWriter.WriteSearchRequest(jsonWriter, Me)
                Return stringWriter.ToString()
            End Using
        End Function

    End Class ' SearchRequest
End Namespace
