using System;
using System.Data.SqlClient;
using System.Web.UI;

namespace SCPMS
{
    public partial class VMS_Login : System.Web.UI.Page
    {
              // Connection string to connect to the database, stored in the web.config file
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
         // Event handler for the Page Load event. This is triggered when the page is loaded.
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Text = string.Empty;
            }
           


        }


        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                lblMessage.Text = "Username and Password are required.";
                return;
            }
            try
            {
                int userStatus = GetUserStatus(username, password);

                if (userStatus == 1)
                {
                    // Save username in session
                    Session["LoggedInUserName"] = username;
                    Session["LoggedInUserId"] = GetUserId(username);

                    Response.Redirect("~/VMS_Dashboard.aspx");
                }
                else if (userStatus == 0)
                {
                    lblMessage.Text = "Your account is inactive. Please contact the administrator.";
                    //ScriptManager.RegisterStartupScript(this, GetType(), "alertMessage", "alert('Your account is inactive. Please contact the administrator.');", true);
                }
                else
                {
                    lblMessage.Text = "Invalid username or password.";
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "An error occurred. Please try again later.";
                System.Diagnostics.Debug.WriteLine($"Login error: {ex.Message}");
            }
        }

        // Method to check user status: 1 = Active, 0 = Inactive, -1 = Not Found
        private int GetUserStatus(string username, string password)
        {
            string query = "SELECT ActiveStatus FROM [SystemUser] WHERE UserName = @Username AND Password = @Password";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Password", password);
                    con.Open();
                    object result = cmd.ExecuteScalar();

                    if (result != null)
                    {
                        return Convert.ToInt32(result); // Return ActiveStatus (1 or 0)
                    }
                }
            }
            return -1; // User not found
        }

        // Method to retrieve the user ID for the given username
        private string GetUserId(string username)
        {
            string query = "SELECT SUId FROM [SystemUser] WHERE UserName = @Username";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }
    }
}