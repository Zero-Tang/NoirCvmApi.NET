Imports System.Runtime.InteropServices
Public Class Miscellaneous
    Public Shared Sub PageLock(ByVal Address As IntPtr, ByVal Size As Integer)
        MinWSet += Size
        MaxWSet += Size
        If SetProcessWorkingSetSize(GetCurrentProcess(), MinWSet, MaxWSet) = False Then Throw New OutOfMemoryException("Failed to increase working set size! Win32 Error Code:" & Str(Err.LastDllError))
        If VirtualLock(Address, Size) = False Then Throw New OutOfMemoryException("Failed to lock memory! Win32 Error Code:" & Str(Err.LastDllError))
    End Sub
    Public Shared Sub PageUnlock(ByVal Address As IntPtr, ByVal Size As Integer)
        If SetProcessWorkingSetSize(GetCurrentProcess(), MinWSet - Size, MaxWSet - Size) = False Then Throw New OutOfMemoryException("Failed to decrease working set size! Win32 Error Code:" & Str(Err.LastDllError))
        If VirtualUnlock(Address, Size) = False Then Throw New OutOfMemoryException("Failed to unlock memory! Win32 Error Code:" & Str(Err.LastDllError))
        MinWSet -= Size
        MaxWSet -= Size
    End Sub
    Public Shared Function PageAlloc(ByVal Size As Integer) As IntPtr
        Dim p As IntPtr = VirtualAlloc(IntPtr.Zero, Size, MEM_COMMIT, PAGE_READWRITE)
        If p = IntPtr.Zero Then Throw New OutOfMemoryException("Failed to allocate pages! Win32 Error Code:" & Str(Err.LastDllError))
        Return p
    End Function
    Public Shared Function PageFree(ByVal Address As IntPtr) As Boolean
        Return VirtualFree(Address, 0, MEM_RELEASE)
    End Function
    Public Shared Sub InitializeLibrary()
        If NvDriverHandle <> IntPtr.Zero Then CloseHandle(NvDriverHandle)
        NvDriverHandle = CreateFileW("\\.\NoirVisor", GENERIC_READ, FILE_SHARE_READ Or FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero)
        If NvDriverHandle = -1 Then
            NvDriverHandle = IntPtr.Zero
            If Err.LastDllError = 2 Then
                Throw New NoirVisorCommunicationException("NoirVisor driver is not loaded! Consult the NoirVisor Driver Loader!")
            Else
                Throw New NoirVisorCommunicationException("Failed to connect to NoirVisor driver! Win32 Error Code:" & Str(Err.LastDllError))
            End If
        End If
        GetProcessWorkingSetSize(GetCurrentProcess(), MinWSet, MaxWSet)
    End Sub
    Public Shared Sub FinalizeLibrary()
        If NvDriverHandle <> IntPtr.Zero Then
            CloseHandle(NvDriverHandle)
            NvDriverHandle = IntPtr.Zero
        Else
            Throw New NoirVisorCommunicationException("NoirVisor driver is not connected!")
        End If
    End Sub
End Class

Public Class HypervisionAbsentException
    Inherits System.Exception
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
    Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
        MyBase.New(Message, InnerException)
    End Sub
End Class

Public Class NoirVisorCommunicationException
    Inherits System.Exception
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
    Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
        MyBase.New(Message, InnerException)
    End Sub
End Class

Public Class InvalidGuestStateException
    Inherits System.Exception
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
    Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
        MyBase.New(Message, InnerException)
    End Sub
End Class

Public Class UnhandledInterceptionException
    Inherits System.Exception
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
    Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
        MyBase.New(Message, InnerException)
    End Sub
End Class

Public Class AddressTranslationException
    Inherits System.Exception
    Public Sub New()
        MyBase.New()
    End Sub
    Public Sub New(ByVal message As String)
        MyBase.New(message)
    End Sub
    Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
        MyBase.New(Message, InnerException)
    End Sub
End Class