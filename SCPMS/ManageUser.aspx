<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="ManageUser.aspx.cs" Inherits="SCPMS.ManageUser" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Users
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Manage Users
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Main Navigation Styling */
        .nav-container {
            display: flex;
            justify-content: center;
            background: #343a40;
            padding: 10px 0;
        }
        .nav-item {
            padding: 10px 20px;
            color: white;
            text-decoration: none;
            font-weight: bold;
            transition: 0.3s;
        }
        .nav-item:hover {
            background: #007bff;
            border-radius: 5px;
        }



    </style>
</asp:Content>
<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <!-- Navigation Bar -->
    <div class="nav-container">
        <a href="UserView.aspx" class="nav-item" onclick="showSection('viewUsers')">View System Users</a>
        <a href="UserManage.aspx" class="nav-item" onclick="showSection('userManagement')">User Management</a>
        <a href="PasswordReset.aspx" class="nav-item" onclick="showSection('')">Password Reset</a>
    </div>

   



    <script>
        function showSection(sectionId) {
            document.getElementById('viewUsers').classList.add('hidden');
            document.getElementById('userManagement').classList.add('hidden');
            document.getElementById(sectionId).classList.remove('hidden');
        }
    </script>
</asp:Content>
