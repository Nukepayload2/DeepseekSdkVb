Imports System.IO
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Nukepayload2.AI.Providers.Deepseek
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Integration tests for the tool APIs.
''' </summary>
''' <remarks>
''' These tests are ignored by default. To run them, set DEEPSEEK_API_KEY and replace the
''' placeholder paths below. They are intentionally not executed in offline builds.
''' </remarks>
<TestClass>
Public Class ToolsExamples

    ''' <summary>
    ''' Placeholder for a local 16-bit PCM WAV file used by the ASR test.
    ''' </summary>
    Private Const SpeechWavPath As String = "<path-to-speech.wav>"

    ''' <summary>
    ''' Placeholder for a local PDF or image used by the OCR test.
    ''' </summary>
    Private Const DocumentPath As String = "<path-to-document.pdf>"

    Private Shared ReadOnly Property ApiKey As String
        Get
            Dim key = Environment.GetEnvironmentVariable("DEEPSEEK_API_KEY")
            If key Is Nothing Then
                Throw New InvalidOperationException("Please set API key to %DEEPSEEK_API_KEY% and try again.")
            End If
            Return key
        End Get
    End Property

    <TestMethod>
    <Ignore("Integration test. Set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestSearchAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim response = Await client.Tools.Search.SearchAsync(
            New SearchRequest With {
                .Query = "DeepSeek V4",
                .Count = 3,
                .NeedContent = False
            })
        Assert.IsTrue(response.Results.Count > 0)
        Assert.IsFalse(String.IsNullOrEmpty(response.Results(0).Title))
    End Function

    <TestMethod>
    <Ignore("Integration test. Set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestFetchAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim response = Await client.Tools.Fetch.FetchAsync(
            New FetchRequest With {.Url = "https://api-docs.deepseek.com/zh-cn/"})
        Assert.IsFalse(String.IsNullOrEmpty(response.Content))
    End Function

    <TestMethod>
    <Ignore("Integration test. Replace DocumentPath, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestOcrAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim response = Await client.Tools.Ocr.RecognizeAsync(DocumentPath, OcrPurposes.TextRecognition)
        Assert.IsTrue(response.PageCount > 0)
        Assert.IsTrue(response.Pages.Count > 0)
    End Function

    <TestMethod>
    <Ignore("Integration test. Replace SpeechWavPath, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestAsrAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim response = Await client.Tools.Asr.TranscribeAsync(SpeechWavPath)
        Assert.IsFalse(String.IsNullOrEmpty(response.Text))
    End Function

    <TestMethod>
    <Ignore("Integration test. Set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestTtsAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim outputPath = Path.Combine(Path.GetTempPath(), "deepseek-tts.wav")
        Using output As New FileStream(outputPath, FileMode.Create, FileAccess.Write)
            Dim info = Await client.Tools.Tts.SynthesizeAsync(
                New TtsRequest With {.Text = "你好，我是 DeepSeek。"},
                output)
            Assert.AreEqual("audio/wav", info.ContentType)
        End Using
        Assert.IsTrue(New FileInfo(outputPath).Length > 0)
    End Function

End Class
