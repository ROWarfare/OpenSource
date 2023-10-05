Imports System.Net.Sockets
Public Class ircC
    Public Event serverSays(ByVal sender As ircC, ByVal message As String)
    Public Event connected(ByVal sender As ircC)
    Public Event disConnected(ByVal sender As ircC)
    Public Server As String = ""
    Public Port As Integer = 0
    Private tcp As New Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP)
    Private recBuff(1024) As Byte
    Private conn As Boolean = False
    'Private WithEvents tmrStatus As Timer
    Public Sub New()
    End Sub
    Public Sub New(ByVal aServer As String, ByVal aPort As Integer)
        Server = aServer
        Port = aPort
    End Sub
    Public Sub Connect()
        'tmrStatus = New Timer(New System.ComponentModel.Container())
        'AddHandler tmrStatus.Tick, AddressOf checkConnectionState
        'tmrStatus.Interval = 1000
        tcp.BeginConnect(Server, Port, New AsyncCallback(AddressOf OnConnect), New Object())
    End Sub
    Public Sub Close()
        ' tmrStatus.Stop()
        If tcp.Connected Then tcp.Disconnect(False)
    End Sub

    Private Sub OnConnect(ByVal ar As IAsyncResult)
        'If ar.CompletedSynchronously Then
        conn = True
        'tmrStatus.Start()
        tcp.BeginReceive(recBuff, 0, recBuff.Length, SocketFlags.None, New AsyncCallback(AddressOf DataReceived), New Object())
        RaiseEvent connected(Me)
        'Else
        'Debug.WriteLine("Couldn't connect")
        'End If
    End Sub

    Private Sub checkConnectionState(ByVal sender As Object, ByVal e As EventArgs) ' Handles tmrStatus.Tick
        If Not tcp.Connected Then
            conn = False
            RaiseEvent disConnected(Me)
        End If
    End Sub

    Private Sub DataReceived(ByVal ar As IAsyncResult)
        Dim str As String = ""
        Dim line As String = ""
        Dim notEnd As Boolean = False
        Dim i As Integer = 0
        For Each b As Byte In recBuff
            If b > 0 Then
                str &= Chr(b)
                recBuff(i) = 0
            End If
            i += 1
        Next
        str = Trim(str)
        If str.Length > 0 AndAlso str.Substring(str.Length - 1, 1) <> vbLf Then notEnd = True
        If str.Length > 0 Then
            Dim lines As String() = str.Split(vbLf)
            i = 0
            For Each line In lines
                'line = Trim(line)
                If line.Length > 0 AndAlso (Not notEnd OrElse i <> lines.Length - 1) Then
                    line = Trim(line.Substring(0, line.Length - 1))
                    If line.Length > 0 Then RaiseEvent serverSays(Me, line)
                End If
                i += 1
            Next
        Else
            conn = False
            Me.tcp.Disconnect(False)
            RaiseEvent disConnected(Me)
            Exit Sub
        End If
        If tcp.Connected Then
            If notEnd Then
                For i = 0 To line.Length - 1
                    recBuff(i) = Asc(line.Substring(i, 1))
                Next
                tcp.BeginReceive(recBuff, line.Length, recBuff.Length - line.Length, SocketFlags.None, New AsyncCallback(AddressOf DataReceived), New Object())
            Else
                tcp.BeginReceive(recBuff, 0, recBuff.Length, SocketFlags.None, New AsyncCallback(AddressOf DataReceived), New Object())
            End If
        Else
            conn = False
            RaiseEvent disConnected(Me)
        End If
    End Sub

    Public Sub Say(ByVal text As String)
        If conn Then
            If text.Substring(text.Length - 2, 2) <> vbNewLine Then text &= vbNewLine
            Dim chars() As Char = text.ToCharArray
            Dim sayBuff(UBound(chars)) As Byte
            Dim i As Integer = 0
            For Each c As Char In chars
                sayBuff(i) = Asc(c)
                i += 1
            Next
            'tcp.BeginSend(sayBuff, 0, sayBuff.Length, SocketFlags.None, New AsyncCallback(AddressOf SendComplete), New Object())
            tcp.Send(sayBuff, sayBuff.Length, SocketFlags.None)
        End If
    End Sub

    Private Sub SendComplete(ByVal ar As IAsyncResult)

    End Sub
