Imports System.Text.Json.Serialization
Imports Newtonsoft.Json.Linq

Namespace Models
    ''' <summary>
    ''' ��������������ģ�����ģ�Ͳ�ȫ�Ի����ݡ�
    ''' </summary>
    Public Class ChatRequest
        ''' <summary>
        ''' �Ի�����Ϣ�б�
        ''' </summary>
        Public Property Messages As IReadOnlyList(Of ChatMessage)

        ''' <summary>
        ''' ʹ�õ�ģ�͵� ID����� <see cref="ModelNames"/>������������˻ᱨ��
        ''' </summary>
        Public Property Model As String

        ''' <summary>
        ''' ���� -2.0 �� 2.0 ֮������֡������ֵΪ������ô�� token ��������������ı��еĳ���Ƶ���ܵ���Ӧ�ĳͷ�������ģ���ظ���ͬ���ݵĿ����ԡ�
        ''' </summary>
        <JsonPropertyName("frequency_penalty")>
        Public Property FrequencyPenalty As Double?

        ''' <summary>
        ''' ����һ��������ģ������ completion ����� token �������� token ����� token ���ܳ�����ģ�͵������ĳ��ȵ����ơ�
        ''' default:4096
        ''' </summary>
        <JsonPropertyName("max_tokens")>
        Public Property MaxTokens As Integer?

        ''' <summary>
        ''' ���� -2.0 �� 2.0 ֮������֡������ֵΪ������ô�� token ��������Ƿ����������ı��г����ܵ���Ӧ�ĳͷ����Ӷ�����ģ��̸��������Ŀ����ԡ�
        ''' </summary>
        <JsonPropertyName("presence_penalty")>
        Public Property PresencePenalty As Double?

        ''' <summary>
        ''' һ�� object��ָ��ģ�ͱ�������ĸ�ʽ��
        ''' </summary>
        <JsonPropertyName("response_format")>
        Public Property ResponseFormat As ResponseFormat

        ''' <summary>
        ''' Up to 16 sequences where the API will stop generating further tokens.
        ''' </summary>
        <JsonPropertyName("stop")>
        Public Property StopWords As IReadOnlyList(Of String)

        ''' <summary>
        ''' �������Ϊ True�������� SSE��server-sent events������ʽ����ʽ������Ϣ��������Ϣ���� data: [DONE] ��β��
        ''' </summary>
        Public Property Stream As Boolean?

        ''' <summary>
        ''' ģ�Ϳ��ܻ���õ� tool ���б�Ŀǰ����֧�� function ��Ϊ���ߡ�ʹ�ô˲������ṩ�� JSON ��Ϊ��������� function �б����֧�� 128 �� function��
        ''' </summary>
        Public Property Tools As IReadOnlyList(Of AICallableTool)

        ''' <summary>
        ''' tool choice<br/>
        ''' String: [none, auto, required]
        ''' Object: <see cref="NamedToolChoice"/>
        ''' </summary>
        <JsonPropertyName("tool_choice")>
        Public Property ToolChoice As StringOrObject(Of NamedToolChoice)

        <JsonPropertyName("stream_options")>
        Public Property StreamOptions As StreamOptions

        ''' <summary>
        ''' �����¶ȣ����� 0 �� 2 ֮�䡣���ߵ�ֵ���� 0.8����ʹ���������������͵�ֵ���� 0.2����ʹ����Ӽ��к�ȷ���� ����ͨ��������Ը������ֵ���߸��� top_p����������ͬʱ�����߽����޸ġ�
        ''' </summary>
        Public Property Temperature As Double?

        ''' <summary>
        ''' ��Ϊ���ڲ����¶ȵ����������ģ�ͻῼ��ǰ top_p ���ʵ� token �Ľ�������� 0.1 ����ζ��ֻ�а�������� 10% �����е� token �ᱻ���ǡ� ����ͨ�������޸����ֵ���߸��� temperature����������ͬʱ�����߽����޸ġ�
        ''' </summary>
        <JsonPropertyName("top_p")>
        Public Property TopP As Double?

        ''' <summary>
        ''' �Ƿ񷵻������ token �Ķ������ʡ����Ϊ true������ message �� content �з���ÿ����� token �Ķ�������
        ''' </summary>
        <JsonPropertyName("logprobs")>
        Public Property Logprobs As Boolean?

        ''' <summary>
        ''' һ������ 0 �� 20 ֮������� N��ָ��ÿ�����λ�÷���������� top N �� token���ҷ�����Щ token �Ķ������ʡ�ָ���˲���ʱ��logprobs ����Ϊ true��
        ''' </summary>
        <JsonPropertyName("top_logprobs")>
        Public Property TopLogprobs As Integer?
    End Class

    ''' <summary>
    ''' �������λ���� JSON ����Ҫô���ַ���Ҫô�Ƕ���
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class StringOrObject(Of T)
        Public Property StringValue As String
        Public Property ObjectValue As T

        Public Shared Widening Operator CType(value As String) As StringOrObject(Of T)
            Return New StringOrObject(Of T) With {.StringValue = value}
        End Operator
    End Class

    ''' <summary>
    ''' ָ��ѡ��Ĺ��ߡ�
    ''' </summary>
    Public Class NamedToolChoice
        ''' <summary>
        ''' tool �����͡�Ŀǰ����֧�� function
        ''' </summary>
        Public Property Type As String
        ''' <summary>
        ''' ָ��ѡ��Ĺ��ߡ�
        ''' </summary>
        <JsonPropertyName("function")>
        Public Property FunctionChoice As NamedToolChoiceFunction
    End Class

    ''' <summary>
    ''' ָ��ѡ��ĺ�����
    ''' </summary>
    Public Class NamedToolChoiceFunction
        ''' <summary>
        ''' Ҫ���õĺ������ơ�
        ''' </summary>
        Public Property Name As String
    End Class

    Public Class StreamOptions
        ''' <summary>
        ''' �������Ϊ true������ʽ��Ϣ���� data: [DONE] ֮ǰ���ᴫ��һ������Ŀ顣
        ''' �˿��ϵ� usage �ֶ���ʾ��������� token ʹ��ͳ����Ϣ���� choices �ֶν�ʼ����һ�������顣
        ''' ����������Ҳ������һ�� usage �ֶΣ�����ֵΪ null��
        ''' </summary>
        <JsonPropertyName("include_usage")>
        Public Property IncludeUsage As Boolean?
    End Class

    Public Class AICallableTool
        ''' <summary>
        ''' �̶�ֵ "function"
        ''' </summary>
        Public Property Type As String = "function"
        ''' <summary>
        ''' Ҫ���õĺ���
        ''' </summary>
        Public Property [Function] As FunctionMetadata
    End Class

    ''' <summary>
    ''' ������Ԫ���ݶ���
    ''' </summary>
    Public Class FunctionMetadata

        ''' <summary>
        ''' ������
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' ������������ʲôʱ����Ҫ���������
        ''' </summary>
        Public Property Description As String

        ''' <summary>
        ''' �����б������д����û�в�����
        ''' </summary>
        Public Property Parameters As FunctionParameters

    End Class

    ''' <summary>
    ''' �����б�� JSON schema
    ''' </summary>
    Public Class FunctionParameters
        Public Property Type As String = "object"

        Public ReadOnly Property Properties As New Dictionary(Of String, FunctionParameterDescriptor)

        Public Property Required As IReadOnlyList(Of String)

    End Class

    ''' <summary>
    ''' ������ JSON schema
    ''' </summary>
    Public Class FunctionParameterDescriptor
        Public Property Type As String

        Public Property Description As String

        Public Property [Default] As Object
            Get
                Return ConvertJTokenToObject(DefaultJson)
            End Get
            Set(value As Object)
                If value Is Nothing Then
                    DefaultJson = Nothing
                    Return
                End If

                DefaultJson = New JValue(value)
            End Set
        End Property

        ' ���⵼������ JSON �������
        Friend Property DefaultJson As JValue

        Public Property [Enum] As IReadOnlyList(Of Object)
            Get
                If EnumJson Is Nothing Then
                    Return Nothing
                End If
                Return Aggregate item In EnumJson Select ConvertJTokenToObject(item) Into ToArray
            End Get
            Set(value As IReadOnlyList(Of Object))
                If value Is Nothing Then
                    EnumJson = Nothing
                    Return
                End If

                EnumJson = Aggregate item In value Select New JValue(item) Into ToArray
            End Set
        End Property

        ' ���⵼������ JSON �������
        Friend Property EnumJson As IReadOnlyList(Of JValue)

    End Class

    ''' <summary>
    ''' ָ��ģ�ͱ�������ĸ�ʽ��
    ''' </summary>
    Public Class ResponseFormat
        ''' <summary>
        ''' ������Ϣ�ĸ�ʽ����� <see cref="ResponseFormatTypes"/>
        ''' </summary>
        ''' <remarks>
        ''' ע��: ʹ�� JSON ģʽʱ���㻹����ͨ��ϵͳ���û���Ϣָʾģ������ JSON��
        ''' ����ģ�Ϳ��ܻ����ɲ��ϵĿհ��ַ���ֱ�����ɴﵽ�������ƣ��Ӷ���������ʱ�����в��Եá���ס����
        ''' ���⣬��� finish_reason="length"�����ʾ���ɳ����� max_tokens ��Ի���������������ĳ��ȣ���Ϣ���ݿ��ܻᱻ���ֽضϡ�
        ''' </remarks>
        Public Property Type As String
    End Class

    ''' <summary>
    ''' ģ�͵�����
    ''' </summary>
    Public Class ModelNames
        ''' <summary>
        ''' ����ģ�� ID "deepseek-chat"
        ''' </summary>
        Public Shared ReadOnly Property ChatModel As String = "deepseek-chat"

        ''' <summary>
        ''' ������ģ�� ID "deepseek-coder"
        ''' </summary>
        Public Shared ReadOnly Property CoderModel As String = "deepseek-coder"

        ''' <summary>
        ''' ����ģ�� ID "deepseek-reasoner"
        ''' </summary>
        Public Shared ReadOnly Property ReasonerModel As String = "deepseek-reasoner"
    End Class

    ''' <summary>
    ''' ��Ӧ��ʽ���ͳ�����
    ''' </summary>
    Public Class ResponseFormatTypes
        ''' <summary>
        ''' �ı���Ӧ���� "text"
        ''' </summary>
        Public Shared ReadOnly Property Text As String = "text"

        ''' <summary>
        ''' JSON ������Ӧ���� "json_object"
        ''' </summary>
        Public Shared ReadOnly Property JsonObject As String = "json_object"
    End Class

    ''' <summary>
    ''' ������Ϣ
    ''' </summary>
    Public Class ChatMessage
        ''' <summary>
        ''' ��Ϣ������
        ''' </summary>
        Public Property Content As String

        ''' <summary>
        ''' ��Ϣ��ɫ����� <see cref="ChatRoles"/>������������˻ᱨ��
        ''' </summary>
        Public Property Role As String

        ''' <summary>
        ''' ����ѡ��Ĳ����ߵ����ƣ�Ϊģ���ṩ��Ϣ��������ͬ��ɫ�Ĳ����ߡ�
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' (Beta) ���ô˲���Ϊ true����ǿ��ģ������ش����Դ� assistant ��Ϣ���ṩ��ǰ׺���ݿ�ʼ��
        ''' ���������� base_url = "https://api.deepseek.com/beta" ��ʹ�ô˹��ܡ�
        ''' </summary>
        Public Property Prefix As Boolean?

        ''' <summary>
        ''' �������ݣ�Ҳ���� &lt;think&gt; ��������ݡ�
        ''' </summary>
        ''' <remarks>���ڴ洢��������صĶ�����Ϣ</remarks>
        <JsonPropertyName("reasoning_content")>
        Public Property ReasoningContent As String

        ''' <summary>
        ''' ����Ϣ����Ӧ�� tool call �� ID��
        ''' </summary>
        <JsonPropertyName("tool_call_id")>
        Public Property ToolCallId As String

    End Class

    ''' <summary>
    ''' ����Ľ�ɫ
    ''' </summary>
    Public Class ChatRoles
        Public Shared ReadOnly Property System As String = "system"
        Public Shared ReadOnly Property User As String = "user"
        Public Shared ReadOnly Property Assistant As String = "assistant"
        Public Shared ReadOnly Property Tool As String = "tool"
    End Class
End Namespace
