using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;

namespace SCPMS
{
    public partial class Dashboard : Page
    {

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadDashboardData();
            }

            // Check access using username from session
            if (!HasAccess("Dashboard.aspx"))
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
            }}
        private void LoadDashboardData()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // 1️⃣ Fetch Total Daily Sales
                using (SqlCommand cmd = new SqlCommand(@"SELECT SUM(TotBillAmount) FROM [SCPMS].[CPMS].[Tickets] 
                                                       WHERE CAST(EnteredDateTime AS DATE) = CAST(GETDATE() AS DATE)", conn))
                {
                    object result = cmd.ExecuteScalar();
                    lblDailySales.Text = result != DBNull.Value ? Convert.ToDecimal(result).ToString("N2") : "0.00";
                }
                // 2️⃣ Fetch Daily Vehicle Count
                using (SqlCommand cmd = new SqlCommand(@"SELECT COUNT(DISTINCT VehicleNo) FROM [SCPMS].[CPMS].[Tickets] 
                                                          WHERE CAST(EnteredDateTime AS DATE) = CAST(GETDATE() AS DATE)", conn))
                {
                    object result = cmd.ExecuteScalar();
                    lblVehicleCount.Text = result != DBNull.Value ? result.ToString() : "0";
                }
                using (SqlCommand cmd = new SqlCommand(@"
    SELECT 
        T.VehicleNo, 
        V.VehicleType, 
        P.PSArea, 
        P.PSNo 
    FROM [SCPMS].[CPMS].[Tickets] T
    INNER JOIN [SCPMS].[CPMS].[ParkingSlots] P ON T.ParkingSlotPSid = P.PSid 
    INNER JOIN [SCPMS].[CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId  -- Ensure correct join condition
    WHERE T.ExitDateTime IS NULL", conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvParkingVehicles.DataSource = dt;
                    gvParkingVehicles.DataBind();
                }
            } }}}
