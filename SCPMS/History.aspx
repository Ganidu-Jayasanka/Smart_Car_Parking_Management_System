<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="History.aspx.cs" Inherits="SCPMS.History" %>
 
<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    History
</asp:Content>
<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Daily History
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/History.css">
    <div>     
<nav>
    <ul>
        <asp:Button Text="Entrance Vehicle Details" runat="server" OnClick="btnEntranceDetail_click" CssClass="sidebar-button-his entrance" />
        <asp:Button Text="Exit Vehicle Details" runat="server" OnClick="btnExitDetail_click" CssClass="sidebar-button-his exit" />
        <asp:Button Text="OverDue Vehicle Details" runat="server" OnClick="btnOverDueDetails_click" CssClass="sidebar-button-his overdue" />
    </ul>
</nav>

        <br />
        <br />

         <asp:Label ID="lblVD" runat="server" CssClass="lblDailyHistory" Text="" Visible="false"></asp:Label>
        <hr />
        
        <!-- Entrance Vehicle Details Grid View -->
        <asp:GridView ID="gvEVD" runat="server" AutoGenerateColumns="false" CssClass="gridview1" Visible="false">
            <Columns>
                <asp:BoundField DataField="InvoiceNo" HeaderText="Invoice No" />
                <asp:BoundField DataField="VehicleType" HeaderText="Vehicle Type" />
                <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" />
                <asp:BoundField DataField="ParkingDetail" HeaderText="Parking Area & Slot" />
                <asp:BoundField DataField="EnteredDateTime" HeaderText="Enter Time"  />
                <asp:BoundField DataField="EnteredGate" HeaderText="Enter Gate"/>
                <asp:BoundField DataField="EnteredBy" HeaderText="Entered User" />

            </Columns>
        </asp:GridView>
        
        <!-- Exit Vehicle Details Grid View -->
        <asp:GridView ID="gvEXVD" runat="server" AutoGenerateColumns="false" CssClass="gridview1" Visible="false">
            <Columns>
                <asp:BoundField DataField="InvoiceNo"  HeaderText="Invoice No" />
                <asp:BoundField DataField="VehicleType" HeaderText="Vehicle Type" />
                <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" />
                <asp:BoundField DataField="ParkingDetail" HeaderText="Parking Area & Slot" />
                <asp:BoundField DataField="EnteredDateTime" HeaderText="Enter Time" />
                <asp:BoundField DataField="ExitedDateTime" HeaderText="Exit Time" />
                <asp:BoundField DataField="Duration" HeaderText="Duration Time" />
                <asp:BoundField DataField="Amount" HeaderText="Amount" />
                <asp:BoundField DataField="ExitedGate" HeaderText="Exit Gate" />
                <asp:BoundField DataField="ExitedBy" HeaderText="Exited User" />
                <asp:BoundField DataField="PaymentStatus" HeaderText="Payment Status" />
            </Columns>
        </asp:GridView>
        
        <!-- OverDue Vehicle Details Grid View -->
        <asp:GridView ID="gvOVD" runat="server" AutoGenerateColumns="false" CssClass="gridview1" Visible="false">
            <Columns>
                <asp:BoundField DataField="InvoiceNo"  HeaderText="Invoice No" />
                <asp:BoundField DataField="VehicleType" HeaderText="Vehicle Type" />
                <asp:BoundField DataField="VehicleNo" HeaderText="Vehicle No" />
                <asp:BoundField DataField="ParkingDetail" HeaderText="Parking Area & Slot" />
                <asp:BoundField DataField="EnteredTime" HeaderText="Enter Time" />
                <asp:BoundField DataField="ParkingTime" HeaderText="Parking Time" />
                <asp:BoundField DataField="Amount" HeaderText="Amount" />
                <asp:BoundField DataField="EnteredGate" HeaderText="Enter Gate"/>
                <asp:BoundField DataField="EnteredBy" HeaderText="Entered User" />
                <asp:BoundField DataField="PaymentStatus" HeaderText="Payment Status" />
            </Columns>
        </asp:GridView>
    </div>
</asp:Content>
