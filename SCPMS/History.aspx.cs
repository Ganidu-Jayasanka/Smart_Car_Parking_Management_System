using System;
using System.Data.SqlClient;
using System.Data;

namespace SCPMS
{
    public partial class History : System.Web.UI.Page
    {
        // GridView controls for displaying vehicle details
        protected System.Web.UI.WebControls.GridView gvOVD; // Overdue Vehicles
        protected System.Web.UI.WebControls.GridView gvEXVD; // Exited Vehicles
        protected System.Web.UI.WebControls.GridView gvEVD; // Entrance Vehicles

        // Label control to display the type of vehicle details
        protected System.Web.UI.WebControls.Label lblVD;

        // Connection string for database access
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;

        // Page Load Event: Ensures only authenticated users can access this page
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null)
            {
                // Redirect to login page if the user is not authenticated
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }


            // Check access using username from session
            if (!HasAccess("History.aspx"))
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
        // Event handler for displaying vehicles that entered today
        protected void btnEntranceDetail_click(object sender, EventArgs e)
        {
            lblVD.Text = "Entrance Vehicle Detail";
            lblVD.Visible = true;

            // Show entrance vehicles GridView and hide others
            gvEVD.Visible = true;
            gvEXVD.Visible = false;
            gvOVD.Visible = false;

            // Fetch entrance vehicle details
            BindEntranceVehicleGrid();
        }

        // Event handler for displaying vehicles that exited today
        protected void btnExitDetail_click(object sender, EventArgs e)
        {
            lblVD.Text = "Exit Vehicle Detail";
            lblVD.Visible = true;

            // Show exited vehicles GridView and hide others
            gvEXVD.Visible = true;
            gvEVD.Visible = false;
            gvOVD.Visible = false;

            // Fetch exit vehicle details
            BindExitVehicleGrid();
        }

        // Event handler for displaying overdue vehicles (parked for more than 8 hours)
        protected void btnOverDueDetails_click(object sender, EventArgs e)
        {
            lblVD.Text = "Overdue Vehicle Detail";
            lblVD.Visible = true;

            // Show overdue vehicles GridView and hide others
            gvOVD.Visible = true;
            gvEVD.Visible = false;
            gvEXVD.Visible = false;

            // Fetch overdue vehicle details
            BindOverDueVehicleGrid();
        }

        // Fetches and binds data of vehicles that entered today
        private void BindEntranceVehicleGrid()
        {
            string query = @"
        SELECT 
            T.InvoiceNo, 
            V.VehicleType, 
            T.VehicleNo, 
            CONCAT(PS.PSArea, ' - ', PS.PSNo) AS ParkingDetail, 
            T.EnteredDateTime, 
            T.EnteredGate, 
            U.UserName AS EnteredBy
        FROM 
            [SCPMS].[CPMS].[Tickets] T
        INNER JOIN 
            [SCPMS].[CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
        INNER JOIN 
            [SCPMS].[CPMS].[ParkingSlots] PS ON T.ParkingSlotPSid = PS.PSid
        LEFT JOIN 
            [SCPMS].[dbo].[SystemUser] U ON T.EnteredBy = U.SUId
        WHERE 
            CONVERT(date, T.EnteredDateTime) = CONVERT(date, GETDATE())";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        sda.Fill(dt);
                        gvEVD.DataSource = dt;
                        gvEVD.DataBind();
                        gvEVD.Visible = true;
                    }
                }
            }
        }

        // Fetches and binds data of vehicles that exited today
        private void BindExitVehicleGrid()
        {
            string query = @"
        SELECT 
            T.InvoiceNo,
            V.VehicleType,
            T.VehicleNo,
            CONCAT(PS.PSArea, ' - ', PS.PSNo) AS ParkingDetail,
            T.EnteredDateTime,
            T.ExitDateTime AS ExitedDateTime,
            T.DurationHours AS Duration,
            T.BillAmount AS Amount,
            T.ExitGate AS ExitedGate,
            U.UserName AS ExitedBy,
            T.PaymentStatus
        FROM 
            [CPMS].[Tickets] T
        JOIN 
            [CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
        JOIN 
            [CPMS].[ParkingSlots] PS ON T.ParkingSlotPSid = PS.PSid
        LEFT JOIN 
            [SCPMS].[dbo].[SystemUser] U ON T.ExitBy = U.SUId
        WHERE 
            CONVERT(date, T.ExitDateTime) = CONVERT(date, GETDATE())
        ORDER BY 
            T.EnteredDateTime DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        connection.Open();
                        adapter.Fill(dataTable);
                        gvEXVD.DataSource = dataTable;
                        gvEXVD.DataBind();
                    }
                }
            }
        }


        // Fetches and binds data of vehicles parked for more than 8 hours
        private void BindOverDueVehicleGrid()
        {
            string query = @"
        SELECT 
            T.InvoiceNo,
            V.VehicleType,
            T.VehicleNo,
            CONCAT(PS.PSArea, ' - ', PS.PSNo) AS ParkingDetail,
            T.EnteredDateTime AS EnteredTime,
            T.ExitDateTime AS ExitedTime,  
            DATEDIFF(HOUR, T.EnteredDateTime, T.ExitDateTime) AS ParkingTime,  
            T.BillAmount AS Amount,
            T.ExitGate AS EnteredGate,  
            U1.UserName AS EnteredBy,
            U2.UserName AS ExitedBy,
            T.PaymentStatus
        FROM 
            [CPMS].[Tickets] T
        JOIN 
            [CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
        JOIN 
            [CPMS].[ParkingSlots] PS ON T.ParkingSlotPSid = PS.PSid
        LEFT JOIN 
            [SCPMS].[dbo].[SystemUser] U1 ON T.EnteredBy = U1.SUId
        LEFT JOIN 
            [SCPMS].[dbo].[SystemUser] U2 ON T.ExitBy = U2.SUId
        WHERE 
            CONVERT(date, T.ExitDateTime) = CONVERT(date, GETDATE())  
            AND DATEDIFF(HOUR, T.EnteredDateTime, T.ExitDateTime) >= 8  
        ORDER BY 
            T.EnteredDateTime DESC";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        connection.Open();
                        adapter.Fill(dataTable);
                        gvOVD.DataSource = dataTable;
                        gvOVD.DataBind();
                    }
                }
            }
        }

    }
}
