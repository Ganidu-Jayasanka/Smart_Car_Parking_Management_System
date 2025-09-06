using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class UserView : System.Web.UI.Page
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Session["LoggedInUserId"] == null)
                {
                    Response.Redirect("~/Login.aspx", false);
                    this.Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                // Check access using username from session
                if (!HasAccess("UserView.aspx"))
                {
                    // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                    Response.Redirect("~/Unauthorized.aspx", false);
                    Context.ApplicationInstance.CompleteRequest();
                    return;
                }
                LoadUserData();
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
        private void LoadUserData()
        { 
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
            SELECT 
                SU.SUId, 
                SU.FirstName, 
                SU.LastName, 
                SU.Email, 
                SU.Mobile, 
                SU.UserName, 
                CASE 
                    WHEN SU.ActiveStatus = 1 THEN 'Active' 
                    ELSE 'Inactive' 
                END AS ActiveStatus, 
                SU.CreateDate, 
                CU.UserName AS CreateUser, 
                SU.UserRoles
            FROM SystemUser SU
            LEFT JOIN SystemUser CU ON SU.CreateUser = CU.SUId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvUsers.DataSource = dt;
                    gvUsers.DataBind();
                }  }} }}
