Imports System.IO
Imports System.Threading.Tasks
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports Nukepayload2.AI.Providers.Deepseek
Imports Nukepayload2.AI.Providers.Deepseek.Models

''' <summary>
''' Integration tests for the Files API and image understanding.
''' </summary>
''' <remarks>
''' Ignored by default. Replace the placeholder paths, set DEEPSEEK_API_KEY, and remove
''' Ignore to run them against the real API.
''' </remarks>
<TestClass>
Public Class FilesExamples

    Private Const ImagePath As String = "<path-to-image.jpg>"
    Private Const DocumentPath As String = "<path-to-document.pdf>"
    Private Const ExistingFileId As String = "<file-id>"

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
    <Ignore("Integration test. Replace ImagePath, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestUploadImageAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim uploaded = Await client.Files.UploadAsync(ImagePath, FilePurposes.UserData)
        Assert.IsFalse(String.IsNullOrEmpty(uploaded.Id))
        Assert.AreEqual(FilePurposes.UserData, uploaded.Purpose)
    End Function

    <TestMethod>
    <Ignore("Integration test. Replace DocumentPath, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestUploadDocumentAndChatAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim uploaded = Await client.Files.UploadAsync(DocumentPath, FilePurposes.TextRecognition)
        Dim message As New ChatMessage(ChatRoles.User, Nothing) With {
            .Parts = {
                ChatContentPart.FromFileId(uploaded.Id),
                ChatContentPart.FromText("总结这份文档的要点。")
            }
        }
        Dim response = Await client.Chat.CompleteAsync(
            New ChatRequest With {
                .Model = ModelNames.V4Flash,
                .Messages = {message}
            })
        Assert.IsFalse(String.IsNullOrEmpty(response.Choices?.FirstOrDefault?.Message?.Content))
    End Function

    <TestMethod>
    <Ignore("Integration test. Replace ImagePath, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestChatWithImageAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim bytes = File.ReadAllBytes(ImagePath)
        Dim message As New ChatMessage(ChatRoles.User, Nothing) With {
            .Parts = {
                ChatContentPart.FromText("这张图片里有什么？"),
                ChatContentPart.FromImageBytes("image/jpeg", bytes, ChatImageDetailLevels.Low)
            }
        }
        Dim response = Await client.Chat.CompleteAsync(
            New ChatRequest With {
                .Model = ModelNames.V4Flash,
                .Messages = {message}
            })
        Assert.IsFalse(String.IsNullOrEmpty(response.Choices?.FirstOrDefault?.Message?.Content))
    End Function

    <TestMethod>
    <Ignore("Integration test. Set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestListFilesAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim result = Await client.Files.ListAsync(New FilesListRequest With {.Limit = 10})
        Assert.IsFalse(result.HasMore.HasValue AndAlso result.HasMore.Value AndAlso result.Data.Count = 0)
    End Function

    <TestMethod>
    <Ignore("Integration test. Replace ExistingFileId, set DEEPSEEK_API_KEY and remove Ignore to run.")>
    Public Async Function TestRetrieveFileAsync() As Task
        Dim client As New DeepSeekClient(ApiKey)
        Dim file = Await client.Files.RetrieveAsync(ExistingFileId)
        Assert.AreEqual(ExistingFileId, file.Id)
    End Function

End Class
