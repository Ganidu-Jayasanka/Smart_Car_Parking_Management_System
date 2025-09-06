using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.Services;
using System.Collections.Generic;

namespace SCPMS
{
    public partial class VMS_Dashboard : System.Web.UI.Page
    {
        private static readonly string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadLatestMeetings();
                GetTodayMeetingCount();
            }

            // Check access using username from session
            if (!HasAccess("VMS_Dashboard.aspx"))
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
        /// <summary>
        /// ✅ WebMethod to get today's meeting count for the dashboard
        /// </summary>
        [WebMethod]
        public static int GetTodayMeetingCount()
        {
            int count = 0;
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM MeetingInfo WHERE MeetingDate = CAST(GETDATE() AS DATE)";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                count = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return count;
        }

        /// <summary>
        /// ✅ WebMethod to get daily meeting count for Chart.js
        /// </summary>
        [WebMethod]
        public static List<MeetingData> GetMeetingData()
        {
            List<MeetingData> meetingList = new List<MeetingData>();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                SELECT CONVERT(VARCHAR, MeetingDate, 23) AS MeetingDate, COUNT(*) AS MeetingCount
                FROM MeetingInfo
                WHERE MeetingDate = CAST(GETDATE() AS DATE)
                GROUP BY CONVERT(VARCHAR, MeetingDate, 23)";

                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    meetingList.Add(new MeetingData
                    {
                        MeetingDate = reader["MeetingDate"].ToString(),
                        MeetingCount = Convert.ToInt32(reader["MeetingCount"])
                    });
                }
            }
            return meetingList;
        }

        /// <summary>
        /// ✅ Load latest meetings only for today with improved layout
        /// </summary>
        private void LoadLatestMeetings()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
            m.MeetingNo, 
            m.MeetingHeader, 
            FORMAT(m.MeetingDate, 'yyyy-MM-dd') AS MeetingDate,
            m.StartTime,  -- Fetch StartTime directly here without formatting
            COALESCE(m.CreatedUser, 'Unknown') AS ScheduledPerson, 
            COALESCE(ma.FirstName + ' ' + ma.LastName, 'No Visitor') AS VisitorName, 
            ma.VehicleNo,
            COALESCE(ma.Identification, 'N/A') AS Identification
        FROM MeetingInfo m
        LEFT JOIN MeetingAttend ma ON m.MeetingID = ma.MeetingID
        WHERE m.MeetingDate = CAST(GETDATE() AS DATE)
        ORDER BY m.StartTime ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvLatestMeetings.DataSource = dt;
                gvLatestMeetings.DataBind();
            }
        }
        /// <summary>
        /// ✅ Helper class for Meeting Data (Used in Chart.js)
        /// </summary>
        public class MeetingData
        {
            public string MeetingDate { get; set; }
            public int MeetingCount { get; set; }
        }}}
