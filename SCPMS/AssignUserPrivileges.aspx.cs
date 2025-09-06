using System;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class AssignUserPrivileges : System.Web.UI.Page
    {
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
               
                LoadUsers();
            }
            // Check access using username from session
            if (!HasAccess("AssignUserPrivileges.aspx"))
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
        // Load all users
        private void LoadUsers() 
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT DISTINCT RoleName FROM UserPrivileges";
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ddlUsers.DataSource = dt;
                ddlUsers.DataTextField = "RoleName";
                ddlUsers.DataValueField = "RoleName";
                ddlUsers.DataBind();
                ddlUsers.Items.Insert(0, new ListItem("-- Select Role --", ""));
            }
        }
        // Load privileges for selected user
        protected void ddlUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadPrivileges(ddlUsers.SelectedValue, gvUserPrivileges);
        }
        // Load privileges function
        private void LoadPrivileges(string entity, GridView gridView)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT p.PageName, ISNULL(up.CanAccess, 0) AS CanAccess
                    FROM (SELECT DISTINCT PageName FROM UserPrivileges) p
                    LEFT JOIN (SELECT * FROM UserPrivileges WHERE RoleName = @Entity) up 
                    ON p.PageName = up.PageName";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Entity", entity);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                gridView.DataSource = dt;
                gridView.DataBind();
            }
        }
        // Save User-Based Privileges
        protected void btnSaveUserPrivileges_Click(object sender, EventArgs e)
        {
            SavePrivileges(ddlUsers.SelectedValue, gvUserPrivileges);
        }
        // Save function (for both user and role privileges)
        private void SavePrivileges(string entity, GridView gridView)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                foreach (GridViewRow row in gridView.Rows)
                {
                    string pageName = row.Cells[0].Text;
                    bool canAccess = ((CheckBox)row.FindControl("chkCanAccess")).Checked;
                    string query = @"
                IF EXISTS (SELECT 1 FROM UserPrivileges WHERE RoleName = @Entity AND PageName = @PageName)
                    UPDATE UserPrivileges SET CanAccess = @CanAccess WHERE RoleName = @Entity AND PageName = @PageName
                ELSE
                    INSERT INTO UserPrivileges (RoleName, PageName, CanAccess) VALUES (@Entity, @PageName, @CanAccess)";

                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Entity", entity);
                    cmd.Parameters.AddWithValue("@PageName", pageName);
                    cmd.Parameters.AddWithValue("@CanAccess", canAccess);
                    cmd.ExecuteNonQuery();
                }
            }
            // Reset session flag to ensure privileges are checked again in the next access attempt
            Session["UserPrivilegesUpdated"] = true; // Indicate that privileges were updated

            Response.Write("<script>alert(' ✅ Privileges Updated Successfully');</script>");
        } }}
