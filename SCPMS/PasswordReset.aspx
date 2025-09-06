<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="PasswordReset.aspx.cs" Inherits="SCPMS.PasswordReset" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Password Reset
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Password Reset
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">
    <!-- Bootstrap 5 CSS -->
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css">
    
    <!-- FontAwesome for icons -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.5.1/css/all.min.css">

  <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/PasswordReset.css">
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mt-4">
        <h3 class="text-center text-primary">Password Reset</h3>
        <div class="card p-4 shadow">
          <div class="row g-3">
    <!-- User Selection Dropdown -->
    <div class="col-12">
        <label class="form-label">Select User</label>
        <div class="input-group">
            <span class="input-group-text"><i class="fa-solid fa-users"></i></span>
            <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-select">
            
                <asp:ListItem Text="Select User" Value="0" Selected="True"></asp:ListItem>
            
            </asp:DropDownList>
        </div>

        <asp:RequiredFieldValidator 
    ID="rfvSelectUser" 
    runat="server" 
    ControlToValidate="ddlUsers" 
    InitialValue="0" 
    ErrorMessage="Please select a user." 
    ForeColor="Red" 
    Display="Dynamic" 
    ValidationGroup="SelectUser" />

    </div>
</div>

                <!-- New Password Field -->
                <div class="col-12">
                    <label class="form-label">New Password</label>
                    <div class="input-group">
                        <span class="input-group-text"><i class="fa-solid fa-lock"></i></span>
                        <asp:TextBox ID="txtNewPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter new password"></asp:TextBox>
                    </div>
                </div>
                <!-- Confirm Password Field -->
                <div class="col-12">
                    <label class="form-label">Confirm Password</label>
                    <div class="input-group">
                        <span class="input-group-text"><i class="fa-solid fa-lock"></i></span>
                        <asp:TextBox ID="txtConfirmPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Confirm new password"></asp:TextBox>
                    </div>
                </div>
                <!-- Buttons -->
                <div class="col-12 text-center mt-3">
                    <asp:Button ID="btnResetPassword" runat="server" Text="Reset Password" CssClass="btn btn-primary px-4" OnClick="btnResetPassword_Click" />
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="btn btn-warning px-4" OnClick="btnClear_Click" />
                </div>
            </div>
        </div>
   
</asp:Content>
