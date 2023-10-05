Public Class frmChatRoom
    Private WithEvents irc As ircClient
    Private showRaw As Boolean = False
    Public Delegate Sub textadd(ByVal txt As String, ByVal colorindex As Integer)
    Public Delegate Sub GotChUsersDel(ByVal sender As ircClient, ByVal channel As String, ByVal users() As String)
    Public colorTable As Color() = {Color.White, Color.Blue, Color.Red, Color.Green, Color.Purple, Color.Yellow}
    Public Sub New(ByVal iIrc As ircClient)
        InitializeComponent()
        irc = iIrc
    End Sub
    Private Sub parseInput(ByVal txt As String)
        If txt.Split(" ").Length > 0 AndAlso txt.Split(" ")(0) = "/msg" Then
            txt = "/PRIVMSG " & txt.Substring(txt.Split(" ")(0).Length + 1)
        End If
        If txt = "/raw on" Then
            showRaw = True
        ElseIf txt = "/raw off" Then
            showRaw = False
        ElseIf txt.Split(" ").Length > 0 AndAlso txt.Split(" ")(0) = "/m" Then
            irc.SendNotice(irc.nick, txt.Substring(txt.Split(" ")(0).Length + 1))
            addToTxt("S:" & txt.Substring(txt.Split(" ")(0).Length + 1), 2)
        Else
            If irc Is Nothing OrElse irc.irc Is Nothing Then
                addToTxt("You are not connected", 2)
            Else
                If txt(0) = "/" Then
                    txt = txt.Substring(1)
                    addToTxt(": " & txt, 0)
                    irc.irc.Say(txt)
                Else
                    addToTxt("<" & irc.nick & "> " & txt, 0)
                    irc.SendMessage(irc.activeChannel, txt)
                End If
            End If
        End If
    End Sub
    Private Function d2(ByVal num As String) As String
        If num.Length < 2 Then num = "0" & num
        Return num
    End Function
    Private Sub addToTxt(ByVal txt As String, ByVal colorindex As Integer)
        If Me.InvokeRequired Then
            Me.Invoke(New textadd(AddressOf addToTxt), New Object() {txt, colorindex})
        ElseIf Not Me Is Nothing AndAlso Not Me.txtMain Is Nothing Then
            Dim ntext As String = vbNewLine & "[" & d2(Now.Hour) & ":" & d2(Now.Minute) & ":" & d2(Now.Second) & "] " & txt
            Me.txtMain.AppendText(ntext)
            Me.txtMain.Select(Me.txtMain.Text.Length - ntext.Length + 1, ntext.Length)
            Me.txtMain.SelectionColor = colorTable(colorindex)
            Me.txtMain.Select(Me.txtMain.Text.Length, 0)
            Me.txtMain.ScrollToCaret()
        End If
    End Sub

    Private Sub irc_ChangedNick(ByVal sender As ircClient, ByVal who As String, ByVal towho As String) Handles irc.ChangedNick
        addToTxt(who & " is now known as " & towho, 3)
    End Sub
    Private Sub irc_channelJoinOk(ByVal sender As ircClient) Handles irc.channelJoinOk
        addToTxt("You have joined the channel", 1)
    End Sub

    Private Sub irc_channelPartOk(ByVal sender As ircClient, ByVal message As String) Handles irc.channelPartOk
        addToTxt("You left the channel: " & message, 2)
    End Sub

    Private Sub irc_connected(ByVal sender As ircClient) Handles irc.connected
        addToTxt("You are now connected", 1)
    End Sub

    Private Sub irc_GotAction(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal message As String) Handles irc.GotAction
        addToTxt("*" & who & " " & message, 4)
    End Sub

    Private Sub irc_GotChUsers(ByVal sender As ircClient, ByVal channel As String, ByVal users() As String) Handles irc.GotChUsers
        If Me.InvokeRequired Then
            Me.Invoke(New GotChUsersDel(AddressOf irc_GotChUsers), New Object() {sender, channel, users})
        Else
            Me.lstUsers.Items.Clear()
            Array.Sort(users)
            For Each u As String In users
                Me.lstUsers.Items.Add(u)
            Next
        End If
    End Sub

    Private Sub irc_GotGlobalMess(ByVal sender As ircClient, ByVal who As String, ByVal whom As String, ByVal message As String) Handles irc.GotGlobalMess
        addToTxt(who & " : " & message, 2)
    End Sub

    Private Sub irc_GotKicked(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal reason As String) Handles irc.GotKicked
        irc_GotChUsers(irc, "", New String() {})
        addToTxt(who & " kicked you from #" & channel & ": " & reason, 2)
    End Sub

    Private Sub irc_GotMessage(ByVal sender As ircClient, ByVal channel As String, ByVal who As String, ByVal message As String) Handles irc.GotMessage
        addToTxt("<" & who & "> " & message, 0)
    End Sub

    Private Sub irc_GotNotice(ByVal sender As ircClient, ByVal who As String, ByVal message As String) Handles irc.GotNotice
        addToTxt("[notice]<" & who & "> " & message, 5)
    End Sub

    Private Sub irc_GotPM(ByVal sender As ircClient, ByVal who As String, ByVal message As String) Handles irc.GotPM
        addToTxt("[PM]<" & who & "> " & message, 1)
    End Sub

    Private Sub txtInput_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txtInput.KeyDown
        If e.KeyCode = Keys.Return Then
            parseInput(Me.txtInput.Text)
            Me.txtInput.Text = ""
            e.Handled = False
            e.SuppressKeyPress = True
        End If
    End Sub

    Private Sub irc_GotRawMessage(ByVal sender As ircClient, ByVal txt As String) Handles irc.GotRawMessage
        If showRaw Then
            addToTxt(txt, 3)
        End If
    End Sub

    Private Sub frmChatRoom_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
        If Not irc Is Nothing AndAlso Not irc.LastUserList Is Nothing AndAlso irc.LastUserList.Length > 0 Then
            irc_GotChUsers(irc, irc.activeChannel, irc.LastUserList)
        End If
    End Sub

    Private Sub irc_UserJoinCh(ByVal sender As ircClient, ByVal who As String) Handles irc.UserJoinCh
        addToTxt(who & " has joined the channel", 3)
    End Sub

    Private Sub irc_UserPartCh(ByVal sender As ircClient, ByVal who As String, ByVal message As String) Handles irc.UserPartCh
        addToTxt(who & " has left the channel: " & message, 3)
    End Sub
End Class