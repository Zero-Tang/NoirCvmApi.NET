Option Strict On
Imports System
Imports System.Runtime.InteropServices
Public Class VirtualProcessor
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByRef InputBuffer As Long, ByVal InputLength As Integer, ByRef OutputBuffer As Integer, ByVal OutputLength As Integer, ByRef ReturnedLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByRef InputBuffer As Long, ByVal InputLength As Integer, ByVal OutputBuffer As IntPtr, ByVal OutputLength As Integer, ByRef ReturnedLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    Private Declare Function DeviceIoControl Lib "kernel32.dll" (ByVal DeviceHandle As IntPtr, ByVal IoControlCode As Integer, ByVal InputBuffer As IntPtr, ByVal InputLength As Integer, ByVal OutputBuffer As IntPtr, ByVal OutputLength As Integer, ByRef ReturnedLength As Integer, ByVal Overlapped As IntPtr) As Boolean
    Private Enum NoirCvmRegisterType
        NoirCvmGeneralPurposeRegister
        NoirCvmFlagsRegister
        NoirCvmInstructionPointer
        NoirCvmControlRegister
        NoirCvmCr2Register
        NoirCvmDebugRegister
        NoirCvmDr67Register
        NoirCvmSegmentRegister
        NoirCvmFsGsRegister
        NoirCvmDescriptorTable
        NoirCvmTrLdtrRegister
        NoirCvmSysCallMsrRegister
        NoirCvmSysEnterMsrRegister
        NoirCvmCr8Register
        NoirCvmFxState
        NoirCvmXsaveArea
        NoirCvmXcr0Register
        NoirCvmEferRegister
        NoirCvmPatRegister
        NoirCvmLastBranchRecordRegister
        NoirCvmMaximumRegisterType
    End Enum
    Private Enum NoirCvmInterceptCode
        InvalidState = 0
        ShutdownCondition = 1
        MemoryAccess = 2
        RsmInstruction = 3
        HltInstruction = 4
        IoInstruction = 5
        CpuidInstruction = 6
        RdmsrInstruction = 7
        WrmsrInstruction = 8
        CrAccess = 9
        DrAccess = 10
        Hypercall = 11
        Exception = 12
        Rescission = 13
        InterruptWindow = 14
        TaskSwitch = 15
    End Enum
    Public Enum EventType
        ExternalInterrupt = 0
        NonMaskableInterrupt = 2
        FaultTrapException = 3
        SoftwareInterrupt = 4
    End Enum
    Private IOCTL_CvmCreateVcpu As Integer = CTL_CODE_GEN(&H890)
    Private IOCTL_CvmDeleteVcpu As Integer = CTL_CODE_GEN(&H891)
    Private IOCTL_CvmRunVcpu As Integer = CTL_CODE_GEN(&H892)
    Private IOCTL_CvmViewVcpuReg As Integer = CTL_CODE_GEN(&H893)
    Private IOCTL_CvmEditVcpuReg As Integer = CTL_CODE_GEN(&H894)
    Private IOCTL_CvmRescindVcpu As Integer = CTL_CODE_GEN(&H895)
    Private IOCTL_CvmInjectEvent As Integer = CTL_CODE_GEN(&H896)
    Private IOCTL_CvmSetVcpuOptions As Integer = CTL_CODE_GEN(&H897)
    Private IOCTL_CvmQueryVcpuStats As Integer = CTL_CODE_GEN(&H898)

    Public Structure GeneralPurposeRegisterSet
        Dim Rax As Long
        Dim Rcx As Long
        Dim Rbx As Long
        Dim Rdx As Long
        Dim Rsp As Long
        Dim Rbp As Long
        Dim Rsi As Long
        Dim Rdi As Long
        Dim R8 As Long
        Dim R9 As Long
        Dim R10 As Long
        Dim R11 As Long
        Dim R12 As Long
        Dim R13 As Long
        Dim R14 As Long
        Dim R15 As Long
    End Structure
    Private Structure ControlRegisterSet
        Dim Cr0 As Long
        Dim Cr3 As Long
        Dim Cr4 As Long
    End Structure
    Private Structure DebugRegisterSet1
        Dim Dr0 As Long
        Dim Dr1 As Long
        Dim Dr2 As Long
        Dim Dr3 As Long
    End Structure
    Private Structure DebugRegisterSet2
        Dim Dr6 As Long
        Dim Dr7 As Long
    End Structure
    Private Structure SysEnterRegisterSet
        Dim SysEnterCs As Long
        Dim SysEnterEsp As Long
        Dim SysEnterEip As Long
    End Structure
    Private Structure SysCallRegisterSet
        Dim Star As Long
        Dim LStar As Long
        Dim CStar As Long
        Dim SfMask As Long
    End Structure
    Private Structure LastBranchRecordRegisterSet
        Dim DebugControl As Long
        Dim LastBranchFromIp As Long
        Dim LastBranchToIp As Long
        Dim LastExceptionFromIp As Long
        Dim LastExceptionToIp As Long
    End Structure
    <StructLayout(LayoutKind.Explicit)> Private Structure SegmentRegister
        <FieldOffset(0)> Dim Selector As Short
        <FieldOffset(2)> Dim Attributes As Short
        <FieldOffset(4)> Dim Limit As Integer
        <FieldOffset(8)> Dim Base As Long
    End Structure
    <StructLayout(LayoutKind.Explicit)> Private Structure SegmentRegisterSet
        <FieldOffset(0)> Dim Es As SegmentRegister
        <FieldOffset(&H10)> Dim Cs As SegmentRegister
        <FieldOffset(&H20)> Dim Ss As SegmentRegister
        <FieldOffset(&H30)> Dim Ds As SegmentRegister
    End Structure
    <StructLayout(LayoutKind.Explicit)> Private Structure SegmentRegisterSet2
        <FieldOffset(0)> Dim Fs As SegmentRegister
        <FieldOffset(&H10)> Dim Gs As SegmentRegister
        <FieldOffset(&H20)> Dim KernelGsBase As Long
    End Structure
    <StructLayout(LayoutKind.Explicit)> Private Structure SegmentRegisterSet3
        <FieldOffset(0)> Dim Tr As SegmentRegister
        <FieldOffset(&H10)> Dim Ldtr As SegmentRegister
    End Structure
    <StructLayout(LayoutKind.Explicit)> Private Structure SegmentRegisterSet4
        <FieldOffset(0)> Dim Gdtr As SegmentRegister
        <FieldOffset(&H10)> Dim Idtr As SegmentRegister
    End Structure

    Private GprState As GeneralPurposeRegisterSet
    Private GprValid As Boolean = False
    Private CrState As ControlRegisterSet
    Private CrValid As Boolean = False
    Private DrState1 As DebugRegisterSet1
    Private DrValid1 As Boolean = False
    Private DrState2 As DebugRegisterSet2
    Private DrValid2 As Boolean = False
    Private SrState As SegmentRegisterSet
    Private SrValid As Boolean = False
    Private FgState As SegmentRegisterSet2
    Private FgValid As Boolean = False
    Private LtState As SegmentRegisterSet3
    Private LtValid As Boolean = False
    Private DtState As SegmentRegisterSet4
    Private DtValid As Boolean = False
    Private ScState As SysCallRegisterSet
    Private ScValid As Boolean = False
    Private SeState As SysEnterRegisterSet
    Private SeValid As Boolean = False

    Public Enum ExtendedStateOffset As Integer
        Fcw = &H0I
        Fsw = &H2I
        Ftw = &H4I
        Fop = &H6I
        Fip = &H8I
        Fdp = &H10I
        MxCsr = &H18I
        MxCsrMask = &H1CI
        St0 = &H20I
        St1 = &H30I
        St2 = &H40I
        St3 = &H50I
        St4 = &H60I
        St5 = &H70I
        St6 = &H80I
        St7 = &H90I
        Xmm0 = &HA0I
        Xmm1 = &HB0I
        Xmm2 = &HC0I
        Xmm3 = &HD0I
        Xmm4 = &HE0I
        Xmm5 = &HF0I
        Xmm6 = &H100I
        Xmm7 = &H110I
        Xmm8 = &H120I
        Xmm9 = &H130I
        Xmm10 = &H140I
        Xmm11 = &H150I
        Xmm12 = &H160I
        Xmm13 = &H170I
        Xmm14 = &H180I
        Xmm15 = &H190I
    End Enum

    Private Const NoirCvmExitContextOffset As Integer = &H8
    Private Const NoirCvmExitContextSize As Integer = &H78

    Private Const NoirCvmVpOptionsInterceptCpuid As Integer = &H1
    Private Const NoirCvmVpOptionsInterceptMsr As Integer = &H2
    Private Const NoirCvmVpOptionsInterruptWindow As Integer = &H4
    Private Const NoirCvmVpOptionsInterceptExceptions As Integer = &H8
    Private Const NoirCvmVpOptionsInterceptCr3 As Integer = &H10
    Private Const NoirCvmVpOptionsInterceptDr As Integer = &H20
    Private Const NoirCvmVpOptionsInterceptPause As Integer = &H40
    Private Const NoirCvmVpOptionsNpiep As Integer = &H80
    Private Const NoirCvmVpOptionsNmiWindow As Integer = &H100
    Private Const NoirCvmVpOptionsInterceptRsm As Integer = &H200

    Public ReadOnly VirtualMachine As VirtualMachine
    Public ReadOnly VirtualProcessorIndex As Integer
    Public ReadOnly ExtendedState As IntPtr
    Private ReadOnly RegisterBuffer As IntPtr
    Private ReadOnly ExitContextBuffer As IntPtr
    Private VpOptions As Integer = 0
    Private VpOptValid As Boolean = False
    Private ExceptionInterceptMap As Integer = 0

    Public Delegate Function ExitIoInstructionHandler(ByRef Context As IoExitContext) As Boolean
    Public Delegate Function ExitCpuidInstructionHandler(ByRef Context As CpuidExitContext) As Boolean
    Public Delegate Function ExitGenericHandler(ByRef Context As ExitContext) As Boolean
    Public Delegate Function ExitRdmsrInstructionHandler(ByRef Context As MsrExitContext) As Boolean
    Public Delegate Function ExitWrmsrInstructionHandler(ByRef Context As MsrExitContext) As Boolean
    Public Delegate Function ExitMemoryAccessHandler(ByRef Context As MemoryAccessExitContext) As Boolean
    Public Delegate Function ExitExceptionHandler(ByRef Context As ExceptionExitContext) As Boolean

    Public IoHandler As ExitIoInstructionHandler = Nothing
    Public CpuidHandler As ExitCpuidInstructionHandler = Nothing
    Public HaltHandler As ExitGenericHandler = Nothing
    Public RdmsrHandler As ExitRdmsrInstructionHandler = Nothing
    Public WrmsrHandler As ExitWrmsrInstructionHandler = Nothing
    Public RescissionHandler As ExitGenericHandler = Nothing
    Public ShutdownHandler As ExitGenericHandler = Nothing
    Public MemoryAccessHandler As ExitMemoryAccessHandler = Nothing
    Public ExceptionHandler As ExitExceptionHandler = Nothing
    Public RsmHandler As ExitGenericHandler = Nothing
    Public HypercallHandler As ExitGenericHandler = Nothing

    Public Property Rax As Long
        ' The rax register is not a synchronizing property.
        Get
            Return GprState.Rax
        End Get
        Set(ByVal value As Long)
            GprState.Rax = value
            GprValid = False
        End Set
    End Property

    Public Property Rbx As Long
        ' The rbx register is not a synchronizing property.
        Get
            Return GprState.Rbx
        End Get
        Set(ByVal value As Long)
            GprState.Rbx = value
            GprValid = False
        End Set
    End Property

    Public Property Rcx As Long
        ' The rcx register is not a synchronizing property.
        Get
            Return GprState.Rcx
        End Get
        Set(ByVal value As Long)
            GprState.Rcx = value
            GprValid = False
        End Set
    End Property

    Public Property Rdx As Long
        ' The rdx register is not a synchronizing property.
        Get
            Return GprState.Rdx
        End Get
        Set(ByVal value As Long)
            GprState.Rdx = value
            GprValid = False
        End Set
    End Property

    Public Property Rsp As Long
        ' The rsp register is not a synchronizing property.
        Get
            Return GprState.Rsp
        End Get
        Set(ByVal value As Long)
            GprState.Rsp = value
            GprValid = False
        End Set
    End Property

    Public Property Rbp As Long
        ' The rbp register is not a synchronizing property.
        Get
            Return GprState.Rbp
        End Get
        Set(ByVal value As Long)
            GprState.Rbp = value
            GprValid = False
        End Set
    End Property

    Public Property Rsi As Long
        ' The rsi register is not a synchronizing property.
        Get
            Return GprState.Rsi
        End Get
        Set(ByVal value As Long)
            GprState.Rsi = value
            GprValid = False
        End Set
    End Property

    Public Property Rdi As Long
        ' The rdi register is not a synchronizing property.
        Get
            Return GprState.Rdi
        End Get
        Set(ByVal value As Long)
            GprState.Rdi = value
            GprValid = False
        End Set
    End Property

    Public Property R8 As Long
        ' The r8 register is not a synchronizing property.
        Get
            Return GprState.R8
        End Get
        Set(ByVal value As Long)
            GprState.R8 = value
            GprValid = False
        End Set
    End Property

    Public Property R9 As Long
        ' The r9 register is not a synchronizing property.
        Get
            Return GprState.R9
        End Get
        Set(ByVal value As Long)
            GprState.R9 = value
            GprValid = False
        End Set
    End Property

    Public Property R10 As Long
        ' The r10 register is not a synchronizing property.
        Get
            Return GprState.R10
        End Get
        Set(ByVal value As Long)
            GprState.R10 = value
            GprValid = False
        End Set
    End Property

    Public Property R11 As Long
        ' The r11 register is not a synchronizing property.
        Get
            Return GprState.R11
        End Get
        Set(ByVal value As Long)
            GprState.R11 = value
            GprValid = False
        End Set
    End Property

    Public Property R12 As Long
        ' The r12 register is not a synchronizing property.
        Get
            Return GprState.R12
        End Get
        Set(ByVal value As Long)
            GprState.R12 = value
            GprValid = False
        End Set
    End Property

    Public Property R13 As Long
        ' The r13 register is not a synchronizing property.
        Get
            Return GprState.R13
        End Get
        Set(ByVal value As Long)
            GprState.R13 = value
            GprValid = False
        End Set
    End Property

    Public Property R14 As Long
        ' The r14 register is not a synchronizing property.
        Get
            Return GprState.R14
        End Get
        Set(ByVal value As Long)
            GprState.R14 = value
            GprValid = False
        End Set
    End Property

    Public Property R15 As Long
        ' The r15 register is not a synchronizing property.
        Get
            Return GprState.R15
        End Get
        Set(ByVal value As Long)
            GprState.R15 = value
            GprValid = False
        End Set
    End Property

    Public Property Rip As Long
        ' The rip register is a synchronizing property.
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmInstructionPointer, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmInstructionPointer, 8)
        End Set
    End Property

    Public Property Rflags As Long
        ' The rflags register is a synchronizing property.
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmFlagsRegister, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmFlagsRegister, 8)
        End Set
    End Property

    Public Property Cr0 As Long
        ' The cr0 register is not a synchronizing property
        Get
            Return CrState.Cr0
        End Get
        Set(ByVal value As Long)
            CrState.Cr0 = value
            CrValid = False
        End Set
    End Property

    Public Property Cr2 As Long
        ' The cr2 register is a synchronizing property
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmCr2Register, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmCr2Register, 8)
        End Set
    End Property

    Public Property Cr3 As Long
        ' The cr3 register is not a synchronizing property
        Get
            Return CrState.Cr3
        End Get
        Set(ByVal value As Long)
            CrState.Cr3 = value
            CrValid = False
        End Set
    End Property

    Public Property Cr4 As Long
        ' The cr4 register is not a synchronizing property
        Get
            Return CrState.Cr4
        End Get
        Set(ByVal value As Long)
            CrState.Cr4 = value
            CrValid = False
        End Set
    End Property

    Public Property Xcr0 As Long
        ' The xcr0 register is a synchronizing property
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmXcr0Register, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmXcr0Register, 8)
        End Set
    End Property

    Public Property Cr8 As Long
        ' The cr8 register is a synchronizing property
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmCr8Register, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmCr8Register, 8)
        End Set
    End Property

    Public Property Dr0 As Long
        ' The dr0 register is not a synchronizing property
        Get
            Return DrState1.Dr0
        End Get
        Set(ByVal value As Long)
            DrState1.Dr0 = value
            DrValid1 = False
        End Set
    End Property

    Public Property Dr1 As Long
        ' The dr1 register is not a synchronizing property
        Get
            Return DrState1.Dr1
        End Get
        Set(ByVal value As Long)
            DrState1.Dr1 = value
            DrValid1 = False
        End Set
    End Property

    Public Property Dr2 As Long
        ' The dr2 register is not a synchronizing property
        Get
            Return DrState1.Dr2
        End Get
        Set(ByVal value As Long)
            DrState1.Dr2 = value
            DrValid1 = False
        End Set
    End Property

    Public Property Dr3 As Long
        ' The dr3 register is not a synchronizing property
        Get
            Return DrState1.Dr3
        End Get
        Set(ByVal value As Long)
            DrState1.Dr3 = value
            DrValid1 = False
        End Set
    End Property

    Public Property Dr6 As Long
        ' The dr6 register is not a synchronizing property
        Get
            Return DrState2.Dr6
        End Get
        Set(ByVal value As Long)
            DrState2.Dr6 = value
            DrValid2 = False
        End Set
    End Property

    Public Property Dr7 As Long
        ' The dr6 register is not a synchronizing property
        Get
            Return DrState2.Dr7
        End Get
        Set(ByVal value As Long)
            DrState2.Dr7 = value
            DrValid2 = False
        End Set
    End Property

    Public Property CsSelector As Short
        Get
            Return SrState.Cs.Selector
        End Get
        Set(ByVal value As Short)
            SrState.Cs.Selector = value
            SrValid = False
        End Set
    End Property

    Public Property CsAttributes As Short
        Get
            Return SrState.Cs.Attributes
        End Get
        Set(ByVal value As Short)
            SrState.Cs.Attributes = value
            SrValid = False
        End Set
    End Property

    Public Property CsLimit As Integer
        Get
            Return SrState.Cs.Limit
        End Get
        Set(ByVal value As Integer)
            SrState.Cs.Limit = value
            SrValid = False
        End Set
    End Property

    Public Property CsBase As Long
        Get
            Return SrState.Cs.Base
        End Get
        Set(ByVal value As Long)
            SrState.Cs.Base = value
            SrValid = False
        End Set
    End Property

    Public Property DsSelector As Short
        Get
            Return SrState.Ds.Selector
        End Get
        Set(ByVal value As Short)
            SrState.Ds.Selector = value
            SrValid = False
        End Set
    End Property

    Public Property DsAttributes As Short
        Get
            Return SrState.Ds.Attributes
        End Get
        Set(ByVal value As Short)
            SrState.Ds.Attributes = value
            SrValid = False
        End Set
    End Property

    Public Property DsLimit As Integer
        Get
            Return SrState.Ds.Limit
        End Get
        Set(ByVal value As Integer)
            SrState.Ds.Limit = value
            SrValid = False
        End Set
    End Property

    Public Property DsBase As Long
        Get
            Return SrState.Ds.Base
        End Get
        Set(ByVal value As Long)
            SrState.Ds.Base = value
            SrValid = False
        End Set
    End Property

    Public Property EsSelector As Short
        Get
            Return SrState.Es.Selector
        End Get
        Set(ByVal value As Short)
            SrState.Es.Selector = value
            SrValid = False
        End Set
    End Property

    Public Property EsAttributes As Short
        Get
            Return SrState.Es.Attributes
        End Get
        Set(ByVal value As Short)
            SrState.Es.Attributes = value
            SrValid = False
        End Set
    End Property

    Public Property EsLimit As Integer
        Get
            Return SrState.Es.Limit
        End Get
        Set(ByVal value As Integer)
            SrState.Es.Limit = value
            SrValid = False
        End Set
    End Property

    Public Property EsBase As Long
        Get
            Return SrState.Es.Base
        End Get
        Set(ByVal value As Long)
            SrState.Es.Base = value
            SrValid = False
        End Set
    End Property

    Public Property SsSelector As Short
        Get
            Return SrState.Ss.Selector
        End Get
        Set(ByVal value As Short)
            SrState.Ss.Selector = value
            SrValid = False
        End Set
    End Property

    Public Property SsAttributes As Short
        Get
            Return SrState.Ss.Attributes
        End Get
        Set(ByVal value As Short)
            SrState.Ss.Attributes = value
            SrValid = False
        End Set
    End Property

    Public Property SsLimit As Integer
        Get
            Return SrState.Ss.Limit
        End Get
        Set(ByVal value As Integer)
            SrState.Ss.Limit = value
            SrValid = False
        End Set
    End Property

    Public Property SsBase As Long
        Get
            Return SrState.Ss.Base
        End Get
        Set(ByVal value As Long)
            SrState.Ss.Base = value
            SrValid = False
        End Set
    End Property

    Public Property FsSelector As Short
        Get
            Return FgState.Fs.Selector
        End Get
        Set(ByVal value As Short)
            FgState.Fs.Selector = value
            FgValid = False
        End Set
    End Property

    Public Property FsAttributes As Short
        Get
            Return FgState.Fs.Attributes
        End Get
        Set(ByVal value As Short)
            FgState.Fs.Attributes = value
            FgValid = False
        End Set
    End Property

    Public Property FsLimit As Integer
        Get
            Return FgState.Fs.Limit
        End Get
        Set(ByVal value As Integer)
            FgState.Fs.Limit = value
            FgValid = False
        End Set
    End Property

    Public Property FsBase As Long
        Get
            Return FgState.Fs.Base
        End Get
        Set(ByVal value As Long)
            FgState.Fs.Base = value
            FgValid = False
        End Set
    End Property

    Public Property GsSelector As Short
        Get
            Return FgState.Gs.Selector
        End Get
        Set(ByVal value As Short)
            FgState.Gs.Selector = value
            FgValid = False
        End Set
    End Property

    Public Property GsAttributes As Short
        Get
            Return FgState.Gs.Attributes
        End Get
        Set(ByVal value As Short)
            FgState.Gs.Attributes = value
            FgValid = False
        End Set
    End Property

    Public Property GsLimit As Integer
        Get
            Return FgState.Gs.Limit
        End Get
        Set(ByVal value As Integer)
            FgState.Gs.Limit = value
            FgValid = False
        End Set
    End Property

    Public Property GsBase As Long
        Get
            Return FgState.Gs.Base
        End Get
        Set(ByVal value As Long)
            FgState.Gs.Base = value
            FgValid = False
        End Set
    End Property

    Public Property TrSelector As Short
        Get
            Return LtState.Tr.Selector
        End Get
        Set(ByVal value As Short)
            LtState.Tr.Selector = value
            LtValid = False
        End Set
    End Property

    Public Property TrAttributes As Short
        Get
            Return LtState.Tr.Attributes
        End Get
        Set(ByVal value As Short)
            LtState.Tr.Attributes = value
            LtValid = False
        End Set
    End Property

    Public Property TrLimit As Integer
        Get
            Return LtState.Tr.Limit
        End Get
        Set(ByVal value As Integer)
            LtState.Tr.Limit = value
            LtValid = False
        End Set
    End Property

    Public Property LdtrSelector As Short
        Get
            Return LtState.Ldtr.Selector
        End Get
        Set(ByVal value As Short)
            LtState.Ldtr.Selector = value
            LtValid = False
        End Set
    End Property

    Public Property LdtrAttributes As Short
        Get
            Return LtState.Ldtr.Attributes
        End Get
        Set(ByVal value As Short)
            LtState.Ldtr.Attributes = value
            LtValid = False
        End Set
    End Property

    Public Property LdtrLimit As Integer
        Get
            Return LtState.Ldtr.Limit
        End Get
        Set(ByVal value As Integer)
            LtState.Ldtr.Limit = value
            LtValid = False
        End Set
    End Property

    Public Property LdtrBase As Long
        Get
            Return LtState.Ldtr.Base
        End Get
        Set(ByVal value As Long)
            LtState.Ldtr.Base = value
            LtValid = False
        End Set
    End Property

    Public Property TrBase As Long
        Get
            Return LtState.Tr.Base
        End Get
        Set(ByVal value As Long)
            LtState.Tr.Base = value
            LtValid = False
        End Set
    End Property

    Public Property GdtrLimit As Integer
        Get
            Return DtState.Gdtr.Limit
        End Get
        Set(ByVal value As Integer)
            DtState.Gdtr.Limit = value
            DtValid = False
        End Set
    End Property

    Public Property GdtrBase As Long
        Get
            Return DtState.Gdtr.Base
        End Get
        Set(ByVal value As Long)
            DtState.Gdtr.Base = value
            DtValid = False
        End Set
    End Property

    Public Property IdtrLimit As Integer
        Get
            Return DtState.Idtr.Limit
        End Get
        Set(ByVal value As Integer)
            DtState.Idtr.Limit = value
            DtValid = False
        End Set
    End Property

    Public Property IdtrBase As Long
        Get
            Return DtState.Idtr.Base
        End Get
        Set(ByVal value As Long)
            DtState.Idtr.Base = value
            DtValid = False
        End Set
    End Property

    Public Property Efer As Long
        ' The EFER register is a synchronizing property.
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmEferRegister, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmEferRegister, 8)
        End Set
    End Property

    Public Property Pat As Long
        ' The PAT register is a synchronizing property.
        Get
            SendViewRequest(NoirCvmRegisterType.NoirCvmPatRegister, 8)
            Return Marshal.ReadInt64(RegisterBuffer, 24)
        End Get
        Set(ByVal value As Long)
            Marshal.WriteInt64(RegisterBuffer, 16, value)
            SendEditRequest(NoirCvmRegisterType.NoirCvmPatRegister, 8)
        End Set
    End Property

    Public Property Star As Long
        Get
            Return ScState.Star
        End Get
        Set(ByVal value As Long)
            ScState.Star = value
            ScValid = False
        End Set
    End Property

    Public Property LStar As Long
        Get
            Return ScState.LStar
        End Get
        Set(ByVal value As Long)
            ScState.LStar = value
            ScValid = False
        End Set
    End Property

    Public Property CStar As Long
        Get
            Return ScState.CStar
        End Get
        Set(ByVal value As Long)
            ScState.CStar = value
            ScValid = False
        End Set
    End Property

    Public Property SfMask As Long
        Get
            Return ScState.SfMask
        End Get
        Set(ByVal value As Long)
            ScState.SfMask = value
            ScValid = False
        End Set
    End Property

    Public Property KernelGsBase As Long
        Get
            Return FgState.KernelGsBase
        End Get
        Set(ByVal value As Long)
            FgState.KernelGsBase = value
            FgValid = False
        End Set
    End Property

    Public Property InterceptCpuid As Boolean
        Get
            Return CBool((VpOptions And &H1) = &H1)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H1
            Else
                VpOptions = VpOptions And &HFFFFFFFE
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptMsr As Boolean
        Get
            Return CBool((VpOptions And &H2) = &H2)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H2
            Else
                VpOptions = VpOptions And &HFFFFFFFD
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptInterruptWindow As Boolean
        Get
            Return CBool((VpOptions And &H4) = &H4)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H4
            Else
                VpOptions = VpOptions And &HFFFFFFFB
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptExceptions As Boolean
        Get
            Return CBool((VpOptions And &H8) = &H8)
        End Get
        Set(ByVal value As Boolean)

            If value Then
                VpOptions = VpOptions Or &H8
            Else
                VpOptions = VpOptions And &HFFFFFFF7
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptCr3 As Boolean
        Get
            Return CBool((VpOptions And &H10) = &H10)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H10
            Else
                VpOptions = VpOptions And &HFFFFFFEF
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptDrx As Boolean
        Get
            Return CBool((VpOptions And &H20) = &H20)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H20
            Else
                VpOptions = VpOptions And &HFFFFFFDF
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptPause As Boolean
        Get
            Return CBool((VpOptions Or &H40) = &H40)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H40
            Else
                VpOptions = VpOptions And &HFFFFFFBF
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptNmiWindow As Boolean
        Get
            Return CBool((VpOptions Or &H80) = &H80)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H80
            Else
                VpOptions = VpOptions And &HFFFFFF7F
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property InterceptRsm As Boolean
        Get
            Return CBool((VpOptions Or &H100) = &H100)
        End Get
        Set(ByVal value As Boolean)
            If value Then
                VpOptions = VpOptions Or &H100
            Else
                VpOptions = VpOptions And &HFFFFFEFF
            End If
            VpOptValid = False
        End Set
    End Property

    Public Property ExceptionBitmap As Integer
        Get
            Return ExceptionInterceptMap
        End Get
        Set(ByVal value As Integer)
            ExceptionInterceptMap = value
            Dim InBuff(2) As Long
            InBuff(0) = VirtualMachine.VmHandle
            InBuff(1) = CLng(VirtualProcessorIndex)
            InBuff(2) = (CLng(ExceptionInterceptMap) << 32) Or &H1
            Dim Status As Integer, ReturnedLength As Integer
            Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmSetVcpuOptions, InBuff(0), 24, Status, 4, ReturnedLength, IntPtr.Zero)
            If Result Then
                NoirThrowByStatus(Status)
            Else
                Throw New NoirVisorCommunicationException("Failed to set vCPU options! Win32 Error Code:" & Str(Err.LastDllError))
            End If
        End Set
    End Property

    Public Sub SynchronizeFrom()
        PullGeneralPurposeRegisters()
        PullControlRegisters()
        PullDebugRegisters()
        PullSegmentRegisters()
    End Sub

    Public Sub SynchronizeTo()
        If GprValid = False Then
            Marshal.StructureToPtr(GprState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmGeneralPurposeRegister, &H80)
            GprValid = True
        End If
        If CrValid = False Then
            Marshal.StructureToPtr(CrState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmControlRegister, &H18)
            CrValid = True
        End If
        If DrValid1 = False Then
            Marshal.StructureToPtr(DrState1, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmDebugRegister, &H20)
            DrValid1 = True
        End If
        If DrValid2 = False Then
            Marshal.StructureToPtr(DrState1, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmDr67Register, &H10)
            DrValid2 = True
        End If
        If SrValid = False Then
            Marshal.StructureToPtr(SrState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmSegmentRegister, &H40)
            SrValid = True
        End If
        If FgValid = False Then
            Marshal.StructureToPtr(FgState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmFsGsRegister, &H28)
            FgValid = True
        End If
        If LtValid = False Then
            Marshal.StructureToPtr(LtState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmTrLdtrRegister, &H20)
            LtValid = True
        End If
        If DtValid = False Then
            Marshal.StructureToPtr(DtState, RegisterBuffer + 16, False)
            SendEditRequest(NoirCvmRegisterType.NoirCvmDescriptorTable, &H20)
            LtValid = True
        End If
        If VpOptValid = False Then
            Dim InBuff(2) As Long
            InBuff(0) = VirtualMachine.VmHandle
            InBuff(1) = CLng(VirtualProcessorIndex)
            InBuff(2) = CLng(VpOptions) << 32
            Dim Status As Integer, ReturnedLength As Integer
            Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmSetVcpuOptions, InBuff(0), 24, Status, 4, ReturnedLength, IntPtr.Zero)
            If Result Then
                NoirThrowByStatus(Status)
            Else
                Throw New NoirVisorCommunicationException("Failed to set vCPU options! Win32 Error Code:" & Str(Err.LastDllError))
            End If
        End If
    End Sub

    Public Sub PullGeneralPurposeRegisters()
        SendViewRequest(NoirCvmRegisterType.NoirCvmGeneralPurposeRegister, &H80)
        GprState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(GeneralPurposeRegisterSet)), GeneralPurposeRegisterSet)
        GprValid = True
    End Sub

    Public Sub PullSegmentRegisters()
        SendViewRequest(NoirCvmRegisterType.NoirCvmSegmentRegister, &H40)
        SrState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(SegmentRegisterSet)), SegmentRegisterSet)
        SrValid = True
        SendViewRequest(NoirCvmRegisterType.NoirCvmFsGsRegister, &H28)
        FgState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(SegmentRegisterSet2)), SegmentRegisterSet2)
        FgValid = True
        SendViewRequest(NoirCvmRegisterType.NoirCvmTrLdtrRegister, &H20)
        LtState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(SegmentRegisterSet3)), SegmentRegisterSet3)
        LtValid = True
        SendViewRequest(NoirCvmRegisterType.NoirCvmDescriptorTable, &H20)
        DtState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(SegmentRegisterSet4)), SegmentRegisterSet4)
        DtValid = True
    End Sub

    Public Sub PullControlRegisters()
        SendViewRequest(NoirCvmRegisterType.NoirCvmControlRegister, &H18)
        CrState = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(ControlRegisterSet)), ControlRegisterSet)
        CrValid = True
    End Sub

    Public Sub PullDebugRegisters()
        SendViewRequest(NoirCvmRegisterType.NoirCvmDebugRegister, &H20)
        DrState1 = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(DebugRegisterSet1)), DebugRegisterSet1)
        DrValid1 = True
        SendViewRequest(NoirCvmRegisterType.NoirCvmDr67Register, &H10)
        DrState2 = CType(Marshal.PtrToStructure(RegisterBuffer + 24, GetType(DebugRegisterSet2)), DebugRegisterSet2)
        DrValid2 = True
    End Sub

    Public Sub PullExtendedState()
        Dim ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmViewVcpuReg, ExtendedState - 16, 16, ExtendedState - 8, &H1008, ReturnedLength, IntPtr.Zero)
        If Result Then
            Dim Status As Integer = Marshal.ReadInt32(ExtendedState, -8)
            NoirThrowByStatus(Status)
        Else
            Throw New NoirVisorCommunicationException("Failed to view extended states! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Public Sub PushExtendedState()
        Dim ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmEditVcpuReg, ExtendedState - 16, &H1010, RegisterBuffer + 16, 4, ReturnedLength, IntPtr.Zero)
        If Result Then
            Dim Status As Integer = Marshal.ReadInt32(RegisterBuffer, 16)
            NoirThrowByStatus(Status)
        Else
            Throw New NoirVisorCommunicationException("Failed to edit extended states! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Private Sub SendViewRequest(ByVal RegisterType As NoirCvmRegisterType, ByVal BufferSize As Integer)
        Dim ReturnedLength As Integer
        Marshal.WriteInt32(RegisterBuffer, 12, RegisterType)
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmViewVcpuReg, RegisterBuffer, 16, RegisterBuffer + 16, BufferSize + 8, ReturnedLength, IntPtr.Zero)
        If Result = False Then
            Throw New NoirVisorCommunicationException("Failed to view register! Win32 Error Code:" & Str(Err.LastDllError))
        Else
            Dim Status As Integer = Marshal.ReadInt32(RegisterBuffer, 16)
            NoirThrowByStatus(Status)
        End If
    End Sub

    Private Sub SendEditRequest(ByVal RegisterType As NoirCvmRegisterType, ByVal BufferSize As Integer)
        Dim ReturnedLength As Integer
        Marshal.WriteInt32(RegisterBuffer, 12, RegisterType)
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmEditVcpuReg, RegisterBuffer, BufferSize + 16, RegisterBuffer + BufferSize + 16, 4, ReturnedLength, IntPtr.Zero)
        If Result Then
            Dim Status As Integer = Marshal.ReadInt32(RegisterBuffer, BufferSize + 16)
            NoirThrowByStatus(Status)
        Else
            Throw New NoirVisorCommunicationException("Failed to edit register! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Public Sub InjectEvent(ByVal Valid As Boolean, ByVal Vector As Byte, ByVal Type As EventType, ByVal Priority As Byte, ByVal ErrorCodeValid As Boolean, ByVal ErrorCode As Integer)
        Dim InBuff(2) As Long
        InBuff(0) = VirtualMachine.VmHandle
        InBuff(1) = CLng(VirtualProcessorIndex)
        InBuff(2) = CLng(Vector) And &HFF
        InBuff(2) = InBuff(2) Or CLng((Type And &H7) << 8)
        InBuff(2) = InBuff(2) Or (CLng(ErrorCodeValid) << 11)
        InBuff(2) = InBuff(2) Or CLng((Priority And &HF) << 27)
        InBuff(2) = InBuff(2) Or (CLng(Valid) << 31)
        InBuff(2) = InBuff(2) Or (CLng(ErrorCode) << 32)
        Dim Status As Integer, ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmInjectEvent, InBuff(0), 24, Status, 4, ReturnedLength, IntPtr.Zero)
        If Result Then
            NoirThrowByStatus(Status)
        Else
            Throw New NoirVisorCommunicationException("Failed to inject event! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Public Sub Rescind()
        Dim InBuff(1) As Long
        InBuff(0) = VirtualMachine.VmHandle
        InBuff(1) = CLng(VirtualProcessorIndex)
        Dim ReturnedLength As Integer, Status As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmRescindVcpu, InBuff(0), 16, Status, 4, ReturnedLength, IntPtr.Zero)
        If Result Then
            NoirThrowByStatus(Status)
        Else
            Throw New NoirVisorCommunicationException("Failed to rescind vCPU! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Public Sub Run()
        Dim InBuff(1) As Long
        InBuff(0) = VirtualMachine.VmHandle
        InBuff(1) = CLng(VirtualProcessorIndex)
        Dim ReturnedLength As Integer
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmRunVcpu, InBuff(0), 16, ExitContextBuffer, &H100, ReturnedLength, IntPtr.Zero)
        If Result Then
            Dim Status As Integer = Marshal.ReadInt32(ExitContextBuffer)
            If Status = NoirSuccess Then
                ' Handle VM-Exits.
                Dim ContinueExecution = False
                Do
                    Dim InterceptCode As NoirCvmInterceptCode = CType(Marshal.ReadInt64(ExitContextBuffer, &H8), NoirCvmInterceptCode)
                    Select Case InterceptCode
                        Case NoirCvmInterceptCode.InvalidState
                            Throw New InvalidGuestStateException
                        Case NoirCvmInterceptCode.ShutdownCondition
                            Dim Context As New ExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = ShutdownHandler(Context)
                        Case NoirCvmInterceptCode.MemoryAccess
                            Dim Context As New MemoryAccessExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = MemoryAccessHandler(Context)
                        Case NoirCvmInterceptCode.RsmInstruction
                            Dim Context As New ExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = RsmHandler(Context)
                        Case NoirCvmInterceptCode.HltInstruction
                            Dim Context As New ExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = HaltHandler(Context)
                        Case NoirCvmInterceptCode.IoInstruction
                            Dim Context As New IoExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = IoHandler(Context)
                        Case NoirCvmInterceptCode.CpuidInstruction
                            Dim Context As New CpuidExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = CpuidHandler(Context)
                        Case NoirCvmInterceptCode.RdmsrInstruction
                            Dim Context As New MsrExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = RdmsrHandler(Context)
                        Case NoirCvmInterceptCode.WrmsrInstruction
                            Dim Context As New MsrExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = WrmsrHandler(Context)
                        Case NoirCvmInterceptCode.Hypercall
                            Dim Context As New ExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = HypercallHandler(Context)
                        Case NoirCvmInterceptCode.Exception
                            Dim Context As New ExceptionExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = ExceptionHandler(Context)
                        Case NoirCvmInterceptCode.Rescission
                            Dim Context As New ExitContext(Me, ExitContextBuffer + 8)
                            ContinueExecution = RescissionHandler(Context)
                        Case Else
                            Throw New UnhandledInterceptionException("Internal Code:" & Str(InterceptCode))
                    End Select
                    Result = DeviceIoControl(NvDriverHandle, IOCTL_CvmRunVcpu, InBuff(0), 16, ExitContextBuffer, &H100, ReturnedLength, IntPtr.Zero)
                    If Result = True Then
                        Status = Marshal.ReadInt32(ExitContextBuffer)
                        If Status <> NoirSuccess Then NoirThrowByStatus(Status)
                    Else
                        Throw New NoirVisorCommunicationException("Failed to run vCPU! Win32 Error Code:" & Str(Err.LastDllError))
                    End If
                Loop While ContinueExecution = True
            Else
                NoirThrowByStatus(Status)
            End If
        Else
            Throw New NoirVisorCommunicationException("Failed to run vCPU! Win32 Error Code:" & Str(Err.LastDllError))
        End If
    End Sub

    Public Sub New(ByVal SourceVirtualMachine As VirtualMachine, ByVal VpIndex As Integer)
        RegisterBuffer = VirtualAlloc(IntPtr.Zero, &H2000, MEM_COMMIT, PAGE_READWRITE)
        If RegisterBuffer = IntPtr.Zero Then Throw New OutOfMemoryException("Failed to allocate buffer for internal register management!")
        ExtendedState = RegisterBuffer + &H1000
        ExitContextBuffer = RegisterBuffer + &H800
        Dim InBuff(1) As Long
        Dim Status As Integer, ReturnedLength As Integer
        InBuff(0) = SourceVirtualMachine.VmHandle
        InBuff(1) = CLng(VpIndex)
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmCreateVcpu, InBuff(0), 16, Status, 4, ReturnedLength, IntPtr.Zero)
        If Result = True And Status = NoirSuccess Then
            VirtualMachine = SourceVirtualMachine
            VirtualProcessorIndex = VpIndex
            ' Initialize Register Buffer
            Marshal.WriteInt64(RegisterBuffer, 0, SourceVirtualMachine.VmHandle)
            Marshal.WriteInt32(RegisterBuffer, 8, VpIndex)
            Marshal.WriteInt64(ExtendedState, -16, SourceVirtualMachine.VmHandle)
            Marshal.WriteInt32(ExtendedState, -8, VpIndex)
            Marshal.WriteInt32(ExtendedState, -4, NoirCvmRegisterType.NoirCvmXsaveArea)
        Else
            VirtualFree(RegisterBuffer, 0, MEM_RELEASE)
            If Result Then
                NoirThrowByStatus(Status)
            Else
                Throw New NoirVisorCommunicationException("Failed to create vCPU! Win32 Error Code:" & Str(Err.LastDllError))
            End If
        End If
    End Sub

    Public Sub Dispose()
        Dim InBuff(1) As Long
        Dim Status As Integer, ReturnedLength As Integer
        InBuff(0) = VirtualMachine.VmHandle
        InBuff(1) = CLng(VirtualProcessorIndex)
        Dim Result As Boolean = DeviceIoControl(NvDriverHandle, IOCTL_CvmDeleteVcpu, InBuff(0), 16, Status, 4, ReturnedLength, IntPtr.Zero)
        If Result = False Then
            Throw New NoirVisorCommunicationException("Failed to delete vCPU! Win32 Error Code:" & Str(Err.LastDllError))
        Else
            NoirThrowByStatus(Status)
        End If
        VirtualFree(RegisterBuffer, 0, MEM_RELEASE)
    End Sub
End Class
