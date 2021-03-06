﻿<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="OneShotExample.aspx.cs" Inherits="NeoEAVWeb.OneShotExample" %>
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
        <div style="padding: 5px;">
            <asp:DropDownList runat="server" ID="ctlSubjects" AutoPostBack="true" OnSelectedIndexChanged="ctlSubjects_SelectedIndexChanged"></asp:DropDownList>
            <asp:Button runat="server" ID="ctlSaveButton" OnClick="ctlSaveButton_Click" Text="Save" />
        </div>
        <div style="padding-top: 35px;">
            <eav:EAVProjectContextControl runat="server" ID="ctlProjectContext" ContextKey="Test Project 1">
                <asp:Panel runat="server" GroupingText='<%# DataBinder.Eval(Container.DataItem, "Description") %>'>
                    <eav:EAVSubjectContextControl runat="server" ID="ctlSubjectContext">
                        <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                            Subject:
                            <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "MemberID") %>'></asp:Label>
                        </div>
                        <eav:EAVContainerContextControl runat="server" ID="ctlContainerContext" ContextKey="Test Root Container 1">
                            <asp:Panel runat="server" GroupingText='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>'>
                                <eav:EAVInstanceContextControl runat="server" ID="ctlInstanceContext">
                                    <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                        Instance:
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "RepeatInstance") %>'></asp:Label>
                                    </div>
                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 1">
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>' />
                                        <eav:EAVTextBox runat="server" ID="ctlAttribute1" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <br />
                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 2">
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>' />
                                        <eav:EAVTextBox runat="server" ID="ctlAttribute2" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <br />
                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 3">
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>' />
                                        <eav:EAVTextBox runat="server" ID="ctlAttribute3" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <br />
                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 4">
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>' />
                                        <eav:EAVTextBox runat="server" ID="ctlAttribute4" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                    <br />
                                    <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 5">
                                        <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "DisplayName") %>' />
                                        <eav:EAVTextBox runat="server" ID="ctlAttribute5" Width="200"></eav:EAVTextBox>
                                    </eav:EAVAttributeContextControl>
                                </eav:EAVInstanceContextControl>
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
