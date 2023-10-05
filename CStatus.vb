Public Class CStatus
    Private online As Boolean = False
    Public servItem As ServerItem
    Public Delegate Sub CallRecheck()
    Public Event ShowLog(ByRef sender As CStatus, ByRef sItem As ServerItem)
    Public Event HideLog(ByRef sender As CStatus, ByRef sItem As ServerItem)

    Public Property MyStatus() As Boolean
        Get
            Return online
        End Get
        Set(ByVal value As Boolean)
            online = value
            If online Then
                Me.lblStatus.Text = "[ON]"
                Me.lblStatus.ForeColor = Color.Green
                Me.lblCloseLog.Enabled = True
                Me.lblOpenLog.Enabled = True
            Else
                Me.lblStatus.Text = "[OFF]"
                Me.lblStatus.ForeColor = Color.Red
                Me.lblCloseLog.Enabled = False
                Me.lblOpenLog.Enabled = False
            End If
        End Set
    End Property

    Public Sub Recheck()
        Me.Invoke(New CallRecheck(AddressOf setOnline))
    End Sub

    Private Sub setOnline()
        MyStatus = servItem.online
    End Sub

    Private Sub lblOpenLog_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblOpenLog.LinkClicked
        RaiseEvent ShowLog(Me, servItem)
    End Sub

    Private Sub lblCloseLog_LinkClicked(ByVal sender As Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblCloseLog.LinkClicked
        RaiseEvent HideLog(Me, servItem)
    End Sub
End Class
