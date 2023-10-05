Public Class CConfigFile
    Public ini As CIniFile
    Public Sub New()
        ini = New CIniFile(Me.FileName)
    End Sub

    Public ReadOnly Property FileName() As String
        Get
            Dim full As String = Application.ExecutablePath
            Dim a() As String = full.Split("\")
            Return Me.appPath & a(UBound(a)).Split(".")(0) & ".ini"
        End Get
    End Property

    Public ReadOnly Property appPath() As String
        Get
            Dim full As String = Application.ExecutablePath
            Dim a() As String = full.Split("\")
            Return full.Substring(0, full.Length - a(UBound(a)).Length)
        End Get
    End Property

    Public Function getList(ByVal section As String, Optional ByVal prefix As String = "item_") As String()
        Dim out() As String = {}
        Dim finished As Boolean = False
        Dim i As Integer = 0
        While Not finished
            i += 1
            Dim serv As String = ini.GetString(section, prefix & i, "")
            If serv <> "" Then
                addStrToArray(out, serv)
            Else
                finished = True
            End If
        End While
        Return out
    End Function

    Public Sub saveList(ByVal items() As String, ByVal section As String, Optional ByVal prefix As String = "item_")
        Dim current() As String = getList(section, prefix)
        Dim i As Integer = 0
        If current.Length > items.Length Then
            For i = current.Length To items.Length + 1 Step -1
                ini.WriteString(section, prefix & i, Nothing)
            Next
        End If
        i = 0
        For Each serv As String In items
            If serv <> "" Then
                i += 1
                ini.WriteString(section, prefix & i, serv)
            End If
        Next
    End Sub

    Public Shared Sub addStrToArray(ByRef arr() As String, ByVal obj As String)
        If arr Is Nothing Then ReDim arr(0) Else ReDim Preserve arr(UBound(arr) + 1)
        arr(UBound(arr)) = obj
    End Sub
End Class

Public Class CIniFile
    ' API functions
    Private Declare Ansi Function GetPrivateProfileString _
      Lib "kernel32.dll" Alias "GetPrivateProfileStringA" _
      (ByVal lpApplicationName As String, _
      ByVal lpKeyName As String, ByVal lpDefault As String, _
      ByVal lpReturnedString As System.Text.StringBuilder, _
      ByVal nSize As Integer, ByVal lpFileName As String) _
      As Integer
    Private Declare Ansi Function WritePrivateProfileString _
      Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
      (ByVal lpApplicationName As String, _
      ByVal lpKeyName As String, ByVal lpString As String, _
      ByVal lpFileName As String) As Integer
    Private Declare Ansi Function GetPrivateProfileInt _
      Lib "kernel32.dll" Alias "GetPrivateProfileIntA" _
      (ByVal lpApplicationName As String, _
      ByVal lpKeyName As String, ByVal nDefault As Integer, _
      ByVal lpFileName As String) As Integer
    Private Declare Ansi Function FlushPrivateProfileString _
      Lib "kernel32.dll" Alias "WritePrivateProfileStringA" _
      (ByVal lpApplicationName As Integer, _
      ByVal lpKeyName As Integer, ByVal lpString As Integer, _
      ByVal lpFileName As String) As Integer
    Dim strFilename As String

    ' Constructor, accepting a filename
    Public Sub New(ByVal Filename As String)
        strFilename = Filename
    End Sub

    ' Read-only filename property
    ReadOnly Property FileName() As String
        Get
            Return strFilename
        End Get
    End Property

    Public Function GetString(ByVal Section As String, _
      ByVal Key As String, ByVal def As String) As String
        ' Returns a string from your INI file
        Dim intCharCount As Integer
        Dim objResult As New System.Text.StringBuilder(256)
        intCharCount = GetPrivateProfileString(Section, Key, _
           def, objResult, objResult.Capacity, strFilename)
        If intCharCount > 0 Then Return Left(objResult.ToString, intCharCount) Else Return def
    End Function

    Public Function GetInteger(ByVal Section As String, _
      ByVal Key As String, ByVal [Default] As Integer) As Integer
        ' Returns an integer from your INI file
        Return GetPrivateProfileInt(Section, Key, _
           [Default], strFilename)
    End Function

    Public Function GetBoolean(ByVal Section As String, _
      ByVal Key As String, ByVal [Default] As Boolean) As Boolean
        ' Returns a boolean from your INI file
        Return (GetPrivateProfileInt(Section, Key, _
           CInt([Default]), strFilename) = 1)
    End Function

    Public Sub WriteString(ByVal Section As String, _
      ByVal Key As String, ByVal Value As String)
        ' Writes a string to your INI file
        WritePrivateProfileString(Section, Key, Value, strFilename)
        Flush()
    End Sub

    Public Sub WriteInteger(ByVal Section As String, _
      ByVal Key As String, ByVal Value As Integer)
        ' Writes an integer to your INI file
        WriteString(Section, Key, CStr(Value))
        Flush()
    End Sub

    Public Sub WriteBoolean(ByVal Section As String, _
      ByVal Key As String, ByVal Value As Boolean)
        ' Writes a boolean to your INI file
        WriteString(Section, Key, CStr(IIf(Value, 1, 0)))
        Flush()
    End Sub

    Private Sub Flush()
        ' Stores all the cached changes to your INI file
        FlushPrivateProfileString(0, 0, 0, strFilename)
    End Sub

End Class
