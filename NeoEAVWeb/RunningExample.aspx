<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RunningExample.aspx.cs" Inherits="NeoEAVWeb.RunningExample" %>
<%@ Register Assembly="EAVEntitiesLib" Namespace="NeoEAV.Web.UI" TagPrefix="eav" %>

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
            <asp:Button runat="server" ID="ctlSaveButton" OnClick="ctlSaveButton_Click" Text="Save" />
        </p>
        <p>
            <eav:EAVProjectContextControl runat="server" ID="ctlProjectContext" ContextSelector="Test Project 2">
                <eav:EAVSubjectContextControl runat="server" ID="ctlSubjectContext" ContextSelector="">
                    <eav:EAVContainerContextControl runat="server" ID="ctlRootContainer" ContextSelector="Test Root Container 2">
                        <eav:EAVInstanceContextRepeater runat="server" ID="ctlInstanceRepeater">
                            <ItemTemplate>
                                <div style="padding: 5px;">
                                    <eav:EAVAttributeContextControl runat="server" ContextSelector="Test Attribute 6">
                                        <eav:EAVTextBox runat="server" ID="ctlValue11" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <eav:EAVAttributeContextControl runat="server" ContextSelector="Test Attribute 7">
                                        <eav:EAVTextBox runat="server" ID="ctlValue12" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <eav:EAVAttributeContextControl runat="server" ContextSelector="Test Attribute 8">
                                        <eav:EAVTextBox runat="server" ID="ctlValue13" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <eav:EAVAttributeContextControl runat="server" ContextSelector="Test Attribute 9">
                                        <eav:EAVTextBox runat="server" ID="ctlValue14" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <eav:EAVAttributeContextControl runat="server" ContextSelector="Test Attribute 10">
                                        <eav:EAVTextBox runat="server" ID="ctlValue15" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                </div>
                            </ItemTemplate>
                        </eav:EAVInstanceContextRepeater>
                    </eav:EAVContainerContextControl>
                </eav:EAVSubjectContextControl>
            </eav:EAVProjectContextControl>
        </p>
    </div>
    </form>
</body>
</html>
