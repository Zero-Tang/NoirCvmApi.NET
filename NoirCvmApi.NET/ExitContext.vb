Imports System.Runtime.InteropServices
Public Class ExitContext
    Public ReadOnly Rflags As Long
    Public ReadOnly Rip As Long
    Public ReadOnly CsSelector As Short
    Public ReadOnly CsAttribtues As Short
    Public ReadOnly CsLimit As Integer
    Public ReadOnly CsBase As Long
    Public ReadOnly CPL As Byte
    Public ReadOnly ProtectedMode As Boolean
    Public ReadOnly LongMode As Boolean
    Public ReadOnly InterruptShadow As Boolean
    Public ReadOnly InstructionLength As Byte
    Public ReadOnly VirtualProcessor As VirtualProcessor
    Private Const Offset_InterceptCode As Integer = &H0
    Private Const Offset_SpecificContext As Integer = &H8
    Private Const Offset_CsSelector As Integer = &H50
    Private Const Offset_CsAttributes As Integer = &H52
    Private Const Offset_CsLimit As Integer = &H54
    Private Const Offset_CsBase As Integer = &H58
    Private Const Offset_Rip As Integer = &H60
    Private Const Offset_Rflags As Integer = &H68
    Private Const Offset_VpState As Integer = &H70
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        VirtualProcessor = VP
        Dim VpState As Integer = Marshal.ReadInt32(ExitContextBuffer, Offset_VpState)
        Rflags = Marshal.ReadInt64(ExitContextBuffer, Offset_Rflags)
        Rip = Marshal.ReadInt64(ExitContextBuffer, Offset_Rip)
        CsBase = Marshal.ReadInt64(ExitContextBuffer, Offset_CsBase)
        CsLimit = Marshal.ReadInt32(ExitContextBuffer, Offset_CsLimit)
        CsAttribtues = Marshal.ReadInt16(ExitContextBuffer, Offset_CsAttributes)
        CsSelector = Marshal.ReadInt16(ExitContextBuffer, Offset_CsSelector)
        CPL = CByte(VpState And &H3)
        ProtectedMode = CBool((VpState And &H4) = &H4)
        LongMode = CBool((VpState And &H8) = &H8)
        InterruptShadow = CBool((VpState And &H10) = &H10)
        InstructionLength = CByte((VpState >> 5) And &HF)
    End Sub

    Public Sub AdvanceRip()
        VirtualProcessor.Rip = Rip + InstructionLength
    End Sub
End Class

Public NotInheritable Class IoExitContext
    Inherits ExitContext
    Public ReadOnly DsSelector As Short
    Public ReadOnly DsAttributes As Short
    Public ReadOnly DsLimit As Integer
    Public ReadOnly DsBase As Long
    Public ReadOnly EsSelector As Short
    Public ReadOnly EsAttributes As Short
    Public ReadOnly EsLimit As Integer
    Public ReadOnly EsBase As Long
    Public ReadOnly Rax As Long
    Public ReadOnly Rcx As Long
    Public ReadOnly Rsi As Long
    Public ReadOnly Rdi As Long
    Public ReadOnly Port As UShort
    Public ReadOnly InputInstruction As Boolean
    Public ReadOnly StringInstruction As Boolean
    Public ReadOnly RepeatInstruction As Boolean
    Public ReadOnly OperandSize As Byte
    Public ReadOnly AddressWidth As Byte
    Private Const Offset_InterceptCode As Integer = &H0
    Private Const Offset_IoAccessType As Integer = &H8
    Private Const Offset_IoPort As Integer = &HA
    Private Const Offset_IoRax As Integer = &H10
    Private Const Offset_IoRcx As Integer = &H18
    Private Const Offset_IoRsi As Integer = &H20
    Private Const Offset_IoRdi As Integer = &H28
    Private Const Offset_IoDsSelector As Integer = &H30
    Private Const Offset_IoDsAttributes As Integer = &H32
    Private Const Offset_IoDsLimit As Integer = &H34
    Private Const Offset_IoDsBase As Integer = &H38
    Private Const Offset_IoEsSelector As Integer = &H40
    Private Const Offset_IoEsAttributes As Integer = &H42
    Private Const Offset_IoEsLimit As Integer = &H44
    Private Const Offset_IoEsBase As Integer = &H48
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        MyBase.New(VP, ExitContextBuffer)
        Dim IoAccess As Short = Marshal.ReadInt16(ExitContextBuffer, Offset_IoAccessType)
        Dim PortBuff(1) As Byte
        Marshal.Copy(ExitContextBuffer + Offset_IoPort, PortBuff, 0, 2)
        Port = BitConverter.ToUInt16(PortBuff, 0)
        Rax = Marshal.ReadInt64(ExitContextBuffer, Offset_IoRax)
        Rcx = Marshal.ReadInt64(ExitContextBuffer, Offset_IoRcx)
        Rsi = Marshal.ReadInt64(ExitContextBuffer, Offset_IoRsi)
        Rdi = Marshal.ReadInt64(ExitContextBuffer, Offset_IoRdi)
        DsSelector = Marshal.ReadInt16(ExitContextBuffer, Offset_IoDsSelector)
        DsAttributes = Marshal.ReadInt16(ExitContextBuffer, Offset_IoDsAttributes)
        DsLimit = Marshal.ReadInt32(ExitContextBuffer, Offset_IoDsLimit)
        DsBase = Marshal.ReadInt64(ExitContextBuffer, Offset_IoDsBase)
        EsSelector = Marshal.ReadInt16(ExitContextBuffer, Offset_IoEsSelector)
        EsAttributes = Marshal.ReadInt16(ExitContextBuffer, Offset_IoEsAttributes)
        EsLimit = Marshal.ReadInt32(ExitContextBuffer, Offset_IoEsLimit)
        EsBase = Marshal.ReadInt64(ExitContextBuffer, Offset_IoEsBase)
        InputInstruction = CBool((IoAccess And &H1) = &H1)
        StringInstruction = CBool((IoAccess And &H2) = &H2)
        RepeatInstruction = CBool((IoAccess And &H4) = &H4)
        OperandSize = CByte((IoAccess >> 3) And &H7)
        AddressWidth = CByte((IoAccess >> 6) And &HF)
    End Sub
