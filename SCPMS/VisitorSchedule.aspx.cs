using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.UI;
using System.Net;
using System.Net.Mail;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace SCPMS
{
    public partial class VisitorSchedule : Page
    {
        // Connection string for database access
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedInUserId"] == null)
            {
                // User is not logged in, redirect to login page
                Response.Redirect("~/VMS_Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check access using username from session
            if (!HasAccess("VisitorSchedule.aspx"))
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
        [WebMethod]
        public static string GetAttendeeData(string searchType, string searchValue)
        {
            if (string.IsNullOrEmpty(searchType) || string.IsNullOrEmpty(searchValue))
            {
                return new JavaScriptSerializer().Serialize(new { error = "Invalid search parameters." });
            }

            string column = "";
            switch (searchType)
            {
                case "NIC":
                    column = "Identification";
                    break;
                case "Mobile":
                    column = "Mobile";
                    break;
                case "Email":
                    column = "Email";
                    break;
                default:
                    return new JavaScriptSerializer().Serialize(new { error = "Invalid search type." });
            }

            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
            string query = $"SELECT Identification, FirstName, LastName, Mobile, Company, Email, VehicleNo FROM MeetingAttend WHERE {column} = @SearchValue";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchValue", searchValue);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var attendee = new
                            {
                                Identification = reader["Identification"] != DBNull.Value ? reader["Identification"].ToString() : "",
                                FirstName = reader["FirstName"] != DBNull.Value ? reader["FirstName"].ToString() : "",
                                LastName = reader["LastName"] != DBNull.Value ? reader["LastName"].ToString() : "",
                                Mobile = reader["Mobile"] != DBNull.Value ? reader["Mobile"].ToString() : "",
                                Company = reader["Company"] != DBNull.Value ? reader["Company"].ToString() : "",
                                Email = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "",
                                VehicleNo = reader["VehicleNo"] != DBNull.Value ? reader["VehicleNo"].ToString() : ""
                            };

                            return new JavaScriptSerializer().Serialize(attendee);
                        }
                        else
                        {
                            return new JavaScriptSerializer().Serialize(new { error = "No record found." });
                        }
                    }
                }
            }
        }



        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            string attendeesData = Request.Form["attendees-data"];

            if (string.IsNullOrEmpty(attendeesData) || attendeesData.Trim() == "[]")
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('❌ Please add at least one attendee before submitting.');", true);
                return;
            }

            // Get values from form
            string meetingHeader = Request.Form["meetingHeader"];
            string description = Request.Form["description"];
            DateTime meetingDate = DateTime.Parse(Request.Form["meetingDate"]);
            TimeSpan startTime = TimeSpan.Parse(Request.Form["startTime"]);
            TimeSpan endTime = TimeSpan.Parse(Request.Form["endTime"]);
            string meetingBuilding = Request.Form["building"];
            string meetingFloor = Request.Form["floor"];
            string meetingPurpose = Request.Form["purpose"];

            string createdUser = Session["LoggedInUserName"] != null ? Session["LoggedInUserName"].ToString() : "UnknownUser";

            int meetingID = InsertMeetingInfo(meetingHeader, description, meetingDate, startTime, endTime, meetingBuilding, meetingFloor, meetingPurpose, createdUser);

            List<Attendee> attendees = GetAttendees();

            if (attendees.Count > 0)
            {
                foreach (var attendee in attendees)
                {
                    InsertAttendee(meetingID, attendee);

                    if (!string.IsNullOrEmpty(attendee.Email))
                    {
                        SendEmailToAttendee(attendee.Email, meetingHeader, description, meetingDate, startTime, endTime, createdUser);
                    }
                }
            }

            // ✅ Show a JavaScript alert
            ScriptManager.RegisterStartupScript(this, this.GetType(), "alert", "alert('✅ Successfully scheduled meeting !!!');", true);
        }




        private void SendEmailToAttendee(string emailAddress, string meetingHeader, string description, DateTime meetingDate, TimeSpan startTime, TimeSpan endTime, string createdUser)
        {
            try
            {
                // Retrieve the email and SMTP password from the app settings
                string smtpEmail = System.Configuration.ConfigurationManager.AppSettings["SMTPEmail"];
                string smtpAppPassword = System.Configuration.ConfigurationManager.AppSettings["SMTPAppPassword"];

                // Create the mail message
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(smtpEmail);  // Sender's email address
                mail.To.Add(emailAddress);  // Recipient's email address
                mail.Subject = "VMS Meeting Invitation: " + meetingHeader;
                mail.Body = $"Dear Visitor, \n\nYou are invited to a scheduled meeting. Please find the details below:\n\n" +
                            $"Meeting Details:\n\n" +
                            $"📌 Meeting: {meetingHeader}\n" +
                            $"📝 Meeting Description: {description}\n" +
                            $"📅 Date: {meetingDate.ToShortDateString()}\n" +
                            $"⏰ Time: From {startTime} - To {endTime}\n" +
                            $"👤 Scheduled By: {createdUser}\n\n" +
                            $"Please mark your calendar.\n\n" +
                            $"Best regards,\nColombo Lotus Tower Management Company (Private) Ltd";


                // SMTP client configuration for Gmail
                SmtpClient smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,  // Gmail's SMTP server port
                    Credentials = new NetworkCredential(smtpEmail, smtpAppPassword),  // Use SMTP app password
                    EnableSsl = true  // Enable SSL
                };

                // Send the email
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // Handle any errors that occur during the email sending process
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }


        private int InsertMeetingInfo(string meetingHeader, string description, DateTime meetingDate, TimeSpan startTime, TimeSpan endTime, string meetingBuilding, string meetingFloor, string meetingPurpose, string createdUser)
        {
            string query = @"
    INSERT INTO [dbo].[MeetingInfo] 
    ([MeetingNo], [MeetingHeader], [Description], [MeetingDate], [StartTime], [EndTime], [MeetingBuilding], [MeetingFloor], [MeetingPurpose], [CreatedAt], [CreatedUser]) 
    VALUES 
    (@MeetingNo, @MeetingHeader, @Description, @MeetingDate, @StartTime, @EndTime, @MeetingBuilding, @MeetingFloor, @MeetingPurpose, GETDATE(), @CreatedUser);
    SELECT SCOPE_IDENTITY();";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MeetingNo", GenerateMeetingNumber());
                    cmd.Parameters.AddWithValue("@MeetingHeader", meetingHeader);
                    cmd.Parameters.AddWithValue("@Description", description);
                    cmd.Parameters.AddWithValue("@MeetingDate", meetingDate);
                    cmd.Parameters.AddWithValue("@StartTime", startTime);
                    cmd.Parameters.AddWithValue("@EndTime", endTime);
                    cmd.Parameters.AddWithValue("@MeetingBuilding", meetingBuilding);
                    cmd.Parameters.AddWithValue("@MeetingFloor", meetingFloor);
                    cmd.Parameters.AddWithValue("@MeetingPurpose", meetingPurpose);
                    cmd.Parameters.AddWithValue("@CreatedUser", createdUser); // Use the logged-in username here

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());  // Returns the MeetingID
                }
            }
        }

        private void InsertAttendee(int meetingID, Attendee attendee)
        {
            string query = @"
        INSERT INTO [dbo].[MeetingAttend] 
        ([MeetingID], [Identification], [FirstName], [LastName], [Mobile], [Company], [Email], [VehicleNo]) 
        VALUES 
        (@MeetingID, @Identification, @FirstName, @LastName, @Mobile, @Company, @Email, @VehicleNo);";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MeetingID", meetingID);
                    cmd.Parameters.AddWithValue("@Identification", attendee.Identification);
                    cmd.Parameters.AddWithValue("@FirstName", attendee.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", attendee.LastName);
                    cmd.Parameters.AddWithValue("@Mobile", attendee.Mobile);
                    cmd.Parameters.AddWithValue("@Company", attendee.Company);
                    cmd.Parameters.AddWithValue("@Email", attendee.Email);
                    cmd.Parameters.AddWithValue("@VehicleNo", attendee.VehicleNo);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private List<Attendee> GetAttendees()
        {
            List<Attendee> attendees = new List<Attendee>();
            string attendeesJson = Request.Form["attendees-data"];  // Retrieve JSON string

            if (!string.IsNullOrEmpty(attendeesJson))
            {
                attendees = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Attendee>>(attendeesJson);
            }

            return attendees;
        }

        private string GenerateMeetingNumber()
        {
            return "M" + DateTime.Now.ToString("yyyyMMddHHmmss"); // Example meeting number generator
        }
    }

    public class Attendee
    {
        public string Identification { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Mobile { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string VehicleNo { get; set; }
    }
}
