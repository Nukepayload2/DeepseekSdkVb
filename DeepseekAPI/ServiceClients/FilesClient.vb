Imports System.Globalization
Imports System.IO
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports Nukepayload2.AI.Providers.Deepseek.Models
Imports Nukepayload2.AI.Providers.Deepseek.Utils

''' <summary>
''' Provides the Files API.
''' </summary>
''' <remarks>
''' Upload files, reference them by <c>file_id</c> in chat requests, and manage their lifecycle.
''' See <see href="https://api-docs.deepseek.com/zh-cn/api/create-file">Create File API</see>
''' and <see href="https://api-docs.deepseek.com/zh-cn/guides/files_api">Files API guide</see>.
''' </remarks>
Public Class FilesClient
    Inherits ClientFeatureBase

    ''' <summary>
    ''' Timeout for file upload and document parsing. Defaults to 5 minutes.
    ''' </summary>
    Public Property RequestTimeout As TimeSpan = TimeSpan.FromMinutes(5)

    Sub New(apiKey As String, Optional client As HttpClient = Nothing)
        MyBase.New(apiKey, client, DeepSeekClient.DefaultLongRunningHttpClient)
    End Sub

    Protected Overridable ReadOnly Property RequestUrl As String = "https://api.deepseek.com/files"

    ''' <summary>
    ''' Uploads a file.
    ''' </summary>
    ''' <param name="request">The upload request.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The uploaded file object.</returns>
    Public Async Function UploadAsync(request As FileUploadRequest,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of FileItem)
        ValidateUpload(request)
        Using cts = CreateTimeout(cancellationToken, RequestTimeout)
            Using form = CreateUploadForm(request)
                Dim json = Await PostMultipartAsync(RequestUrl, form, cts.Token)
                Dim memory = Await IoUtils.ToMemoryStreamAsync(json, cts.Token)
                Return FileItem.FromJson(memory)
            End Using
        End Using
    End Function

    ''' <summary>
    ''' Uploads a local file.
    ''' </summary>
    ''' <param name="filePath">Path of the file to upload.</param>
    ''' <param name="purpose">One of <see cref="FilePurposes"/>.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The uploaded file object.</returns>
    Public Async Function UploadAsync(filePath As String, purpose As String,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of FileItem)
        If filePath Is Nothing Then Throw New ArgumentNullException(NameOf(filePath))
        Using fs As New FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)
            Dim request As New FileUploadRequest With {
                .File = fs,
                .FileName = Path.GetFileName(filePath),
                .Purpose = purpose
            }
            Return Await UploadAsync(request, cancellationToken)
        End Using
    End Function

    ''' <summary>
    ''' Lists files using default query parameters.
    ''' </summary>
    Public Async Function ListAsync(Optional cancellationToken As CancellationToken = Nothing) As Task(Of FilesListResponse)
        Return Await ListAsync(New FilesListRequest(), cancellationToken)
    End Function

    ''' <summary>
    ''' Lists files.
    ''' </summary>
    ''' <param name="request">The list query parameters.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    ''' <returns>The file list page.</returns>
    Public Async Function ListAsync(request As FilesListRequest,
                                    Optional cancellationToken As CancellationToken = Nothing) As Task(Of FilesListResponse)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        ValidateListRequest(request)
        Dim json = Await GetAsync(BuildListUrl(request), cancellationToken)
        Return FilesListResponse.FromJson(json)
    End Function

    ''' <summary>
    ''' Retrieves information about one file.
    ''' </summary>
    ''' <param name="fileId">The file identifier.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    Public Async Function RetrieveAsync(fileId As String,
                                        Optional cancellationToken As CancellationToken = Nothing) As Task(Of FileItem)
        ValidateFileId(fileId)
        Dim json = Await GetAsync(RequestUrl & "/" & Uri.EscapeDataString(fileId), cancellationToken)
        Return FileItem.FromJson(json)
    End Function

    ''' <summary>
    ''' Deletes a file.
    ''' </summary>
    ''' <param name="fileId">The file identifier.</param>
    ''' <param name="cancellationToken">Use it to cancel the request.</param>
    Public Async Function DeleteAsync(fileId As String,
                                      Optional cancellationToken As CancellationToken = Nothing) As Task(Of FileDeleteResponse)
        ValidateFileId(fileId)
        Dim json = Await DeleteResourceAsync(RequestUrl & "/" & Uri.EscapeDataString(fileId), cancellationToken)
        Return FileDeleteResponse.FromJson(json)
    End Function

    Private Shared Function CreateUploadForm(request As FileUploadRequest) As MultipartFormDataContent
        Dim form As New MultipartFormDataContent()
        ' The API requires purpose before file.
        form.Add(New StringContent(request.Purpose), "purpose")
        If request.ExpiresAfterAnchor IsNot Nothing Then
            form.Add(New StringContent(request.ExpiresAfterAnchor), "expires_after[anchor]")
        End If
        If request.ExpiresAfterSeconds.HasValue Then
            form.Add(New StringContent(request.ExpiresAfterSeconds.Value.ToString(CultureInfo.InvariantCulture)),
                     "expires_after[seconds]")
        End If
        Dim fileContent As New LeaveOpenStreamContent(request.File)
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream")
        form.Add(fileContent, "file", request.FileName)
        Return form
    End Function

    Private Function BuildListUrl(request As FilesListRequest) As String
        Dim query As New List(Of String)
        If Not String.IsNullOrEmpty(request.After) Then
            query.Add("after=" & Uri.EscapeDataString(request.After))
        End If
        If request.Limit.HasValue Then
            query.Add("limit=" & request.Limit.Value.ToString(CultureInfo.InvariantCulture))
        End If
        If Not String.IsNullOrEmpty(request.Order) Then
            query.Add("order=" & Uri.EscapeDataString(request.Order))
        End If
        If Not String.IsNullOrEmpty(request.Purpose) Then
            query.Add("purpose=" & Uri.EscapeDataString(request.Purpose))
        End If
        If query.Count = 0 Then Return RequestUrl
        Return RequestUrl & "?" & String.Join("&", query)
    End Function

    Private Shared Sub ValidateUpload(request As FileUploadRequest)
        If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
        If request.File Is Nothing Then
            Throw New ArgumentException("File is required.", NameOf(request))
        End If
        If String.IsNullOrEmpty(request.FileName) Then
            Throw New ArgumentException("FileName is required.", NameOf(request))
        End If
        If Not IsValidPurpose(request.Purpose) Then
            Throw New ArgumentException("Purpose must be user_data, text_recognition or rich_layout_recognition.", NameOf(request))
        End If
        If request.ExpiresAfterAnchor IsNot Nothing OrElse request.ExpiresAfterSeconds.HasValue Then
            If request.ExpiresAfterAnchor <> "created_at" Then
                Throw New ArgumentException("ExpiresAfterAnchor must be created_at when provided.", NameOf(request))
            End If
            If Not request.ExpiresAfterSeconds.HasValue Then
                Throw New ArgumentException("ExpiresAfterSeconds is required with ExpiresAfterAnchor.", NameOf(request))
            End If
            If request.ExpiresAfterSeconds.Value < 3600 OrElse request.ExpiresAfterSeconds.Value > 2592000 Then
                Throw New ArgumentOutOfRangeException(NameOf(request), "ExpiresAfterSeconds must be between 3600 and 2592000.")
            End If
        End If
    End Sub

    Private Shared Sub ValidateListRequest(request As FilesListRequest)
        If request.Limit.HasValue AndAlso (request.Limit.Value < 1 OrElse request.Limit.Value > 1000) Then
            Throw New ArgumentOutOfRangeException(NameOf(request), "Limit must be between 1 and 1000.")
        End If
        If Not String.IsNullOrEmpty(request.Order) AndAlso
           request.Order <> "asc" AndAlso request.Order <> "desc" Then
            Throw New ArgumentException("Order must be asc or desc.", NameOf(request))
        End If
        If Not String.IsNullOrEmpty(request.Purpose) AndAlso Not IsValidPurpose(request.Purpose) Then
            Throw New ArgumentException("Purpose must be user_data, text_recognition or rich_layout_recognition.", NameOf(request))
        End If
    End Sub

    Private Shared Function IsValidPurpose(purpose As String) As Boolean
        Return purpose = FilePurposes.UserData OrElse
               purpose = FilePurposes.TextRecognition OrElse
               purpose = FilePurposes.RichLayoutRecognition
    End Function

    Private Shared Sub ValidateFileId(fileId As String)
        If String.IsNullOrWhiteSpace(fileId) Then
            Throw New ArgumentException("FileId is required.", NameOf(fileId))
        End If
    End Sub

End Class
