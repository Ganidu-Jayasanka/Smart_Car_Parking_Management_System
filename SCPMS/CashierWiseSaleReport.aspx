<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="CashierWiseSaleReport.aspx.cs" Inherits="SCPMS.CashierWiseSaleReport" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Cashier Wise Sale Report
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
   Cashier Wise Sale Report
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
     
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/CashierWiseSaleReport.css">

    <main class="container py-5">
        <section class="bg-white shadow-md rounded-lg">
          
            <!-- Filter Section -->
            <div class="filter-container">
                <label for="txtStartDate">Start Date:</label>
                <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>

                <label for="txtEndDate">End Date:</label>
                <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="form-control"></asp:TextBox>

                <label for="ddlExitBy">Exit By:</label>
                <asp:DropDownList ID="ddlExitBy" runat="server" CssClass="form-control"></asp:DropDownList>

                <asp:Button ID="btnView" runat="server" Text="View" CssClass="btn-filter" OnClientClick="return validateDates();" OnClick="btnView_Click" />
            </div>

            <!-- Export Buttons -->
           <div class="d-flex justify-content-between mb-4">
                <asp:Button ID="btnExportExcel" runat="server" Text="Export to Excel" CssClass="btn-export-excell" OnClick="btnExportExcel_Click" />
                <asp:Button ID="btnExportPDF" runat="server" Text="Export to PDF" CssClass="btn-export-pdf" OnClick="btnExportPDF_Click" />
            </div>

            <!-- GridView for Sales Report -->
            <div class="table-container">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" ShowFooter="True" CssClass="border-table"
                    OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No." />
                        <asp:BoundField DataField="VehicleType" HeaderText="Vehicle Type" />
                        <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No." />
                        <asp:BoundField DataField="ParkingDetail" HeaderText="Parking Area and Slot" />
                        <asp:BoundField DataField="EnteredDateTime" HeaderText="Enter Time" />
                        <asp:BoundField DataField="ExitDateTime" HeaderText="Exit Time" />
                        <asp:BoundField DataField="ExitBy" HeaderText="Exit By" />
                        <asp:BoundField DataField="Duration" HeaderText="Duration Time" />
                        <asp:BoundField DataField="Amount" HeaderText="Amount" />
                        <asp:BoundField DataField="Discount" HeaderText="Discount" />
                        <asp:BoundField DataField="TotalAmount" HeaderText="Total Amount" />
                        <asp:BoundField DataField="PaymentType" HeaderText="Payment Type" />
                        <asp:BoundField DataField="PaymentStatus" HeaderText="Payment Status" />
                    </Columns>
                </asp:GridView>
            </div>
        </section>
    </main>

    <script type="text/javascript">
        function validateDates() {
            var startDate = document.getElementById('<%= txtStartDate.ClientID %>').value;
            var endDate = document.getElementById('<%= txtEndDate.ClientID %>').value;

            if (startDate === "" || endDate === "") {
                alert("Please select both start and end dates.");
                return false;
            }

            if (new Date(startDate) > new Date(endDate)) {
                alert("Start Date should not be greater than End Date.");
                return false;
            }

            return true;
        }

        window.onload = function () {
            var startDateInput = document.getElementById('<%= txtStartDate.ClientID %>');
            var endDateInput = document.getElementById('<%= txtEndDate.ClientID %>');
            startDateInput.type = 'date';
            endDateInput.type = 'date';
        }
    </script>
</asp:Content>
   
    