VERSION 5.00
Object = "{248DD890-BB45-11CF-9ABC-0080C7E7B78D}#1.0#0"; "MSWINSCK.OCX"
Begin VB.Form Form1 
   BorderStyle     =   5  'Sizable ToolWindow
   Caption         =   "Bot"
   ClientHeight    =   435
   ClientLeft      =   60
   ClientTop       =   300
   ClientWidth     =   1560
   LinkTopic       =   "Form1"
   MaxButton       =   0   'False
   MinButton       =   0   'False
   ScaleHeight     =   435
   ScaleWidth      =   1560
   ShowInTaskbar   =   0   'False
   StartUpPosition =   3  'Windows Default
   Begin MSWinsockLib.Winsock sckDCC 
      Index           =   0
      Left            =   750
      Top             =   0
      _ExtentX        =   741
      _ExtentY        =   741
      _Version        =   393216
   End
   Begin MSWinsockLib.Winsock sckServer 
      Left            =   390
      Top             =   0
      _ExtentX        =   741
      _ExtentY        =   741
      _Version        =   393216
   End
End
Attribute VB_Name = "Form1"
Attribute VB_GlobalNameSpace = False
Attribute VB_Creatable = False
Attribute VB_PredeclaredId = True
Attribute VB_Exposed = False
Option Explicit

'' IRC Bot
'' Jamie Plenderleith
'' plenderj@tcd.ie
'' http://www.coolground.com/plenderj
''


Private Sub Form_Load()
    CenterMe Me: loadConf
    If info.connectToIRC Then
        sckServer.Connect Split(strServers(0), ":")(0), Split(strServers(0), ":")(1)
    End If
End Sub

Private Sub sckDCC_Connect(Index As Integer)
    If status(Index) = DCCCHat Then
        sckDCC(Index).SendData "Password : " & vbCrLf
        status(Index) = DCCCHatWaitPass
    ElseIf status(Index) = DCCCHatWaitPass Then
        dccSendMOTD sckDCC(Index)
    End If
End Sub

Private Sub sckDCC_ConnectionRequest(Index As Integer, ByVal requestID As Long)
    sckDCC(Index).Close
    sckDCC(Index).Accept requestID
    Do Until sckDCC(Index).State = sckConnected
        DoEvents
    Loop
    If status(Index) = DCCSend Then
        Dim strBuff As String, attemptCPS As Long
        attemptCPS = info.attemptedCPS
        Open strFileName1 For Binary As #1
            Do Until Loc(1) = LOF(1)
                DoEvents
                strBuff = Input(attemptCPS, 1)
                If sckDCC(Index).State = sckConnected Then
                    sckDCC(Index).SendData strBuff
                End If
                DoEvents
            Loop
        Close #1
    End If
End Sub

Private Sub sckDCC_DataArrival(Index As Integer, ByVal bytesTotal As Long)
    dccHandleData sckDCC(Index), bytesTotal, Index
End Sub

Private Sub sckServer_Connect()
    sendToIRC "USER a a a a" & vbCrLf & "NICK " & info.nickname
End Sub

Private Sub sckServer_DataArrival(ByVal bytesTotal As Long)
    Dim sockBuff As String, y() As String, z() As String, host As String
    Dim numeric As Long, i As Long, j As Long, k As Long, didFind As Boolean
    sckServer.GetData sockBuff, vbString
    If doDebug Then Debug.Print sockBuff
    If InStr(sockBuff, "376 " & info.nickname) <> 0 Then doConnectString
    If InStr(sockBuff, "PRIVMSG") <> 0 Then
        Select Case True
            Case InStr(sockBuff, "DCC CHAT") <> 0:
                y = Split(Mid(sockBuff, InStr(sockBuff, " :DCC ")), " ")
                status(0) = DCCCHat
                sckDCC(sckDCC.UBound).Connect ircGetIP(y(UBound(y) - 1)), Replace(y(UBound(y)), "", "")
            Case InStr(sockBuff, ":DCC SEND") <> 0:
                y = Split(Mid(sockBuff, InStr(sockBuff, ":DCC SEND")), " ")
                status(0) = DCCReceive
                fileSize = CLng(Replace(y(5), "", ""))
                Open "c:\" & y(2) For Binary As #1
                sckDCC(sckDCC.UBound).Connect longIP2Dotted(y(3)), y(4)
            Case InStr(sockBuff, "PRIVMSG " & info.nickname & " :op") <> 0:
                sockBuff = Replace(sockBuff, vbCrLf, "")
                y = Split(Mid(sockBuff, 2), "!")
                z = Split(Mid(sockBuff, InStr(sockBuff, info.nickname & " :op ") + Len(info.nickname & " :op ")), " ")
                For i = 0 To UBound(chans)
                    For j = 0 To UBound(chans(i).autoOpUsers)
                        If chans(i).autoOpUsers(j) = y(0) Then
                            GoTo foundUser
                        End If
                    Next
                Next
                j = -1
