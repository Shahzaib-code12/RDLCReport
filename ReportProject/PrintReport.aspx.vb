Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.Drawing.Printing
Imports System.IO
Imports System.Text
Imports Microsoft.Reporting.WebForms

Partial Public Class PrintReport
    Inherits System.Web.UI.Page

    Private streamFiles As List(Of String)
    Private openStreams As List(Of FileStream)
    Private currentPageIndex As Integer

    Protected Sub Page_Load(sender As Object, e As EventArgs) Handles Me.Load
        If Not IsPostBack Then
            BindReport()
        End If
    End Sub

    Private Sub BindReport()
        Dim payID As Long = 1

        ReportViewer1.LocalReport.ReportPath = Server.MapPath("~/EmpPayReport.rdlc")
        ReportViewer1.LocalReport.DataSources.Clear()

        Dim detailsAdapter As New dsEmpPayReportTableAdapters.sp_Get_EmpPay_DetailsTableAdapter()
        Dim detailsTable As dsEmpPayReport.sp_Get_EmpPay_DetailsDataTable = detailsAdapter.GetData(payID)
        Dim detailsRds As New ReportDataSource("dsEmpPayReport_sp_Get_EmpPay_Details", CType(detailsTable, System.Data.DataTable))
        ReportViewer1.LocalReport.DataSources.Add(detailsRds)

        Dim headerAdapter As New dsEmpPayReportTableAdapters.sp_Get_EmpPay_HeaderTableAdapter()
        Dim headerTable As dsEmpPayReport.sp_Get_EmpPay_HeaderDataTable = headerAdapter.GetData(payID)
        Dim headerRds As New ReportDataSource("dsEmpPayReport_sp_Get_EmpPay_Header", CType(headerTable, System.Data.DataTable))
        ReportViewer1.LocalReport.DataSources.Add(headerRds)

        ReportViewer1.LocalReport.Refresh()
    End Sub

    Private Function CreateStream(name As String, fileNameExtension As String, encoding As Encoding, mimeType As String, willSeek As Boolean) As Stream
        Dim safeName As String = name.Replace(":", "_").Replace("/", "_").Replace("\", "_")
        Dim filename As String = Server.MapPath("~/Temp/" & safeName & "." & fileNameExtension)
        Dim fs As New FileStream(filename, FileMode.Create, FileAccess.Write)

        streamFiles.Add(filename)
        openStreams.Add(fs)

        Return fs
    End Function

    Private Sub ExportToImage(report As LocalReport)
        Dim deviceInfo As String =
        "<DeviceInfo>" &
        "  <OutputFormat>EMF</OutputFormat>" &
        "  <PageWidth>3in</PageWidth>" &
        "  <PageHeight>4in</PageHeight>" &
        "  <MarginTop>0in</MarginTop>" &
        "  <MarginLeft>0in</MarginLeft>" &
        "  <MarginRight>0in</MarginRight>" &
        "  <MarginBottom>0in</MarginBottom>" &
        "</DeviceInfo>"

        Dim warnings As Warning() = Nothing
        streamFiles = New List(Of String)()
        openStreams = New List(Of FileStream)()

        report.Render("Image", deviceInfo, AddressOf CreateStream, warnings)

        For Each s In openStreams
            s.Close()
            s.Dispose()
        Next
    End Sub

    Private Sub PrintPage(sender As Object, e As PrintPageEventArgs)
        Using fs As New FileStream(streamFiles(currentPageIndex), FileMode.Open, FileAccess.Read)
            Using pageImage As New Metafile(fs)
                e.Graphics.DrawImage(pageImage, e.MarginBounds)
            End Using
        End Using
        currentPageIndex += 1
        e.HasMorePages = (currentPageIndex < streamFiles.Count)
    End Sub

    Protected Sub btnPrintThermal_Click(sender As Object, e As EventArgs)
        Dim tempFolder As String = Server.MapPath("~/Temp/")
        If Not Directory.Exists(tempFolder) Then
            Directory.CreateDirectory(tempFolder)
        End If

        ExportToImage(ReportViewer1.LocalReport)

        Dim pd As New PrintDocument()
        pd.PrinterSettings.PrinterName = "BlackCopper 80mm Series"

        If Not pd.PrinterSettings.IsValid Then
            Response.Write("<script>alert('Printer not found. Check printer name.');</script>")
            Return
        End If

        ' Printer ke apne "297 mm" size ko exact naam se dhoondo (sabse behtar match)
        Dim selectedSize As PaperSize = Nothing
        For Each ps As PaperSize In pd.PrinterSettings.PaperSizes
            If ps.PaperName.Contains("297") Then
                selectedSize = ps
                Exit For
            End If
        Next

        If selectedSize Is Nothing Then
            selectedSize = pd.DefaultPageSettings.PaperSize
        End If

        pd.DefaultPageSettings.PaperSize = selectedSize
        pd.DefaultPageSettings.Margins = New Margins(0, 0, 0, 0)
        pd.OriginAtMargins = False

        AddHandler pd.PrintPage, AddressOf PrintPage

        currentPageIndex = 0

        pd.Print()

        For Each s In streamFiles
            Try
                File.Delete(s)
            Catch
            End Try
        Next
    End Sub

End Class