<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="Report.aspx.cs" Inherits="SCPMS.Report" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Report
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Report
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">   
   
        <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/Report.css">

    <hr />

    <!-- Navigation Menu for Reports -->
    <nav>
        <ul class="report-list">
            <li>
                <asp:HyperLink NavigateUrl="CashierWiseSaleReport.aspx" runat="server" CssClass="report-link">
                    <div class="report-button">
                    <img src="/Image/Cashier.gif" class="sidebar-image" alt="Cashier Wise Sale Report"/>
                    <asp:Label runat="server" CssClass="Report">Cashier Wise Sale Report</asp:Label>
                </div>
             </asp:HyperLink>
            </li>

            <li>
                <asp:HyperLink NavigateUrl="SaleSummaryReport.aspx" runat="server" CssClass="report-link">
                 <div class="report-button">
                    <img src="/Image/Sale.gif" class="sidebar-image" alt="Sale Summary Report"/>
                    <asp:Label runat="server" CssClass="Report">Sale Summary Report</asp:Label>
                  </div>
                    </asp:HyperLink>
            </li>

            <li>
                <asp:HyperLink NavigateUrl="VehicleCount.aspx" runat="server" CssClass="report-link">
                    <div class="report-button">
                    <img src="/Image/Vehicle.gif" class="sidebar-image" alt="Vehicle Count Report"/>
                    <asp:Label runat="server" CssClass="Report">Vehicle Count Report</asp:Label>
                    </div>
                        </asp:HyperLink>
            </li>
        </ul>
    </nav>

    <hr />

   <script type="text/javascript">
    function showAccessDeniedMessage() {
        // Create a div or alert to display the "You have no permission to access" message
        var messageDiv = document.createElement("div");
        messageDiv.style.position = "fixed";
        messageDiv.style.top = "50%";
        messageDiv.style.left = "50%";
        messageDiv.style.transform = "translate(-50%, -50%)";
        messageDiv.style.backgroundColor = "#f44336";
        messageDiv.style.color = "#fff";
        messageDiv.style.padding = "20px";
        messageDiv.style.borderRadius = "5px";
        messageDiv.style.zIndex = "9999";
        messageDiv.style.fontSize = "16px";
        messageDiv.innerHTML = "You do not have permission to access this page.";

        // Append the message div to the body
        document.body.appendChild(messageDiv);

        // Optionally, you can redirect after showing the message
        setTimeout(function() {
            window.location.href = "/";  // Redirect to the home page after 3 seconds
        }, 3000);  // 3000 milliseconds = 3 seconds
    }
   </script>

</asp:Content>
