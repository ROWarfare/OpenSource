Attribute VB_Name = "modInit"
Option Explicit

Public strServers() As String
Public strInitiallySend() As String
Public strDCCMOTD() As String
Public strCTCP() As ctcpCouple

Public confStatus As confStatusEnum
Public Enum confStatusEnum
    statusAutoOp
    statusAutoVoice
    statusChans
    statusCTCP
    statusDCCMOTD
    statusInitiallySend
    statusNoStatus
    statusServers
    statusUsers
End Enum

Public Type ctcpCouple
    name As String
    reply As String
End Type

Public Sub loadConf()
    Dim strBuff As String, strBuff2 As String: confStatus = statusNoStatus
    ReDim strServers(0): ReDim strInitiallySend(0): ReDim strDCCMOTD(0)
    ReDim status(0): ReDim strCTCP(0): ReDim users(0)
    ReDim chans(0): ReDim chans(0).autoOpUsers(0): ReDim chans(0).autoVoiceUsers(0)
    Dim strEquals As String, i As Long
    Open App.Path & "\bot.conf" For Input As #1
        Do Until EOF(1)
            DoEvents
            Line Input #1, strBuff
            If Not strBuff = "" Then
                Select Case strBuff
                    Case "Servers {":
                        confStatus = statusServers
                    Case "InitiallySend {":
                        confStatus = statusInitiallySend
                    Case "DCCMOTD {":
                        confStatus = statusDCCMOTD
                    Case "CTCP {":
                        confStatus = statusCTCP
                    Case "Users {":
                        confStatus = statusUsers
                    Case "AutoOp {":
                        confStatus = statusAutoOp
                    Case "Channels {":
                        confStatus = statusChans
                    Case "}":
                        confStatus = statusNoStatus
                    Case Else:
                        If Left(strBuff, 4) = "set " Then
                            strEquals = Mid(strBuff, InStr(strBuff, " = ") + 3)
                            With info
                                Select Case Mid(strBuff, 5, InStr(strBuff, " = ") - 5)
                                    Case "nickname":
                                        .nickname = strEquals
                                    Case "altnick":
                                        .altnick = strEquals
                                    Case "connectToIRC":
                                        .connectToIRC = CBool(strEquals)
                                    Case "dccPass":
                                        .dccPassword = strEquals
                                    Case "quitMessage":
                                        .quitMessage = strEquals
                                    Case "attemptedCPS":
                                        .attemptedCPS = CLng(strEquals)
                                End Select
                            End With
                        Else
                            If confStatus = statusServers Then
                                strServers(UBound(strServers)) = Replace(strBuff, vbTab, "")
                                ReDim Preserve strServers(UBound(strServers) + 1)
                            ElseIf confStatus = statusInitiallySend Then
                                strInitiallySend(UBound(strInitiallySend)) = Replace(strBuff, vbTab, "")
                                ReDim Preserve strInitiallySend(UBound(strInitiallySend) + 1)
                            ElseIf confStatus = statusDCCMOTD Then
                                strDCCMOTD(UBound(strDCCMOTD)) = Replace(strBuff, vbTab, "", , 1)
                                ReDim Preserve strDCCMOTD(UBound(strDCCMOTD) + 1)
                            ElseIf confStatus = statusChans Then
                                chans(UBound(chans)).name = Replace(strBuff, vbTab, "", , 1)
                                ReDim Preserve chans(UBound(chans) + 1)
                            ElseIf confStatus = statusUsers Then
                                With users(UBound(users))
                                    strBuff2 = Replace(strBuff, vbTab, "", , 1)
                                    .nickname = Left(strBuff2, InStr(strBuff2, ":") - 1)
                                    .password = Mid(strBuff2, InStr(strBuff2, ":") + 1)
                                End With
                                ReDim Preserve users(UBound(users) + 1)
                            ElseIf confStatus = statusCTCP Then
                                With strCTCP(UBound(strCTCP))
                                    strBuff2 = Replace(strBuff, vbTab, "", , 1)
                                    .name = Left(strBuff2, InStr(strBuff2, ":") - 1)
                                    .reply = Mid(strBuff2, InStr(strBuff2, ":") + 1)
                                End With
                                ReDim Preserve strCTCP(UBound(strCTCP) + 1)
                            ElseIf confStatus = statusAutoOp Then
                                For i = 0 To UBound(chans)
                                    strBuff = Replace(strBuff, vbTab, "")
                                    If (Left(strBuff, InStr(strBuff, ":") - 1) = chans(i).name) Then
                                        GoTo foundChan
                                    End If
                                Next
                                i = -1
foundChan:
                                If Not i = -1 Then
                                    With chans(i)
                                        strBuff2 = Replace(strBuff, vbTab, "", , 1)
                                        .name = Left(strBuff2, InStr(strBuff2, ":") - 1)
                                        .autoOpUsers(UBound(.autoOpUsers)) = Mid(strBuff2, InStr(strBuff2, ":") + 1)
                                    End With
                                End If
                            ElseIf confStatus = statusAutoVoice Then
                                For i = 0 To UBound(chans)
                                    If (Left(strBuff2, InStr(strBuff2, ":") - 1) = chans(i).name) Then
                                        GoTo foundChan2
                                    End If
                                Next
                                i = -1
foundChan2:
                                If Not i = -1 Then
                                    With chans(i)
                                        strBuff2 = Replace(strBuff, vbTab, "", , 1)
                                        .name = Left(strBuff2, InStr(strBuff2, ":") - 1)
                                        .autoVoiceUsers(UBound(.autoVoiceUsers)) = Mid(strBuff2, InStr(strBuff2, ":") + 1)
                                    End With
                                End If
                            End If
                        End If
                End Select
            End If
        Loop
    Close #1
End Sub
