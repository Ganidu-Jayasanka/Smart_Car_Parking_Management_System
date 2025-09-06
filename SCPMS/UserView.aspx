<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="UserView.aspx.cs" Inherits="SCPMS.UserView" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    System Users View
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    System Users View
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/UserView.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <main class="p-6" style="width: 100%; max-width: 1800px; margin: auto;">
        <section class="bg-white shadow-md rounded-lg">
            <div class="section-header">System Users </div>

            <!-- Table Container -->
            <div class="table-container">
              <asp:GridView ID="gvUsers" runat="server" CssClass="border-table" 
    AutoGenerateColumns="False" DataKeyNames="SUId">

                    <Columns>
                        <asp:BoundField DataField="SUId" HeaderText="ID" />
                        <asp:BoundField DataField="FirstName" HeaderText="First Name" />
                        <asp:BoundField DataField="LastName" HeaderText="Last Name" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="Mobile" HeaderText="Mobile" />
                        <asp:BoundField DataField="UserName" HeaderText="Username" />
                        <asp:BoundField DataField="ActiveStatus" HeaderText="Status" />
                        <asp:BoundField DataField="CreateDate" HeaderText="Created Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="CreateUser" HeaderText="Created By" />
                        <asp:BoundField DataField="UserRoles" HeaderText="Role" />
                        
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>
</asp:Content>
