<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="Exit.aspx.cs" Inherits="SCPMS.Exit" %>


<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Exit
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Exit
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/Exit.css">

    <!-- Search Section: Allows users to enter an Invoice No. or Vehicle No. to retrieve parking details -->
    <div class="section">
        <h2>Enter Invoice No / Vehicle No:</h2>
        <div class="search-group">
            <asp:Label ID="lblInvoiceVehicleNo" runat="server" Text="Invoice No/ Vehicle No:" 
                AssociatedControlID="txtSearch" CssClass="form-label"></asp:Label>
            <asp:TextBox ID="txtSearch" runat="server" CssClass="form-control search-input" 
                placeholder="Enter Invoice No or Vehicle No"></asp:TextBox>
            <asp:Button ID="btnSearch" runat="server" Text="Search" CssClass="btn btn-primary search-btn" 
                OnClick="btnSearch_Click" />
            <asp:Label ID="lblSearchError" runat="server" CssClass="message error" Visible="false"></asp:Label>
        </div>
        <hr />
        <!-- Vehicle Information Section: Displays retrieved parking details -->
        <div id="vehicleInfoCard" runat="server" class="form-group">
            <div class="form-row">
                <asp:Label ID="lblInvoiceNo" runat="server" Text="Invoice No:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtInvoiceNo" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="form-row">
                <asp:Label ID="lblVehicleNo" runat="server" Text="Vehicle No:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtVehicleNo" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="form-row">
                <asp:Label ID="lblVehicleType" runat="server" Text="Vehicle Type:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtVehicleType" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <div class="form-row">
                <asp:Label ID="lblInTime" runat="server" Text="In Time:" CssClass="form-label"></asp:Label>
                <asp:TextBox ID="txtInTime" runat="server" CssClass="form-control" ReadOnly="true" />
            </div>
            <!-- Button to proceed to payment -->
            <div class="actions">
                <asp:Button ID="btnProceedToPay" runat="server" Text="Proceed to Pay" CssClass="btn btn-success" 
                    OnClick="btnProceedToPay_Click" />
            </div>
        </div>
        <asp:Label ID="lblProceedToPayError" runat="server" CssClass="message error" Visible="false"></asp:Label>
    </div>
    <!-- Payment and Exit Processing Section -->
    <div class="grid-container">
        <!-- Payment Details Section -->
        <div class="grid-item">
            <div class="section" id="paymentDetailsCard" runat="server">
                <h2>Payment</h2>
                <div class="form-group">
                    <div class="form-row">
                        <asp:Label ID="lblParkedDuration" runat="server" Text="Parked Duration:" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtParkedDuration" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="form-row">
                        <asp:Label ID="lblBillAmount" runat="server" Text="Amount:" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtBillAmount" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="form-row">
                        <asp:Label ID="lblDiscount" runat="server" Text="Discount:" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtDiscount" runat="server" CssClass="form-control" 
                            OnTextChanged="txtDiscount_TextChanged" AutoPostBack="True" />
                    </div>
                    <div class="form-row">
                        <asp:Label ID="lblTotal" runat="server" Text="Total:" CssClass="form-label"></asp:Label>
                        <asp:TextBox ID="txtTotal" runat="server" CssClass="form-control" ReadOnly="true" />
                    </div>
                    <div class="form-row">
                        <asp:Label ID="lblPaymentType" runat="server" Text="Payment Type:" CssClass="form-label"></asp:Label>
                        <asp:DropDownList ID="ddlPaymentType" runat="server" CssClass="form-control">
                            <asp:ListItem Text="Cash" Value="Cash"></asp:ListItem>
                            <asp:ListItem Text="Credit Card" Value="CreditCard"></asp:ListItem>
                            <asp:ListItem Text="Mobile Payment" Value="MobilePayment"></asp:ListItem>
                            <asp:ListItem Text="Free" Value="Free"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>
                <!-- Payment Status Message -->
                <asp:Label ID="lblMessage" runat="server" CssClass="message" Visible="true"></asp:Label>
                <asp:Label ID="lblExitVehicleError" runat="server" CssClass="message error" Visible="false"></asp:Label>
                <!-- Button to confirm vehicle exit -->
                <div class="actions">
                    <asp:Button ID="btnExitVehicle" runat="server" Text="Exit Vehicle" CssClass="btn btn-primary" 
                        OnClick="btnExitVehicle_Click" />
                </div>
            </div>
        </div>
        <!-- Display Last Exited Vehicles -->
        <div class="grid-item">
            <div class="section">
                <h2>Last Exited Vehicles</h2>
                <div class="vehicle-table">
                    <asp:GridView ID="gvLastExitedVehicles" runat="server" CssClass="table table-bordered" AutoGenerateColumns="False">
                        <Columns>
                            <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" />
                            <asp:BoundField DataField="Amount" HeaderText="Amount" />
                        </Columns>
                    </asp:GridView>
                </div>
            </div>
        </div>
    </div>

     <script type="text/javascript">
    window.onload = function () {
        if (performance.navigation.type === 2) {
            // User used back/forward button
            window.location.href = window.location.pathname; // reload without POST
        }
    };
     </script>
</asp:Content>
