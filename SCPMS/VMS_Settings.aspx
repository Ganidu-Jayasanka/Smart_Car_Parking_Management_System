<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master"AutoEventWireup="true" CodeBehind="VMS_Settings.aspx.cs" Inherits="SCPMS.VMS_Settings" %>



<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Settings
</asp:Content>
<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Settings
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
   
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/Settings.css">
    

 <!-- Navigation Menu for Reports -->
<nav>
    <ul class="report-list">
        <li>
            <asp:HyperLink NavigateUrl="VMS_ManageUser.aspx" runat="server" CssClass="report-link">
                <div class="report-button">
                    <img src="/Image/UserManagement.gif" class="sidebar-image" alt="Cashier Wise Sale Report"/>
                    <asp:Label runat="server" CssClass="Setting">Manage Users</asp:Label>
                </div>
            </asp:HyperLink>
        </li>

        <li>
            <asp:HyperLink NavigateUrl="VMS_AssignUserPrivileges.aspx" runat="server" CssClass="report-link">
                <div class="report-button">
                    <img src="/Image/AssignPriviledge.gif" class="sidebar-image" alt="Sale Summary Report"/>
                    <asp:Label runat="server" CssClass="Report">Assign User Privileges</asp:Label>
                </div>
            </asp:HyperLink>
        </li>

       
    </ul>
</nav>
</asp:Content>
