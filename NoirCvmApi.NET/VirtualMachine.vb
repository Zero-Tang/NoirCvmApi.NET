Option Strict On
Imports System
Imports System.Runtime.InteropServices
Public Class VirtualMachine
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByRef InputBuffer As Long, ByVal InputLength As Integer, ByRef OutputBuffer As Integer, ByVal OutputLength As Integer, ByRef ReturnLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByVal InputBuffer As IntPtr, ByVal InputLength As Integer, ByRef OutputBuffer As Long, ByVal OutputLength As Integer, ByRef ReturnLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByRef InputBuffer As NoirAddressMapping, ByVal InputLength As Integer, ByRef OutputBuffer As Integer, ByVal OutputLength As Integer, ByRef ReturnLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    ' Memory Mapping
    Private Structure NoirAddressMapping
        Dim GPA As Long
        Dim HVA As Long
        Dim NumberOfPages As Integer
        Dim Attributes As Integer
        Dim Handle As Long
    End Structure
    Private Const NoirAddressMapPresent As Integer = &H1UI
    Private Const NoirAddressMapWrite As Integer = &H2UI
    Private Const NoirAddressMapExecute As Integer = &H4UI
    Private Const NoirAddressMapUser As Integer = &H8UI
    Private Const NoirAddressMapLargePage As Integer = &H80UI
    Private Const NoirAddressMapHugePage As Integer = &H100UI
    Private Const NoirAddressMapCacheShift As Integer = 4
    Private Const NoirAddressMapPageSizeShift As Integer = 7
    Public Enum PageSizeType
        NormalPageSize = 0
        LargePageSize = 1
        HugePageSize = 2
    End Enum
    Public Enum MemoryCacheType
        MemoryTypeUncacheable = 0
        MemoryTypeWriteCombining = 1
        MemoryTypeWriteThrough = 4
        MemoryTypeWriteProtect = 5
        MemoryTypeWriteBack = 6
    End Enum
    ' I/O Control Codes for NoirVisor CVM
    Private IOCTL_CvmCreateVm As Integer = CTL_CODE_GEN(&H880)
    Private IOCTL_CvmDeleteVm As Integer = CTL_CODE_GEN(&H881)
    Private IOCTL_CvmSetMapping As Integer = CTL_CODE_GEN(&H882)
    Private IOCTL_CvmQueryGpaAdMap As Integer = CTL_CODE_GEN(&H883)
    Private IOCTL_CvmClearGpaAdBit As Integer = CTL_CODE_GEN(&H884)
    Private IOCTL_CvmSetMappingEx As Integer = CTL_CODE_GEN(&H885)
    ' The handle of the VM.
    Public ReadOnly VmHandle As Long = 0
    Public Sub SetAddressMapping(ByVal GPA As Long, ByVal HVA As IntPtr, ByVal NumberOfPages As Integer, Optional ByVal Present As Boolean = True, Optional ByVal Write As Boolean = True, Optional ByVal Execute As Boolean = True, Optional ByVal User As Boolean = True, Optional ByVal MemoryType As MemoryCacheType = MemoryCacheType.MemoryTypeWriteBack, Optional ByVal PageSize As PageSizeType = PageSizeType.NormalPageSize)
        Dim MapInfo As NoirAddressMapping
        MapInfo.GPA = GPA
        MapInfo.HVA = HVA.ToInt64()
        MapInfo.NumberOfPages = NumberOfPages
        MapInfo.Attributes = 0
        If Present Then MapInfo.Attributes = MapInfo.Attributes Or NoirAddressMapPresent
        If Write Then MapInfo.Attributes = MapInfo.Attributes Or NoirAddressMapWrite
        If Execute Then MapInfo.Attributes = MapInfo.Attributes Or NoirAddressMapExecute
        If User Then MapInfo.Attributes = MapInfo.Attributes Or NoirAddressMapUser
        MapInfo.Attributes = MapInfo.Attributes Or (MemoryType << NoirAddressMapCacheShift)
        MapInfo.Attributes = MapInfo.Attributes Or (PageSize << NoirAddressMapPageSizeShift)
        MapInfo.Handle = VmHandle
        Dim Status As Integer, ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmSetMapping, MapInfo, Marshal.SizeOf(MapInfo), Status, 4, ReturnedLength, IntPtr.Zero)
        If Result = False Then
            Throw New NoirVisorCommunicationException("Failed to set mapping! Win32 Error Code:" & Str(Err.LastDllError))
        Else
            NoirThrowByStatus(Status)
        End If
    End Sub

    Public Sub New()
        Dim OutBuff(1) As Long
        Dim ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmCreateVm, IntPtr.Zero, 0, OutBuff(0), 16, ReturnedLength, IntPtr.Zero)
        Dim Status As Integer = CInt(OutBuff(0))
        If Result = True And Status = NoirSuccess Then
            VmHandle = OutBuff(1)
        Else
            If Result Then
                NoirThrowByStatus(Status)
            Else
                Throw New NoirVisorCommunicationException("Failed to create VM! Win32 Error Code:" & Str(Err.LastDllError))
            End If
        End If
    End Sub

    Public Sub Dispose()
        Dim Status As Integer
        Dim ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmDeleteVm, VmHandle, 8, Status, 4, ReturnedLength, IntPtr.Zero)
        If Result = False Then
            Throw New NoirVisorCommunicationException("Failed to delete VM! Win32 Error Code:" & Str(Err.LastDllError))
        Else
            NoirThrowByStatus(Status)
        End If
    End Sub
End Class
