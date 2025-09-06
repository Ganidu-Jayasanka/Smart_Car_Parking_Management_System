using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class ManageSlots : System.Web.UI.Page
    {
        // Database connection string
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                
                LoadAreas();
                LoadAreasForRemove();  // Load areas for removal
            }
            // Check access using username from session
            if (!HasAccess("ManageSlots.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
        }
        // Method to check user privileges from the database
        private bool HasAccess(string pageName)
        {
            if (Session["LoggedInUserName"] == null)
            {
                return false; // If user is not logged in, deny access
            }
            string userLoginId = Session["LoggedInUserName"].ToString();  // Get the username from session
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // SQL query to check the user's role and access to the given page
                string query = @"
            SELECT CanAccess
            FROM UserPrivileges UP
            INNER JOIN SystemUser SU ON SU.UserName = UP.RoleName or SU.UserRoles = UP.RoleName
            WHERE SU.UserName = @UserLoginID 
            AND UP.PageName = @PageName
            AND UP.CanAccess = 1";  // Ensure CanAccess = 1 for access to be granted
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserLoginID", userLoginId);  // Use the logged-in user's username
                cmd.Parameters.AddWithValue("@PageName", pageName);  // The page you want to check access for
                con.Open();
                object result = cmd.ExecuteScalar();
                // If result is null or CanAccess is 0, deny access
                return result != null && Convert.ToBoolean(result);
            }  }
        // Load areas for renaming or assigning new slots
        private void LoadAreas()
        {
            string query = "SELECT DISTINCT PSArea FROM [CPMS].[ParkingSlots]";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlArea.DataSource = dt;
                ddlArea.DataTextField = "PSArea";
                ddlArea.DataValueField = "PSArea";
                ddlArea.DataBind();
                ddlArea.Items.Insert(0, new ListItem("Select Area", ""));
            } }
        // Add new parking slot
        protected void btnAddSlot_Click(object sender, EventArgs e)
        {
            string slotNoText = txtNewSlotNo.Text.Trim();
            string area = ddlArea.SelectedValue.Trim();
            string createdBy = Session["LoggedInUserId"]?.ToString();

            // Validate inputs
            if (string.IsNullOrEmpty(area))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please select an area.');", true);
                return;
            }

            if (string.IsNullOrEmpty(slotNoText))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter a slot number.');", true);
                return;
            }

            if (!int.TryParse(slotNoText, out int slotNo))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Slot number must be numeric.');", true);
                return;
            }

            if (string.IsNullOrEmpty(createdBy))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Session expired. Please log in again.');", true);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Check if slot already exists in the same area
                string checkQuery = "SELECT 1 FROM [CPMS].[ParkingSlots] WHERE PSArea = @area AND PSNo = @slotNo";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@area", area);
                checkCmd.Parameters.AddWithValue("@slotNo", slotNo);

                object exists = checkCmd.ExecuteScalar();

                if (exists != null)
                {
                    // Slot already exists
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ This slot number already exists for the selected area.');", true);
                }
                else
                {
                    // Slot does not exist → insert
                    string insertQuery = "INSERT INTO [CPMS].[ParkingSlots] (PSArea, PSNo, CreatedBy, CreatedAt) VALUES (@area, @slotNo, @createdBy, GETDATE())";
                    SqlCommand insertCmd = new SqlCommand(insertQuery, conn);
                    insertCmd.Parameters.AddWithValue("@area", area);
                    insertCmd.Parameters.AddWithValue("@slotNo", slotNo);
                    insertCmd.Parameters.AddWithValue("@createdBy", createdBy);
                    insertCmd.ExecuteNonQuery();

                    // Clear and show success
                    txtNewSlotNo.Text = "";
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('✅ Slot added successfully!');", true);
                }
            }
        }





        // Load Areas into ddlRemoveArea
        private void LoadAreasForRemove()
        {
            string query = "SELECT DISTINCT PSArea FROM [CPMS].[ParkingSlots]";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlRemoveArea.DataSource = dt;
                ddlRemoveArea.DataTextField = "PSArea";
                ddlRemoveArea.DataValueField = "PSArea";
                ddlRemoveArea.DataBind();
                ddlRemoveArea.Items.Insert(0, new ListItem("Select Area", ""));
            }}
        // Load Slots based on Selected Area
        protected void ddlRemoveArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedArea = ddlRemoveArea.SelectedValue;
            if (!string.IsNullOrEmpty(selectedArea))
            {
                LoadSlotsForArea(selectedArea);
            }}
        private void LoadSlotsForArea(string area)
        {
            string query = "SELECT PSid, PSNo FROM [CPMS].[ParkingSlots] WHERE PSArea = @PSArea";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@PSArea", area);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlDeleteSlot.DataSource = dt;
                ddlDeleteSlot.DataTextField = "PSNo";
                ddlDeleteSlot.DataValueField = "PSid";
                ddlDeleteSlot.DataBind();
                ddlDeleteSlot.Items.Insert(0, new ListItem("Select Slot", ""));
            }}
        // Remove Selected Slot
        protected void btnRemoveSlot_Click(object sender, EventArgs e)
        {
            string slotId = ddlDeleteSlot.SelectedValue;
            if (string.IsNullOrEmpty(slotId))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Please select a slot to remove.');", true);
                return;
            }
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Check if the slot is occupied (Status = 'Parked')
                string checkQuery = "SELECT Status FROM [CPMS].[ParkingSlots] WHERE PSid = @PSid";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@PSid", slotId);
                string status = checkCmd.ExecuteScalar()?.ToString();
                if (status == "Parked")
                {
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Cannot delete slot. A vehicle is currently parked.');", true);
                    return;
                }
                // Proceed with deletion if the slot is not occupied
                string deleteQuery = "DELETE FROM [CPMS].[ParkingSlots] WHERE PSid = @PSid";
                SqlCommand deleteCmd = new SqlCommand(deleteQuery, conn);
                deleteCmd.Parameters.AddWithValue("@PSid", slotId);
                deleteCmd.ExecuteNonQuery();
            }
            // Refresh slot list after deletion
            LoadSlotsForArea(ddlRemoveArea.SelectedValue);
            ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Slot removed successfully!');", true);
        }}}
