Attribute VB_Name = "modMisc"
Option Explicit


Public Const doDebug As Boolean = False


Public status() As statusEnum
Public Enum statusEnum
    ServerSocket
    DCCReceive
    DCCSend
    DCCCHat
    DCCCHatWaitPass
    DCCChatSendMOTD
    DCCListenForChat
End Enum

Public info As botInfo
Public Type botInfo
    nickname As String
    altnick As String
    connectToIRC As Boolean
    dccPassword As String
    quitMessage As String
    attemptedCPS As Long
End Type

Public users() As userInfo
Public Type userInfo
    nickname As String
    password As String
End Type

Public chans() As chanInfo
Public Type chanInfo
    name As String
    autoOpUsers() As String
    autoVoiceUsers() As String
End Type

Public Sub CenterMe(frmForm As Form)
    frmForm.Left = (Screen.Width - frmForm.Width) / 2
    frmForm.Top = (Screen.Height - frmForm.Height) / 2
End Sub

Public Function longIP2Dotted(ByVal LongIP As String) As String
    Dim i As Long, num As Currency
    For i = 1 To 4
        num = Int(LongIP / 256 ^ (4 - i))
        LongIP = LongIP - (num * 256 ^ (4 - i))
        If num > 255 Then Err.Raise vbObjectError + 1
        If i = 1 Then
            longIP2Dotted = num
        Else
            longIP2Dotted = longIP2Dotted & "." & num
        End If
    Next
End Function

Public Function ircGetIP(ByVal IPL As String) As String
    Dim lpStr As Long, nStr As Long
    Dim retString As String, inn As String
    If Val(IPL) > 2147483647 Then inn = Val(IPL) - 4294967296# Else inn = Val(IPL)
    inn = ntohl(inn): retString = String(32, 0): lpStr = inet_ntoa(inn)
    If lpStr = 0 Then ircGetIP = "0.0.0.0": Exit Function
    nStr = lstrlen(lpStr): If nStr > 32 Then nStr = 32
    MemCopy ByVal retString, ByVal lpStr, nStr
    retString = Left(retString, nStr): ircGetIP = retString
End Function

Public Function myLongIp() As Long
    Dim retVal As Currency, myIPSegments() As String, i As Long
    myIPSegments = Split(Form1.sckServer.LocalIP, ".")
    myIPSegments = invertArray(myIPSegments)
    For i = 0 To 3
        retVal = retVal + (myIPSegments(i) * (256 ^ i))
    Next
    myLongIp = CLng(retVal)
End Function

Public Function invertArray(ByRef arrayName() As String) As String()
    Dim i As Long, tempArr() As String: ReDim tempArr(UBound(arrayName))
    For i = 0 To UBound(arrayName)
        tempArr(UBound(arrayName) - i) = arrayName(i)
    Next
    invertArray = tempArr
End Function

Public Function doCharCodes(ByVal strToDo As String) As String
    Dim i As Long, toReturn As String
    For i = 1 To Len(strToDo)
        toReturn = toReturn & Asc(Mid(strToDo, i, 1)) & ","
    Next i
    doCharCodes = Left(toReturn, Len(toReturn) - 1)
End Function

Public Function returnNameOfFile(ByVal strFile As String) As String
    returnNameOfFile = Mid(strFile, InStrRev(strFile, "\") + 1)
End Function