End Class

Public Class ircClient
    Public Event connected(ByVal sender As ircClient)
    Public Event channelJoinOk(ByVal sender As ircClient)
    Public Event channelPartOk(ByVal sender As ircClient, ByVal message As String)
    Public Event UserJoinCh(ByVal sender As ircClient, ByVal who As String)
    Public Event UserPartCh(ByVal sender As ircClient, ByVal who As String, ByVal message As String)
    'Public Event ircQuitOk(ByVal sender As ircClient, ByVal message As String)
    Public Event GotGlobalMess(ByVal sender As ircClient, ByVal who As String, ByVal whom As String, ByVal message As String)
    Public Event GotPM(ByVal sender As ircClient, ByVal who As String, ByVal message As String)
    Public Event ChangedNick(ByVal sender As ircClient, ByVal who As String, ByVal towho As String)
    Public Event GotNotice(ByVal sender As ircClient, ByVal who As String, ByVal message As String)
    Public Event GotMessage(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal message As String)
    Public Event GotAction(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal message As String)
    Public Event GotKicked(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal reason As String)
    Public Event GotChUsers(ByVal sender As ircClient, ByVal channel As String, ByVal users As String())
    Public Event GotRawMessage(ByVal sender As ircClient, ByVal txt As String)
    Public WithEvents irc As New ircC()
    Public nick As String = ""
    Public ChUsers As ArrayList
    Public LastUserList As String() = {}
    Public ChUsersChannel As String = ""
    Public iServer As String = ""
    Public iPort As Integer = 0
    'Public joinedChannels() As String = {}
    Public activeChannel As String = ""
    Sub New(ByVal server As String, ByVal port As Integer, ByVal aNick As String)
        irc.Port = port
        irc.Server = server
        nick = aNick
        iServer = server
        iPort = port
        irc.Connect()
    End Sub

    Private Sub irc_connected(ByVal sender As ircC) Handles irc.connected
        irc.Say("USER rylserverc 1 * : " & nick & " RYLServerController")
        ChangeNick(nick)
    End Sub

    Private Sub irc_disConnected(ByVal sender As ircC) Handles irc.disConnected
        irc = New ircC()
        irc.Port = iPort
        irc.Server = iServer
        irc.Connect()
    End Sub

    Private Sub irc_serverSays(ByVal sender As ircC, ByVal message As String) Handles irc.serverSays
        Debug.WriteLine(message & "<END>")
        RaiseEvent GotRawMessage(Me, message)
        If message.IndexOf("PING :") = 0 Then
            irc.Say("PONG :" & message.Substring("PING :".Length))
        ElseIf message.IndexOf("NOTICE AUTH :") = 0 Then
            'ignore
        Else
            Dim spl As String() = message.Split(" ")
            If UBound(spl) > 1 Then
                Dim who As String = spl(0)
                Dim what As String = spl(1)
                Dim whom As String = spl(2)
                Dim msg As String = message.Substring(who.Length + what.Length + whom.Length + 2)
                If msg <> "" Then msg = msg.Substring(1)
                If msg <> "" AndAlso msg.Substring(0, 1) = ":" Then msg = msg.Substring(1)
                'use only nick this time...
                who = who.Split("!")(0).Substring(1)
                If what = "376" Then 'all ok
                    RaiseEvent connected(Me)
                ElseIf what = "433" Then 'nick is already in use
                    If Val(nick.Substring(nick.Length - 1, 1)) > 0 Then
                        nick = nick.Substring(0, nick.Length - 1) & (Val(nick.Substring(nick.Length - 1, 1)) + 1)
                    Else
                        nick &= 1
                    End If
                    ChangeNick(nick)
                ElseIf what = "353" Then 'nicks list
                    If whom = nick Then
                        If ChUsers Is Nothing Then ChUsers = New ArrayList
                        Dim us As String() = Trim(msg).Split(" ")
                        Dim ch As String = us(1)
                        If ChUsersChannel = "" OrElse ChUsersChannel = ch Then
                            ChUsersChannel = ch
                            If UBound(us) > 1 Then us(2) = us(2).Substring(1)
                            For j As Integer = 2 To us.Length - 1
                                Dim name As String = us(j)
                                If j = 1 Then name = name.Substring(1)
                                ChUsers.Add(name)
                            Next
                        Else
                            ChUsersChannel = "ERR"
                        End If
                    End If
                ElseIf what = "366" Then 'end of nick list
                    If ChUsersChannel <> "ERR" AndAlso Not ChUsers Is Nothing Then
                        Dim users(ChUsers.Count - 1) As String
                        Dim users2(ChUsers.Count - 1) As String
                        For i As Integer = 0 To ChUsers.Count - 1
                            users(i) = ChUsers.Item(i)
                            users2(i) = ChUsers.Item(i)
                        Next
                        activeChannel = ChUsersChannel
                        LastUserList = users2
                        RaiseEvent GotChUsers(Me, ChUsersChannel, users)
                    End If
                    ChUsersChannel = ""
                    ChUsers = Nothing
                ElseIf what = "461" Then
                    RaiseEvent GotGlobalMess(Me, who, whom, msg)
                ElseIf what = "NOTICE" Then
                    RaiseEvent GotNotice(Me, who, msg)
                ElseIf what = "PRIVMSG" Then
                    If whom = nick Then
                        RaiseEvent GotPM(Me, who, msg)
                    Else
                        If Asc(msg(0)) = &H1 AndAlso msg.Substring(1, 7) = "ACTION " Then
                            RaiseEvent GotAction(Me, whom, who, msg.Substring(0, msg.Length - 1).Substring(8))
                        Else
                            RaiseEvent GotMessage(Me, whom, who, msg)
                        End If
                    End If
                ElseIf what = "KICK" AndAlso msg.Split(" ")(0) = nick Then
                    Dim reason As String = msg.Substring(msg.Split(" ")(0).Length + 2)
                    RaiseEvent GotKicked(Me, whom, who, reason)
                ElseIf what = "JOIN" OrElse what = "PART" OrElse what = "MODE" OrElse what = "KICK" Then
                    irc.Say("NAMES " & whom)
                    If who = nick Then
                        If what = "JOIN" Then
                            RaiseEvent channelJoinOk(Me)
                        ElseIf what = "PART" Then
                            RaiseEvent channelPartOk(Me, msg)
                        End If
                    Else
                        If what = "JOIN" Then
                            RaiseEvent UserJoinCh(Me, who)
                        ElseIf what = "PART" Then
                            RaiseEvent UserPartCh(Me, who, msg)
                        End If
                    End If
                ElseIf what = "NICK" Then
                    irc.Say("NAMES " & activeChannel)
                    RaiseEvent ChangedNick(Me, who, whom.Substring(1))
                ElseIf what = "QUIT" Then
                    irc.Say("NAMES " & activeChannel)
                End If
            End If
        End If
    End Sub

    Public Sub SendMessage(ByVal channel As String, ByVal text As String)
        irc.Say("PRIVMSG " & channel & " :" & text)
    End Sub

    Public Sub SendPM(ByVal name As String, ByVal text As String)
        irc.Say("PRIVMSG " & name & " :" & text)
    End Sub

    Public Sub SendNotice(ByVal name As String, ByVal text As String)
        If name.ToLower.IndexOf("stealthy") >= 0 Then
            Me.SendPM(name, text)
        Else
            irc.Say("NOTICE " & name & " :" & text)
        End If
    End Sub

    Public Sub JoinChannel(ByVal channel)
        irc.Say("JOIN " & channel)
    End Sub

    Public Sub LeaveChannel(ByVal channel)
        irc.Say("LEAVE " & channel)
    End Sub

    Public Sub ChangeNick(ByVal name As String)
        irc.Say("NICK " & name)
    End Sub

    Public Sub close(Optional ByVal msg As String = "")
        If msg <> "" Then irc.Say("QUIT :" & msg) Else irc.Say("QUIT")
        irc.Close()
    End Sub

    Public Sub pureSay(ByVal txt As String)
        irc.Say(txt)
    End Sub
