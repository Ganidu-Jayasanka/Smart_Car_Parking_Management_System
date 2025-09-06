<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="ManageSlots.aspx.cs" Inherits="SCPMS.ManageSlots" %>

<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
    Manage Slot
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
    Manage Slot
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">

    <link href="https://stackpath.bootstrapcdn.com/bootstrap/4.3.1/css/bootstrap.min.css" rel="stylesheet">

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
    <!-- Form to Add New Slot -->
    <div class="row mb-4">
        <div class="col-md-6">
            <h4>Add New Slot</h4>

            <!-- DropDownList for Areas -->
            <div class="form-group">
                <label for="ddlArea">Select Area</label>
                <asp:DropDownList ID="ddlArea" runat="server" CssClass="form-control"></asp:DropDownList>
            </div>

            <!-- Slot Number -->
            <div class="form-group">
                <label for="txtNewSlotNo">Slot No</label>
                <asp:TextBox ID="txtNewSlotNo" runat="server" placeholder="Enter Slot No" CssClass="form-control"  onkeypress="return event.charCode >= 48 && event.charCode <= 57;"/>
            </div>

            <!-- Add Slot Button -->
            <asp:Button ID="btnAddSlot" runat="server" Text="Add Slot" OnClick="btnAddSlot_Click" CssClass="btn btn-primary" />
        </div>
    </div>
        <hr />

       <!-- Form to Remove Slot -->
<div class="row mb-4">
    <div class="col-md-6">
        <h4>Remove Slot</h4>

        <!-- DropDownList for selecting Area -->
        <div class="form-group">
            <label for="ddlRemoveArea">Select Area</label>
            <asp:DropDownList ID="ddlRemoveArea" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlRemoveArea_SelectedIndexChanged"></asp:DropDownList>
        </div>

        <!-- DropDownList for selecting Slot in the selected Area -->
        <div class="form-group">
            <label for="ddlDeleteSlot">Select Slot</label>
            <asp:DropDownList ID="ddlDeleteSlot" runat="server" CssClass="form-control"></asp:DropDownList>
        </div>

        <!-- Remove Slot Button -->
        <asp:Button ID="btnRemoveSlot" runat="server" Text="Remove Slot" OnClientClick="return confirmRemoveSlot();" OnClick="btnRemoveSlot_Click" CssClass="btn btn-danger mt-2" />
    </div>
</div>
        <hr />
</asp:Content>
