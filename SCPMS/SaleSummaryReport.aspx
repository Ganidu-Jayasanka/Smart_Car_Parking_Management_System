<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="SaleSummaryReport.aspx.cs" Inherits="SCPMS.SaleSummaryReport" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Sale Summary Report
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
     Sale Summary Report
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server"> 
        <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/SaleSummaryReport.css">

    <main class="p-6" style="width: 100%; max-width: 1800px; margin: auto;">
        <section class="bg-white shadow-md rounded-lg">
            <div class="section-header">Sale Summary Report</div>

            <!-- Filter Section -->
            <div class="filter-container">
                <label for="txtStartDate">Start Date:</label>
                <asp:TextBox ID="txtStartDate" runat="server" TextMode="Date" CssClass="date-picker"></asp:TextBox>

                <label for="txtEndDate">End Date:</label>
                <asp:TextBox ID="txtEndDate" runat="server" TextMode="Date" CssClass="date-picker"></asp:TextBox>

                <asp:Button runat="server" Text="View" OnClientClick="return validateDates();" OnClick="btnView_Click" CssClass="btn-filter" />
            </div>
            <!-- Export Buttons Section -->
            <div class="export-buttons">
                <asp:Button runat="server" Text="Export to Excel" OnClick="btnExportExcel_Click" CssClass="btn-export-excell" />
                <asp:Button runat="server" Text="Export to PDF" OnClick="btnExportPDF_Click" CssClass="btn-export-pdf" />
            </div>
            <!-- GridView for Sale Summary Data -->
            <div class="table-container">
                <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" ShowFooter="True" CssClass="border-table"
                    OnRowDataBound="GridView1_RowDataBound">
                    <Columns>
                        <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No." />
                        <asp:BoundField DataField="VehicleType" HeaderText="Vehicle Type" />
                        <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No." />
                        <asp:BoundField DataField="ParkingDetail" HeaderText="Parking Area & Slot" />
                        <asp:BoundField DataField="EnteredDateTime" HeaderText="Enter Time" />
                        <asp:BoundField DataField="EnteredBy" HeaderText="Entered By" />
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
            document.getElementById("reportFilter").classList.add("hidden");
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
