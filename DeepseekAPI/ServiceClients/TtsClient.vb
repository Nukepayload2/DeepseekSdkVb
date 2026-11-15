Imports System.IO
Imports System.Net.Http
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Provides the text-to-speech API for whole-text HTTP synthesis.
''' </summary>
''' <remarks>
''' The audio is streamed into the caller-provided <see cref="Stream"/>.
''' WebSocket streaming is not supported yet.
''' See <see href="https://api-docs.deepseek.com/zh-cn/api/tts">TTS API</see>.
''' </remarks>
Public Class TtsClient
    Inherits ClientFeatureBase

    ''' <summary>
    ''' Timeout for the whole synthesis request. Defaults to 30 minutes.
    ''' </summary>
    Public Property RequestTimeout As TimeSpan = TimeSpan.FromMinutes(30)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client, DeepSeekClient.DefaultLongRunningHttpClient)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/v1/tools/tts"

    ''' <summary>
    ''' Synthesizes speech and writes the audio bytes to <paramref name="destination"/>.
    ''' </summary>
    ''' <param name="request">The synthesis request.</param>
    ''' <param name="destination">The stream that receives the audio data.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>Format information about the produced audio.</returns>
    ''' <exception cref="DeepSeekHttpRequestException">
    ''' The request was not successful and the response contains a JSON error.
    ''' </exception>
    ''' <exception cref="HttpRequestException">
    ''' The request was not successful.
    ''' </exception>
    ''' <exception cref="IOException">
    ''' The response stream ended before the audio was complete.
    ''' </exception>
    Public Async Function SynthesizeAsync(request As TtsRequest, destination As Stream,
                                          Optional cancellationToken As CancellationToken = Nothing) As Task(Of TtsAudioInfo)
        ValidateRequest(request, destination)
        Dim format = If(String.IsNullOrEmpty(request.Format), TtsFormats.Wav, request.Format)
        Using cts = CreateTimeout(cancellationToken, RequestTimeout)
            Dim json = request.ToJsonUtf8()
            Using response = Await PostForStreamAsync(RequestUrl, json, GetAccept(format), cts.Token)
#If NET5_0_OR_GREATER Then
                Dim source = Await response.Content.ReadAsStreamAsync(cts.Token)
#Else
                Dim source = Await response.Content.ReadAsStreamAsync()
#End If
                Try
                    Await source.CopyToAsync(destination, 81920, cts.Token)
                Finally
                    source.Dispose()
                End Try
                Dim contentType = If(response.Content.Headers.ContentType Is Nothing,
                                     Nothing,
                                     response.Content.Headers.ContentType.MediaType)
                Return New TtsAudioInfo(contentType, format, response.Content.Headers.ContentLength)
            End Using
        End Using
    End Function

    Private Shared Function GetAccept(format As String) As String
        Return If(format = TtsFormats.Pcm, "audio/pcm", "audio/wav")
    End Function

    Private Shared Sub ValidateRequest(request As TtsRequest, destination As Stream)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If String.IsNullOrWhiteSpace(request.Text) Then
            Throw New ArgumentException("Text is required and cannot be blank.", NameOf(request))
        End If
        If destination Is Nothing Then Throw New ArgumentNullException(NameOf(destination))
        If Not destination.CanWrite Then
            Throw New ArgumentException("Destination stream must be writable.", NameOf(destination))
        End If
        If Not String.IsNullOrEmpty(request.Format) AndAlso
           request.Format <> TtsFormats.Wav AndAlso request.Format <> TtsFormats.Pcm Then
            Throw New ArgumentException("Format must be wav or pcm.", NameOf(request))
        End If
    End Sub

End Class
