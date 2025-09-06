<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SCPMS.Dashboard" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Dashboard
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
   
</asp:Content>

<asp:Content ID="HeadContent" ContentPlaceHolderID="HeadContent" runat="server">

    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/bootstrap/5.3.0/css/bootstrap.min.css">

    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
         a {
            text-decoration: none;
            color: #000000;
        }
        a.ml-4 {
            text-decoration: none;
            color: #000000;
        }
    </style>
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server"> 
        <div class="container-fluid mt-4">
            <h3 class="text-center mb-4">🚗 WELCOME TO SMART CAR PARKING MANAGEMENT SYSTEM 🚗</h3>
            <div class="row">
                <!-- 🟢 Daily Sales -->
                <div class="col-md-6 mb-3">
                    <div class="card p-3 shadow">
                        <h5 class="text-secondary">Total Daily Sales (LKR)</h5>
                        <h3 class="text-success">
                            <asp:Label ID="lblDailySales" runat="server" Text="0.00"></asp:Label>
                        </h3>
                        <canvas id="salesChart" height="150"></canvas>
                    </div>
                </div>
                <!-- 🔵 Daily Vehicle Count -->
                <div class="col-md-6 mb-3">
                    <div class="card p-3 shadow">
                        <h5 class="text-secondary">Total Vehicles Parked Today</h5>
                        <h3 class="text-primary">
                            <asp:Label ID="lblVehicleCount" runat="server" Text="0"></asp:Label>
                        </h3>
                        <canvas id="vehicleChart" height="150"></canvas>
                    </div>
                </div>
            </div>
            <!-- 🚘 Current Parking Vehicles -->
            <div class="row">
                <div class="col-md-12">
                    <div class="card p-3 shadow">
                        <h5 class="text-secondary">Current Parking Vehicles</h5>
                        <asp:GridView ID="gvParkingVehicles" runat="server" CssClass="table table-bordered" AutoGenerateColumns="True"></asp:GridView>
                    </div>
                </div>
            </div>
        </div>
    <!-- 📊 Chart.js for Dynamic Graphs -->
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            var dailySales = parseFloat('<%= lblDailySales.Text %>') || 0;
            var vehicleCount = parseInt('<%= lblVehicleCount.Text %>') || 0;
            // 🟢 Sales Chart
            var ctxSales = document.getElementById('salesChart').getContext('2d');
            new Chart(ctxSales, {
                type: 'doughnut',
                data: {
                    labels: ["Today's Sales"],
                    datasets: [{
                        label: "Sales (LKR)",
                        data: [dailySales],
                        backgroundColor: ["#28a745"]
                    }]
                }
            });
            // 🔵 Vehicle Count Chart
            var ctxVehicles = document.getElementById('vehicleChart').getContext('2d');
            new Chart(ctxVehicles, {
                type: 'bar',
                data: {
                    labels: ["Today's Vehicle Count"],
                    datasets: [{
                        label: "Vehicles",
                        data: [vehicleCount],
                        backgroundColor: "#007bff"
                    }]
                }
            });
        });
    </script>
</asp:Content>
