<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="AutoGenExample.aspx.cs" Inherits="NeoEAVWeb.AutoGenExample" %>
<%@ Register Assembly="EAVEntitiesLib" Namespace="NeoEAV.Web.UI" TagPrefix="eav" %>
<%@ Register Assembly="EAVEntitiesLib" Namespace="NeoEAV.Web.UI.AutoGen" TagPrefix="auto" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <p>
                <asp:DropDownList runat="server" ID="ctlSubjects" AutoPostBack="true" OnSelectedIndexChanged="ctlSubjects_SelectedIndexChanged"></asp:DropDownList>
                <asp:DropDownList runat="server" ID="ctlContainers" AutoPostBack="true" OnSelectedIndexChanged="ctlContainers_SelectedIndexChanged"></asp:DropDownList>
                <asp:Button runat="server" ID="ctlSaveButton" OnClick="ctlSaveButton_Click" Text="Save" />
            </p>
            <div style="padding-top: 35px;">
                <eav:eavprojectcontextcontrol runat="server" id="ctlProjectContext" contextkey="Test Project 1">
                <asp:Panel runat="server" GroupingText='<%# Eval("Description") %>'>
                    <eav:EAVSubjectContextControl runat="server" ID="ctlSubjectContext" ContextKey="">
                        <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                            Subject:
                            <asp:Label runat="server" Text='<%# Eval("Container.MemberID") %>'></asp:Label>
                        </div>
                        <hr />
                        <auto:EAVAutoContainerContextControl runat="server" ID="ctlContainerContext" ContextKey="Test Root Container 1" />
                        <hr />
                    </eav:EAVSubjectContextControl>
                </asp:Panel>
            </eav:eavprojectcontextcontrol>
            </div>
        </div>
    </form>
</body>
</html>
