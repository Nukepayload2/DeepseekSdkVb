Imports System.IO
Imports System.Net.Http

Namespace Utils
    ''' <summary>
    ''' A <see cref="StreamContent"/> that leaves the underlying stream open when disposed.
    ''' </summary>
    ''' <remarks>
    ''' Multipart forms own their parts; this wrapper keeps caller-provided file streams usable
    ''' after the request completes.
    ''' </remarks>
    Friend Class LeaveOpenStreamContent
        Inherits StreamContent

        Sub New(stream As Stream)
            MyBase.New(stream)
        End Sub

        Protected Overrides Sub Dispose(disposing As Boolean)
            ' Intentionally do not dispose the caller-provided stream.
        End Sub
    End Class
End Namespace
