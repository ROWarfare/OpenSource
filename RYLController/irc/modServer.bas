Attribute VB_Name = "modServer"
Option Explicit

Public strFileName1 As String

Public Sub doConnectString()
    Dim i As Long
    For i = 0 To UBound(strInitiallySend)
        sendToIRC strInitiallySend(i)
    Next
End Sub

Public Sub sendToIRC(ByVal strData As String)
    If Form1.sckServer.State = sckConnected Then Form1.sckServer.SendData strData & vbCrLf
End Sub
