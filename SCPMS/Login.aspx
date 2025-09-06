<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SCPMS.Login" %>

<!DOCTYPE html>
<html lang="en">
<head>
    <!-- Meta tags for character encoding and viewpoint settings -->
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>SCPMS Login</title>
    <!-- Goodle Font Link for using 'Roboto' font family -->
    <link href="https://fonts.googleapis.com/css2?family=Roboto:wght@400;500;700&display=swap" rel="stylesheet">
    <!-- Cascading Style Sheets Link -->
    <link rel="stylesheet" type="text/css" href="Css/Login.css"> 
    <!-- JavaScript function to navigate between SCPMS and VMS login pages -->
    <script>
        function navigate(page) {
            if (page === 'SCPMS') {
                window.location.href = 'Login.aspx';  // Redirect to SCPMS Login page
            } else if (page === 'VMS') {
                window.location.href = 'VMS_Login.aspx';  // Redirect to VMS Login page
            }
        }
    </script>
</head>
<body>
    <!-- Main container holding all content -->
    <div class="container">
        <div class="navigation">
            <button onclick="navigate('SCPMS')">SCPMS</button>
            <button onclick="navigate('VMS')">VMS</button>
        </div>
   <!-- Main content section with two parts: Features and login -->
        <div class="content">
            <div class="features">
                <img id="features-img" src="/Image/SCPMS.jpg" alt="Feature Image">
                <div class="features-content">
                    <h1 id="features-title">Smart Car Parking Management System</h1>
                </div>
            </div>
   <!-- Login section with logo, form inputs for username/password, and login button -->
            <div class="login-section">
                <img src="/Image/the-colombo-lotus-tower.jpg" alt="Company Logo">
                <h2>Sign in to your account</h2>
                <form id="formLogin" runat="server">
                    <asp:Label ID="lblMessage" runat="server" CssClass="error-message"></asp:Label>
  <!-- Username input field-->
                    <div class="form-group">
                        <label for="txtUsername">Username</label>
                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Enter Username"></asp:TextBox>
                    </div>
   <!-- Password input field-->
                    <div class="form-group">
                        <label for="txtPassword">Password</label>
                        <asp:TextBox ID="txtPassword" runat="server" CssClass="form-control" TextMode="Password" placeholder="Enter Password"></asp:TextBox>
                    </div>
   <!-- Login button-->  
                    <div class="form-actions">
                        <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn" OnClick="btnLogin_Click" />
                    </div>
                </form>
            </div>
        </div>
    </div>
</body>
</html>
