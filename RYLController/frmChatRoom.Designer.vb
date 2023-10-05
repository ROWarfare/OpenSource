<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmChatRoom
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
        Me.txtMain = New System.Windows.Forms.RichTextBox
        Me.txtInput = New System.Windows.Forms.TextBox
        Me.lstUsers = New System.Windows.Forms.ListBox
        Me.SuspendLayout()
        '
        'txtMain
        '
        Me.txtMain.BackColor = System.Drawing.Color.Black
        Me.txtMain.ForeColor = System.Drawing.Color.White
        Me.txtMain.Location = New System.Drawing.Point(6, 7)
        Me.txtMain.Name = "txtMain"
        Me.txtMain.ReadOnly = True
        Me.txtMain.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical
        Me.txtMain.Size = New System.Drawing.Size(608, 394)
        Me.txtMain.TabIndex = 1
        Me.txtMain.Text = ""
        '
        'txtInput
        '
        Me.txtInput.BackColor = System.Drawing.Color.Black
        Me.txtInput.ForeColor = System.Drawing.Color.White
        Me.txtInput.Location = New System.Drawing.Point(6, 410)
        Me.txtInput.Name = "txtInput"
        Me.txtInput.Size = New System.Drawing.Size(733, 20)
        Me.txtInput.TabIndex = 0
        '
        'lstUsers
        '
        Me.lstUsers.BackColor = System.Drawing.Color.Black
        Me.lstUsers.ForeColor = System.Drawing.Color.White
        Me.lstUsers.FormattingEnabled = True
        Me.lstUsers.Location = New System.Drawing.Point(620, 7)
        Me.lstUsers.Name = "lstUsers"
        Me.lstUsers.Size = New System.Drawing.Size(115, 394)
        Me.lstUsers.TabIndex = 2
        '
        'frmChatRoom
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(743, 441)
        Me.Controls.Add(Me.lstUsers)
        Me.Controls.Add(Me.txtInput)
        Me.Controls.Add(Me.txtMain)
        Me.Name = "frmChatRoom"
        Me.Text = "frmChatRoom"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents txtMain As System.Windows.Forms.RichTextBox
    Friend WithEvents txtInput As System.Windows.Forms.TextBox
    Friend WithEvents lstUsers As System.Windows.Forms.ListBox
End Class
