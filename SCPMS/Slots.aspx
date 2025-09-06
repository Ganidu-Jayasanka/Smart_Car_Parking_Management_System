<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="Slots.aspx.cs" Inherits="SCPMS.Slots" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Slots
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Parking Slot
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
    <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/Slots.css">

    <div>
        <!-- Navigation Section with Buttons for Different Parking Slots -->
        <nav> 
            <ul>
                <!-- Each button corresponds to a parking slot and triggers an event on click -->
                <asp:Button Text="Parking Slot A" runat="server" OnClick="btnSlotA_click" CssClass="sidebar-button-slot A"/>
                <asp:Button Text="Parking Slot B" runat="server" OnClick="btnSlotB_click" CssClass="sidebar-button-slot B"/>
                <asp:Button Text="Parking Slot C" runat="server" OnClick="btnSlotC_click" CssClass="sidebar-button-slot C"/>
                <asp:Button Text="Parking Slot D" runat="server" OnClick="btnSlotD_click" CssClass="sidebar-button-slot D"/>
                <asp:Button Text="Parking Slot E" runat="server" OnClick="btnSlotE_click" CssClass="sidebar-button-slot E"/>
                <asp:Button Text="Parking Slot F" runat="server" OnClick="btnSlotF_click" CssClass="sidebar-button-slot F"/>
            </ul>
        </nav>

        <hr />
        <!-- Label to Display Selected Parking Slot (Initially Hidden) -->
        <asp:Label ID="lblParkingSlot" runat="server" CssClass="ParkingSlotLbl" Text="" Visible="false"></asp:Label>

        <hr />
        <!-- GridView for Parking Slot A - Displays Slot No and Status -->
        <asp:GridView ID="gvPSA" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />            
            </Columns>          
        </asp:GridView>
        <!-- GridView for Parking Slot B -->
        <asp:GridView ID="gvPSB" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
            </Columns>
        </asp:GridView> 
        <!-- GridView for Parking Slot C -->
        <asp:GridView ID="gvPSC" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
            </Columns>
        </asp:GridView>
        <!-- GridView for Parking Slot D -->
        <asp:GridView ID="gvPSD" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
            </Columns>
        </asp:GridView>
        <!-- GridView for Parking Slot E -->
        <asp:GridView ID="gvPSE" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
            </Columns>
        </asp:GridView>
        <!-- GridView for Parking Slot F -->
        <asp:GridView ID="gvPSF" runat="server" AutoGenerateColumns="false" CssClass="gridview1" visible="false" OnRowDataBound="GridView1_RowDataBound">                   
            <Columns>
                <asp:BoundField DataField="SlotNo" HeaderText="Slot No" />
                <asp:BoundField DataField="Status" HeaderText="Status" />
            </Columns>
        </asp:GridView>        
    </div>
</asp:Content>
