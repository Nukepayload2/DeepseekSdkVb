Imports Newtonsoft.Json
Imports Nukepayload2.AI.Providers.Deepseek.Serialization
Imports Nukepayload2.IO.Json.Serialization.NewtonsoftJson
Imports System.IO

Namespace Models
    ''' <summary>
    ''' Represents user balance response.
    ''' </summary>
    Public Class UserBalanceResponse
        ''' <summary>
        ''' 当前账户是否有余额可供 API 调用
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>is_available</c> in json.
        ''' </remarks>
        Public Property IsAvailable As Boolean?
        ''' <summary>
        ''' Reads or writes <c>balance_infos</c> in json.
        ''' </summary>
        Public Property BalanceInfos As IReadOnlyList(Of BalanceInfo)

        Public Shared Function FromJson(json As Stream) As UserBalanceResponse
            Using jsonReader As New JsonTextReader(New StreamReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return UserBalanceReader.ReadUserBalanceResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function

        Public Shared Function FromJson(json As String) As UserBalanceResponse
            Using jsonReader As New JsonTextReader(New StringReader(json))
                jsonReader.DateParseHandling = DateParseHandling.None
                Return UserBalanceReader.ReadUserBalanceResponse(jsonReader, JsonReadErrorHandler.DefaultHandler)
            End Using
        End Function
    End Class ' UserBalanceResponse

    ''' <summary>
    ''' Represents balance info.
    ''' </summary>
    Public Class BalanceInfo
        ''' <summary>
        ''' 货币，人民币或美元
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>currency</c> in json.
        ''' </remarks>
        ''' <value>
        ''' Can be value of <c>"CNY"</c>, <c>"USD"</c>
        ''' </value>
        Public Property Currency As String
        ''' <summary>
        ''' 总的可用余额，包括赠金和充值余额
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>total_balance</c> in json.
        ''' </remarks>
        Public Property TotalBalance As String
        ''' <summary>
        ''' 未过期的赠金余额
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>granted_balance</c> in json.
        ''' </remarks>
        Public Property GrantedBalance As String
        ''' <summary>
        ''' 充值余额
        ''' </summary>
        ''' <remarks>
        ''' Reads or writes <c>topped_up_balance</c> in json.
        ''' </remarks>
        Public Property ToppedUpBalance As String
    End Class
End Namespace
