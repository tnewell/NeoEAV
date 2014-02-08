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
            <div style="padding: 5px;">
                <asp:DropDownList runat="server" ID="ctlSubjects" AutoPostBack="true" OnSelectedIndexChanged="ctlSubjects_SelectedIndexChanged"></asp:DropDownList>
                <asp:Button runat="server" ID="ctlSaveButton" OnClick="ctlSaveButton_Click" Text="Save" />
            </div>
            <div style="padding-top: 35px;">
                <eav:EAVProjectContextControl runat="server" ID="ctlProjectContext" ContextKey="Test Project 2">
                    <asp:Panel runat="server" GroupingText='<%# DataBinder.Eval(Container.DataItem, "Description") %>'>
                        <eav:EAVSubjectContextControl runat="server" ID="ctlSubjectContext">
                            <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                Subject:
                                <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "MemberID") %>'></asp:Label>
                            </div>
                            <eav:EAVContainerContextControl runat="server" ID="ctlRootContainer" ContextKey="Test Root Container 2">
                                <asp:Panel runat="server" GroupingText='<%# DataBinder.GetPropertyValue(Container.DataItem, "Name") %>'>
                                    <eav:EAVInstanceContextRepeater runat="server" ID="ctlInstanceRepeater">
                                        <ItemTemplate>
                                            <eav:EAVInstanceContextControl runat="server">
                                                <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                                    Instance:
                                                    <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "RepeatInstance") %>'></asp:Label>
                                                </div>
                                                <div style="padding: 5px;">
                                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 6">
                                                        <eav:EAVTextBox runat="server" ID="ctlValue11" Width="200"></eav:EAVTextBox>
                                                    </eav:EAVAttributeContextControl>
                                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 7">
                                                        <eav:EAVTextBox runat="server" ID="ctlValue12" Width="200"></eav:EAVTextBox>
                                                    </eav:EAVAttributeContextControl>
                                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 8">
                                                        <eav:EAVTextBox runat="server" ID="ctlValue13" Width="200"></eav:EAVTextBox>
                                                    </eav:EAVAttributeContextControl>
                                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 9">
                                                        <eav:EAVTextBox runat="server" ID="ctlValue14" Width="200"></eav:EAVTextBox>
                                                    </eav:EAVAttributeContextControl>
                                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 10">
                                                        <eav:EAVTextBox runat="server" ID="ctlValue15" Width="200"></eav:EAVTextBox>
                                                    </eav:EAVAttributeContextControl>
                                                </div>
                                            </eav:EAVInstanceContextControl>
                                        </ItemTemplate>
                                    </eav:EAVInstanceContextRepeater>
                                </asp:Panel>
                            </eav:EAVContainerContextControl>
                        </eav:EAVSubjectContextControl>
                    </asp:Panel>
                </eav:EAVProjectContextControl>
            </div>
        </div>
    </form>
</body>
</html>
