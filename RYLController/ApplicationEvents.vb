Namespace My
    Class props
        Public Shared startProg As Boolean = True
        Public Shared autoStartServer As Boolean = False
        Public Shared gotCrash As Boolean = False
        Public Shared logFile As String = "RYLServerController_crash.log"
    End Class
    ' The following events are availble for MyApplication:
    ' 
    ' Startup: Raised when the application starts, before the startup form is created.
    ' Shutdown: Raised after all application forms are closed.  This event is not raised if the application terminates abnormally.
    ' UnhandledException: Raised if the application encounters an unhandled exception.
    ' StartupNextInstance: Raised when launching a single-instance application and the application is already active. 
    ' NetworkAvailabilityChanged: Raised when the network connection is connected or disconnected.
    Partial Friend Class MyApplication

        Private Sub MyApplication_Startup(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.StartupEventArgs) Handles Me.Startup
            Dim winStartUp As Boolean = False
            For Each c As String In e.CommandLine
                If c = "/startup" Then
                    winStartUp = True
                End If
            Next
            If winStartUp Then
                Dim frmS As New frmStartupCanceler
                Dim res As DialogResult = frmS.ShowDialog
                If res = DialogResult.OK Then 'kill the startup
                    e.Cancel = True
                    My.props.startProg = False
                Else 'start up
                    e.Cancel = False
                    My.props.autoStartServer = True
                End If
            End If
        End Sub

        Public Shared Sub MyApplication_UnhandledException(ByVal sender As Object, ByVal e As Microsoft.VisualBasic.ApplicationServices.UnhandledExceptionEventArgs) Handles Me.UnhandledException

            Try
                My.props.gotCrash = True
                'write log
                Dim line As String = Now() & " : [X0] " & e.Exception.Message & " - " & e.Exception.GetBaseException.Message & " - " & e.Exception.GetBaseException.StackTrace
                If Not My.Computer.FileSystem.FileExists(props.logFile) OrElse FileSystem.FileLen(props.logFile) < 1 Then
                    line = "RYL Server controller log" & vbNewLine & vbNewLine & "Log started at " & Now() & vbNewLine & "---------------------------------------------------------------------" & vbNewLine & line
                End If
                Try
                    Dim sr As New IO.StreamWriter(props.logFile, True)
                    sr.WriteLine(line)
                    sr.Close()
                Catch ex As Exception
                    MsgBox("Got crash. Problem with log file." & vbNewLine & "Please check you'r ini file." & vbNewLine & "Crash details: " & e.Exception.Message & vbNewLine & e.Exception.StackTrace, MsgBoxStyle.Critical, "RYLServerController")
                End Try
                'restart
                Dim i As String = Windows.Forms.Application.ExecutablePath
                Dim bat_file As String = My.Computer.FileSystem.SpecialDirectories.Temp & "\rsc.cmd"
                Dim sw As New IO.StreamWriter(bat_file)
                sw.WriteLine("SLEEP 3")
                sw.WriteLine("start /b " & i & " /crash")
                sw.WriteLine("DELETE " & bat_file)
                sw.Close()
                Shell(bat_file, AppWinStyle.MinimizedNoFocus, False)

                e.ExitApplication = True
                frmMain.Visible = False
            Catch ex As Exception
            End Try
        End Sub
    End Class

End Namespace

