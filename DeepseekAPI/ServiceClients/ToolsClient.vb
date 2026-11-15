Imports System.Net.Http

''' <summary>
''' Provides the DeepSeek tool APIs: web search, page fetching, OCR, speech recognition
''' and text-to-speech.
''' </summary>
''' <remarks>
''' All members use the same API key as <see cref="DeepSeekClient"/>.
''' See <see href="https://api-docs.deepseek.com/zh-cn/guides/search">Search &amp; Fetch</see>,
''' <see href="https://api-docs.deepseek.com/zh-cn/guides/ocr">OCR</see>,
''' <see href="https://api-docs.deepseek.com/zh-cn/guides/asr">ASR</see> and
''' <see href="https://api-docs.deepseek.com/zh-cn/guides/tts">TTS</see>.
''' </remarks>
Public Class ToolsClient

    ''' <summary>
    ''' Web search API.
    ''' </summary>
    Public ReadOnly Property Search As SearchClient

    ''' <summary>
    ''' Page fetching API.
    ''' </summary>
    Public ReadOnly Property Fetch As FetchClient

    ''' <summary>
    ''' OCR API.
    ''' </summary>
    Public ReadOnly Property Ocr As OcrClient

    ''' <summary>
    ''' Speech recognition API.
    ''' </summary>
    Public ReadOnly Property Asr As AsrClient

    ''' <summary>
    ''' Text-to-speech API (whole-text HTTP synthesis).
    ''' </summary>
    Public ReadOnly Property Tts As TtsClient

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        Search = New SearchClient(apiKey, client)
        Fetch = New FetchClient(apiKey, client)
        Ocr = New OcrClient(apiKey, client)
        Asr = New AsrClient(apiKey, client)
        Tts = New TtsClient(apiKey, client)
    End Sub

End Class
