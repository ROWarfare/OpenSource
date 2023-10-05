Public Class frmMain
    '##########################
    '#                        #
    '#  RYL Server Controller #
    '#                        #
    '#  © 2006-2007 AlphA     #
    '#                        #
    '##########################

    '
    ' Log events:
    '
    ' [A0] --------------------------------------------------------------------------
    ' [A1] Autorun server with windows startup
    ' [A2] RYL Server Controller online
    ' [A3] Starting server
    ' [A4] Server online
    '
    ' "[B4] {" & servI.id & "} " & servI.name & " crashed" & IIf(inf <> "", ", " & inf, "")
    ' [B3] Stopping server
    ' [B2] Server fully down
    ' [B1] RYL Server Controller Shutting down
    '
    '
    '
    Public Delegate Sub frmMainD(ByVal sender As Object, ByVal e As System.EventArgs)
    Public Delegate Sub frmMainF()
    Public Delegate Function frmMainFFB() As Boolean
    Public Delegate Sub crashD(ByRef servI As ServerItem)
    Public Delegate Sub sendLogComD(ByVal command As String, ByVal serverId As Integer)
    Public Shared config As New CConfigFile()
    Public Shared serverDir As String = config.ini.GetString("main", "ServerDir", "")
    Public Shared serverItems As New CServerController(config, serverDir)
    Private Shared shuttingDown As Boolean = False
    Private Shared isRestart As Boolean = False
    Private Shared loginServID As Integer = -1
    Private Shared msgbox_open As Boolean = False
    Private Shared loginSignalSended As Boolean = False
    Public Event allAreShutDown()
    Public Event serverOnline()
    Public Event serverReadyForCommand()
    Private Shared isFullShutdown As Boolean = False
    Private Shared logFile As String = ""
    Private Shared onlineLogFile As String = ""
    Private Shared statusFile As String = ""
    Private Shared multiCrashNeedTime As Integer = 5
    Private Shared waitTime As Integer = config.ini.GetInteger("main", "WaitTime", 2000)
    Private Shared multiCrashTimeout As Integer = config.ini.GetInteger("main", "MultiCrashTimeout", 5000)
    Private Shared loginEnableCode As Integer = config.ini.GetInteger("main", "LoginEnableCode", 141)
    Public WithEvents hooks As New apiHook

    Public Shared ircChannel As String = config.ini.GetString("irc", "channel", "")
    Public Shared ircUserName As String = config.ini.GetString("irc", "uname", "RylBot[RSC]")
    Public Shared ircTopicChanger As String = config.ini.GetString("irc", "TopicChanger", "")
    Public Shared ircStartupCommand As String = config.ini.GetString("irc", "startup", "")
    Public Shared ircServer As String = config.ini.GetString("irc", "server", "irc.quakenet.org")
    Public Shared ircAutoAdd As String = config.ini.GetString("irc", "autoListenUsers", "")
    Public Shared mainWindowClass As String = config.ini.GetString("main", "mainWindowClass", "RylConsoleWindow")
    Public Shared ircStaySilent As Boolean = config.ini.GetBoolean("irc", "silent", False)
    Public Shared lastOnlineTime As String = "Offline"
    Dim ircBot As New ControllerBot(Me)
    Private Shared crashesCol As Integer()() = {}
    Public Shared progRunTime As Integer = 0
    Private crashTimers As New ArrayList
    Private onlineCheckTimers As New ArrayList
    Dim botUsing As Boolean = config.ini.GetBoolean("irc", "UseIrcBot", True)

    Private Sub frmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing
        If e.CloseReason = CloseReason.WindowsShutDown OrElse e.CloseReason = CloseReason.TaskManagerClosing OrElse My.props.gotCrash Then
            Me.Visible = False
            e.Cancel = False
        ElseIf Me.Visible Then
            Me.Visible = False
            e.Cancel = True
        ElseIf serverItems.OverAllStatus <> CServerController.Status.ALL_ARE_OFFLINE Then
            If MsgBox("Server is still running. Are you sure you want to close?", MsgBoxStyle.YesNo, "RYL Server Controller") <> MsgBoxResult.Yes Then
                e.Cancel = True
            End If
        End If
        If e.Cancel = False Then
            hooks.close()
            For Each t As Timer In crashTimers
                t.Stop()
            Next
            For Each t As Timer In onlineCheckTimers
                t.Stop()
            Next
            crashTimers.Clear()
            onlineCheckTimers.Clear()
            logit("[B1] RYL Server Controller Shutting down")
            config.ini.WriteBoolean("main", "DoRestart", Me.chkRestart.Checked)
            config.ini.WriteBoolean("main", "DoMapStart", Me.chkStartMap.Checked)
            config.ini.WriteBoolean("main", "KillProcesses", Me.chkKillProcess.Checked)
            config.ini.WriteBoolean("main", "NoWatching", Me.chkDoNothing.Checked)
            If botUsing Then ircBot.StopIt()
        End If
    End Sub
    Public Sub KillMe()
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainF(AddressOf KillMe))
        Else
            Me.Hide()
            Application.Exit()
        End If
    End Sub
    Private Sub frmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If My.props.startProg Then
            Me.chkRestart.Checked = config.ini.GetBoolean("main", "DoRestart", True)
            Me.chkStartMap.Checked = config.ini.GetBoolean("main", "DoMapStart", True)
            Me.chkKillProcess.Checked = config.ini.GetBoolean("main", "KillProcesses", True)
            Me.chkDoNothing.Checked = config.ini.GetBoolean("main", "NoWatching", True)
            Dim lName As String = config.ini.GetString("main", "LoginServerName", "")
            logFile = config.ini.GetString("main", "Logfile", "")
            If logFile = "" Then
                Dim l As String() = Application.ExecutablePath.Split("\")
                Dim dir As String = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - l(UBound(l)).Length)
                Dim fName As String = l(UBound(l))
                logFile = dir & fName.Split(".")(0) & ".log"
                config.ini.WriteString("main", "LogFile", logFile)
            End If
            My.props.logFile = logFile
            statusFile = config.ini.GetString("main", "StatusFile", "")
            If statusFile = "" Then
                Dim l As String() = Application.ExecutablePath.Split("\")
                Dim dir As String = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - l(UBound(l)).Length)
                Dim fName As String = l(UBound(l))
                statusFile = dir & fName.Split(".")(0) & "_status.csv"
                config.ini.WriteString("main", "StatusFile", statusFile)
            End If
            onlineLogFile = config.ini.GetString("main", "OnlineLog", "")
            If onlineLogFile = "" Then
                Dim l As String() = Application.ExecutablePath.Split("\")
                Dim dir As String = Application.ExecutablePath.Substring(0, Application.ExecutablePath.Length - l(UBound(l)).Length)
                Dim fName As String = l(UBound(l))
                onlineLogFile = dir & fName.Split(".")(0) & "_online.csv"
                config.ini.WriteString("main", "OnlineLog", onlineLogFile)
            End If
            startupTimer.Interval = waitTime
            Dim i As Integer = 0
            For Each sItem As ServerItem In serverItems.items
                Dim lbl As New CStatus
                lbl.lblName.Text = sItem.name
                lbl.servItem = sItem
                lbl.Visible = True
                sItem.label = lbl
                If sItem.name = lName Then loginServID = i
                Me.flowStatus.Controls.Add(lbl)
                AddHandler lbl.ShowLog, AddressOf statusItem_ShowLog
                AddHandler lbl.HideLog, AddressOf statusItem_HideLog
                lbl.Recheck()
                i += 1
            Next
            Dim isOnline As Boolean = False
            If hookToServer() Then 'try to hook, maybe server is running already
                isOnline = True
            End If


            If botUsing Then ircBot.Start()

            My.Application.DoEvents()
            logit("[A0] --------------------------------------------------------------------------")
            If My.props.autoStartServer Then
                logit("[A1] Autorun server with windows startup")
                Me.Visible = True
                Me.Close() 'minimize to system tray
                If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_OFFLINE Then startProcesses()
            Else
                logit("[A2] RYL Server Controller online")
            End If
            If isOnline Then RaiseEvent serverOnline()
            RaiseEvent serverReadyForCommand()
        Else
            Me.quit()
        End If
    End Sub

    Public Sub reCatch()
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainFFB(AddressOf hookToServer))
            Me.Invoke(New frmMainF(AddressOf hook_listeners))
        Else
            hookToServer()
            hook_listeners()
        End If

    End Sub
    Private Function hookToServer() As Boolean
        Dim prc As Process() = Process.GetProcesses
        Dim cnt As Integer = 0
        For Each proc As Process In prc
            Try
                Dim i As Integer = serverItems.IndexOf(proc.ProcessName)
                If i >= 0 Then
                    Dim sItem As ServerItem = serverItems.items(i)
                    'hook
                    sItem.proc = proc
                    sItem.online = True
                    sItem.processName = sItem.proc.ProcessName
                    sItem.label.Recheck()
                    sItem.proc.EnableRaisingEvents = True
                    AddHandler sItem.proc.Exited, AddressOf processExit
                    cnt += 1
                    'end hook
                    Dim tmrAllOnline As New Timer
                    tmrAllOnline.Interval = waitTime
                    AddHandler tmrAllOnline.Tick, AddressOf checkAllOnline
                    onlineCheckTimers.Add(tmrAllOnline)
                    tmrAllOnline.Start()
                End If
            Catch ex As Exception 'doesn't have a name or error in hooking/just ignore
            End Try
        Next
        If cnt = serverItems.items.Length Then
            Return True
        Else
            Return False
        End If
    End Function

    Private Sub startProcesses()
        hooks = New apiHook
        loginSignalSended = False
        If serverItems.items.Length > 0 Then
            logit("[A3] Starting server")
            Me.startupTimer.Tag = 0
            If Me.InvokeRequired Then Me.Invoke(New frmMainD(AddressOf startupTimer_Tick), New Object() {Me, New EventArgs}) Else Me.startupTimer_Tick(Me, New EventArgs)
        End If
    End Sub

    Private Sub startupProc(ByVal index As Integer)
        Dim sitem As ServerItem = serverItems.items(index)
        If loginServID >= 0 AndAlso index = loginServID + 1 AndAlso Not loginSignalSended Then
            Dim splices As String() = serverItems.items(loginServID).filename.Split(New Char() {"\", "/"})
            If splices.Length > 0 Then
                Dim name As String() = splices(UBound(splices)).Split(".")
                If name.Length = 2 Then
                    apiHook.startLogin(name(0), loginEnableCode)
                    loginSignalSended = True
                End If
            End If
        End If
        Try
            If sitem.proc Is Nothing Then
                sitem.proc = Process.Start(sitem.filename, sitem.arguments)
                sitem.proc.EnableRaisingEvents = True
                AddHandler sitem.proc.Exited, AddressOf processExit
                sitem.online = True
                sitem.processName = sitem.proc.ProcessName
                sitem.label.Recheck()
                Dim tmrAllOnline As New Timer
                tmrAllOnline.Interval = waitTime
                AddHandler tmrAllOnline.Tick, AddressOf checkAllOnline
                onlineCheckTimers.Add(tmrAllOnline)
                tmrAllOnline.Start()
            End If
        Catch ex As Exception
            If Not msgbox_open Then
                msgbox_open = True 'doesnt work right now... cose 1 threaded here
                If MsgBox(ex.Message, MsgBoxStyle.Critical, "Check your config file or contact AlphA") = MsgBoxResult.Ok Then
                    msgbox_open = False
                    killProcesses()
                End If
            End If
        End Try
    End Sub
    Private Sub nothProcessExit(ByVal sender As Object, ByVal e As EventArgs)
        Dim pro As Process = CType(sender, Process)
        If Not pro Is Nothing Then
            Dim lo As Integer = serverItems.IndexOf(pro)
            If lo >= 0 Then
                If hooks.isHooked(serverItems.items(lo)) Then
                    Try
                        hooks.close(serverItems.items(lo))
                    Catch ex As Exception
                        logit("ERR: Cant remove hook from " & serverItems.items(lo).name)
                    End Try
                End If
                For Each p As Timer In onlineCheckTimers
                    p.Stop()
                Next
                onlineCheckTimers.Clear()
                serverItems.items(lo).online = False
                serverItems.items(lo).label.Recheck()
                serverItems.items(lo).proc = Nothing
                If Not serverItems.items(lo).problematicStartup AndAlso isMultiCrash(lo) Then
                    crashWait(serverItems.items(lo))
                    Dim tmrMultiCrashWaiter As New Timer
                    tmrMultiCrashWaiter.Tag = lo
                    tmrMultiCrashWaiter.Interval = multiCrashTimeout
                    AddHandler tmrMultiCrashWaiter.Tick, AddressOf tmrMultiCrashWaiter_Tick
                    crashTimers.Add(tmrMultiCrashWaiter)
                    tmrMultiCrashWaiter.Start()
                Else
                    processExitPhase2(lo)
                End If
            End If
        End If
    End Sub
    Private Sub processExitPhase2(ByVal lo As Integer, Optional ByVal notice As Boolean = True)
        If Me.chkStartMap.Checked AndAlso serverItems.items(lo).supportsDirectStart AndAlso Not shuttingDown AndAlso Not Me.chkDoNothing.Checked Then
            If notice Then crash(serverItems.items(lo))
            startupProc(lo)
        ElseIf Not shuttingDown AndAlso serverItems.items(lo).problematicStartup AndAlso Not Me.chkDoNothing.Checked Then
            'Problematic startup, continous starting up
            If serverItems.items.Length > lo + 1 AndAlso Not serverItems.items(lo + 1).online Then
                startupProc(lo)
            Else
                If notice Then crash(serverItems.items(lo))
                killProcesses()
            End If
        Else
            If Not shuttingDown AndAlso Not Me.chkDoNothing.Checked Then
                If notice Then crash(serverItems.items(lo))
                killProcesses()
            End If
            lastOnlineTime = "Offline from " & Now()
            If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_OFFLINE Then RaiseEvent allAreShutDown()
        End If
    End Sub
    Private Function isMultiCrash(ByVal lo As Integer) As Boolean
        Dim lastCrash As Integer = 0
        For Each cc As Integer() In crashesCol
            If cc(0) = lo Then lastCrash = cc(1)
        Next
        ReDim Preserve crashesCol(UBound(crashesCol) + 1)
        crashesCol(UBound(crashesCol)) = New Integer() {lo, progRunTime}

        If lastCrash + multiCrashNeedTime >= progRunTime Then
            Return True
        Else
            Return False
        End If
    End Function
    Private Sub processExit(ByVal sender As Object, ByVal e As EventArgs)
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainD(AddressOf nothProcessExit), New Object() {sender, e})
        Else
            nothProcessExit(sender, e)
        End If
    End Sub
    Private Sub crashWait(ByRef servI As ServerItem)
        Dim txt As String = servI.name & " crashed. Staying in wait-time."
        Dim inf = ""
        lastOnlineTime = "Offline from " & Now()
        If Me.chkStartMap.Checked AndAlso servI.supportsDirectStart Then
            inf = "Restarting map only..."
        ElseIf Me.chkRestart.Checked Then
            inf = "Restarting server..."
        End If
        If inf <> "" Then txt &= vbNewLine & inf
        logit("[B4] {" & servI.id & "} " & servI.name & " crashed. Wait-time is on. " & IIf(inf <> "", ", " & inf, ""))
        If botUsing Then ircBot.restart(servI.name & " crashed. Initializing wait time cose of multi crashes." & IIf(inf <> "", ", " & inf, ", Auto restart off, staying offline"))
        txt &= vbNewLine & Now()
        notifI.ShowBalloonTip(5, "RYL Server Controller", txt, ToolTipIcon.Error)
    End Sub
    Private Sub crash(ByRef servI As ServerItem)
        Dim txt As String = servI.name & " crashed"
        Dim inf = ""
        lastOnlineTime = "Offline from " & Now()
        If Me.chkStartMap.Checked AndAlso servI.supportsDirectStart Then
            inf = "Restarting map only..."
        ElseIf Me.chkRestart.Checked Then
            inf = "Restarting server..."
        End If
        If inf <> "" Then txt &= vbNewLine & inf
        logit("[B4] {" & servI.id & "} " & servI.name & " crashed" & IIf(inf <> "", ", " & inf, ""))
        If botUsing Then ircBot.restart(servI.name & " crashed" & IIf(inf <> "", ", " & inf, ", Auto restart off, staying offline"))
        txt &= vbNewLine & Now()
        notifI.ShowBalloonTip(5, "RYL Server Controller", txt, ToolTipIcon.Error)
    End Sub
    Private Sub killProcesses()
        If Not shuttingDown Then
            Me.lblLoginOnline.ForeColor = Color.Yellow
            Me.lblLoginOnline.Text = "Unknown"
            shuttingDown = True
            logit("[B3] Stopping server")
            hooks.close()
            For Each sItem As ServerItem In serverItems.items
                Try
                    If Not sItem.proc Is Nothing Then
                        If Me.chkKillProcess.Checked Then
                            sItem.proc.Kill()
                        Else
                            apiHook.closeProg(sItem.proc.Threads(0).Id)
                        End If
                    End If
                Catch ex As Exception
                    MsgBox(ex.Message, MsgBoxStyle.Critical, "Check your config file or contact AlphA")
                End Try
            Next
        End If
    End Sub

    Private Sub frmMain_allAreShutDown() Handles Me.allAreShutDown
        startupTimer.Enabled = False
        logit("[B2] Server fully down")
        hooks_ServInfoChanged(New apiHook.ServerInfoStructure)
        shuttingDown = False
        If (Not isFullShutdown AndAlso Me.chkRestart.Checked AndAlso Not Me.chkDoNothing.Checked) OrElse isRestart Then
            isFullShutdown = False
            isRestart = False
            startProcesses()
        Else
            If Not Me.chkRestart.Checked AndAlso Not isFullShutdown AndAlso botUsing AndAlso Not Me.chkDoNothing.Checked Then
                ircBot.setOffline("Server crashed. Auto restart is off")
            ElseIf botUsing And isFullShutdown Then
                ircBot.setOffline("Server shutdown by Admin")
            End If
            isFullShutdown = False
        End If
    End Sub

    Private Sub btnStart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnStart.Click
        If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_OFFLINE Then
            startProcesses()
        Else
            If MsgBox("Some or all are already online" & vbNewLine & "Do we make a restart?", MsgBoxStyle.YesNo, "RYLServerController") = MsgBoxResult.Yes Then
                btnRestart_Click(Me, New EventArgs)
            End If
        End If
    End Sub

    Public Sub btnShutdown_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnShutdown.Click
        If serverItems.OverAllStatus <> CServerController.Status.ALL_ARE_OFFLINE Then
            isFullShutdown = True
            For Each t As Timer In crashTimers
                t.Stop()
            Next
            crashTimers.Clear()
            killProcesses()
        End If
    End Sub

    Public Sub forceKill()
        If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_OFFLINE Then
            startProcesses()
        Else
            If botUsing Then ircBot.restart("Admin makes restart")
            isRestart = True
            Dim pS As Boolean = Me.chkKillProcess.Checked
            Me.chkKillProcess.Checked = True
            shuttingDown = False
            killProcesses()
            Me.chkKillProcess.Checked = pS
        End If
    End Sub
    Public Sub btnRestart_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnRestart.Click
        If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_OFFLINE Then
            startProcesses()
        Else
            If botUsing Then ircBot.restart("Admin makes restart")
            isRestart = True
            killProcesses()
        End If
    End Sub

    Private Sub startupTimer_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles startupTimer.Tick
        Me.startupTimer.Stop()
        Dim index As Integer = CType(startupTimer.Tag, Integer)
        If shuttingDown OrElse crashTimers.Count > 0 Then
        ElseIf serverItems.items.Length = index Then
        ElseIf serverItems.items.Length > index Then
            startupProc(index)
            If serverItems.items.Length > index Then
                startupTimer.Tag = index + 1
                Me.startupTimer.Interval = waitTime
                startupTimer.Start()
            End If
        End If
    End Sub

    Private Sub notifI_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles notifI.DoubleClick
        Me.Visible = True
        If Me.WindowState = FormWindowState.Minimized Then Me.WindowState = FormWindowState.Normal
        Me.Focus()
    End Sub

    Private Sub mnuExit_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuExit.Click
        If Me.Visible Then Me.Visible = False
        Me.Close()
    End Sub

    Private Sub mnuShow_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuShow.Click
        Me.Visible = True
    End Sub

    Private Sub mnuStart_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuStart.Click
        btnStart_Click(Me, New System.EventArgs)
    End Sub

    Private Sub mnuStop_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles mnuStop.Click
        btnShutdown_Click(Me, New System.EventArgs)
    End Sub

    Private Sub quit()
        Me.Visible = False
        Me.Close()
    End Sub

    Public Sub logit(ByVal line As String)
        If botUsing Then ircBot.sendMsg(line)
        line = Now() & " : " & line
        If Not My.Computer.FileSystem.FileExists(logFile) OrElse FileSystem.FileLen(logFile) < 1 Then
            line = "RYL Server controller log" & vbNewLine & vbNewLine & "Log started at " & Now() & vbNewLine & "---------------------------------------------------------------------" & vbNewLine & line
        End If
        Try
            Dim sr As New IO.StreamWriter(logFile, True)
            sr.WriteLine(line)
            sr.Close()
        Catch ex As Exception
            If Me.Visible Then
                MsgBox("Problem with log file." & vbNewLine & "Please check you'r ini file", MsgBoxStyle.Critical, "RYLServerController")
                Me.quit()
            End If
        End Try
    End Sub

    Private Sub frmMain_VisibleChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.VisibleChanged
        If Me.Visible Then
            For Each ite As ServerItem In serverItems.items
                If Not ite.label Is Nothing Then
                    ite.label.Recheck()
                End If
            Next
        End If
    End Sub

    Public Sub remote_shutdown()
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainD(AddressOf btnShutdown_Click), New Object() {New Object, New EventArgs})
        Else
            btnShutdown_Click(New Object, New EventArgs)
        End If
    End Sub

    Public Sub remote_restart()
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainD(AddressOf btnRestart_Click), New Object() {New Object, New EventArgs})
        Else
            btnRestart_Click(New Object, New EventArgs)
        End If
    End Sub

    Public Sub remote_kill()
        If Me.InvokeRequired Then
            Me.Invoke(New frmMainF(AddressOf forceKill))
        Else
            forceKill()
        End If
    End Sub

    Private Sub hook_listeners()
        For Each sItem As ServerItem In serverItems.items
            If Not sItem.proc Is Nothing Then
                Try
                    If sItem.proc.Threads.Count > 0 Then
                        If Not hooks.isHooked(sItem) Then hooks.getOnLineUsers(sItem, sItem.proc.Threads(0).Id, mainWindowClass)
                    End If
                Catch ex As Exception
                    logit("ERR: Can't hook to " & sItem.name & " first thread main window handle")
                End Try
            End If
        Next
    End Sub
    Private Sub frmMain_serverOnline() Handles Me.serverOnline
        logit("[A4] Server online")
        If botUsing Then ircBot.setOnline()
        lastOnlineTime = "Online from " & Now()
        hook_listeners()
    End Sub
    Private Sub checkAllOnline(ByVal sender As Object, ByVal e As EventArgs)
        sender.stop()
        If onlineCheckTimers.IndexOf(sender) >= 0 Then onlineCheckTimers.Remove(sender)
        If serverItems.OverAllStatus = CServerController.Status.ALL_ARE_ONLINE Then
            For Each t As Timer In onlineCheckTimers
                t.Stop()
            Next
            onlineCheckTimers.Clear()
            RaiseEvent serverOnline()
        End If
    End Sub

    Private Sub tmrAddProcTest_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrAddProcTest.Tick
        For Each sItem As ServerItem In serverItems.items
            If Not sItem.proc Is Nothing AndAlso sItem.online Then
                If sItem.proc.HasExited Then
                    processExit(sItem.proc, New EventArgs)
                Else
                    Dim hand As Integer = clsHook.GetChildHandle(sItem.processName & ".exe", vbNullString, 0)
                    If hand > 0 Then
                        Dim subhand As Integer = clsHook.GetChildHandle(vbNullString, "Static", hand)
                        Do While subhand > 0
                            Dim txt As String = clsHook.WindowText(subhand)
                            If txt.IndexOf("encountered a problem") Then
                                'processExit(sItem.proc, New EventArgs)
                                '
                                ' the post message will force the process to kill anyway
                                apiHook.PostMessage(hand, apiHook.WM_QUIT, &H0, &H0)
                                subhand = 0
                            Else
                                subhand = clsHook.GetChildHandle(vbNullString, "Static", hand, subhand)
                            End If

                        Loop
                    End If
                End If
            End If
        Next
    End Sub

    Public Sub remoteSendLogCom(ByVal command As String, ByVal serverId As Integer)
        If Me.InvokeRequired Then
            Me.Invoke(New sendLogComD(AddressOf sendLogCom), New Object() {command, serverId})
        Else
            sendLogCom(command, serverId)
        End If
    End Sub
    Public Sub sendLogCom(ByVal command As String, ByVal serverId As Integer)
        Dim sItem As ServerItem = serverItems.items(serverId)
        If Not sItem Is Nothing AndAlso Not sItem.hook Is Nothing Then
            If sItem.hook.hwndIn > 0 Then
                sItem.hook.SendLogCommand(command)
            End If
        End If
    End Sub
    Private Sub frmMain_serverReadyForCommand() Handles Me.serverReadyForCommand
        If serverItems.OverAllStatus <> CServerController.Status.ALL_ARE_OFFLINE Then hook_listeners()
        hooks_ServInfoChanged(New apiHook.ServerInfoStructure)
    End Sub
    Private Structure mostOnlineMap
        Dim name As String
        Dim count As Integer
        Dim daate As Long
    End Structure
    Private Function mapIndexInstruct(ByVal mass As ArrayList, ByVal mapName As String) As Integer
        Dim i As Integer = 0
        For Each m As mostOnlineMap In mass
            If m.name = mapName Then Return i
            i += 1
        Next
        Return -1
    End Function
    Private Sub hooks_ServInfoChanged(ByVal servInfo As apiHook.ServerInfoStructure) Handles hooks.ServInfoChanged
        If servInfo.LoginOpen Then
            Me.lblLoginOnline.ForeColor = Color.Green
            Me.lblLoginOnline.Text = "Open"
        Else
            Me.lblLoginOnline.ForeColor = Color.Red
            Me.lblLoginOnline.Text = "Closed"
        End If

        Dim haveZones As Integer() = {}
        For i As Integer = 0 To Me.flowMapOnline.Controls.Count - 1
            Dim c As Windows.Forms.Control = Me.flowMapOnline.Controls.Item(i)
            If Not c.Tag Is Nothing AndAlso Val(c.Tag) <> 0 Then
                ReDim Preserve haveZones(UBound(haveZones) + 1)
                haveZones(UBound(haveZones)) = Val(c.Tag)
            End If
        Next
        Dim k As Integer = 0
        For j As Integer = -3 To -1 Step 1
            k = Array.IndexOf(haveZones, j)
            If k < 0 Then
                Dim map As New CMapInfo
                map.lblname.Text = IIf(j = -3, "Total", IIf(j = -2, "Humans", IIf(j = -1, "Akhans", "???")))
                map.Tag = j
                k = Me.flowMapOnline.Controls.Count
                Me.flowMapOnline.Controls.Add(map)
            End If
            If k > -1 Then
                Dim mapC As CMapInfo = CType(Me.flowMapOnline.Controls.Item(k), CMapInfo)
                If Not mapC Is Nothing Then
                    mapC.lblCount.Text = IIf(j = -3, servInfo.LoginTotal, IIf(j = -2, servInfo.LoginHuman, IIf(j = -1, servInfo.LoginAkkan, 0)))
                End If
            End If
        Next
        k = 0
        For Each zone As Integer In servInfo.LoginMapZones
            Dim index As Integer = Array.IndexOf(haveZones, zone)
            If index < 0 Then
                Dim map As New CMapInfo
                Dim name As String = "[zone: " & zone & "]"
                For Each sitems As ServerItem In serverItems.items
                    If sitems.mapServerZone = zone Then
                        name = sitems.name
                    End If
                Next
                map.lblname.Text = name
                map.Tag = zone
                map.Visible = True
                index = Me.flowMapOnline.Controls.Count
                Me.flowMapOnline.Controls.Add(map)
            End If
            If index > -1 Then
                Dim mapC As CMapInfo = CType(Me.flowMapOnline.Controls.Item(index), CMapInfo)
                If Not mapC Is Nothing Then
                    mapC.lblCount.Text = servInfo.LoginMap(k)
                End If
            End If
            k += 1
        Next

        If statusFile <> "" Then
            Dim uTimeP As New UnixTimestamp
            Dim maxOnline As Integer = 0
            Dim maxDate As Long = 0
            Dim maxHuman As Integer = 0
            Dim maxHumanDate As Long = 0
            Dim maxAkkan As Integer = 0
            Dim maxAkkanDate As Long = 0
            Dim mapCounts As New ArrayList() 'type=mostOnlineMap
            Try
                Dim sr As New IO.StreamReader(statusFile)
                Do While Not sr.EndOfStream
                    Dim l As String = sr.ReadLine
                    If l <> "" Then
                        Dim spl As String() = l.Split(";")
                        If spl.Length > 1 Then
                            Select Case spl(0)
                                Case "MostOnlineNr"
                                    maxOnline = Val(spl(1))
                                Case "MostOnlineDate"
                                    Long.TryParse(Val(spl(1)), maxDate)
                                Case "MostHumansNr"
                                    maxHuman = Val(spl(1))
                                Case "MostHumansDate"
                                    Long.TryParse(Val(spl(1)), maxHumanDate)
                                Case "MostAkkansNr"
                                    maxAkkan = Val(spl(1))
                                Case "MostAkkansDate"
                                    Long.TryParse(Val(spl(1)), maxAkkanDate)
                            End Select
                            If spl(0).Length > 10 AndAlso spl(0).Substring(0, 8) = "MostMapNr_" Then
                                Dim mName As String = spl(0).Substring(8)
                                Dim cnt As Integer = Val(spl(1))
                                Dim i As Integer = mapIndexInstruct(mapCounts, mName)
                                If i < 0 Then
                                    Dim nStruct As New mostOnlineMap
                                    nStruct.name = mName
                                    mapCounts.Add(nStruct)
                                    i = mapCounts.Count - 1
                                End If
                                mapCounts(i).count = cnt
                            ElseIf spl(0).Length > 12 AndAlso spl(0).Substring(0, 8) = "MostMapDate_" Then
                                Dim mName As String = spl(0).Substring(8)
                                Dim daate As Long = 0
                                Long.TryParse(spl(1), daate)
                                Dim i As Integer = mapIndexInstruct(mapCounts, mName)
                                If i < 0 Then
                                    Dim nStruct As New mostOnlineMap
                                    nStruct.name = mName
                                    mapCounts.Add(nStruct)
                                    i = mapCounts.Count - 1
                                End If
                                mapCounts(i).daate = daate
                            End If
                        End If
                    End If
                Loop
                sr.Close()
            Catch ex As Exception
                logit("WAR: No status file or invalid directory or corrupted data - " & ex.Message)
            End Try
            Try
                Dim sw As New IO.StreamWriter(statusFile)
                Dim r As String = ""
                sw.WriteLine("ObjectName;Value;")
                r &= "isLoginOnline;" & IIf(servInfo.LoginOpen, "1", "0") & ";" & vbNewLine
                r &= "OnlineNr;" & servInfo.LoginTotal & ";" & vbNewLine
                r &= "Humans;" & servInfo.LoginHuman & ";" & vbNewLine
                r &= "Akkans;" & servInfo.LoginAkkan & ";" & vbNewLine
                For h As Integer = 0 To servInfo.LoginMapZones.Length - 1
                    Dim zone As Integer = servInfo.LoginMapZones(h)
                    Dim name As String = "[zone:" & zone & "]"
                    For Each sitems As ServerItem In serverItems.items
                        If sitems.mapServerZone = zone Then
                            name = sitems.name.Replace(" ", "_") & name
                        End If
                    Next
                    r &= "[MAP]" & name & ";" & servInfo.LoginMap(h) & ";" & vbNewLine
                Next
                If servInfo.LoginTotal >= maxOnline Then
                    r &= "MostOnlineNr;" & servInfo.LoginTotal & ";" & vbNewLine
                    r &= "MostOnlineDate;" & uTimeP.GetNow & ";" & vbNewLine
                Else
                    r &= "MostOnlineNr;" & maxOnline & ";" & vbNewLine
                    r &= "MostOnlineDate;" & maxDate & ";" & vbNewLine
                End If
                If servInfo.LoginHuman >= maxHuman Then
                    r &= "MostHumansNr;" & servInfo.LoginHuman & ";" & vbNewLine
                    r &= "MostHumansDate;" & uTimeP.GetNow & ";" & vbNewLine
                Else
                    r &= "MostHumansNr;" & maxHuman & ";" & vbNewLine
                    r &= "MostHumansDate;" & maxHumanDate & ";" & vbNewLine
                End If
                If servInfo.LoginAkkan >= maxAkkan Then
                    r &= "MostAkkansNr;" & servInfo.LoginAkkan & ";" & vbNewLine
                    r &= "MostAkkansDate;" & uTimeP.GetNow & ";" & vbNewLine
                Else
                    r &= "MostAkkansNr;" & maxAkkan & ";" & vbNewLine
                    r &= "MostAkkansDate;" & maxAkkanDate & ";" & vbNewLine
                End If
                For h As Integer = 0 To servInfo.LoginMapZones.Length - 1
                    Dim zone As Integer = servInfo.LoginMapZones(h)
                    Dim name As String = "[zone:" & zone & "]"
                    For Each sitems As ServerItem In serverItems.items
                        If sitems.mapServerZone = zone Then
                            name = sitems.name.Replace(" ", "_") & name
                        End If
                    Next
                    Dim map As Integer = mapIndexInstruct(mapCounts, name)
                    If map < 0 Then
                        Dim nStruct As New mostOnlineMap
                        nStruct.name = name
                        nStruct.daate = uTimeP.GetNow
                        nStruct.count = servInfo.LoginMap(h)
                        mapCounts.Add(nStruct)
                        map = mapCounts.Count - 1
                    Else
                        Dim nStruct As mostOnlineMap = mapCounts(map)
                        If servInfo.LoginMap(h) >= nStruct.count Then
                            mapCounts(map).count = servInfo.LoginMap(h)
                            mapCounts(map).daate = uTimeP.GetNow
                        End If
                    End If
                Next
                For Each m As mostOnlineMap In mapCounts
                    r &= "MostMapNr_" & m.name & ";" & m.count & ";" & vbNewLine
                    r &= "MostMapDate_" & m.name & ";" & m.daate & ";" & vbNewLine
                Next
                sw.Write(r)
                sw.Close()
            Catch ex As Exception
                logit("ERR: in status file: " & ex.Message)
            End Try
            Try
                Dim uTime As New UnixTimestamp
                Dim timeStamp As Long = uTime.GetUnixTimestamp(Now(), True, Now.Hour, Now.Minute, Now.Second)
                Dim sw As New IO.StreamWriter(onlineLogFile, True)
                Dim s As String = timeStamp & ";" & servInfo.LoginTotal & ";" & servInfo.LoginHuman & ";" & servInfo.LoginAkkan & ";"
                For h As Integer = 0 To servInfo.LoginMapZones.Length - 1
                    s &= "z=" & servInfo.LoginMapZones(h) & ",c=" & servInfo.LoginMap(h) & ":"
                Next
                sw.WriteLine(s)
                sw.Close()
            Catch ex As Exception
                logit("ERR: in online log file: " & ex.Message)
            End Try
        End If
    End Sub

    Private Sub statusItem_ShowLog(ByRef sender As CStatus, ByRef sItem As ServerItem)
        If Not sItem.hook Is Nothing AndAlso sItem.hook.isHooked Then apiHook.showWindowEx(sItem.hook.ownerHwnd)
    End Sub

    Private Sub statusItem_HideLog(ByRef sender As CStatus, ByRef sItem As ServerItem)
        If Not sItem.hook Is Nothing AndAlso sItem.hook.isHooked Then apiHook.hideWindowEx(sItem.hook.ownerHwnd)
    End Sub

    Private Sub lblCopyright_LinkClicked(ByVal sender As System.Object, ByVal e As System.Windows.Forms.LinkLabelLinkClickedEventArgs) Handles lblCopyright.LinkClicked
        Process.Start("mailto:tommy2@hot.ee?subject=RYLServerController")
    End Sub

    Private Sub btnReCatch_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnReCatch.Click
        reCatch()
    End Sub

    Private Sub btnOpenIRC_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOpenIRC.Click
        Dim f As New frmChatRoom(ircBot.irc)
        f.Show()
    End Sub

    Private Sub tmrRunTime_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles tmrRunTime.Tick
        progRunTime += 1
    End Sub

    Private Sub tmrMultiCrashWaiter_Tick(ByVal sender As Object, ByVal e As System.EventArgs)
        sender.stop()
        crashTimers.Remove(sender)
        Dim p As Integer = sender.Tag
        reCatch()
        If serverItems.items.Length > p AndAlso serverItems.items(p).online = False Then
            processExitPhase2(p, False)
        End If
    End Sub
