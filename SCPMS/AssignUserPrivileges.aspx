<%@ Page Language="C#" MasterPageFile="~/SCPMSMain.Master" AutoEventWireup="true" CodeBehind="AssignUserPrivileges.aspx.cs" Inherits="SCPMS.AssignUserPrivileges" %>


<asp:Content ID="TitleContent" ContentPlaceHolderID="TitleContent" runat="server">
  Assign User Privileges
</asp:Content>

<asp:Content ID="PageTitleContent" ContentPlaceHolderID="PageTitleContent" runat="server">
   Assign User Privileges
</asp:Content>


<asp:Content ID="MainContent" ContentPlaceHolderID="MainContent" runat="server">
   
      <!-- Link to External Stylesheet for Custom Styling -->
    <link rel="stylesheet" type="text/css" href="Css/AssignUserPrivileges.css">
     
    <div class="container mt-5">
       
        <div class="card shadow-sm">
            <div class="card-body">
                <!-- User Selection Section -->
                <div class="mb-4">
                    <h5 class="font-weight-bold">Select User</h5>
                    <asp:DropDownList ID="ddlUsers" runat="server" CssClass="form-control" AutoPostBack="true" OnSelectedIndexChanged="ddlUsers_SelectedIndexChanged">
                    </asp:DropDownList>
                </div>

                <!-- Privileges Section -->
                <div>
                    <h5 class="font-weight-bold">Privileges</h5>
                    <asp:GridView ID="gvUserPrivileges" runat="server" AutoGenerateColumns="False" CssClass="table table-bordered table-striped">
                        <Columns>
                            <asp:BoundField DataField="PageName" HeaderText="Form (.aspx)" />
                            <asp:TemplateField HeaderText="Access">
                                <ItemTemplate>
                                    <asp:CheckBox ID="chkCanAccess" runat="server" Checked='<%# Eval("CanAccess") %>' />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

                <!-- Save Button Section -->
                <div class="text-center mt-4">
                    <asp:Button ID="btnSaveUserPrivileges" runat="server" Text="Save User Privileges" OnClick="btnSaveUserPrivileges_Click" CssClass="btn btn-success btn-lg" />
                </div>
            </div>
        </div>
    </div>
          
</asp:Content>
