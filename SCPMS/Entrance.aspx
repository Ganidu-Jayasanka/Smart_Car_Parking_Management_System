<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="Entrance.aspx.cs" Inherits="SCPMS.Entrance" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Entrance
</asp:Content>
<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Entrance
</asp:Content>

<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
     <!-- Cascading Style Sheets Link -->
    <link rel="stylesheet" type="text/css" href="Css/Entrance.css">

    <div class="container-fluid">
        <!-- Vehicle Details Section -->
        <div class="section">
            <h5>Vehicle Details</h5>
            <div class="form-group row">
                <div class="col-md-6">
                    <asp:Label ID="lblVehicleNo" runat="server" Text="Vehicle No:" CssClass="form-label" />
                   <asp:TextBox  ID="txtVehicleNo" runat="server" CssClass="form-control" AutoPostBack="True" OnTextChanged="txtVehicleNo_TextChanged" />

                </div>
                <div class="col-md-6">
                    <asp:Label ID="lblVehicleType" runat="server" Text="Vehicle Type:" CssClass="form-label" />
                    <asp:DropDownList ID="ddlVehicleType" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Select Vehicle Type" Value="" />
                    </asp:DropDownList>
                </div>
            </div>
        </div>

        <!-- Parking Area Section -->
        <div class="section">
            <h5>Parking Area</h5>
            <div class="form-group">
                <div class="col-md-6">
                    <asp:Label ID="lblArea" runat="server" Text="Area:" CssClass="form-label" />
                    <asp:DropDownList ID="ddlArea" runat="server" CssClass="form-select" AutoPostBack="true" OnSelectedIndexChanged="ddlArea_SelectedIndexChanged">
                        <asp:ListItem Text="Select Area" Value="" />
                    </asp:DropDownList>
                </div>
                <div class="col-md-6">
                    <asp:Label ID="lblSlotNumber" runat="server" Text="Slot Number:" CssClass="form-label" />
                    <asp:DropDownList ID="ddlSlotNumber" runat="server" CssClass="form-select">
                        <asp:ListItem Text="Slot No." Value="" />
                    </asp:DropDownList>
                </div>
            </div>
        </div>

        <!-- Actions Section -->
        <div class="section">
            <h5>Actions</h5>
            <div class="actions">
                <div class="checkbox-label">
                    <asp:CheckBox ID="chkEnterAndOpen" runat="server" CssClass="me-2" />
                    <asp:Label ID="lblEnterAndOpen" runat="server" Text="Enter and Open Barrier Gates" AssociatedControlID="chkEnterAndOpen" />
                </div>
                <asp:Button ID="btnEnterVehicle" runat="server" Text="Enter Vehicle" CssClass="btn btn-primary" OnClick="btnEnterVehicle_Click" />
            </div>
        </div>

        <!-- Message Section for Success or Error Display -->
        <div class="section">
            <asp:Label ID="lblMessage" runat="server" CssClass="message" Text=""></asp:Label>
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
