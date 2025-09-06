using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Data;
using System.Data.SqlClient;

namespace SCPMS
{
    public partial class Slots : System.Web.UI.Page
    {
        // Database connection string
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            // Redirect to login if user is not authenticated
            if (Session["LoggedInUserId"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            // Check access using username from session
            if (!HasAccess("Slots.aspx"))
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
            }
        }
        // Event handler for selecting Parking Slot A
        protected void btnSlotA_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("A", gvPSA);
        }
        // Fetch and bind data for Parking Slot A
        private void BindPrakingSlotAGrid()
        {
            BindParkingSlotGrid("A", gvPSA);
        }
        // Event handler for selecting Parking Slot B
        protected void btnSlotB_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("B", gvPSB);
        }
        // Fetch and bind data for Parking Slot B
        private void BindPrakingSlotBGrid()
        {
            BindParkingSlotGrid("B", gvPSB);
        }
        // Event handler for selecting Parking Slot C
        protected void btnSlotC_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("C", gvPSC);
        }
        // Fetch and bind data for Parking Slot C
        private void BindPrakingSlotCGrid()
        {
            BindParkingSlotGrid("C", gvPSC);
        }
        // Event handler for selecting Parking Slot D
        protected void btnSlotD_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("D", gvPSD);
        }
        // Fetch and bind data for Parking Slot D
        private void BindPrakingSlotDGrid()
        {
            BindParkingSlotGrid("D", gvPSD);
        }
        // Event handler for selecting Parking Slot E
        protected void btnSlotE_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("E", gvPSE);
        }
        // Fetch and bind data for Parking Slot E
        private void BindPrakingSlotEGrid()
        {
            BindParkingSlotGrid("E", gvPSE);
        }
        // Event handler for selecting Parking Slot F
        protected void btnSlotF_click(object sender, EventArgs e)
        {
            UpdateParkingSlotView("F", gvPSF);
        }
        // Fetch and bind data for Parking Slot F
        private void BindPrakingSlotFGrid()
        {
            BindParkingSlotGrid("F", gvPSF);
        }
        // Generic method to fetch and bind parking slot data
        private void BindParkingSlotGrid(string area, GridView gridView)
        {
            string query = "SELECT PSNo AS SlotNo, Status FROM [CPMS].[ParkingSlots] WHERE PSArea = '" + area + "'";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gridView.DataSource = dt;
                gridView.DataBind();
            }
        }
        // Updates UI based on selected parking slot
        private void UpdateParkingSlotView(string slotName, GridView gridView)
        {
            lblParkingSlot.Text = "Parking Slot " + slotName;
            lblParkingSlot.Visible = true;
            // Hide all grid views first
            gvPSA.Visible = gvPSB.Visible = gvPSC.Visible = gvPSD.Visible = gvPSE.Visible = gvPSF.Visible = false;
            // Show the selected grid view
            gridView.Visible = true;
            // Fetch and bind data for the selected slot
            BindParkingSlotGrid(slotName, gridView);
        }
        // Change row color based on parking slot status
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string status = DataBinder.Eval(e.Row.DataItem, "Status").ToString();
                if (status.Equals("Parked", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.BackColor = System.Drawing.Color.LightPink;
                }
                else if (status.Equals("Free", StringComparison.OrdinalIgnoreCase))
                {
                    e.Row.BackColor = System.Drawing.Color.LightGreen;
                }}}}}
