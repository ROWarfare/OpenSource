
Public Class ServerItem
    Public id As Integer = -1
    Public name As String = "[UNKNOWN]"
    Public processName As String = ""
    Public online As Boolean = False
    Public filename As String = ""
    Public arguments As String = ""
    Public proc As Process = Nothing
    Public label As CStatus = Nothing
    Public supportsDirectStart As Boolean = False
    Public problematicStartup As Boolean = False
    Public codeToOpenLog As Integer = 0
    Public hook As clsHook = Nothing
    Public mapServerZone As Integer = 0
End Class

Public Class CServerController
    Public items As ServerItem() = {}
    Public Sub New()

    End Sub
    Public Sub New(ByVal config As CConfigFile, Optional ByVal serverDir As String = "")
        Dim names As String() = config.getList("items", "Name_")
        Dim addrs As String() = config.getList("items", "Addr_")
        Dim args As String() = config.getList("items", "Args_")
        Dim codes As String() = config.getList("items", "Code_")
        Dim maps As String = config.ini.GetString("main", "MapServers", "")
        Dim prob As String = config.ini.GetString("main", "ProblematicStartup", "")
        Dim mapsA As String() = {}
        Dim mapsI As Integer() = {}
        Dim probA As String() = {}
        Dim probI As Integer() = {}

        If serverDir <> "" Then
            serverDir = serverDir.Replace("/", "\")
            If serverDir.Substring(serverDir.Length - 1, 1) <> "\" Then serverDir &= "\"
            My.Computer.FileSystem.CurrentDirectory = serverDir
        End If

        If maps.Length > 0 Then mapsA = maps.Split(",")
        If mapsA.Length > 0 Then
            For Each m As String In mapsA
                ReDim Preserve mapsI(UBound(mapsI) + 1)
                mapsI(UBound(mapsI)) = CInt(Trim(m))
            Next
        End If

        If prob.Length > 0 Then probA = prob.Split(",")
        If probA.Length > 0 Then
            For Each m As String In probA
                ReDim Preserve probI(UBound(probI) + 1)
                probI(UBound(probI)) = CInt(Trim(m))
            Next
        End If

        If addrs.Length >= names.Length Then
            For i As Integer = 0 To names.Length - 1
                Dim sItem As New ServerItem
                sItem.name = names(i).Replace("_", " ")
                sItem.filename = IIf(serverDir <> "", serverDir, "") & addrs(i)

                Dim splices As String() = sItem.filename.Split(New Char() {"\", "/"})
                If splices.Length > 0 Then
                    Dim name As String() = splices(UBound(splices)).Split(".")
                    If name.Length = 2 Then
                        sItem.processName = name(0)
                    End If
                End If

                sItem.id = i
                If UBound(args) >= i AndAlso args(i) <> "null" Then sItem.arguments = args(i)
                If UBound(codes) >= i AndAlso codes(i) <> "null" Then sItem.codeToOpenLog = Val(codes(i))
                If mapsI.Length > 0 AndAlso Array.IndexOf(mapsI, (i + 1)) >= 0 Then sItem.supportsDirectStart = True
                If probI.Length > 0 AndAlso Array.IndexOf(probI, (i + 1)) >= 0 Then sItem.problematicStartup = True
                If sItem.arguments <> "" Then
                    Dim a As String = sItem.arguments
                    If a.IndexOf("-z") >= 0 Then
                        Dim s As String() = apiHook.arrayRemoveEmpty(a.Split(" "))
                        Dim index As Integer = Array.IndexOf(s, "-z")
                        If index >= 0 AndAlso UBound(s) > index AndAlso Val(s(index + 1)) > 0 Then
                            sItem.mapServerZone = Val(s(index + 1))
                        End If
                    End If
                End If

                Me.add(sItem)
            Next
        End If
    End Sub
    Public Sub add(ByRef item As ServerItem)
        ReDim Preserve items(UBound(items) + 1)
        items(UBound(items)) = item
    End Sub
    Public Sub setStatus(ByRef proc As Process, ByVal online As Boolean)
        Dim i As Integer = IndexOf(proc)
        If i >= 0 Then
            items(i).online = online
        End If
    End Sub
    Public Function IndexOf(ByRef proc As Process) As Integer
        Dim i As Integer = 0
        For Each ite As ServerItem In items
            If ite.proc Is proc Then
                Return i
            End If
            i += 1
        Next
        Return -1
    End Function
    Public Function IndexOf(ByRef sItem As ServerItem) As Integer
        Dim i As Integer = 0
        For Each ite As ServerItem In items
            If ite Is sItem Then
                Return i
            End If
            i += 1
        Next
        Return -1
    End Function
    Public Function IndexOf(ByRef processName As String) As Integer
        Dim i As Integer = 0
        For Each ite As ServerItem In items
            If ite.processName = processName Then
                Return i
            End If
            i += 1
        Next
        Return -1
    End Function
    Public Enum Status
        ALL_ARE_ONLINE = 1
        ALL_ARE_OFFLINE = -1
        MIXED = 0
    End Enum
    Public ReadOnly Property OverAllStatus() As Status
        Get
            Dim cnt As Integer = 0
            For Each ite As ServerItem In items
                If ite.online Then cnt += 1
            Next
            If cnt = 0 Then
                Return Status.ALL_ARE_OFFLINE
            ElseIf cnt = items.Length Then
                Return Status.ALL_ARE_ONLINE
            Else
                Return Status.MIXED
            End If
        End Get
    End Property
End Class

Public Class apiHook
    Public Class ServerInfoStructure
        Public LoginAgent As Integer
        Public LoginTotal As Integer
        Public LoginHuman As Integer
        Public LoginAkkan As Integer
        Public LoginOpen As Boolean
        Public LoginMap() As Integer = {}
        Public LoginMapZones() As Integer = {}
    End Class
    Public Event ServInfoChanged(ByVal servInfo As ServerInfoStructure)
    Public Declare Function PostMessage Lib "user32" Alias "PostMessageA" (ByVal hWnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByRef lParam As Integer) As Integer
    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String) As Integer
    Private Declare Function FindWindow Lib "user32" Alias "FindWindowA" (ByVal Classname As String, ByVal WindowName As String) As IntPtr
    Private Declare Function ShowWindow Lib "user32" (ByVal hWnd As Integer, ByVal nCmdShow As Integer) As Integer
    Private Const WM_MOUSEMOVE = &H200
    Private Const WM_COMMAND = &H111
    Public Const WM_QUIT = &H12
    Private Const WM_SETTEXT = &HC
    Public serverInfo As New ServerInfoStructure

    Public Declare Function GetParent Lib "user32" (ByVal hwnd As IntPtr) As IntPtr
    Private Declare Function GetWindowThreadProcessId Lib "user32" (ByVal hwnd As IntPtr, ByRef lpdwProcessId As IntPtr) As IntPtr
    Private Declare Function GetWindow Lib "user32" (ByVal hwnd As IntPtr, ByVal wCmd As Integer) As IntPtr

    Public Declare Function GetWindowText Lib "user32.dll" Alias "GetWindowTextA" (ByVal hWnd As IntPtr, ByVal lpString As String, ByVal cch As Integer) As Integer
    Private Declare Function GetWindowTextLength Lib "user32" Alias "GetWindowTextLengthA" (ByVal hwnd As IntPtr) As Integer
    Private Declare Function GetClassName Lib "user32" Alias "GetClassNameA" (ByVal hwnd As IntPtr, ByVal lpClassName As String, ByVal nMaxCount As Integer) As Integer
    Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" (ByVal hwndParent As IntPtr, ByVal hwndChildAfter As IntPtr, ByVal lpszClass As String, ByVal lpszCaption As String) As IntPtr
    Private hooks As clsHook() = {}
    Private servItems As ServerItem() = {}
    Private Const GW_HWNDNEXT = 2

    Public Sub close(Optional ByRef servItem As ServerItem = Nothing)
        If servItem Is Nothing Then
            For Each hook As clsHook In hooks
                If Not hook Is Nothing AndAlso hook.isHooked Then
                    hook.RemoveHook()
                    hook.sItem.hook = Nothing
                    hook = Nothing
                End If
            Next
            serverInfo.LoginAgent = 0
            serverInfo.LoginAkkan = 0
            serverInfo.LoginHuman = 0
            serverInfo.LoginTotal = 0
            For i As Integer = 0 To serverInfo.LoginMapZones.Length - 1
                serverInfo.LoginMap(i) = 0
            Next
            Array.Resize(hooks, 0)
            Array.Resize(servItems, 0)
        Else
            Dim j As Integer = Array.IndexOf(servItems, servItem)
            Dim hooksNew As clsHook() = {}
            Dim servitemsNew As ServerItem() = {}
            If j >= 0 Then
                Select Case servItem.codeToOpenLog
                    Case &H87 'login 
                        serverInfo.LoginOpen = False
                    Case &H83 'UID
                    Case &H9C4B 'DBAgent
                        serverInfo.LoginAgent = 0
                        serverInfo.LoginAkkan = 0
                        serverInfo.LoginHuman = 0
                        serverInfo.LoginTotal = 0
                        For i As Integer = 0 To serverInfo.LoginMapZones.Length - 1
                            serverInfo.LoginMap(i) = 0
                        Next
                    Case &H6D 'auth
                        serverInfo.LoginOpen = False
                    Case &H6C 'chat
                    Case &H77 'game
                End Select
                hooks(j).RemoveHook()
                hooks(j).sItem.hook = Nothing
                hooks(j) = Nothing
            End If
            For Each sItem As ServerItem In servItems
                If Not sItem Is servItem Then
                    ReDim Preserve hooksNew(UBound(hooksNew) + 1)
                    ReDim Preserve servitemsNew(UBound(servitemsNew) + 1)
                    hooksNew(UBound(hooksNew)) = hooks(Array.IndexOf(servItems, sItem))
                    servitemsNew(UBound(servitemsNew)) = sItem
                End If
            Next
            servItems = servitemsNew
            hooks = hooksNew
        End If
        RaiseEvent ServInfoChanged(serverInfo)
    End Sub

    Public Function isHooked(ByVal sItem As ServerItem) As Boolean
        If Array.IndexOf(servItems, sItem) >= 0 Then Return True Else Return False
    End Function

    Public Shared Function getClassName(ByVal hwnd As IntPtr) As String
        Dim txt As String = New String(" ", 255)
        getClassName(hwnd, txt, txt.Length - 1)
        Return Trim(txt.Substring(0, txt.IndexOf(Chr(&H0))))
    End Function

    Public Shared Function InstanceToWnd(ByVal target_pid As IntPtr) As IntPtr()
        Dim test_hwnd As IntPtr, test_thread_id As IntPtr
        'Find the first window
        Dim hands() As IntPtr = {}
        test_hwnd = FindWindow(vbNullString, vbNullString)
        Do While test_hwnd <> 0
            Dim test_pid As IntPtr
            test_thread_id = GetWindowThreadProcessId(test_hwnd, test_pid)
            If test_thread_id = target_pid Then
                ReDim Preserve hands(UBound(hands) + 1)
                hands(UBound(hands)) = test_hwnd
            End If
            test_hwnd = GetWindow(test_hwnd, GW_HWNDNEXT)
        Loop
        Return hands
    End Function

    Public Shared Sub closeProg(ByVal thread As Integer)
        Dim hands() As IntPtr = InstanceToWnd(thread)
        Dim ok As Boolean = False
        For Each hand As IntPtr In hands
            If PostMessage(hand, WM_QUIT, &H0, &H0) Then
                ok = True
            End If
        Next
    End Sub

    Public Shared Sub showWindowEx(ByVal hwnd As Integer)
        Try
            ShowWindow(hwnd, &H1) 'show the win
        Catch ex As Exception

        End Try
    End Sub

    Public Shared Sub hideWindowEx(ByVal hwnd As Integer)
        Try
            ShowWindow(hwnd, &H0) 'hide the win
        Catch ex As Exception

        End Try
    End Sub

    Public Sub getOnLineUsers(ByRef sItem As ServerItem, ByVal thread As Integer, Optional ByVal clas As String = "RylConsoleWindow")
        '(0, &H87) 'Login
        '(1, &H83) 'UID
        '(2, &H9C4B) 'DBAgent
        '(3, &H6D) 'Auth
        '(4, &H6C) 'Chat
        '(5, &H77) 'Game server
        Dim hands() As IntPtr = InstanceToWnd(thread)
        If hands.Length > 0 AndAlso Array.IndexOf(servItems, sItem) < 0 Then
            Dim biggest As Integer = 0
            Dim hWndMain As Integer = 0
            If sItem.codeToOpenLog > 0 Then
                For Each hand As IntPtr In hands
                    PostMessage(hand, WM_COMMAND, sItem.codeToOpenLog, &H0) 'select from menu to open win
                    If hand.ToInt32 > biggest Then biggest = hand.ToInt32
                Next
                hWndMain = biggest
            End If
            If hWndMain <> 0 Then
                Dim eHook As New clsHook
                ReDim Preserve hooks(UBound(hooks) + 1)
                ReDim Preserve servItems(UBound(servItems) + 1)
                hooks(UBound(hooks)) = eHook
                servItems(UBound(servItems)) = sItem
                Dim t As New Timer
                t.Tag = New Object() {eHook, hWndMain, clas, thread, sItem}
                AddHandler t.Tick, AddressOf run1
                t.Interval = 100
                t.Start()
            End If
        End If
    End Sub

    Private Sub run1(ByVal sender As Object, ByVal e As EventArgs)
        sender.stop()
        Dim hwndEdit As Integer
        Dim hWndMain As Integer = sender.tag(1)
        Dim eHook As clsHook = sender.tag(0)
        Dim clas As String = sender.tag(2)
        Dim sitem As ServerItem = sender.tag(4)
        Dim hands() As IntPtr = InstanceToWnd(Val(sender.tag(3)))
        For Each hand As IntPtr In hands
            Dim cl As String = GetClassName(hand)
            If cl = clas Then
                hWndMain = hand
                Exit For
            End If
        Next
        hideWindowEx(hWndMain) 'hide the win
        hwndEdit = clsHook.GetChildHandle(vbNullString, "Edit", hWndMain)
        Dim hwndEdit2 As Integer = clsHook.GetChildHandle(vbNullString, "Edit", hWndMain, hwndEdit)
        'Debug.WriteLine(Hex(hWndMain) & " - " & Hex(hwndEdit) & " - " & Hex(hwndEdit2))
        If hwndEdit <> 0 Then
            eHook.TargethWnd = hwndEdit
            eHook.ownerHwnd = hWndMain
            eHook.hwndIn = hwndEdit2
            eHook.sItem = sitem
            eHook.threadId = Val(sender.tag(3))
            sitem.hook = eHook
            AddHandler eHook.textChanged, AddressOf gotEHook_mess
            AddHandler eHook.gotException, AddressOf serverException
            eHook.SetHook()
        End If
    End Sub
    Private Sub serverException(ByRef sender As clsHook)
        If Not sender.sItem Is Nothing Then
            If Not sender.sItem.proc Is Nothing Then
                sender.sItem.proc.Kill()
            Else
                MsgBox("Server " & sender.sItem.name & " got exception but no process mapped. Ignoring")
            End If
        Else
            MsgBox("Some server got excepton. Ignoring")
        End If
    End Sub

    'Login server:
    '
    '@ Clients Server Open 
    'G:00 Kron1x    (0x00000003)   85.146.48.175, Ver:1753    ALLOW_ALL  H:23/17
    'Conneted Agent : 1 / Total User : 40 

    'Auth server
    '@ Limit Players : 0
    '@ Client Open
    '
    'Client Version : 1602, CheckSum : 0x414CA637
    'PatchAddress :  127.0.0.1
    ' Human:    0, Akhan:    0, Total:    0

    ' DBAgent:
    'Ver:1602 / Checksum:0x3a7170cd / PattchAddress:127.0.0.1
    'Connected LoginServer  (      127.0.0.1: 12002)
    'Connected UIDServer    (      127.0.0.1: 12001)
    'Connected AuthServer   (      127.0.0.1:  2880) : Connected User(0)
    'Connected ChatServer   (      127.0.0.1:  2887)
    '(Z:08 C:00) GameServer(0x08000002:      127.0.0.1) CharNum:0
    '(Z:12 C:00) GameServer(0x0c000002:      127.0.0.1) CharNum:0
    '(Z:14 C:00) GameServer(0x0e000502:      127.0.0.1) CharNum:0
    'Session(USER:0/CHAR:0)
    'StoreData(0/0) / CharacterData(0/0)

    'Human:0/Akhan:0
    'Current Total User Count : 0

    Private Sub gotEHook_mess(ByVal sender As clsHook, ByVal mystring As String)
        Dim rE As Boolean = False
        Select Case sender.sItem.codeToOpenLog
            Case &H87 'login 
                'Debug.WriteLine("E: " & " - " & mystring)
                Dim ls As String() = arrayRemoveEmpty(mystring.Split(vbNewLine))
                If ls.Length > 1 AndAlso ls(0).IndexOf("@ Clients Server") = 0 Then 'login server ryl2
                    If ls(0).IndexOf("Open") > 0 AndAlso Not serverInfo.LoginOpen Then
                        serverInfo.LoginOpen = True
                        rE = True
                    ElseIf ls(0).IndexOf("Closed") > 0 AndAlso serverInfo.LoginOpen Then
                        serverInfo.LoginOpen = False
                        rE = True
                    End If
                    'If ls(1).IndexOf("  H:") > 6 Then '6 becose then we dont catch the server name if it starts with H:
                    '    Dim t As String() = Trim(ls(1).Substring(ls(1).IndexOf("  H:", 6) + "  H:".Length)).Split("/")
                    '    If t.Length = 2 Then
                    '        If serverInfo.LoginHuman <> Val(t(0)) Then rE = True
                    '        If serverInfo.LoginAkkan <> Val(t(1).Substring(1)) Then rE = True
                    '        serverInfo.LoginHuman = Val(t(0))
                    '        serverInfo.LoginAkkan = Val(t(1).Substring(1))
                    '    End If
                    'End If
                ElseIf ls.Length > 1 AndAlso ls(0).IndexOf("@ Clients Allow") = 0 Then 'login server ryl1
                    If ls(0).IndexOf("all") > 0 AndAlso Not serverInfo.LoginOpen Then
                        serverInfo.LoginOpen = True
                        rE = True
                    ElseIf (ls(0).IndexOf("Closed") > 0 OrElse ls(0).IndexOf("some") > 0) AndAlso serverInfo.LoginOpen Then
                        serverInfo.LoginOpen = False
                        rE = True
                    End If
                End If
                Dim la As String() = ls(UBound(ls)).Split("/")
                If la.Length > 1 Then
                    For Each l As String In la
                        Dim lw As String() = l.Split(":")
                        If lw.Length = 2 Then
                            lw(0) = Trim(lw(0))
                            lw(1) = Val(Trim(lw(1)))
                            Select Case lw(0)
                                Case "Conneted Agent"
                                    If serverInfo.LoginAgent <> lw(1) Then rE = True
                                    serverInfo.LoginAgent = lw(1)
                                    'Case "Total User"
                                    '    If serverInfo.LoginTotal <> lw(1) Then rE = True
                                    '    serverInfo.LoginTotal = lw(1)
                            End Select

                        End If
                    Next
                End If
            Case &H83 'UID
            Case &H6F 'DBAgent ryl1
                Dim ls As String() = arrayRemoveEmpty(mystring.Split(vbNewLine))
                Dim mapZones As Integer() = serverInfo.LoginMapZones
                Dim mapCount As Integer() = serverInfo.LoginMap
                Dim foundMaps As Integer() = {}
                For Each l As String In ls
                    '(Z:14 C:00) GameServer(0x0e000502:      127.0.0.1) CharNum:0
                    If l.IndexOf("GameServer") > 0 Then
                        Dim zone As Integer = Val(Trim(l.Substring(l.IndexOf("Z:") + 2, 2)))
                        Dim count As Integer = Val(Trim(l.Substring(l.IndexOf("Connected User :") + "Connected User :".Length)))
                        Dim index As Integer = Array.IndexOf(mapZones, zone)
                        ReDim Preserve foundMaps(UBound(foundMaps) + 1)
                        foundMaps(UBound(foundMaps)) = zone
                        If index < 0 Then
                            ReDim Preserve mapZones(UBound(mapZones) + 1)
                            ReDim Preserve mapCount(UBound(mapCount) + 1)
                            index = UBound(mapZones)
                            mapZones(index) = zone
                            serverInfo.LoginMapZones = mapZones
                            serverInfo.LoginMap = mapCount
                        End If
                        If serverInfo.LoginMap(index) <> count Then rE = True
                        serverInfo.LoginMap(index) = count
                    End If
                Next
                If foundMaps.Length < mapZones.Length Then
                    For i As Integer = 0 To mapZones.Length - 1
                        If Array.IndexOf(foundMaps, mapZones(i)) >= 0 Then
                        Else
                            mapCount(i) = 0
                        End If
                    Next
                    rE = True
                End If
            Case &H9C4B 'DBAgent ryl2
                Dim ls As String() = arrayRemoveEmpty(mystring.Split(vbNewLine))
                Dim mapZones As Integer() = serverInfo.LoginMapZones
                Dim mapCount As Integer() = serverInfo.LoginMap
                Dim foundMaps As Integer() = {}
                For Each l As String In ls
                    '(Z:14 C:00) GameServer(0x0e000502:      127.0.0.1) CharNum:0
                    If l.IndexOf("GameServer") > 0 Then
                        Dim zone As Integer = Val(Trim(l.Substring(l.IndexOf("Z:") + 2, 2)))
                        Dim count As Integer = Val(Trim(l.Substring(l.IndexOf("CharNum:") + "CharNum:".Length)))
                        Dim index As Integer = Array.IndexOf(mapZones, zone)
                        ReDim Preserve foundMaps(UBound(foundMaps) + 1)
                        foundMaps(UBound(foundMaps)) = zone
                        If index < 0 Then
                            ReDim Preserve mapZones(UBound(mapZones) + 1)
                            ReDim Preserve mapCount(UBound(mapCount) + 1)
                            index = UBound(mapZones)
                            mapZones(index) = zone
                            serverInfo.LoginMapZones = mapZones
                            serverInfo.LoginMap = mapCount
                        End If
                        If serverInfo.LoginMap(index) <> count Then rE = True
                        serverInfo.LoginMap(index) = count
                    End If
                Next
                If foundMaps.Length < mapZones.Length Then
                    'Dim nZ As Integer() = {}
                    'Dim nC As Integer() = {}
                    For i As Integer = 0 To mapZones.Length - 1
                        If Array.IndexOf(foundMaps, mapZones(i)) >= 0 Then
                            'ReDim Preserve nZ(UBound(nZ) + 1)
                            'ReDim Preserve nC(UBound(nC) + 1)
                            'nZ(UBound(nZ)) = mapZones(i)
                            'nC(UBound(nC)) = mapCount(i)
                        Else
                            mapCount(i) = 0
                        End If
                    Next
                    rE = True
                    'serverInfo.LoginMapZones = nZ
                    'serverInfo.LoginMap = nC
                End If
            Case &H6D 'auth
                Dim ls As String() = arrayRemoveEmpty(mystring.Split(vbNewLine))
                If ls.Length > 0 Then
                    Dim l As String = ls(UBound(ls))
                    If l.Length > 0 AndAlso l.IndexOf(",") > 0 Then
                        Dim la As String() = l.Split(",")
                        If la.Length = 3 Then
                            For Each s As String In la
                                If s.IndexOf(":") > 0 Then
                                    Dim sa As String() = s.Split(":")
                                    Dim k As String = Trim(sa(0))
                                    Dim v As Integer = Val(Trim(sa(1)))
                                    Select Case k
                                        Case "Human"
                                            If serverInfo.LoginHuman <> v Then rE = True
                                            serverInfo.LoginHuman = v
                                        Case "Akhan"
                                            If serverInfo.LoginAkkan <> v Then rE = True
                                            serverInfo.LoginAkkan = v
                                        Case "Total"
                                            If serverInfo.LoginTotal <> v Then rE = True
                                            serverInfo.LoginTotal = v
                                    End Select
                                End If
                            Next
                        End If
                    End If
                End If
            Case &H6C 'chat
            Case &H77 'game
        End Select
        If rE Then
            RaiseEvent ServInfoChanged(serverInfo)
        End If
    End Sub

    Public Shared Sub startLogin(ByVal name As String, ByVal code As Integer)
        Dim hwnd As IntPtr = Nothing
        hwnd = FindWindow(name, name)
        If hwnd.ToInt32 > 0 Then
            'Debug.WriteLine(hwnd.ToInt32 & " - 0x" & Hex(hwnd.ToInt32))
            PostMessage(hwnd, WM_COMMAND, code, &H0)
        End If
    End Sub

    Public Shared Function arrayRemoveEmpty(ByVal arr As String()) As String()
        Dim a As String() = {}
        Dim bad As String() = {vbNewLine, vbCr, vbLf}
        For Each s As String In arr
            For Each c As String In bad
                s = s.Replace(c, "")
            Next
            If Trim(s) <> "" AndAlso Not Array.IndexOf(bad, Trim(s)) >= 0 Then
                ReDim Preserve a(UBound(a) + 1)
                a(UBound(a)) = s
            End If
        Next
        Return a
    End Function
End Class

Public Class clsHook
    Private Declare Function FindWindowEx Lib "user32" Alias "FindWindowExA" ( _
    ByVal hWnd1 As Integer, _
    ByVal hWnd2 As Integer, _
    ByVal lpsz1 As String, _
    ByVal lpsz2 As String _
) As Integer
    Public Const numberOfWins As Integer = 4
    Private Declare Function SendMessage Lib "user32" Alias "SendMessageA" (ByVal hwnd As IntPtr, ByVal wMsg As Integer, ByVal wParam As Integer, ByVal lParam As String) As Integer
    Private Const WM_GETTEXT = &HD
    Private Const WM_GETTEXTLENGTH = &HE
    Private Const VK_RETURN = &HD
    Private Const WM_SETTEXT = &HC
    Private Const WM_CHAR = &H102
    Private WithEvents time As New Timer
    Private WithEvents closeTime As New Timer
    Private hwnd As Integer
    Public hwndIn As Integer
    Public ownerHwnd As Integer
    Private prevText As String = ""
    Public Event textChanged(ByVal sender As clsHook, ByVal text As String)
    Public Event gotException(ByRef sender As clsHook)
    Public sItem As ServerItem = Nothing
    Public threadId As Integer = 0

    Public Shared Function GetChildHandle(ByVal Caption As String, ByVal ClassName As String, ByVal TopLevelHandle As Integer, Optional ByVal prevWin As Integer = 0) As Integer
        'Returns the handle to a child window
        Dim i As Integer, ErrorNumber As Integer

        i = FindWindowEx(TopLevelHandle, prevWin, ClassName, Caption)
        If i = 0 Then
            ErrorNumber = Err.LastDllError
        End If
        GetChildHandle = i

    End Function

    Public Function SetHook() As Boolean
        time.Interval = 2000
        time.Start()
        time_Tick(time, New EventArgs)
    End Function

    Public Function RemoveHook() As Boolean
        time.Stop()
        closeTime.Stop()
    End Function

    Public Property TargethWnd() As Integer
        Get
            Return hwnd
        End Get
        Set(ByVal value As Integer)
            hwnd = value
        End Set
    End Property

    Private Sub time_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles time.Tick
        checkIsAlive()
        Try

            Dim txt As String = WindowText(hwnd)
            If txt <> prevText Then
                RaiseEvent textChanged(Me, txt)
                prevText = txt
            End If
        Catch ex As Exception

        End Try
    End Sub
    Public ReadOnly Property isHooked() As Boolean
        Get
            Return time.Enabled
        End Get
    End Property

    Public Shared Function WindowText(ByVal window_hwnd As Integer) As String
        Dim txtlen As Integer
        Dim txt As String

        WindowText = ""
        If window_hwnd = 0 Then Exit Function

        txtlen = SendMessage(window_hwnd, WM_GETTEXTLENGTH, 0, 0)
        If txtlen = 0 Then Exit Function

        txtlen = txtlen + 1
        txt = Space$(txtlen)
        txtlen = SendMessage(window_hwnd, WM_GETTEXT, txtlen, txt)
        Return (Left$(txt, txtlen))
    End Function

    Public Shared Sub SetWindowText(ByVal window_hwnd As Integer, ByVal text As String)
        If window_hwnd = 0 Then Exit Sub
        SendMessage(window_hwnd, WM_SETTEXT, 0, text)
        SendMessage(window_hwnd, WM_CHAR, VK_RETURN, 0)
    End Sub
    Public Sub SendLogCommand(ByVal text As String)
        SetWindowText(hwndIn, text)
    End Sub
    Public Sub checkIsAlive()
        If Me.threadId > 0 Then
            Dim hands() As IntPtr = apiHook.InstanceToWnd(Me.threadId)
            For Each hand As IntPtr In hands
                Dim cl As String = WindowText(hand).ToLower
                If cl.IndexOf("exception") >= 0 OrElse cl.IndexOf("encountered a problem") >= 0 Then
                    closeTime.Tag = hand
                    closeTime.Interval = 3000
                    closeTime.Start()
                    Exit For
                End If
            Next
            'If hands.Length > numberOfWins Then
            'End If
        End If
    End Sub

    Private Sub raiseGotException(ByVal sender As Object, ByVal e As System.EventArgs) Handles closeTime.Tick
        Try
            closeTime.Stop()
            Dim parent As Integer = apiHook.GetParent(sender.Tag)
            If parent > 0 Then apiHook.PostMessage(parent, apiHook.WM_QUIT, &H0, &H0)
            RaiseEvent gotException(Me)
        Catch ex As Exception
        End Try
    End Sub
End Class