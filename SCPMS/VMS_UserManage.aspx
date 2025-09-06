<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master" AutoEventWireup="true" CodeBehind="VMS_UserManage.aspx.cs" Inherits="SCPMS.VMS_UserManage" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    User Management
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    User Management
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Latest Bootstrap 5 CDN -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css">

    <style>
        /* Custom Styles */
        a {
            text-decoration: none;
            color: #000000;
        }

        a.ml-4 {
            text-decoration: none;
            color: #000000;
        }

        .container {
            max-width: 800px;
            background: white;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
        }

        .form-control, .form-select {
            padding: 12px;
            font-size: 16px;
            border-radius: 8px;
        }

        .btn {
            padding: 12px;
            font-weight: bold;
            border-radius: 8px;
            transition: all 0.3s ease-in-out;
        }

        .btn:hover {
            transform: scale(1.05);
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h3 class="text-center text-primary">User Management</h3>
        <div class="card p-4 shadow">
            <div id="pnlUserForm" runat="server">
                <div class="row g-3">
                    <!-- Left Column (4 Fields) -->
                    <div class="col-md-6">
    <div class="input-group">
        <span class="input-group-text"><i class="fa-solid fa-user"></i></span>
        <asp:TextBox ID="txtFirstName"  CssClass="form-control" runat="server" placeholder="First Name" />
    </div>
    <asp:RequiredFieldValidator 
        ID="rfvFirstName" 
        runat="server" 
        ControlToValidate="txtFirstName" 
        ErrorMessage="First Name is required." 
        ForeColor="Red" 
        Display="Dynamic" />
</div>
<!-- Last Name -->
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-user"></i></span>
                            <asp:TextBox ID="txtLastName" CssClass="form-control" runat="server" placeholder="Last Name" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvLastName" 
                            runat="server" 
                            ControlToValidate="txtLastName" 
                            ErrorMessage="Last Name is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                    </div>

                    <!-- Email -->
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-envelope"></i></span>
                            <asp:TextBox ID="txtEmail" CssClass="form-control" runat="server" placeholder="Email" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvEmail" 
                            runat="server" 
                            ControlToValidate="txtEmail" 
                            ErrorMessage="Email is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                        <asp:RegularExpressionValidator 
                            ID="revEmail" 
                            runat="server" 
                            ControlToValidate="txtEmail" 
                            ErrorMessage="Invalid email format." 
                            ValidationExpression="^\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}$"
                            ForeColor="Red" 
                            Display="Dynamic" />
                    </div>

                    <!-- Mobile -->
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-phone"></i></span>
                            <asp:TextBox ID="txtMobile" CssClass="form-control" runat="server" placeholder="Mobile" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvMobile" 
                            runat="server" 
                            ControlToValidate="txtMobile" 
                            ErrorMessage="Mobile number is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                        <asp:RegularExpressionValidator 
                                ID="revMobile" 
                                  runat="server" 
                                      ControlToValidate="txtMobile" 
                                       ErrorMessage="Invalid mobile number. (Must start with 070|071|072|074|075|076|077|078 and have 10 digits)" 
                                ValidationExpression="^(070|071|072|074|075|076|077|078)[0-9]{7}$" 
                                      ForeColor="Red" 
                                              Display="Dynamic" />

                    </div>

                    <!-- Username -->
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-user"></i></span>
                            <asp:TextBox ID="txtUsername" CssClass="form-control" runat="server" placeholder="Username" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvUsername" 
                            runat="server" 
                            ControlToValidate="txtUsername" 
                            ErrorMessage="Username is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                    </div>

                    <!-- Password -->
                    <div class="col-md-6" id="divPasswordWrapper" runat="server">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-lock"></i></span>
                            <asp:TextBox ID="txtPassword" CssClass="form-control" runat="server" TextMode="Password" placeholder="Password" />
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvPassword" 
                            runat="server" 
                            ControlToValidate="txtPassword" 
                            ErrorMessage="Password is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                    </div>

                    <!-- User Roles -->
                    <div class="col-md-6">
                        <div class="input-group">
                            <span class="input-group-text"><i class="fa-solid fa-user-tag"></i></span>
                            <asp:DropDownList ID="ddlUserRoles" CssClass="form-select" runat="server">
                                <asp:ListItem Text="Select User Role" Value="" />
                                <asp:ListItem Text="Admin" Value="Admin" />
                                <asp:ListItem Text="Standard User" Value="Standard User" />
                                <asp:ListItem Text="Manager" Value="Manager" />
                                <asp:ListItem Text="Supervisor" Value="Supervisor" />
                                <asp:ListItem Text="Cashier" Value="Cashier" />
                            </asp:DropDownList>
                        </div>
                        <asp:RequiredFieldValidator 
                            ID="rfvUserRoles" 
                            runat="server" 
                            ControlToValidate="ddlUserRoles" 
                            InitialValue="" 
                            ErrorMessage="User role selection is required." 
                            ForeColor="Red" 
                            Display="Dynamic" />
                    </div>

                <div class="col-md-6 d-flex align-items-center">
    <asp:CheckBox ID="chkActive" runat="server" CssClass="form-check-input me-2" />
    <label class="form-check-label">Active Status</label>

    <asp:CustomValidator 
        ID="cvActive" 
        runat="server"
        ErrorMessage="Please confirm active status." 
        ForeColor="Red" 
        Display="Dynamic" 
        ClientValidationFunction="validateActiveCheckbox"
        EnableClientScript="true" />
</div>
                </div>
            </div>
            <!-- User Selection Panel -->
<div id="pnlSelectUser" runat="server" style="display: none;">
    <div class="input-group">
        <span class="input-group-text"><i class="fa-solid fa-users"></i></span>
        
        <asp:DropDownList ID="ddlSelectUser" CssClass="form-select" runat="server">
            <asp:ListItem Text="Select User" Value=""></asp:ListItem>
        </asp:DropDownList>

       <asp:Button ID="btnSelectUser" runat="server" CssClass="btn btn-primary" 
    Text="Select" 
    OnClick="btnSelectUser_Click" 
    ValidationGroup="SelectUser" />

    </div>

    <asp:RequiredFieldValidator 
        ID="rfvSelectUser" 
        runat="server" 
        ControlToValidate="ddlSelectUser" 
        InitialValue="" 
        ErrorMessage="Please select a user." 
        ForeColor="Red" 
        Display="Dynamic" 
        ValidationGroup="SelectUser" />
</div>

             <div class="text-center mt-4">
                <asp:Button ID="btnSave" runat="server" Text="Create User" CssClass="btn btn-primary px-4" OnClick="btnSave_Click" />

                <asp:Button ID="btnEdit" runat="server" Text="Edit User" CssClass="btn btn-success px-4" OnClientClick="toggleUserSelection(); return false;" OnClick="btnEdit_Click" />
                <asp:Button ID="btnUpdate" runat="server" Text="Update User" CssClass="btn btn-success px-4" OnClientClick="return onUpdateClick();" OnClick="btnUpdate_Click"  />
                <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-warning px-4" OnClick="btnClear_Click" />
            </div>
        </div>
    </div>
    <script>
        // Toggle the User Management and User Selection panels
        function toggleUserSelection() {
            var userForm = document.getElementById('<%= pnlUserForm.ClientID %>');
            var selectUser = document.getElementById('<%= pnlSelectUser.ClientID %>');
            var btnEdit = document.getElementById('<%= btnEdit.ClientID %>');
            var btnUpdate = document.getElementById('<%= btnUpdate.ClientID %>');
            var btnSave = document.getElementById('<%= btnSave.ClientID %>');
            var btnClear = document.getElementById('<%= btnClear.ClientID %>');
            var btnSelectUser = document.getElementById('<%= btnSelectUser.ClientID %>');
            var passwordIcon = passwordField.previousElementSibling; // The <span> with the lock icon
    // Show the user selection dropdown
    userForm.style.display = 'none';
    selectUser.style.display = 'inline-block';
    // Hide all buttons except "Select User"
    btnSave.style.display = 'none';
    btnEdit.style.display = 'none';
    btnClear.style.display = 'none';
    btnUpdate.style.display = 'none';
    txtPassword.style.display = 'none';  // Hide password field
            btnSelectUser.style.display = 'inline-block';
            // Hide the password field and its lock icon
            passwordField.style.display = 'none';
            if (passwordIcon) {
                passwordIcon.style.display = 'none';
            }
}
        function hidePasswordField() {
            var passwordWrapper = document.getElementById('<%= divPasswordWrapper.ClientID %>');
            if (passwordWrapper) {
                passwordWrapper.style.display = 'none';
            }
        }
        function validateActiveCheckbox(sender, args) {
            // If you want it to be mandatory (checked), use this:
            args.IsValid = document.getElementById('<%= chkActive.ClientID %>').checked;
        }


    </script>
</asp:Content>