using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.UI;

namespace SCPMS
{
    public partial class VMS_UserManage : System.Web.UI.Page
    {
        private string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ToString();

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null || Session["LoggedInUserName"] == null)
            {
                Response.Redirect("~/VMS_Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            if (!HasAccess("VMS_UserManage.aspx"))
            {
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

        private bool HasAccess(string pageName)
        {
            if (Session["LoggedInUserName"] == null)
                return false;

            string userLoginId = Session["LoggedInUserName"].ToString();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT CanAccess
                    FROM UserPrivileges UP
                    INNER JOIN SystemUser SU ON SU.UserName = UP.RoleName OR SU.UserRoles = UP.RoleName
                    WHERE SU.UserName = @UserLoginID 
                    AND UP.PageName = @PageName
                    AND UP.CanAccess = 1";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@UserLoginID", userLoginId);
                cmd.Parameters.AddWithValue("@PageName", pageName);

                con.Open();
                object result = cmd.ExecuteScalar();

                return result != null && Convert.ToBoolean(result);
            }
        }

        private void PopulateUserDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    string query = "SELECT SUId, UserName FROM SystemUser";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    ddlSelectUser.Items.Clear();
                    ddlSelectUser.Items.Add(new ListItem("Select User", ""));

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
            if (!ValidateFields(isCreating: true))
                return;

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

            if (string.IsNullOrEmpty(createUserId) || string.IsNullOrEmpty(createUserName))
            {
                Response.Write("<script>alert('❌ User is not authenticated.');</script>");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"INSERT INTO [dbo].[SystemUser] 
                                ([FirstName], [LastName], [Email], [Mobile], [UserName], [Password], [ActiveStatus], [CreateDate], [CreateUser], [UserRoles]) 
                                 VALUES 
                                (@FirstName, @LastName, @Email, @Mobile, @UserName, @Password, @ActiveStatus, @CreateDate, @CreateUser, @UserRoles)";

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
                    PopulateUserDropdown();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
        }

        protected void btnEdit_Click(object sender, EventArgs e)
        {
            PopulateUserDropdown();
            pnlUserForm.Visible = false;
            pnlSelectUser.Style["display"] = "inline-block";
            ddlSelectUser.Style["display"] = "inline-block";
            btnSelectUser.Style["display"] = "inline-block";

            btnSave.Visible = false;
            btnEdit.Visible = false;
            btnUpdate.Visible = false;
            btnClear.Visible = false;
            txtPassword.Style["display"] = "none";
            divPasswordWrapper.Style["display"] = "none";
        }

        protected void btnSelectUser_Click(object sender, EventArgs e)
        {
            string selectedUserId = ddlSelectUser.SelectedValue;

            if (!string.IsNullOrEmpty(selectedUserId))
            {
                string query = "SELECT * FROM SystemUser WHERE SUId = @SUId";

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@SUId", selectedUserId);
                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtFirstName.Text = reader["FirstName"].ToString();
                        txtLastName.Text = reader["LastName"].ToString();
                        txtEmail.Text = reader["Email"].ToString();
                        txtMobile.Text = reader["Mobile"].ToString();
                        txtUsername.Text = reader["UserName"].ToString();
                        txtPassword.Text = reader["Password"].ToString();
                        ddlUserRoles.SelectedValue = reader["UserRoles"].ToString();
                        chkActive.Checked = Convert.ToBoolean(reader["ActiveStatus"]);
                        btnUpdate.Visible = true;
                    }
                }

                pnlSelectUser.Visible = false;
                pnlUserForm.Visible = true;
                btnSave.Visible = false;
                btnEdit.Visible = false;
                btnUpdate.Style["display"] = "inline-block";
                btnClear.Visible = false;
            }
        }

        protected void btnUpdate_Click(object sender, EventArgs e)
        {
            if (!ValidateFields(isCreating: false))
                return;

            string selectedUserId = ddlSelectUser.SelectedValue;
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string mobile = txtMobile.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();
            string userRole = ddlUserRoles.SelectedValue;
            bool isActive = chkActive.Checked;
            string updateDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            string updateUserId = Session["LoggedInUserId"]?.ToString();
            string updateUserName = Session["LoggedInUserName"]?.ToString();

            if (string.IsNullOrEmpty(updateUserId) || string.IsNullOrEmpty(updateUserName))
            {
                Response.Write("<script>alert('❌ User is not authenticated.');</script>");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"UPDATE [dbo].[SystemUser] 
                                 SET [FirstName] = @FirstName, [LastName] = @LastName, [Email] = @Email, [Mobile] = @Mobile,
                                     [UserName] = @UserName, [Password] = @Password, [ActiveStatus] = @ActiveStatus, 
                                     [CreateDate] = @CreateDate, [CreateUser] = @CreateUser, [UserRoles] = @UserRoles
                                 WHERE SUId = @UserID";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@FirstName", firstName);
                cmd.Parameters.AddWithValue("@LastName", lastName);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@Mobile", mobile);
                cmd.Parameters.AddWithValue("@UserName", username);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@ActiveStatus", isActive ? 1 : 0);
                cmd.Parameters.AddWithValue("@CreateDate", updateDate);
                cmd.Parameters.AddWithValue("@CreateUser", updateUserId);
                cmd.Parameters.AddWithValue("@UserRoles", userRole);
                cmd.Parameters.AddWithValue("@UserID", selectedUserId);

                try
                {
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Response.Write("<script>alert('✅ User updated successfully by " + updateUserName + "');</script>");
                    ClearFields();
                    PopulateUserDropdown();
                }
                catch (Exception ex)
                {
                    Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
                }
            }
        }

        private bool ValidateFields(bool isCreating)
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                Response.Write("<script>alert('❗ First Name is required.');</script>");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtLastName.Text))
            {
                Response.Write("<script>alert('❗ Last Name is required.');</script>");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                Response.Write("<script>alert('❗ Email is required.');</script>");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtMobile.Text))
            {
                Response.Write("<script>alert('❗ Mobile number is required.');</script>");
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                Response.Write("<script>alert('❗ Username is required.');</script>");
                return false;
            }
            if (isCreating && string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                Response.Write("<script>alert('❗ Password is required.');</script>");
                return false;
            }
            if (ddlUserRoles.SelectedValue == "")
            {
                Response.Write("<script>alert('❗ Please select a User Role.');</script>");
                return false;
            }
            return true;
        }

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
        }
    }
}