End Class

Public Class UnixTimestamp

    Private mdtIniUnixDate As DateTime

    Public Sub New()
        mdtIniUnixDate = New DateTime(1970, 1, 1, 0, 0, 0)
    End Sub

    Public Function GetDate(ByVal TimestampToConvert As Long, ByVal Local As Boolean) As DateTime
        If Local Then
            Return mdtIniUnixDate.AddSeconds(TimestampToConvert).ToLocalTime
        Else
            Return mdtIniUnixDate.AddSeconds(TimestampToConvert)
        End If
    End Function

    Public Function GetUnixTimestamp(ByVal DateToConvert As DateTime, ByVal Local As Boolean, Optional ByVal Hour As Integer = 0, Optional ByVal Minut As Integer = 0, Optional ByVal Second As Integer = 0) As Long
        Dim dtCurrent As New DateTime(DateToConvert.Year, DateToConvert.Month, DateToConvert.Day, Hour, Minut, Second)
        If Local Then
            Return CLng((dtCurrent.ToUniversalTime.Subtract(mdtIniUnixDate)).TotalSeconds)
        Else
            Return CLng((dtCurrent.Subtract(mdtIniUnixDate)).TotalSeconds)
        End If
    End Function
    Public Function GetUnixTimestamp2(ByVal DateToConvert As DateTime, ByVal Local As Boolean) As Long
        Dim dtCurrent As New DateTime(DateToConvert.Year, DateToConvert.Month, DateToConvert.Day, DateToConvert.Hour, DateToConvert.Minute, DateToConvert.Second)
        If Local Then
            Return CLng((dtCurrent.ToUniversalTime.Subtract(mdtIniUnixDate)).TotalSeconds)
        Else
            Return CLng((dtCurrent.Subtract(mdtIniUnixDate)).TotalSeconds)
        End If
    End Function
    Public Function GetNow(Optional ByVal local As Boolean = True) As Long
        Dim dtCurrent As DateTime = Now
        If local Then
            Return CLng((dtCurrent.ToUniversalTime.Subtract(mdtIniUnixDate)).TotalSeconds)
        Else
            Return CLng((dtCurrent.Subtract(mdtIniUnixDate)).TotalSeconds)
        End If
    End Function
End Class