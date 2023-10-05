Imports System.Windows.Forms

Public Class frmStartupCanceler
    Private done As Integer = 0
    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

    Private Sub tmrNo_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles tmrNo.Tick
        Me.DialogResult = System.Windows.Forms.DialogResult.Ignore
        Me.Close()
    End Sub

    Private Sub tmrProgress_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrProgress.Tick
        done += Me.tmrProgress.Interval
        If done > Me.tmrNo.Interval Then Exit Sub
        Me.prgDie.Value = Math.Round((done / Me.tmrNo.Interval) * 100, 1)
    End Sub
End Class
