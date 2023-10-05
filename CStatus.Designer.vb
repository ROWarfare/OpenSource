<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class CStatus
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.lblName = New System.Windows.Forms.Label
        Me.lblStatus = New System.Windows.Forms.Label
        Me.lblOpenLog = New System.Windows.Forms.LinkLabel
        Me.lblCloseLog = New System.Windows.Forms.LinkLabel
        Me.SuspendLayout()
        '
        'lblName
        '
        Me.lblName.AutoSize = True
        Me.lblName.Location = New System.Drawing.Point(41, 0)
        Me.lblName.Name = "lblName"
        Me.lblName.Size = New System.Drawing.Size(0, 13)
        Me.lblName.TabIndex = 0
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 8.25!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.Location = New System.Drawing.Point(3, 0)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(19, 13)
        Me.lblStatus.TabIndex = 1
        Me.lblStatus.Text = "[-]"
        '
        'lblOpenLog
        '
        Me.lblOpenLog.AutoSize = True
        Me.lblOpenLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(186, Byte))
        Me.lblOpenLog.Location = New System.Drawing.Point(17, 13)
        Me.lblOpenLog.Name = "lblOpenLog"
        Me.lblOpenLog.Size = New System.Drawing.Size(48, 12)
        Me.lblOpenLog.TabIndex = 2
        Me.lblOpenLog.TabStop = True
        Me.lblOpenLog.Text = "[show log]"
        '
        'lblCloseLog
        '
        Me.lblCloseLog.AutoSize = True
        Me.lblCloseLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 6.75!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(186, Byte))
        Me.lblCloseLog.Location = New System.Drawing.Point(66, 13)
        Me.lblCloseLog.Name = "lblCloseLog"
        Me.lblCloseLog.Size = New System.Drawing.Size(42, 12)
        Me.lblCloseLog.TabIndex = 3
        Me.lblCloseLog.TabStop = True
        Me.lblCloseLog.Text = "[hide log]"
        '
        'CStatus
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.lblCloseLog)
        Me.Controls.Add(Me.lblOpenLog)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.lblName)
        Me.Name = "CStatus"
        Me.Size = New System.Drawing.Size(230, 26)
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Public WithEvents lblName As System.Windows.Forms.Label
    Friend WithEvents lblOpenLog As System.Windows.Forms.LinkLabel
    Friend WithEvents lblCloseLog As System.Windows.Forms.LinkLabel

End Class
