<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master" AutoEventWireup="true" CodeBehind="VMS_AssignUserPrivileges.aspx.cs" Inherits="SCPMS.VMS_AssignUserPrivileges" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
  Assign User Privileges
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
   Assign User Privileges
</asp:Content>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
   
    <style>

        /* General Body Styling */
body {
    background-color: #f8f9fa;
    font-family: Arial, sans-serif;
}

/* Container for the content */
.container {
    max-width: 800px;
}

/* Card Design */
.card {
    border: none;
    border-radius: 8px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
}

/* Card Body */
.card-body {
    padding: 30px;
    background-color: #fff;
}

/* Heading Styles */
h2 {
    font-size: 32px;
    color: #343a40;
    font-weight: 600;
}

h5 {
    font-size: 20px;
    color: #495057;
    font-weight: 600;
}

/* Dropdown styling */
select.form-control {
    border-radius: 5px;
    height: 45px;
    padding-left: 15px;
    padding-right: 15px;
}

/* Table Styling for GridView */
.table {
    border-collapse: collapse;
    width: 100%;
    margin-top: 20px;
}

/* Table Rows and Borders */
.table th, .table td {
    text-align: left;
    padding: 12px 15px;
}

.table th {
    background-color: #007bff;
    color: #fff;
}

.table tbody tr:nth-child(odd) {
    background-color: #f8f9fa;
}

.table tbody tr:hover {
    background-color: #e2e6ea;
}

/* CheckBox Styling */
input[type="checkbox"] {
    transform: scale(1.2);
}

/* Button Styling */
.btn {
    padding: 12px 30px;
    font-size: 16px;
    border-radius: 25px;
    transition: background-color 0.3s ease;
}

.btn:hover {
    background-color: #218838;
}

.btn-success {
    background-color: #28a745;
}

.btn-success:hover {
    background-color: #218838;
}

    </style>
    
    
    <div class="container mt-5">
       
        <div class="card shadow-sm">
            <div class="card-body">
                <!-- User Selection Section -->
                <div class="mb-4">
                    <h5 class="font-weight-bold">Select User</h5>
                    <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlUsers_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>

                <!-- Privileges Section -->
                <div>
                    <h5 class="font-weight-bold">Privileges</h5>
                    <asp:GridView ID="gvUserPrivileges" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped">
                        <Columns>
                            <asp:BoundField DataField="PageName" HeaderText="Form (.aspx)" />
                            <asp:TemplateField HeaderText="Access">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkCanAccess" runat="server" Checked='<%# Eval("CanAccess") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <!-- Save Button Section -->
                <div class="text-center mt-4">
                    <asp:Button ID="btnSaveUserPrivileges" runat="server" Text="Save User Privileges" OnClick="btnSaveUserPrivileges_Click" CssClass="btn btn-success btn-lg" />
                </div>
            </div>
        </div>
    </div>
          
</asp:Content>
