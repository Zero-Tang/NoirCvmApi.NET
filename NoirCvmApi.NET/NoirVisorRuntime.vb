Imports System.Threading
Module NoirVisorRuntime
    Public Declare Unicode Function CreateFileW Lib "kernel32.dll" (ByVal FileName As String, ByVal DesiredAccess As Integer, ByVal ShareMode As Integer, ByVal SecurityAttributes As IntPtr, ByVal CreateDisposition As Integer, ByVal FlagsAndAttributes As Integer, ByVal TemplateFile As IntPtr) As IntPtr
    Public Declare Function CloseHandle Lib "kernel32.dll" (ByVal Handle As IntPtr) As Boolean
    Public Declare Function GetProcessWorkingSetSize Lib "kernel32.dll" (ByVal ProcessHandle As IntPtr, ByRef MinimumWorkingSetSize As UIntPtr, ByRef MaximumWorkingSetSize As UIntPtr) As Boolean
    Public Declare Function SetProcessWorkingSetSize Lib "kernel32.dll" (ByVal ProcessHandle As IntPtr, ByVal MinimumWorkingSetSize As UIntPtr, ByVal MaximumWorkingSetSize As UIntPtr) As Boolean
    Public Declare Function GetCurrentProcess Lib "kernel32.dll" () As IntPtr
    Public Declare Function VirtualAlloc Lib "kernel32.dll" (ByVal Address As IntPtr, ByVal Size As Integer, ByVal AllocationType As Integer, ByVal Protection As Integer) As IntPtr
    Public Declare Function VirtualFree Lib "kernel32.dll" (ByVal Address As IntPtr, ByVal Size As Integer, ByVal FreeType As Integer) As IntPtr
    Public Declare Function VirtualLock Lib "kernel32.dll" (ByVal Address As IntPtr, ByVal Size As Integer) As Boolean
    Public Declare Function VirtualUnlock Lib "kernel32.dll" (ByVal Address As IntPtr, ByVal Size As Integer) As Boolean
    Public Declare Sub CopyMemory Lib "kernel32.dll" Alias "RtlMoveMemory" (ByVal Destination As IntPtr, ByVal Source As IntPtr, ByVal Length As Integer)
    ' File Operation Constants
    Public Const GENERIC_READ As Integer = &H80000000
    Public Const FILE_SHARE_READ As Integer = &H1
    Public Const FILE_SHARE_WRITE As Integer = &H2
    Public Const FILE_ATTRIBUTE_NORMAL As Integer = &H80
    Public Const OPEN_EXISTING As Integer = 3
    ' Memory Operation Constants
    Public Const MEM_COMMIT As Integer = &H1000
    Public Const MEM_RESERVE As Integer = &H2000
    Public Const MEM_DECOMMIT As Integer = &H4000
    Public Const MEM_RELEASE As Integer = &H8000
    Public Const PAGE_READWRITE As Integer = &H4
    ' Device I/O Control Constants
    Public Const FILE_DEVICE_UNKNOWN As Integer = &H22
    Public Const METHOD_BUFFERED As Integer = 0
    Public Const METHOD_IN_DIRECT As Integer = 1
    Public Const METHOD_OUT_DIRECT As Integer = 2
    Public Const METHOD_NEITHER As Integer = 3
    Public Const FILE_ANY_ACCESS As Integer = 0
    ' NoirVisor Driver CVM Constants
    Public Const NoirSuccess As Integer = 0
    Public Const NoirAlreadyRescinded As Integer = &H40000001
    Public Const NoirUnsuccessful As Integer = &HC0000000
    Public Const NoirInsufficientResources As Integer = &HC0000001
    Public Const NoirNotImplemented As Integer = &HC0000002
    Public Const NoirUnknownProcessor As Integer = &HC0000003
    Public Const NoirInvalidParameter As Integer = &HC0000004
    Public Const NoirHypervisionAbsent As Integer = &HC0000005
    Public Const NoirVcpuAlreadyCreated As Integer = &HC0000006
    Public Const NoirBufferTooSmall As Integer = &HC0000007
    Public Const NoirVcpuNotExist As Integer = &HC0000008
    Public Const NoirUserPageViolation As Integer = &HC0000009
    Public Const NoirGuestPageAbsent As Integer = &HC000000A
    Public Const NoirAccessDenied As Integer = &HC000000B
    ' Variables 
    Public NvDriverHandle As IntPtr = IntPtr.Zero
    Public MinWSet As UIntPtr = UIntPtr.Zero
    Public MaxWSet As UIntPtr = UIntPtr.Zero
    Public WSetMutex As New Mutex
    Public Function CTL_CODE(ByVal DeviceType As Integer, ByVal FunctionCode As Integer, ByVal Method As Integer, ByVal Access As Integer) As Integer
        Return (DeviceType << 16) Or (Access << 14) Or (FunctionCode << 2) Or Method
    End Function
    Public Function CTL_CODE_GEN(ByVal FunctionCode As Integer) As Integer
        Return CTL_CODE(FILE_DEVICE_UNKNOWN, FunctionCode, METHOD_BUFFERED, FILE_ANY_ACCESS)
    End Function
    Public Sub NoirThrowByStatus(ByVal Status As Integer)
        Select Case Status
            Case NoirInsufficientResources
                Throw New OutOfMemoryException("NoirVisor failed to allocate memory from kernel!")
            Case NoirNotImplemented
                Throw New NotImplementedException("Customizable VM feature is not implemented for this processor!")
            Case NoirUnknownProcessor
                Throw New NotImplementedException("Your processor supports hardware-accelerated virtualization, but the Customizable VM feature is not implemented for this processor!")
            Case NoirHypervisionAbsent
                Throw New HypervisionAbsentException("NoirVisor did not subvert this system!")
        End Select
    End Sub
End Module
