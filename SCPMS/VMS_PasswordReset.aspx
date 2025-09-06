<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master" AutoEventWireup="true" CodeBehind="VMS_PasswordReset.aspx.cs" Inherits="SCPMS.VMS_PasswordReset" %>

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

    <style>
        /* Custom Styling */

          a {
            text-decoration: none;
            color: #000000;
        }

        a.ml-4 {
            text-decoration: none;
            color: #000000;
        }
        .container {
            max-width: 500px;
            background: white;
            padding: 25px;
            border-radius: 10px;
            box-shadow: 0px 4px 10px rgba(0, 0, 0, 0.1);
        }

        .form-label {
            font-weight: bold;
        }

        .form-control {
            padding: 12px;
            font-size: 16px;
            border-radius: 8px;
            border: 1px solid #ccc;
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

        .card {
            border-radius: 12px;
            padding: 20px;
            box-shadow: 0px 5px 15px rgba(0, 0, 0, 0.1);
        }
    </style>
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
                            <asp:ListItem Text="Select User" Value=""></asp:ListItem>
                        </asp:DropDownList>
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
    </div>
</asp:Content>
