﻿Imports Microsoft.Office.Interop
Imports System.Net

Public Class Form1

    Private _interval As Integer
    Private _Email As String
    Private _SMTPServer As String 'This is used to send error messages only!

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Me.Hide()
        _interval = Int(GetConfig("interval"))
        _Email = GetConfig("to")
        _SMTPServer = GetConfig("smtpserver") 'This is used to send error messages only!
        Timer1.Interval = _interval * 1000
        Call ForwardMyMail()
        Timer1.Start()
        Button1.Text = "stop"
    End Sub

    Private Sub ForwardMyMail()
        Dim OLapp As New Outlook.Application()
        Dim OLNS As Outlook.NameSpace = OLapp.GetNamespace("MAPI")
        Dim OLFolder As Outlook.MAPIFolder = OLNS.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderInbox)
        Dim OLMailItem As Outlook.MailItem
        Dim OLMailItemFWD As Outlook.MailItem
        Dim OLMAILItemGeneric As Object
        Try
            For Each OLMAILItemGeneric In OLFolder.Items
                Try
                    OLMailItem = OLMAILItemGeneric
                    If OLMailItem.UnRead Then
                        Try
                            OLMailItemFWD = OLMailItem.Forward
                            OLMailItemFWD.To = _Email
                            OLMailItemFWD.AutoForwarded = False
                            OLMailItemFWD.Send()
                            OLMailItem.UnRead = False
                            DoMessage(OLMailItem.SenderEmailAddress & ": " & Now().ToString(), "message")
                            'OLMailItem.Delete()

                        Catch ex2 As Exception
                            OLMailItem.UnRead = True
                        End Try
                    End If
                Catch ex As Exception
                    Call SendInvite()
                End Try
            Next
            DoMessage("Last check: " & Now().ToString(), "message")
        Catch ex As Exception
            DoMessage("Error!" & ex.ToString(), "error")
            Call SendErrorText()
        Finally
            OLapp = Nothing
            OLNS = Nothing
            OLFolder = Nothing
            OLMailItem = Nothing
            OLMailItemFWD = Nothing

        End Try

    End Sub

    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        Call ForwardMyMail()
        Button1.Text = "stop"

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        If Button1.Text = "start" Then
            Timer1.Start()
            Button1.Text = "stop"
        Else
            Timer1.Stop()
            Button1.Text = "start"
        End If
    End Sub

    Private Sub DoMessage(ByVal sMessage As String, ByVal sType As String)
        If sMessage.Length > 63 Then
            NotifyIcon1.Text = sMessage.Substring(0, 63)
        Else
            NotifyIcon1.Text = sMessage
        End If
        rtbDisplay.Text = sMessage & vbCrLf & rtbDisplay.Text
        If sType = "error" Then
            NotifyIcon1.BalloonTipText = sMessage
        End If
    End Sub

    Private Sub NotifyIcon1_MouseDoubleClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.MouseEventArgs) Handles NotifyIcon1.MouseDoubleClick
        Me.Show()
        Me.ShowInTaskbar = True
        Me.WindowState = FormWindowState.Normal
    End Sub

    Private Sub Form1_Deactivate(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Deactivate
        Me.ShowInTaskbar = False
        Me.WindowState = FormWindowState.Minimized
    End Sub

    Public Function GetConfig(ByVal strElement) As String
        Dim objXMl As New Xml.XmlDocument()

        Call objXMl.Load("config.xml")

        Dim objXMLNodes As Xml.XmlNodeList = objXMl.GetElementsByTagName(strElement)
        If objXMLNodes.Count = 0 Then
            Return ""
        Else
            Return objXMLNodes(0).InnerText
        End If
        objXMl = Nothing

    End Function

    Public Sub SendErrorText()
        Dim sendnow As New System.Net.Mail.SmtpClient(_SMTPServer)
        Try
            ' sendnow.Send(GetConfig("ErrorFrom"), GetConfig("ErrorTo"), "DeVry outlooksender isnt working", "")
        Catch e As Exception

        End Try
        sendnow = Nothing
        ''  Timer1.Stop()
        '  Button1.Text = "Start"
    End Sub

    Public Sub SendInvite()
        Dim sendnow As New System.Net.Mail.SmtpClient(_SMTPServer)
        Try
            sendnow.Send(GetConfig("ErrorFrom"), GetConfig("ErrorTo"), "You might have an invite in your DeVry Email", "")
        Catch e As Exception

        End Try
        sendnow = Nothing
        Timer1.Stop()
        Button1.Text = "Start"
    End Sub

End Class
