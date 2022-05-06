Imports NoirCvmApi
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Threading
Public Class Form1
    Private Const StdInPort As Integer = 0
    Private Const StdOutPort As Integer = 1
    Private Const StdErrPort As Integer = 2
    Private Const StdIoCtrlPort As Integer = 3
    Private Const StdIoIntVPort As Integer = 4
    Private Const PowerMgmtPort As Integer = 5
    Private Const PageBaseMask As Long = &HFFFFFFFFF000
    Private Const PageBaseMask2MB As Long = &HFFFFFFE00000
    Private Const PageBaseMask1GB As Long = &HFFFFC0000000
    Private Const PageOffsetMask As Long = &HFFF
    Private Const PageOffsetMask2MB As Long = &H1FFFFF
    Private Const PageOffsetMask1GB As Long = &H3FFFFFFF
    Dim ProgramPath As String = ""
    Dim PagingPath As String = ""
    Dim SegmentationPath As String = ""
    Dim SmRamPath As String = ""
    Dim MainRam As IntPtr = IntPtr.Zero
    Dim SmRam As IntPtr = IntPtr.Zero
    Dim VM As VirtualMachine
    Dim VP As VirtualProcessor
    Dim VpThread As Thread

    Private Function ExceptionCallback(ByRef Context As ExceptionExitContext) As Boolean
        MsgBox("Exception occured! Vector:" & Str(Context.Vector), vbExclamation)
        Return False
    End Function

    Private Function TranslateGva(ByVal Cr3 As Long, ByVal GVA As Long, ByVal RequestRead As Boolean, ByVal RequestWrite As Boolean, ByVal RequestExecute As Boolean, ByVal RequestUser As Boolean, ByRef FaultCode As Integer) As Long
        Dim Pml4Base As Long = Cr3 And PageBaseMask
        Dim Pml4Index As Long = (GVA >> 39) And &H1FF
        Dim Pml4Entry As Long = Marshal.ReadInt64(MainRam, CInt(Pml4Base + (Pml4Index << 3)))
        FaultCode = 0
        If RequestRead And ((Pml4Entry And &H1) = 0) Then FaultCode = FaultCode Or &H1
        If RequestWrite And ((Pml4Entry And &H2) = 0) Then FaultCode = FaultCode Or &H2
        If RequestUser And ((Pml4Entry And &H4) = 0) Then FaultCode = FaultCode Or &H4
        If RequestExecute And ((Pml4Entry And &H8000000000000000) = 0) Then FaultCode = FaultCode Or &H10
        If FaultCode Then
            Debug.Print("#PF occurs in 0x" & Hex(GVA) & " during GVA translation (PML4E Stage).")
            Return 0
        End If
        Dim PdptBase As Long = Pml4Entry And PageBaseMask
        Dim PdptIndex As Long = (GVA >> 30) And &H1FF
        Dim PdptEntry As Long = Marshal.ReadInt64(MainRam, CInt(PdptBase + (PdptIndex << 3)))
        If RequestRead And ((PdptEntry And &H1) = 0) Then FaultCode = FaultCode Or &H1
        If RequestWrite And ((PdptEntry And &H2) = 0) Then FaultCode = FaultCode Or &H2
        If RequestUser And ((PdptEntry And &H4) = 0) Then FaultCode = FaultCode Or &H4
        If RequestExecute And ((PdptEntry And &H8000000000000000) = 0) Then FaultCode = FaultCode Or &H10
        If FaultCode Then
            Debug.Print("#PF occurs in 0x" & Hex(GVA) & " during GVA translation (PDPTE Stage).")
            Return 0
        End If
        If PdptEntry And &H80 Then Return (PdptEntry And PageBaseMask1GB) Or (GVA And PageOffsetMask1GB)
        Dim PdBase As Long = PdptEntry And PageBaseMask
        Dim PdIndex As Long = (GVA >> 21) And &H1FF
        Dim PdEntry As Long = Marshal.ReadInt64(MainRam, CInt(PdBase + (PdIndex << 3)))
        If RequestRead And ((PdEntry And &H1) = 0) Then FaultCode = FaultCode Or &H1
        If RequestWrite And ((PdEntry And &H2) = 0) Then FaultCode = FaultCode Or &H2
        If RequestUser And ((PdEntry And &H4) = 0) Then FaultCode = FaultCode Or &H4
        If RequestExecute And ((PdEntry And &H8000000000000000) = 0) Then FaultCode = FaultCode Or &H10
        If FaultCode Then
            Debug.Print("#PF occurs in 0x" & Hex(GVA) & " during GVA translation (PDE Stage).")
            Return 0
        End If
        If PdEntry And &H80 Then Return (PdEntry And PageBaseMask2MB) Or (GVA And PageOffsetMask2MB)
        Dim PtBase As Long = PdEntry And PageBaseMask
        Dim PtIndex As Long = (GVA >> 12) And &H1FF
        Dim PtEntry As Long = Marshal.ReadInt64(MainRam, CInt(PtBase + (PtIndex << 3)))
        If RequestRead And ((PtEntry And &H1) = 0) Then FaultCode = FaultCode Or &H1
        If RequestWrite And ((PtEntry And &H2) = 0) Then FaultCode = FaultCode Or &H2
        If RequestUser And ((PtEntry And &H4) = 0) Then FaultCode = FaultCode Or &H4
        If RequestExecute And ((PtEntry And &H8000000000000000) = 0) Then FaultCode = FaultCode Or &H10
        If FaultCode Then
            Debug.Print("#PF occurs in 0x" & Hex(GVA) & " during GVA translation (PTE Stage).")
            Return 0
        End If
        Return (PtEntry And PageBaseMask) Or (GVA And PageOffsetMask)
    End Function

    Private Function IoCallback(ByRef Context As IoExitContext) As Boolean
        Select Case Context.Port
            Case StdOutPort
                If Context.StringInstruction Then
                    Dim FaultCode As Integer
                    Dim GPA As Long = TranslateGva(Context.VirtualProcessor.Cr3, Context.Rsi, True, False, False, Context.CPL = 3, FaultCode)
                    If FaultCode Then MsgBox("#PF occurs in this I/O! Error Code=0x" & Hex(FaultCode), vbExclamation)
                    If Context.RepeatInstruction Then
                        ' FIXME: Cross-page read.
                        Dim OutputString As String = Marshal.PtrToStringAnsi(MainRam + GPA, CInt(Context.Rcx))
                        MsgBox("Message sent to stdout by guest:" & vbCrLf & OutputString, vbInformation)
                    End If
                End If
            Case PowerMgmtPort
                If Not Context.RepeatInstruction And Not Context.StringInstruction Then
                    Dim Command As Byte = CByte(Context.Rax)
                    If Command And &H1 Then
                        MsgBox("Guest issued a shutdown command!", vbInformation)
                        Return False
                    End If
                End If
            Case Else
                MsgBox("Unknown I/O!", vbExclamation)
                Return False
        End Select
        Context.AdvanceRip()
        Return True
    End Function

    Private Sub InitializeVpState()
        ' General Purpose Registers
        VP.Rsp = &HFFFFF800001FFFF0
        VP.Rip = &HFFFF800000000000
        VP.Rflags = &H2L
        ' GDT and IDT Registers
        VP.GdtrLimit = &H2F
        VP.GdtrBase = &HFFFFF80000000000
        VP.IdtrLimit = &HFFF
        VP.IdtrBase = &HFFFFF80000001000
        ' Segment Register - CS
        VP.CsSelector = &H10
        VP.CsAttributes = &H209B
        VP.CsLimit = &HFFFFFFFF
        VP.CsBase = 0
        ' Segment Register - DS
        VP.DsSelector = &H18
        VP.DsAttributes = &HC093S
        VP.DsLimit = &HFFFFFFFF
        VP.DsBase = 0
        ' Segment Register - ES
        VP.EsSelector = &H18
        VP.EsAttributes = &HC093S
        VP.EsLimit = &HFFFFFFFF
        VP.EsBase = 0
        ' Segment Register - FS
        VP.FsSelector = &H18
        VP.FsAttributes = &HC093S
        VP.FsLimit = &HFFFFFFFF
        VP.FsBase = 0
        ' Segment Register - GS
        VP.GsSelector = &H18
        VP.GsAttributes = &HC093S
        VP.GsLimit = &HFFFFFFFF
        VP.GsBase = 0
        ' Segment Register - SS
        VP.SsSelector = &H18
        VP.SsAttributes = &HC093S
        VP.SsLimit = &HFFFFFFFF
        VP.SsBase = 0
        ' Task Register
        VP.TrSelector = &H20
        VP.TrAttributes = &H8B
        VP.TrLimit = &H67
        VP.TrBase = &HFFFFF80000000200L
        ' Local Descriptor Table Register
        VP.LdtrSelector = 0
        VP.LdtrAttributes = &H2
        VP.LdtrLimit = 0
        VP.LdtrBase = 0
        ' Model Specific Registers
        VP.Efer = &HD00L
        VP.Pat = &H7040600070406L
        ' Debug Registers
        VP.Dr6 = &HFFFF0FF0L
        VP.Dr7 = &H400L
        ' Control Registers
        VP.Cr0 = &H80050033L
        VP.Cr3 = &H400000L
        VP.Cr4 = &H406F8L
        VP.Cr8 = 0
        VP.Xcr0 = 1
        ' Extended State
        Marshal.WriteInt16(VP.ExtendedState, VirtualProcessor.FcwOffset, &H40)
        Marshal.WriteInt16(VP.ExtendedState, VirtualProcessor.MxCsrOffset, &H1F80)
        ' vCPU Options
        VP.ExceptionBitmap = &HFFFFFFFF
        VP.InterceptExceptions = True
        ' Interception callbacks...
        VP.ExceptionHandler = AddressOf ExceptionCallback
        VP.IoHandler = AddressOf IoCallback
        ' Synchronize to the vCPU scheduler.
        VP.SynchronizeTo()
        VP.PushExtendedState()
        VP.Run()
        VP.Dispose()
        VM.Dispose()
    End Sub

    Private Sub StartGuest()
        Dim ProgramFs As New FileStream(ProgramPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim PagingFs As New FileStream(PagingPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim SegmentFs As New FileStream(SegmentationPath, FileMode.Open, FileAccess.Read, FileShare.Read)
        Dim ProgramBuff(ProgramFs.Length - 1) As Byte
        Dim PagingBuff(PagingFs.Length - 1) As Byte
        Dim SegmentBuff(SegmentFs.Length - 1) As Byte
        ProgramFs.Read(ProgramBuff, 0, ProgramFs.Length)
        PagingFs.Read(PagingBuff, 0, PagingFs.Length)
        SegmentFs.Read(SegmentBuff, 0, SegmentFs.Length)
        MainRam = Miscellaneous.PageAlloc(&H800000)
        SmRam = Miscellaneous.PageAlloc(&H10000)
        Marshal.Copy(ProgramBuff, 0, MainRam, ProgramFs.Length)
        Marshal.Copy(PagingBuff, 0, MainRam + &H400000, PagingFs.Length)
        Marshal.Copy(SegmentBuff, 0, MainRam + &H600000, SegmentFs.Length)
        ProgramFs.Dispose()
        PagingFs.Dispose()
        SegmentFs.Dispose()
        Miscellaneous.PageLock(MainRam, &H800000)
        Miscellaneous.PageLock(SmRam, &H10000)
        VM = New VirtualMachine()
        VP = New VirtualProcessor(VM, 0)
        VM.SetAddressMapping(0, MainRam, 2048)
        InitializeVpState()

    End Sub

    Private Sub Form1_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        Miscellaneous.FinalizeLibrary()
    End Sub

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Miscellaneous.InitializeLibrary()
    End Sub

    Private Sub ToolStripMenuItem2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem2.Click
        OpenFileDialog1.Title = "Select a segmentation file..."
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SegmentationPath = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub ToolStripMenuItem3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem3.Click
        OpenFileDialog1.Title = "Select a paging file..."
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            PagingPath = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub ToolStripMenuItem4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem4.Click
        OpenFileDialog1.Title = "Select a program file..."
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            ProgramPath = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub ToolStripMenuItem5_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem5.Click
        OpenFileDialog1.Title = "Select an SMRAM file..."
        If OpenFileDialog1.ShowDialog() = Windows.Forms.DialogResult.OK Then
            SmRamPath = OpenFileDialog1.FileName
        End If
    End Sub

    Private Sub ToolStripMenuItem7_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ToolStripMenuItem7.Click
        StartGuest()
    End Sub
End Class
