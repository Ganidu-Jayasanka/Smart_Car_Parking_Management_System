<%@ Page Language="C#" MasterPageFile="~/VMSMain.Master" AutoEventWireup="true" CodeBehind="VMS_Dashboard.aspx.cs" Inherits="SCPMS.VMS_Dashboard" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
  Dashboard
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
   
</asp:Content>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

     <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/VMSDashboard.css">

    <div class="container mt-4">

        <h2 class="text-center mb-4">👔 WELCOME TO THE VISITOR MANAGEMENT SYSTEM 👔</h2>

        <div class="row">
            <!-- Meeting Count Card -->
            <div class="col-md-6">
                <div class="card text-center shadow p-3 mb-4 meeting-count-card">
                    <h5>Today's Meeting Count</h5>
                    <div id="meetingCount" class="meeting-count">0</div>
                </div>
            </div>
            <!-- Daily Meeting Count Chart -->
            <div class="col-md-6">
                <div class="card shadow p-3 mb-4">
                    <h5 class="card-title text-center">Daily Meeting Count</h5>
                    <canvas id="meetingChart"></canvas>
                </div>
            </div>
        </div>
        <div class="row">
            <!-- Latest Meetings Table -->
            <div class="col-md-12">
                <div class="card shadow p-3">
                    <h5 class="card-title text-center">Today's Meetings</h5>
                    <div class="table-responsive">
                        <asp:GridView ID="gvLatestMeetings" runat="server" CssClass="table table-bordered table-striped table-hover"
                            AutoGenerateColumns="False">
                            <Columns>
                                <asp:BoundField DataField="MeetingNo" HeaderText="Meeting No" SortExpression="MeetingNo" ItemStyle-Width="150px"/>
                                <asp:BoundField DataField="MeetingHeader" HeaderText="Title" SortExpression="MeetingHeader" ItemStyle-Width="250px"/>
                                <asp:BoundField DataField="MeetingDate" HeaderText="Date" SortExpression="MeetingDate" 
                                    DataFormatString="{0:yyyy-MM-dd}" ItemStyle-Width="150px"/>
                               <asp:BoundField DataField="StartTime" HeaderText="Start Time" SortExpression="StartTime" ItemStyle-Width="150px"/>
                                <asp:BoundField DataField="VisitorName" HeaderText="Visitor" SortExpression="VisitorName" ItemStyle-Width="200px"/>
                                <asp:BoundField DataField="Identification" HeaderText="NIC" SortExpression="Identification" ItemStyle-Width="150px"/>
                                <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" SortExpression="VehicleNo" ItemStyle-Width="150px"/>
                                <asp:BoundField DataField="ScheduledPerson" HeaderText="Scheduled By" SortExpression="ScheduledPerson" ItemStyle-Width="200px"/>
                            </Columns>
                        </asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    </div>
</asp:Content>
<asp:Content ID="ScriptsContent" ContentPlaceHolderID="ScriptsContent" runat="server">
    <!-- Bootstrap & Chart.js -->
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            // ✅ Fetch Meeting Count
            fetch('VMS_Dashboard.aspx/GetTodayMeetingCount', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    document.getElementById("meetingCount").textContent = data.d;
                })
                .catch(error => console.error("Error fetching count:", error));

            // ✅ Fetch Meeting Chart Data
            fetch('VMS_Dashboard.aspx/GetMeetingData', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({})
            })
                .then(response => response.json())
                .then(data => {
                    const ctx = document.getElementById("meetingChart").getContext("2d");
                    new Chart(ctx, {
                        type: "bar",
                        data: {
                            labels: data.d.map(item => item.MeetingDate),
                            datasets: [{
                                label: "Meetings",
                                data: data.d.map(item => item.MeetingCount),
                                backgroundColor: "rgba(54, 162, 235, 0.6)",
                                borderColor: "rgba(54, 162, 235, 1)",
                                borderWidth: 1
                            }]
                        },
                        options: { responsive: true, maintainAspectRatio: false }
                    });
                })
                .catch(error => console.error("Error fetching chart data:", error));
        });
    </script>
</asp:Content>
