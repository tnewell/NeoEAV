<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Sandbox.aspx.cs" Inherits="NeoEAVWeb.Sandbox" %>
<%@ Register Assembly="EAVEntitiesLib" Namespace="NeoEAV.Web.UI" TagPrefix="eav" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <asp:Button runat="server" ID="ctlGoButton" OnClick="ctlGoButton_Click" Text="Go" /> 
        </div>
        <div>
            <a href="RunningExample.aspx">Running Form Example</a>
            <br />
            <a href="RecurringExample.aspx">Recurring Form Example</a>
            <br />
            <a href="OneShotExample.aspx">One-Shot Form Example</a>
        </div>
    </form>
</body>
</html>
