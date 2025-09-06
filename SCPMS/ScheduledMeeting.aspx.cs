using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class ScheduledMeeting : System.Web.UI.Page
    {
        // Connection string for database access
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null)
            {
                Response.Redirect("~/VMS_Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            // Check access using username from session
            if (!HasAccess("ScheduledMeeting.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }
            if (!IsPostBack)
            {
                BindMeetingGrid(); // Call BindMeetingGrid() on first page load
            }}
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
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            BindMeetingGrid();
        }
        protected void gvMeetings_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvMeetings.PageIndex = e.NewPageIndex;
            BindMeetingGrid();
        }
        private void BindMeetingGrid()
        {
            string fromDate = txtFromDate.Text;
            string toDate = txtToDate.Text;

            if (string.IsNullOrEmpty(fromDate) || string.IsNullOrEmpty(toDate))
            {
                gvMeetings.DataSource = null;
                gvMeetings.DataBind();
                return;
            }
            DataTable dtMeetings = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
    SELECT 
        mi.MeetingHeader AS MeetingTitle, 
        mi.MeetingDate AS ScheduleDate, 
        mi.StartTime, 
        mi.EndTime, 
        CONCAT(ma.FirstName, ' ', ma.LastName) AS VisitorName, 
        ma.Identification, 
        ma.Mobile AS ContactNo,
        ma.Email,
        ma.VehicleNo,  
        mi.CreatedUser AS SchedulePerson
    FROM 
        SCPMS.dbo.MeetingInfo mi
    LEFT JOIN 
        SCPMS.dbo.MeetingAttend ma ON mi.MeetingID = ma.MeetingID
    WHERE 
        mi.MeetingDate BETWEEN @FromDate AND @ToDate
    ORDER BY 
        mi.MeetingDate DESC;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FromDate", fromDate);
                    cmd.Parameters.AddWithValue("@ToDate", toDate);

                    using (SqlDataAdapter sda = new SqlDataAdapter(cmd))
                    {
                        sda.Fill(dtMeetings);
                    }}}
            if (dtMeetings.Rows.Count == 0)
            {
                gvMeetings.DataSource = null;
                gvMeetings.DataBind();
            }
            else
            {
                gvMeetings.AllowPaging = dtMeetings.Rows.Count > gvMeetings.PageSize;
                gvMeetings.DataSource = dtMeetings;
                gvMeetings.DataBind();
            }}}}
