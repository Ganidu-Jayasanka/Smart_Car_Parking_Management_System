<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master" AutoEventWireup="true" CodeBehind="ScheduledMeeting.aspx.cs" Inherits="SCPMS.ScheduledMeeting" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Scheduled Meeting
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
   <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/ScheduledMeeting.css">

    <main class="p-6" style="width: 98%; max-width: 1600px; margin: auto;">
        <section class="bg-white shadow-md rounded-lg">
            <div class="section-header">Scheduled Meeting Information</div>

            <!-- Filter Section -->
            <div class="filter-container">
                <label for="txtFromDate">From Date:</label>
                <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"></asp:TextBox>

                <label for="txtToDate">To Date:</label>
                <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"></asp:TextBox>

                <asp:Button ID="btnFilter" runat="server" Text="Filter" CssClass="btn-filter" OnClick="btnFilter_Click" OnClientClick="return validateDateRange();" />
            </div>

            <!-- GridView for Scheduled Meetings -->
            <div class="table-container">
                <asp:GridView ID="gvMeetings" runat="server" AutoGenerateColumns="False" CssClass="border-table"
                    GridLines="None" AllowPaging="true" PageSize="10" OnPageIndexChanging="gvMeetings_PageIndexChanging"
                    ShowFooter="true" PagerStyle-CssClass="gridview-pagination">
                    <Columns>
                        <asp:BoundField DataField="MeetingTitle" HeaderText="Meeting Title" />
                        <asp:BoundField DataField="ScheduleDate" HeaderText="Schedule Date" DataFormatString="{0:yyyy-MM-dd}" />
                        <asp:BoundField DataField="StartTime" HeaderText="Start Time" />
                        <asp:BoundField DataField="EndTime" HeaderText="End Time" />
                        <asp:BoundField DataField="VisitorName" HeaderText="Visitor Name" />
                        <asp:BoundField DataField="Identification" HeaderText="Identification" />
                        <asp:BoundField DataField="ContactNo" HeaderText="Contact No" />
                        <asp:BoundField DataField="Email" HeaderText="Email" />
                        <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" />
                        <asp:BoundField DataField="SchedulePerson" HeaderText="Scheduled Person" />
                    </Columns>
                    <PagerStyle CssClass="gridview-pagination" />
                </asp:GridView>
            </div>
        </section>
    </main>

    <script type="text/javascript">
    function validateDateRange() {
        var fromDate = document.getElementById('<%= txtFromDate.ClientID %>').value;
        var toDate = document.getElementById('<%= txtToDate.ClientID %>').value;

        // Clear previous alerts
        if (!fromDate || !toDate) {
            alert("Please select both From Date and To Date.");
            return false;
        }

        var start = new Date(fromDate);
        var end = new Date(toDate);

        if (start > end) {
            alert("From Date cannot be later than To Date.");
            return false;
        }

        return true;
    }
    </script>

</asp:Content>