End Class

Public NotInheritable Class MsrExitContext
    Inherits ExitContext
    Public ReadOnly Eax As Integer
    Public ReadOnly Ecx As Integer
    Public ReadOnly Edx As Integer
    Private Const Offset_Eax As Integer = &H8
    Private Const Offset_Edx As Integer = &HC
    Private Const Offset_Ecx As Integer = &H10
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        MyBase.New(VP, ExitContextBuffer)
        Eax = Marshal.ReadInt32(ExitContextBuffer, Offset_Eax)
        Edx = Marshal.ReadInt32(ExitContextBuffer, Offset_Edx)
        Ecx = Marshal.ReadInt32(ExitContextBuffer, Offset_Ecx)
    End Sub
End Class

Public NotInheritable Class CpuidExitContext
    Inherits ExitContext
    Public ReadOnly Eax As Integer
    Public ReadOnly Ecx As Integer
    Private Const Offset_Eax As Integer = &H8
    Private Const Offset_Ecx As Integer = &HC
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        MyBase.New(VP, ExitContextBuffer)
        Eax = Marshal.ReadInt32(ExitContextBuffer, Offset_Eax)
        Ecx = Marshal.ReadInt32(ExitContextBuffer, Offset_Ecx)
    End Sub
End Class

Public NotInheritable Class MemoryAccessExitContext
    Inherits ExitContext
    Public ReadOnly ReadAccess As Boolean
    Public ReadOnly WriteAccess As Boolean
    Public ReadOnly ExecuteAccess As Boolean
    Public ReadOnly UserAccess As Boolean
    Public ReadOnly FetchedBytes As Byte
    Public ReadOnly InstructionBytes(14) As Byte
    Public ReadOnly GuestPhysicalAddress As Long
    Private Const Offset_Access As Integer = &H8
    Private Const Offset_Bytes As Integer = &H9
    Private Const Offset_GPA As Integer = &H18
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        MyBase.New(VP, ExitContextBuffer)
        Dim Access As Byte = Marshal.ReadByte(ExitContextBuffer, Offset_Access)
        Marshal.Copy(ExitContextBuffer + Offset_Bytes, InstructionBytes, 0, 15)
        GuestPhysicalAddress = Marshal.ReadInt64(ExitContextBuffer, Offset_GPA)
        ReadAccess = CBool((Access And &H1) = &H1)
        WriteAccess = CBool((Access And &H2) = &H2)
        ExecuteAccess = CBool((Access And &H4) = &H4)
        UserAccess = CBool((Access And &H8) = &H8)
        FetchedBytes = Access >> 4
    End Sub
End Class

Public NotInheritable Class ExceptionExitContext
    Inherits ExitContext
    Public ReadOnly Vector As Byte
    Public ReadOnly ErrorCodeValid As Boolean
    Public ReadOnly ErrorCode As Integer
    Public ReadOnly PageFaultAddress As Long
    Private Const Offset_BasicInfo As Integer = &H8
    Private Const Offset_ErrorCode As Integer = &HC
    Private Const Offset_PageFaultAddress As Integer = &H10
    Private Const PageFaultVector As Byte = 14
    Public Sub New(ByVal VP As VirtualProcessor, ByVal ExitContextBuffer As IntPtr)
        MyBase.New(VP, ExitContextBuffer)
        Dim BasicInfo As Integer = Marshal.ReadInt32(ExitContextBuffer, Offset_BasicInfo)
        Vector = CByte(BasicInfo And &H1F)
        ErrorCodeValid = CBool((BasicInfo And &H20) = &H20)
        If ErrorCodeValid Then ErrorCode = Marshal.ReadInt32(ExitContextBuffer, Offset_ErrorCode)
        If Vector = PageFaultVector Then PageFaultAddress = Marshal.ReadInt64(ExitContextBuffer, Offset_PageFaultAddress)
    End Sub
End Class