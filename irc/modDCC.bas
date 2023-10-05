Attribute VB_Name = "modDCC"
Option Explicit

Public fileSoFar As Long
Public fileSize As Long

Public Sub dccSendMOTD(ByRef sock As Winsock)
    Dim i As Long: sock.SendData vbCrLf
    For i = 0 To UBound(strDCCMOTD)
        sock.SendData strDCCMOTD(i) & vbCrLf
    Next
End Sub

Public Sub dccHandleData(ByRef sock As Winsock, ByVal bytesTotal As Long, ByVal Index As Integer)
    Dim sockBuff As String
    If sock.State = sckConnected Then sock.GetData sockBuff, vbString
    If status(Index) = DCCCHatWaitPass Then
        If Replace(sockBuff, vbLf, "") = info.dccPassword Then
            dccSendMOTD sock
        End If
    ElseIf status(Index) = DCCReceive Then
        If sock.State = sckConnected Then
            fileSoFar = fileSoFar + bytesTotal
            Put #1, , sockBuff
            If fileSoFar = fileSize Then
                Close #1
                sock.Close
            End If
        End If
    ElseIf status(Index) = DCCCHat Then
        sendToIRC "PRIVMSG plenderj :" & sockBuff
    End If
End Sub

Public Sub dccSendFileToUser(ByVal strFilePath As String, ByVal strNickName As String)
    Dim listenPort As Long
    With Form1
        If .sckServer.State = sckConnected Then
            sendToIRC "NOTICE " & strNickName & " :DCC Send " & _
                        returnNameOfFile(strFilePath) & " (" & .sckServer.LocalIP & ")"
            listenPort = Int(64512 * Rnd + 1024)
            Load .sckDCC(.sckDCC.UBound + 1)
            sendToIRC "PRIVMSG " & strNickName & " :DCC SEND " & _
                                returnNameOfFile(strFilePath) & " " & myLongIp() & _
                                " " & listenPort & " " & FileLen(strFilePath) & ""
            ReDim Preserve status(.sckDCC.UBound)
            status(.sckDCC.UBound) = DCCSend
            With .sckDCC(.sckDCC.UBound)
                .LocalPort = listenPort
                .Listen
                strFileName1 = strFilePath
            End With
        End If
    End With
End Sub