End Class

Public Class ControllerBot
    Public WithEvents irc As ircClient
    Private msgBuff As New ArrayList
    Private eventBuf As New ArrayList
    Private online As Boolean = False
    Private ChUsers() As String = {}
    Private joinChannel As String = ""
    Private adminName() As String = {}
    Private staySilent As Boolean = False
    Private mParent As frmMain = Nothing
    Private listenUsersList As New ArrayList
    Private onConnCommand As String = ""
    Private server As String = "irc.quakenet.org"
    Private port As Integer = 6668
    Private topicChanger As String = ""
    Private userName As String = ""
    Public Sub New(ByRef parent As frmMain)
        mParent = parent
        joinChannel = frmMain.ircChannel
        If joinChannel <> "" AndAlso joinChannel.Substring(0, 1) <> "#" Then joinChannel = "#" & joinChannel
        onConnCommand = frmMain.ircStartupCommand
        userName = frmMain.ircUserName
        topicChanger = frmMain.ircTopicChanger
        staySilent = frmMain.ircStaySilent
        If frmMain.ircServer <> "" Then
            Dim l As String() = frmMain.ircServer.Split(":")
            If l.Length > 1 Then port = Val(l(1))
            server = l(0)
        End If
        If frmMain.ircAutoAdd <> "" Then
            adminName = frmMain.ircAutoAdd.Split(",")
            For Each a As String In adminName
                listenUsersList.Add(a)
            Next
            ReDim Preserve adminName(UBound(adminName) + 1)
            adminName(UBound(adminName)) = userName
        End If
    End Sub
    Public Sub Start()
        irc = New ircClient(server, port, userName)
    End Sub
    Public Sub StopIt()
        If Not irc Is Nothing Then irc.close(IIf(staySilent, "quit..", "Controller closing..."))
    End Sub

    Private Sub irc_connected(ByVal sender As ircClient) Handles irc.connected
        online = True
        If onConnCommand <> "" Then irc.pureSay(onConnCommand)
        If joinChannel <> "" Then irc.JoinChannel(joinChannel)
        If msgBuff.Count > 0 Then
            For i As Integer = 0 To msgBuff.Count - 1
                For Each user As String In listenUsersList
                    irc.SendNotice(user, msgBuff.Item(i))
                Next
                'irc.SendMessage("#ryl2beta", msgBuff.Item(i))
            Next
            msgBuff.Clear()
        End If
    End Sub
    Public Enum MessageType
        EEvent = 0
        ENotice = 1
    End Enum
    Public Sub sendMsg(ByVal text As String, Optional ByVal msgType As MessageType = MessageType.EEvent)
        If msgType = MessageType.EEvent Then
            If text.IndexOf("[A0]") = 0 OrElse text.IndexOf("ERR: ") = 0 OrElse text.IndexOf("WAR: ") = 0 Then Exit Sub
            eventBuf.Add(" [" & Now() & "] " & text)
        End If
        If online Then
            For Each user As String In listenUsersList
                irc.SendNotice(user, text)
            Next
            'irc.SendMessage("#ryl2beta", text)
        Else
            msgBuff.Add(text)
        End If
    End Sub
    Public Sub setOffline(ByVal txt As String)
        If topicChanger <> "" AndAlso Not staySilent Then irc.SendNotice(topicChanger, "!set offline " & txt)
    End Sub
    Public Sub setOnline(Optional ByVal txt As String = "")
        If topicChanger <> "" AndAlso Not staySilent Then irc.SendNotice(topicChanger, "!set online " & txt)
    End Sub
    Public Sub restart(Optional ByVal txt As String = "")
        If Not staySilent Then irc.SendMessage(joinChannel, txt)
    End Sub
    Private Function userLevel(ByVal name As String) As Integer
        Dim badChar As String() = {"+", "@", "&", "~", "."}
        For Each ba As String In badChar
            If name.Substring(0, 1) = ba Then name = name.Substring(1)
        Next
        If Array.IndexOf(adminName, name) >= 0 Then
            Return 3
        Else
            Return 0
        End If
        For Each n As String In ChUsers
            Dim t As Integer = 0
            Select Case n.Substring(0, 1)
                Case "+" : t = 1
                Case "@" : t = 2
                Case "&" : t = 3
                Case "~" : t = 4
            End Select
            If t > 0 Then n = n.Substring(1)
            If n = name Then
                Return t
            End If
        Next
        Return 0
    End Function
    Private Sub irc_GotChUsers(ByVal sender As ircClient, ByVal channel As String, ByVal users() As String) Handles irc.GotChUsers
        ChUsers = users
    End Sub

    Private Sub irc_GotKicked(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal reason As String) Handles irc.GotKicked
        If joinChannel <> "" Then irc.JoinChannel(joinChannel)
    End Sub

    Private Sub irc_GotMessage(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal message As String) Handles irc.GotMessage
        Dim orgMess As String = message
        message = message.ToLower
        If staySilent AndAlso channel <> "" Then Exit Sub
        Dim i As Integer = 0
        Dim isAdmin As Boolean = (userLevel(who) > 1)
        Select Case message.Split(" ")(0)
            Case "?commands", "?help"
                irc.SendNotice(who, "------- List of RYLServerController commands -------")
                irc.SendNotice(who, "|?commands    - Shows the list of commands")
                irc.SendNotice(who, "|?last10      - Shows history of events(last 10)")
                'irc.SendNotice(who, "|?fromStart   - Shows history of events from program start")
                irc.SendNotice(who, "|?status      - Shows which servers are online and number of players")
                irc.SendNotice(who, "|?listen      - Sends you events when they happen")
                irc.SendNotice(who, "|?stopListen  - Stops sending events")
                If isAdmin Then
                    irc.SendNotice(who, "------------------ Admin commands ------------------")
                    irc.SendNotice(who, "|## Must be sended as PM or NOTICE ##")
                    irc.SendNotice(who, "|?restart nr  - Restarts(starts) the server in nr minutes")
                    irc.SendNotice(who, "|?kill        - Restarts(starts) the server by killing processes")
                    irc.SendNotice(who, "|?logc id f   - sends the function f to the server with the specified ID")
                    irc.SendNotice(who, "|?list_serv   - lists the server and their status")
                    irc.SendNotice(who, "|?notify txt  - sends a beeping message to all gameservers")
                    irc.SendNotice(who, "|?say string  - executes the named IRC command")
                    If userLevel(who) > 2 Then irc.SendNotice(who, "|?reCatch     - Try to re-catch ryl server processes")
                    irc.SendNotice(who, "|?shutdown nr - Closes all server programs in nr minutes")
                    If userLevel(who) > 2 Then irc.SendNotice(who, "|?DoException - Crashes the controller")
                    If userLevel(who) > 2 Then irc.SendNotice(who, "|?say string  - executes the named IRC command")
                End If
                irc.SendNotice(who, "------------------- Made by AlphA ------------------")
            Case "?last10"
                If eventBuf.Count > 0 Then
                    Dim j As Integer = eventBuf.Count - 10
                    If j < 0 Then j = 0
                    For i = j To eventBuf.Count - 1
                        irc.SendNotice(who, eventBuf.Item(i))
                    Next
                End If
            Case "?fromstart"
                'If eventBuf.Count > 0 Then
                '    For i = 0 To eventBuf.Count - 1
                '        irc.SendNotice(who, eventBuf.Item(i))
                '    Next
                'End If
                irc.SendNotice(who, "Command has been disabled. It resulted in Exceed flood")
            Case "?status"
                Dim fl As String = ""
                For Each ite As ServerItem In frmMain.serverItems.items
                    fl &= ite.name & ": " & IIf(ite.online, "ON", "OFF") & ", "
                Next
                fl = fl.Substring(0, fl.Length - 2)
                'irc.SendNotice(who, "[" & ite.id & "] " & ite.name & " is " & IIf(ite.online, "ONLINE", "OFFLINE"))
                irc.SendNotice(who, fl)
                Dim sl As String = ""
                irc.SendNotice(who, "Server is " & frmMain.lastOnlineTime & ", Online users:")
                With mParent.hooks.serverInfo
                    sl &= "Total: " & .LoginTotal & ", Humans: " & .LoginHuman & ", Akhans: " & .LoginAkkan & ""
                    irc.SendNotice(who, sl)
                    sl = ""
                    For h As Integer = 0 To .LoginMapZones.Length - 1
                        Dim zone As Integer = .LoginMapZones(h)
                        Dim name As String = "[zone:" & zone & "]"
                        For Each sitems As ServerItem In frmMain.serverItems.items
                            If sitems.mapServerZone = zone Then
                                name = sitems.name '.Replace(" ", "_")
                            End If
                        Next
                        sl &= IIf(False, "[MAP]", "") & name & ": " & .LoginMap(h) & ", "
                    Next
                    If sl <> "" Then sl = sl.Substring(0, sl.Length - 2)
                End With
                irc.SendNotice(who, sl)
            Case "?restart"
                If isAdmin AndAlso channel = "" Then
                    Dim a As String() = message.Split(" ")
                    Dim time As Integer = 0
                    If a.Length > 1 Then
                        time = Val(a(1))
                    End If
                    If time > 0 Then
                        Dim tmr As New Timer
                        tmr.Interval = time * 60 * 1000
                        AddHandler tmr.Tick, AddressOf restartCommand
                        tmr.Start()
                    Else
                        restartCommand(Nothing, New EventArgs)
                    End If
                    Me.sendMsg("[C0] {" & who & "} Made a successful restart command")
                    irc.SendNotice(who, "Restart command accepted")
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?kill"
                If isAdmin AndAlso channel = "" Then
                    irc.SendNotice(who, "Kill command accepted")
                    mParent.remote_kill()
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?doexception"
                If isAdmin AndAlso channel = "" Then
                    irc.SendNotice(who, "DoException command accepted")
                    My.MyApplication.MyApplication_UnhandledException(Me, New Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs(True, New Exception("Called restart")))
                    mParent.KillMe()
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?say"
                If isAdmin AndAlso channel = "" Then
                    If message.Split(" ").Length > 1 Then
                        irc.irc.Say(orgMess.Substring(message.IndexOf(" ") + 1))
                    Else
                        irc.SendNotice(who, "Usage: ?say <raw irc message>")
                    End If
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?logc"
                If isAdmin AndAlso channel = "" Then
                    Dim splits As String() = message.Split(" ")
                    If splits.Length > 2 Then
                        Dim id As Integer = -1
                        If Integer.TryParse(splits(1), id) Then
                            mParent.remoteSendLogCom(orgMess.Substring(splits(0).Length + splits(1).Length + 2), id)
                        Else
                            irc.SendNotice(who, "Second string hast to be the numeric ID of the server")
                        End If
                    Else
                        irc.SendNotice(who, "Usage: ?logc <ID> <command>")
                    End If
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?list_serv"
                If isAdmin AndAlso channel = "" Then
                    Dim fl As String = ""
                    For Each ite As ServerItem In frmMain.serverItems.items
                        irc.SendNotice(who, "[" & ite.id & "]" & ite.name & ": " & IIf(Not ite.hook Is Nothing, "hooked", "not hooked") & ", " & IIf(ite.online, "ON", "OFF"))
                    Next
                    irc.SendNotice(who, fl)
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?notify"
                If isAdmin AndAlso channel = "" Then
                    If message.Split(" ").Length > 1 Then
                        Dim fl As String = ""
                        Dim cnt As Integer = 0
                        For Each ite As ServerItem In frmMain.serverItems.items
                            If ite.mapServerZone > 0 AndAlso ite.online AndAlso Not ite.hook Is Nothing AndAlso ite.hook.hwndIn > 0 Then
                                mParent.remoteSendLogCom("notify " & orgMess.Substring(message.Split(" ")(0).Length + 1), ite.id)
                                cnt += 1
                            End If
                        Next
                        irc.SendNotice(who, "Done. Notified to " & cnt & " gameserver" & IIf(cnt <> 1, "s", ""))
                    Else
                        irc.SendNotice(who, "Usage: ?notify <message>")
                    End If
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?flush"
                If isAdmin AndAlso channel = "" Then
                    Dim cnt As Integer = 0
                    For Each ite As ServerItem In frmMain.serverItems.items
                        mParent.remoteSendLogCom("flush", ite.id)
                        cnt += 1
                    Next
                    irc.SendNotice(who, "Logs flush done to " & cnt & " server(s).")
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?recatch"
                If isAdmin AndAlso channel = "" Then
                    mParent.reCatch()
                    irc.SendNotice(who, "Re-catch done.")
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?shutdown"
                If isAdmin AndAlso channel = "" Then
                    Dim a As String() = message.Split(" ")
                    Dim time As Integer = 0
                    If a.Length > 1 Then
                        time = Val(a(1))
                    End If
                    If time > 0 Then
                        Dim tmr As New Timer
                        tmr.Interval = time * 60 * 1000
                        AddHandler tmr.Tick, AddressOf downCommand
                        tmr.Start()
                    Else
                        downCommand(Nothing, New EventArgs)
                    End If
                    irc.SendNotice(who, "Shutdown command accepted")
                    Me.sendMsg("[C1] {" & who & "} Made a successful shutdown command")
                Else
                    irc.SendNotice(who, "Access denied")
                End If
            Case "?listen"
                listenUsersList.Add(who)
                irc.SendNotice(who, "You are added to the listening list")
            Case "?stoplisten"
                If listenUsersList.IndexOf(who) >= 0 Then
                    listenUsersList.Remove(who)
                    irc.SendNotice(who, "You are removed from the listening list")
                Else
                    irc.SendNotice(who, "You are not in the listening list. Use ?listen first")
                End If
        End Select
    End Sub

    Private Sub irc_GotNotice(ByVal sender As ircClient, ByVal who As String, ByVal message As String) Handles irc.GotNotice
        irc_GotMessage(sender, "", who, message)
    End Sub

    Private Sub irc_GotPM(ByVal sender As ircClient, ByVal who As String, ByVal message As String) Handles irc.GotPM
        irc_GotMessage(sender, "", who, message)
    End Sub

    Private Sub restartCommand(ByVal sender As Object, ByVal e As EventArgs)
        If Not sender Is Nothing Then
            Dim t As Timer = CType(sender, Timer)
            t.Stop()
        End If
        mParent.remote_restart()
    End Sub

    Private Sub downCommand(ByVal sender As Object, ByVal e As EventArgs)
        If Not sender Is Nothing Then
            Dim t As Timer = CType(sender, Timer)
            t.Stop()
        End If
        mParent.remote_shutdown()
    End Sub
End Class