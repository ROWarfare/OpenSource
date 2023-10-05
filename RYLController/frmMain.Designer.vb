<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmMain
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        If disposing AndAlso components IsNot Nothing Then
            components.Dispose()
        End If
        MyBase.Dispose(disposing)
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(frmMain))
        Me.flowStatus = New System.Windows.Forms.FlowLayoutPanel
        Me.chkRestart = New System.Windows.Forms.CheckBox
        Me.btnStart = New System.Windows.Forms.Button
        Me.btnShutdown = New System.Windows.Forms.Button
        Me.btnRestart = New System.Windows.Forms.Button
        Me.startupTimer = New System.Windows.Forms.Timer(Me.components)
        Me.chkStartMap = New System.Windows.Forms.CheckBox
        Me.chkKillProcess = New System.Windows.Forms.CheckBox
        Me.notifI = New System.Windows.Forms.NotifyIcon(Me.components)
        Me.cmNotify = New System.Windows.Forms.ContextMenuStrip(Me.components)
        Me.mnuShow = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator1 = New System.Windows.Forms.ToolStripSeparator
        Me.mnuStart = New System.Windows.Forms.ToolStripMenuItem
        Me.mnuStop = New System.Windows.Forms.ToolStripMenuItem
        Me.ToolStripSeparator2 = New System.Windows.Forms.ToolStripSeparator
        Me.mnuExit = New System.Windows.Forms.ToolStripMenuItem
        Me.tmrAddProcTest = New System.Windows.Forms.Timer(Me.components)
        Me.GroupBox1 = New System.Windows.Forms.GroupBox
        Me.flowMapOnline = New System.Windows.Forms.FlowLayoutPanel
        Me.lblLoginOnline = New System.Windows.Forms.Label
        Me.Label2 = New System.Windows.Forms.Label
        Me.Label1 = New System.Windows.Forms.Label
        Me.GroupBox2 = New System.Windows.Forms.GroupBox
        Me.lblCopyright = New System.Windows.Forms.LinkLabel
        Me.btnReCatch = New System.Windows.Forms.Button
        Me.chkDoNothing = New System.Windows.Forms.CheckBox
        Me.btnOpenIRC = New System.Windows.Forms.Button
        Me.tmrRunTime = New System.Windows.Forms.Timer(Me.components)
        Me.cmNotify.SuspendLayout()
        Me.GroupBox1.SuspendLayout()
        Me.SuspendLayout()
        '
        'flowStatus
        '
        Me.flowStatus.AutoScroll = True
        Me.flowStatus.Location = New System.Drawing.Point(18, 87)
        Me.flowStatus.Name = "flowStatus"
        Me.flowStatus.Size = New System.Drawing.Size(259, 363)
        Me.flowStatus.TabIndex = 0
        '
        'chkRestart
        '
        Me.chkRestart.AutoSize = True
        Me.chkRestart.Location = New System.Drawing.Point(291, 9)
        Me.chkRestart.Name = "chkRestart"
        Me.chkRestart.Size = New System.Drawing.Size(169, 17)
        Me.chkRestart.TabIndex = 1
        Me.chkRestart.Text = "Restart if someone goes down"
        Me.chkRestart.UseVisualStyleBackColor = True
        '
        'btnStart
        '
        Me.btnStart.Location = New System.Drawing.Point(17, 12)
        Me.btnStart.Name = "btnStart"
        Me.btnStart.Size = New System.Drawing.Size(85, 21)
        Me.btnStart.TabIndex = 2
        Me.btnStart.Text = "Startup"
        Me.btnStart.UseVisualStyleBackColor = True
        '
        'btnShutdown
        '
        Me.btnShutdown.Location = New System.Drawing.Point(108, 12)
        Me.btnShutdown.Name = "btnShutdown"
        Me.btnShutdown.Size = New System.Drawing.Size(85, 21)
        Me.btnShutdown.TabIndex = 3
        Me.btnShutdown.Text = "Shutdown"
        Me.btnShutdown.UseVisualStyleBackColor = True
        '
        'btnRestart
        '
        Me.btnRestart.Location = New System.Drawing.Point(200, 12)
        Me.btnRestart.Name = "btnRestart"
        Me.btnRestart.Size = New System.Drawing.Size(85, 21)
        Me.btnRestart.TabIndex = 4
        Me.btnRestart.Text = "Restart"
        Me.btnRestart.UseVisualStyleBackColor = True
        '
        'startupTimer
        '
        '
        'chkStartMap
        '
        Me.chkStartMap.AutoSize = True
        Me.chkStartMap.Location = New System.Drawing.Point(291, 32)
        Me.chkStartMap.Name = "chkStartMap"
        Me.chkStartMap.Size = New System.Drawing.Size(222, 30)
        Me.chkStartMap.TabIndex = 5
        Me.chkStartMap.Text = "If map server goes down, dont do restart, " & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "start only map serv again"
        Me.chkStartMap.UseVisualStyleBackColor = True
        '
        'chkKillProcess
        '
        Me.chkKillProcess.AutoSize = True
        Me.chkKillProcess.Location = New System.Drawing.Point(291, 68)
        Me.chkKillProcess.Name = "chkKillProcess"
        Me.chkKillProcess.Size = New System.Drawing.Size(121, 17)
        Me.chkKillProcess.TabIndex = 6
        Me.chkKillProcess.Text = "Use Process killing?"
        Me.chkKillProcess.UseVisualStyleBackColor = True
        '
        'notifI
        '
        Me.notifI.ContextMenuStrip = Me.cmNotify
        Me.notifI.Icon = CType(resources.GetObject("notifI.Icon"), System.Drawing.Icon)
        Me.notifI.Text = "RYLServerController"
        Me.notifI.Visible = True
        '
        'cmNotify
        '
        Me.cmNotify.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.mnuShow, Me.ToolStripSeparator1, Me.mnuStart, Me.mnuStop, Me.ToolStripSeparator2, Me.mnuExit})
        Me.cmNotify.Name = "cmNotify"
        Me.cmNotify.Size = New System.Drawing.Size(145, 104)
        '
        'mnuShow
        '
        Me.mnuShow.Name = "mnuShow"
        Me.mnuShow.Size = New System.Drawing.Size(144, 22)
        Me.mnuShow.Text = "&Show"
        '
        'ToolStripSeparator1
        '
        Me.ToolStripSeparator1.Name = "ToolStripSeparator1"
        Me.ToolStripSeparator1.Size = New System.Drawing.Size(141, 6)
        '
        'mnuStart
        '
        Me.mnuStart.Name = "mnuStart"
        Me.mnuStart.Size = New System.Drawing.Size(144, 22)
        Me.mnuStart.Text = "Start Server"
        '
        'mnuStop
        '
        Me.mnuStop.Name = "mnuStop"
        Me.mnuStop.Size = New System.Drawing.Size(144, 22)
        Me.mnuStop.Text = "Stop Server"
        '
        'ToolStripSeparator2
        '
        Me.ToolStripSeparator2.Name = "ToolStripSeparator2"
        Me.ToolStripSeparator2.Size = New System.Drawing.Size(141, 6)
        '
        'mnuExit
        '
        Me.mnuExit.Name = "mnuExit"
        Me.mnuExit.Size = New System.Drawing.Size(144, 22)
        Me.mnuExit.Text = "E&xit"
        '
        'tmrAddProcTest
        '
        Me.tmrAddProcTest.Enabled = True
        Me.tmrAddProcTest.Interval = 1000
        '
        'GroupBox1
        '
        Me.GroupBox1.Controls.Add(Me.flowMapOnline)
        Me.GroupBox1.Controls.Add(Me.lblLoginOnline)
        Me.GroupBox1.Controls.Add(Me.Label2)
        Me.GroupBox1.Controls.Add(Me.Label1)
        Me.GroupBox1.Location = New System.Drawing.Point(291, 140)
        Me.GroupBox1.Name = "GroupBox1"
        Me.GroupBox1.Size = New System.Drawing.Size(230, 315)
        Me.GroupBox1.TabIndex = 7
        Me.GroupBox1.TabStop = False
        Me.GroupBox1.Text = "Info"
        '
        'flowMapOnline
        '
        Me.flowMapOnline.Location = New System.Drawing.Point(10, 66)
        Me.flowMapOnline.Name = "flowMapOnline"
        Me.flowMapOnline.Size = New System.Drawing.Size(164, 180)
        Me.flowMapOnline.TabIndex = 9
        '
        'lblLoginOnline
        '
        Me.lblLoginOnline.AutoSize = True
        Me.lblLoginOnline.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(186, Byte))
        Me.lblLoginOnline.ForeColor = System.Drawing.Color.Red
        Me.lblLoginOnline.Location = New System.Drawing.Point(66, 27)
        Me.lblLoginOnline.Name = "lblLoginOnline"
        Me.lblLoginOnline.Size = New System.Drawing.Size(45, 13)
        Me.lblLoginOnline.TabIndex = 8
        Me.lblLoginOnline.Text = "Closed"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(6, 50)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(37, 13)
        Me.Label2.TabIndex = 1
        Me.Label2.Text = "Online"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(7, 27)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(39, 13)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Login: "
        '
        'GroupBox2
        '
        Me.GroupBox2.Location = New System.Drawing.Point(17, 68)
        Me.GroupBox2.Name = "GroupBox2"
        Me.GroupBox2.Size = New System.Drawing.Size(268, 387)
        Me.GroupBox2.TabIndex = 8
        Me.GroupBox2.TabStop = False
        Me.GroupBox2.Text = "Server status"
        '
        'lblCopyright
        '
        Me.lblCopyright.AutoSize = True
        Me.lblCopyright.Location = New System.Drawing.Point(442, 468)
        Me.lblCopyright.Name = "lblCopyright"
        Me.lblCopyright.Size = New System.Drawing.Size(79, 13)
        Me.lblCopyright.TabIndex = 9
        Me.lblCopyright.TabStop = True
        Me.lblCopyright.Text = "Made by AlphA"
        '
        'btnReCatch
        '
        Me.btnReCatch.Location = New System.Drawing.Point(17, 41)
        Me.btnReCatch.Name = "btnReCatch"
        Me.btnReCatch.Size = New System.Drawing.Size(85, 21)
        Me.btnReCatch.TabIndex = 10
        Me.btnReCatch.Text = "Re-Catch"
        Me.btnReCatch.UseVisualStyleBackColor = True
        '
        'chkDoNothing
        '
        Me.chkDoNothing.AutoSize = True
        Me.chkDoNothing.Location = New System.Drawing.Point(291, 91)
        Me.chkDoNothing.Name = "chkDoNothing"
        Me.chkDoNothing.Size = New System.Drawing.Size(193, 30)
        Me.chkDoNothing.TabIndex = 11
        Me.chkDoNothing.Text = "Dont shutdown if someone goes off" & Global.Microsoft.VisualBasic.ChrW(13) & Global.Microsoft.VisualBasic.ChrW(10) & "(ignore mode)"
        Me.chkDoNothing.UseVisualStyleBackColor = True
        '
        'btnOpenIRC
        '
        Me.btnOpenIRC.Location = New System.Drawing.Point(108, 41)
        Me.btnOpenIRC.Name = "btnOpenIRC"
        Me.btnOpenIRC.Size = New System.Drawing.Size(85, 21)
        Me.btnOpenIRC.TabIndex = 12
        Me.btnOpenIRC.Text = "Open IRC"
        Me.btnOpenIRC.UseVisualStyleBackColor = True
        '
        'tmrRunTime
        '
        Me.tmrRunTime.Enabled = True
        Me.tmrRunTime.Interval = 1000
        '
        'frmMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(541, 486)
        Me.Controls.Add(Me.chkDoNothing)
        Me.Controls.Add(Me.btnOpenIRC)
        Me.Controls.Add(Me.lblCopyright)
        Me.Controls.Add(Me.btnReCatch)
        Me.Controls.Add(Me.GroupBox1)
        Me.Controls.Add(Me.chkKillProcess)
        Me.Controls.Add(Me.btnShutdown)
        Me.Controls.Add(Me.btnRestart)
        Me.Controls.Add(Me.chkStartMap)
        Me.Controls.Add(Me.flowStatus)
        Me.Controls.Add(Me.GroupBox2)
        Me.Controls.Add(Me.chkRestart)
        Me.Controls.Add(Me.btnStart)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.Name = "frmMain"
        Me.Text = "RYLServerController"
        Me.cmNotify.ResumeLayout(False)
        Me.GroupBox1.ResumeLayout(False)
        Me.GroupBox1.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents flowStatus As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents chkRestart As System.Windows.Forms.CheckBox
    Friend WithEvents btnStart As System.Windows.Forms.Button
    Friend WithEvents btnShutdown As System.Windows.Forms.Button
    Friend WithEvents btnRestart As System.Windows.Forms.Button
    Friend WithEvents chkStartMap As System.Windows.Forms.CheckBox
    Friend WithEvents chkKillProcess As System.Windows.Forms.CheckBox
    Friend WithEvents startupTimer As System.Windows.Forms.Timer
    Friend WithEvents notifI As System.Windows.Forms.NotifyIcon
    Friend WithEvents cmNotify As System.Windows.Forms.ContextMenuStrip
    Friend WithEvents mnuExit As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuShow As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents mnuStart As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator1 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents mnuStop As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ToolStripSeparator2 As System.Windows.Forms.ToolStripSeparator
    Friend WithEvents tmrAddProcTest As System.Windows.Forms.Timer
    Friend WithEvents GroupBox1 As System.Windows.Forms.GroupBox
    Friend WithEvents GroupBox2 As System.Windows.Forms.GroupBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents lblLoginOnline As System.Windows.Forms.Label
    Friend WithEvents flowMapOnline As System.Windows.Forms.FlowLayoutPanel
    Friend WithEvents lblCopyright As System.Windows.Forms.LinkLabel
    Friend WithEvents btnReCatch As System.Windows.Forms.Button
    Friend WithEvents chkDoNothing As System.Windows.Forms.CheckBox
    Friend WithEvents btnOpenIRC As System.Windows.Forms.Button
    Friend WithEvents tmrRunTime As System.Windows.Forms.Timer
End Class