foundUser:
                If Not j = -1 Then
                    For k = 0 To UBound(users)
                        If users(k).nickname = chans(i).autoOpUsers(j) Then
                            If users(k).password = z(1) Then
                                '' superfluous ?
                                If z(0) = chans(i).name Then
                                    sendToIRC "MODE " & chans(i).name & " +o " & chans(i).autoOpUsers(j)
                                End If
                            End If
                        End If
                    Next
                End If
            Case InStr(sockBuff, "PRIVMSG " & info.nickname & " :voice") <> 0:
                sockBuff = Replace(sockBuff, vbCrLf, "")
                y = Split(Mid(sockBuff, 2), "!")
                z = Split(Mid(sockBuff, InStr(sockBuff, info.nickname & " :voice ") + Len(info.nickname & " :voice ")), " ")
                For i = 0 To UBound(chans)
                    For j = 0 To UBound(chans(i).autoVoiceUsers)
                        If chans(i).autoVoiceUsers(j) = y(0) Then
                            GoTo foundUser2
                        End If
                    Next
                Next
                j = -1
foundUser2:
                If Not j = -1 Then
                    For k = 0 To UBound(users)
                        If users(k).nickname = chans(i).autoVoiceUsers(j) Then
                            If users(k).password = z(1) Then
                                '' superfluous ?
                                If z(0) = chans(i).name Then
                                    sendToIRC "MODE " & chans(i).name & " +v " & chans(i).autoVoiceUsers(j)
                                End If
                            End If
                        End If
                    Next
                End If
            Case Else:
                If InStrRev(sockBuff, "") > InStr(sockBuff, "") Then
                    For i = 0 To UBound(strCTCP) - 1
                        If InStr(LCase(sockBuff), "" & LCase(strCTCP(i).name) & "") <> 0 Then
                            didFind = True
                            y = Split(Mid(sockBuff, 2), "!")
                            sendToIRC "NOTICE " & y(0) & " :" & strCTCP(i).name & " " & strCTCP(i).reply & ""
                        End If
                    Next
                    If Not didFind Then
                        If InStr(sockBuff, ":VERSION") <> 0 Then
                            y = Split(Mid(sockBuff, 2), "!")
                            sendToIRC "NOTICE " & y(0) & " :VERSION some bot or something"
                        ElseIf InStr(sockBuff, ":TIME") <> 0 Then
                            y = Split(Mid(sockBuff, 2), "!")
                            sendToIRC "NOTICE " & y(0) & " :TIME " & Format(Now, "ddd mmm dd") & " " & Time & " " & Format(Now, "yyyy") & ""
                        ElseIf InStr(sockBuff, ":PING") <> 0 Then
                            y = Split(Mid(sockBuff, 2), "!")
                            sendToIRC "NOTICE " & y(0) & " " & Mid(sockBuff, InStr(sockBuff, ":PING"))
                        End If
                    End If
                End If
        End Select
    Else
        If Len(sockBuff) > 6 Then
            If Left(sockBuff, 6) = "PING :" Then
                sendToIRC "PONG :" & Mid(sockBuff, InStr(sockBuff, "PING :") + 6)
            End If
        End If
    End If
End Sub

Private Sub sckServer_Error(ByVal Number As Integer, Description As String, ByVal Scode As Long, ByVal Source As String, ByVal HelpFile As String, ByVal HelpContext As Long, CancelDisplay As Boolean)
    If Number = 10061 Then
        MsgBox "Invalid port or server ..." & vbCrLf & _
                sckServer.RemoteHost & ":" & sckServer.RemotePort, vbCritical Or vbOKOnly
        End
    Else
        sckServer.Connect
    End If
End Sub
