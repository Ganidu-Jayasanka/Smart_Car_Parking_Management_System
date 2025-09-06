using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace SCPMS
{
    public partial class VMS_PasswordReset : System.Web.UI.Page
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Session["LoggedInUserId"] == null || Session["LoggedInUserName"] == null)
            {
                Response.Redirect("~/VMS_Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check access using username from session
            if (!HasAccess("VMS_PasswordReset.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!IsPostBack)
            {
                LoadUsers();
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

        private void LoadUsers()
        {
            // Fetch users from the database and bind to dropdown
            string connStr = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            using (SqlConnection conn = new SqlConnection(connStr))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("SELECT SUId, UserName FROM SystemUser", conn);
                SqlDataReader reader = cmd.ExecuteReader();

                ddlUsers.DataSource = reader;
                ddlUsers.DataTextField = "UserName";
                ddlUsers.DataValueField = "SUId";
                ddlUsers.DataBind();

                reader.Close();
            }
        }


        protected void btnResetPassword_Click(object sender, EventArgs e)
        {


            // Get the currently logged-in user ID and username
            string deleteUserId = Session["LoggedInUserId"]?.ToString();
            string deleteUserName = Session["LoggedInUserName"]?.ToString();

            if (string.IsNullOrEmpty(deleteUserId) || string.IsNullOrEmpty(deleteUserName))
            {
                Response.Write("Error: User is not authenticated.");
                return;
            }

            if (txtNewPassword.Text == txtConfirmPassword.Text)
            {
                string connStr = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("UPDATE SystemUser SET Password = @Password WHERE SUId = @SUId", conn);
                    cmd.Parameters.AddWithValue("@Password", txtNewPassword.Text);
                    cmd.Parameters.AddWithValue("@SUId", ddlUsers.SelectedValue);
                    cmd.ExecuteNonQuery();
                }

                // Display the username of the user who made the edit
                Response.Write("<script>alert('✅ User Password Reset successfully by " + deleteUserName + "');</script>");

                // Optionally, refresh the GridView or clear fields
                ClearFields();

            }
            else
            {
                ScriptManager.RegisterStartupScript(this, GetType(), "alert", "alert('Passwords do not match!');", true);
            }
        }


        // Clear Fields
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            ddlUsers.SelectedIndex = 0;
            txtNewPassword.Text = "";
            txtConfirmPassword.Text = "";

        }
    }
}