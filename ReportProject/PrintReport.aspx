<%@ Page Language="VB" AutoEventWireup="true" CodeBehind="PrintReport.aspx.vb" Inherits="ReportProject.PrintReport" %>
<%@ Register Assembly="Microsoft.ReportViewer.WebForms" Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html>
<html>
<head runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <title>Print Report</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:ScriptManager ID="ScriptManager1" runat="server" />
            <rsweb:ReportViewer ID="ReportViewer1" runat="server" Width="900px" Height="600px" ShowPrintButton="false" />
            <br />
            <asp:Button ID="btnPrintThermal" runat="server" Text="Print on Thermal Printer" OnClick="btnPrintThermal_Click" />
        </div>
    </form>
</body>
</html>