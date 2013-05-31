<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="RecurringExample.aspx.cs" Inherits="NeoEAVWeb.RecurringExample" %>

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
                <asp:DropDownList runat="server" ID="ctlInstances" AutoPostBack="true" OnSelectedIndexChanged="ctlInstances_SelectedIndexChanged"></asp:DropDownList>
                <asp:Button runat="server" ID="ctlSaveButton" OnClick="ctlSaveButton_Click" Text="Save" />
            </div>
            <div style="padding-top: 35px;">
                <eav:EAVProjectContextControl runat="server" ID="ctlProjectContext" ContextKey="Test Project 3">
                    <asp:Panel runat="server" GroupingText='<%# DataBinder.Eval(Container.DataItem, "Description") %>'>
                        <eav:EAVSubjectContextControl runat="server" ID="ctlSubjectContext" DynamicContextKey="true">
                            <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                Subject:
                                <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "MemberID") %>'></asp:Label>
                            </div>
                            <eav:EAVContainerContextControl runat="server" ID="ctlContainerContext" ContextKey="Test Root Container 3">
                                <asp:Panel runat="server" GroupingText='<%# DataBinder.GetPropertyValue(Container.DataItem, "Name") %>'>
                                    <eav:EAVInstanceContextControl runat="server" ID="ctlInstanceContext" DynamicContextKey="true">
                                        <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                            Instance:
                                            <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "RepeatInstance") %>'></asp:Label>
                                        </div>
                                        <div style="padding: 5px;">
                                            <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 11">
                                                <eav:EAVTextBox runat="server" ID="ctlValue1" Width="200"></eav:EAVTextBox>
                                            </eav:EAVAttributeContextControl>
                                            <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 12">
                                                <eav:EAVTextBox runat="server" ID="ctlValue2" Width="200"></eav:EAVTextBox>
                                            </eav:EAVAttributeContextControl>
                                            <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 13">
                                                <eav:EAVTextBox runat="server" ID="ctlValue3" Width="200"></eav:EAVTextBox>
                                            </eav:EAVAttributeContextControl>
                                            <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 14">
                                                <eav:EAVTextBox runat="server" ID="ctlValue4" Width="200"></eav:EAVTextBox>
                                            </eav:EAVAttributeContextControl>
                                            <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 15">
                                                <eav:EAVTextBox runat="server" ID="ctlValue5" Width="200"></eav:EAVTextBox>
                                            </eav:EAVAttributeContextControl>
                                        </div>

                                        <eav:EAVContainerContextControl runat="server" ID="ctlChildContainerContext1" ContextKey="Test Child Container 1">
                                            <asp:Panel runat="server" GroupingText='<%# DataBinder.GetPropertyValue(Container.DataItem, "Name") %>'>
                                                <eav:EAVInstanceContextRepeater runat="server" ID="ctlChildInstance1Repeater">
                                                    <ItemTemplate>
                                                        <eav:EAVInstanceContextControl runat="server" DynamicContextKey="true">
                                                            <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                                                Instance:
                                                                <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "RepeatInstance") %>'></asp:Label>
                                                            </div>
                                                            <div style="padding: 5px;">
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 16">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue6" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 17">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue7" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 18">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue8" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 19">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue9" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 20">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue10" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                            </div>
                                                        </eav:EAVInstanceContextControl>
                                                    </ItemTemplate>
                                                </eav:EAVInstanceContextRepeater>
                                            </asp:Panel>
                                        </eav:EAVContainerContextControl>

                                        <eav:EAVContainerContextControl runat="server" ID="ctlChildContainerContext2" ContextKey="Test Child Container 2">
                                            <asp:Panel runat="server" GroupingText='<%# DataBinder.GetPropertyValue(Container.DataItem, "Name") %>'>
                                                <eav:EAVInstanceContextRepeater runat="server" ID="ctlChildInstance2Repeater">
                                                    <ItemTemplate>
                                                        <eav:EAVInstanceContextControl runat="server" DynamicContextKey="true">
                                                            <div style="margin: 3px; padding: 5px; background-color: #CCCCCC;">
                                                                Instance:
                                                                <asp:Label runat="server" Text='<%# DataBinder.Eval(Container.DataItem, "RepeatInstance") %>'></asp:Label>
                                                            </div>
                                                            <div style="padding: 5px;">
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 21">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue11" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 22">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue12" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 23">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue13" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 24">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue14" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                                <eav:EAVAttributeContextControl runat="server" ContextKey="Test Attribute 25">
                                                                    <eav:EAVTextBox runat="server" ID="ctlValue15" Width="200"></eav:EAVTextBox>
                                                                </eav:EAVAttributeContextControl>
                                                            </div>
                                                        </eav:EAVInstanceContextControl>
                                                    </ItemTemplate>
                                                </eav:EAVInstanceContextRepeater>
                                            </asp:Panel>
                                        </eav:EAVContainerContextControl>
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
