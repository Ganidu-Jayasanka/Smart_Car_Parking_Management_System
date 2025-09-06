using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SCPMS
{
    public partial class UserManage : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            Page.UnobtrusiveValidationMode = UnobtrusiveValidationMode.None;

            if (Session["LoggedInUserId"] == null || Session["LoggedInUserName"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            // Check access using username from session
            if (!HasAccess("UserManage.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack)
            {

                btnUpdate.Style["display"] = "none"; // Hide Update button initially            
                PopulateUserDropdown();
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
      // Populate the dropdown with active users
        private void PopulateUserDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    // Query to get active users from the database
                    string query = "SELECT SUId, UserName FROM SystemUser "; // Modify if needed
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    // Clear previous items from the dropdown
                    ddlSelectUser.Items.Clear();
                    ddlSelectUser.Items.Add(new ListItem("Select User", ""));

                    // Add users to the dropdown
                    while (reader.Read())
                    {
                        string userId = reader["SUId"].ToString();
                        string userName = reader["UserName"].ToString();
                        ddlSelectUser.Items.Add(new ListItem(userName, userId));
                    }
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
        }
        protected void btnSave_Click(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string mobile = txtMobile.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string userRole = ddlUserRoles.SelectedValue;
            bool isActive = chkActive.Checked;
            string createDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string createUserId = Session["LoggedInUserId"]?.ToString();
            string createUserName = Session["LoggedInUserName"]?.ToString();

            // ==== Validation Starts ====
            if (string.IsNullOrEmpty(firstName))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter First Name.');", true);
                return;
            }
            if (string.IsNullOrEmpty(lastName))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter Last Name.');", true);
                return;
            }
            if (string.IsNullOrEmpty(email))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter Email.');", true);
                return;
            }
            if (string.IsNullOrEmpty(mobile))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter Mobile number.');", true);
                return;
            }
            if (string.IsNullOrEmpty(username))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter Username.');", true);
                return;
            }
            if (string.IsNullOrEmpty(password))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please enter Password.');", true);
                return;
            }
            if (string.IsNullOrEmpty(userRole))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('⚠️ Please select a User Role.');", true);
                return;
            }
            if (string.IsNullOrEmpty(createUserId) || string.IsNullOrEmpty(createUserName))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('User session expired. Please log in again.');", true);
                return;
            }
            // ==== Validation Ends ====

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO [dbo].[SystemUser] ([FirstName], [LastName], [Email], [Mobile], [UserName], [Password], [ActiveStatus], [CreateDate], [CreateUser], [UserRoles]) " +
                               "VALUES (@FirstName, @LastName, @Email, @Mobile, @UserName, @Password, @ActiveStatus, @CreateDate, @CreateUser, @UserRoles)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@ActiveStatus", isActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreateDate", createDate);
                cmd.Parameters.AddWithValue("@CreateUser", createUserId);
                cmd.Parameters.AddWithValue("@UserRoles", userRole);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Response.Write("<script>alert('✅ User created successfully by " + createUserName + "');</script>");
                    ClearFields();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {

            // Optionally, populate the dropdown with the current users if it's not already populated
           PopulateUserDropdown();
            pnlUserForm.Visible = false; // Hide user management form
           pnlSelectUser.Style["display"] = "inline-block"; // Show user selection form
            // Hide dropdown and selection button after clicking Edit
            ddlSelectUser.Style["display"] = "inline-block";
            btnSelectUser.Style["display"] = "inline-block";
            // Show user management form for updating
            btnSave.Visible = false; // Hide Create User button
           btnEdit.Visible = false; // Hide Edit button          
            btnUpdate.Visible = false; // Show Update button
           btnClear.Visible = false; // Show Clear button
            txtPassword.Style["display"] = "none";
            // Hide the password field container
            divPasswordWrapper.Style["display"] = "none";
        }
        // Handle user selection and populate data for editing
        // Handle user selection and populate data for editing
        protected void btnSelectUser_Click(object sender, EventArgs e)
        {
            string selectedUserId = ddlSelectUser.SelectedValue;
            if (!string.IsNullOrEmpty(selectedUserId))
            {
                // Retrieve user data from the database
                string query = "SELECT * FROM SystemUser WHERE SUId = @SUId";
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SUId", selectedUserId);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        // Populate the form fields with the selected user's data
                        txtFirstName.Text = reader["FirstName"].ToString();
                        txtLastName.Text = reader["LastName"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                        txtMobile.Text = reader["Mobile"].ToString();
                        txtUsername.Text = reader["UserName"].ToString();
                        txtPassword.Text = reader["Password"].ToString();
                        ddlUserRoles.SelectedValue = reader["UserRoles"].ToString();
                        chkActive.Checked = Convert.ToBoolean(reader["ActiveStatus"]);

                        // Ensure the Update button is displayed after data is populated
                        btnUpdate.Visible = true;
                    }
                    else
                    {
                        // Hide update button if no data is found
                        btnUpdate.Visible = true;
                    }}
                // Hide the user selection panel and show the form for editing
                pnlSelectUser.Visible = false;
                pnlUserForm.Visible = true; 
                btnSave.Visible = false; // Hide Create User button
                btnEdit.Visible = false;
                btnUpdate.Style["display"]  = "inline-block";                
                btnClear.Visible = false;
            }}
        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            string firstName = txtFirstName.Text;
            string lastName = txtLastName.Text;
            string email = txtEmail.Text;
            string mobile = txtMobile.Text;
            string username = txtUsername.Text;
            string password = txtPassword.Text;
            string userRole = ddlUserRoles.SelectedValue;
            bool isActive = chkActive.Checked;
            string updateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

           

            // Get the currently logged-in user ID and username
            string updateUserId = Session["LoggedInUserId"]?.ToString();
            string updateUserName = Session["LoggedInUserName"]?.ToString();
            if (string.IsNullOrEmpty(updateUserId) || string.IsNullOrEmpty(updateUserName))
            {
                Response.Write("Error: User is not authenticated.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "UPDATE [dbo].[SystemUser] SET [FirstName] = @FirstName, [LastName] = @LastName, [Email] = @Email, [Mobile] = @Mobile, " +
                               "[UserName] = @UserName, [Password] = @Password, [ActiveStatus] = @ActiveStatus, [CreateDate] = @CreateDate, [CreateUser] = @CreateUser, [UserRoles] = @UserRoles " +
                               "WHERE SUId = @UserID";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@ActiveStatus", isActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreateDate", updateDate);
                cmd.Parameters.AddWithValue("@CreateUser", updateUserId); // Store the update user ID
                cmd.Parameters.AddWithValue("@UserRoles", userRole);
                cmd.Parameters.AddWithValue("@UserID", ddlSelectUser.SelectedValue);
                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    // Display the username of the user who made the edit
                    Response.Write("<script>alert('✅ User updated successfully by " + updateUserName + "');</script>");
                    // Optionally, refresh the GridView or clear fields
                    ClearFields();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
        }

        // Clear Fields
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }
        private void ClearFields()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtEmail.Text = "";
            txtMobile.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            ddlUserRoles.SelectedIndex = 0;
            chkActive.Checked = true;
        }}}
