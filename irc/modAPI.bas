Attribute VB_Name = "modAPI"
Option Explicit

Public Declare Function inet_addr Lib "wsock32.dll" (ByVal addr As String) As Long
Public Declare Function htonl Lib "wsock32.dll" (ByVal hostlong As Long) As Long
Public Declare Function ntohl Lib "wsock32.dll" (ByVal netlong As Long) As Long
Public Declare Function inet_ntoa Lib "wsock32.dll" (ByVal inn As Long) As Long
Public Declare Function lstrlen Lib "kernel32" Alias "lstrlenA" (ByVal lpString As Any) As Long
Public Declare Sub MemCopy Lib "kernel32" Alias "RtlMoveMemory" (Dest As Any, Src As Any, ByVal cb&)
Public Declare Function GetTickCount Lib "kernel32" () As Long
