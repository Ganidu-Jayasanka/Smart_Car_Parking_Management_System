using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class Entrance : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));

            if (Session["LoggedInUserId"] == null)
            {
                // User is not logged in, redirect to login page
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check access using username from session
            if (!HasAccess("Entrance.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            } 

      
                if (!IsPostBack)
                {
                    LoadVehicleTypes();
                    LoadParkingAreas(false);  // Normal load: exclude Area F
                }

                
        }
        //Connection String
        string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;


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


        // Load available parking areas into dropdown
        private void LoadParkingAreas(bool includeAreaF = false)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    string query = @"
                SELECT DISTINCT PSArea 
                FROM [CPMS].[ParkingSlots] 
                WHERE Status = 'Free' ";

                    if (!includeAreaF)
                    {
                        query += " AND PSArea <> 'F'";
                    }

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            ddlArea.DataSource = reader;
                            ddlArea.DataTextField = "PSArea";
                            ddlArea.DataValueField = "PSArea";
                            ddlArea.DataBind();
                        }
                        else
                        {
                            ddlArea.Items.Clear();
                        }
                    }
                }

                ddlArea.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Area", ""));
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading parking areas: " + ex.Message;
                lblMessage.CssClass = "message error";
            }
        }



        // Event handler for when a parking area is selected
        protected void ddlArea_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadParkingSlots();
        }
        // Load available parking slots for the selected area
        private void LoadParkingSlots()
        {
            string selectedArea = ddlArea.SelectedValue;
            if (!string.IsNullOrEmpty(selectedArea))
            {
                string query = "SELECT PSNo FROM CPMS.ParkingSlots WHERE PSArea = @PSArea AND Status = 'Free'";
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@PSArea", selectedArea);
                        con.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        if (reader.HasRows)
                        {
                            ddlSlotNumber.DataSource = reader;
                            ddlSlotNumber.DataTextField = "PSNo";
                            ddlSlotNumber.DataValueField = "PSNo";
                            ddlSlotNumber.DataBind();
                        }
                        ddlSlotNumber.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Slot No.", ""));
                    }}}
        }
        // Load vehicle types into dropdown
        private void LoadVehicleTypes()
        {
            string query = "SELECT DISTINCT VehicleType FROM CPMS.Vehicles";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        ddlVehicleType.DataSource = reader;
                        ddlVehicleType.DataTextField = "VehicleType";
                        ddlVehicleType.DataValueField = "VehicleType";
                        ddlVehicleType.DataBind();
                    }
                    ddlVehicleType.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select Vehicle Type", ""));
                }}
        }


        /// <summary>
        /// Event handler for Vehicle Number text change.
        /// Checks if the vehicle is a meeting attendee and sets parking area accordingly.
        /// </summary>
        protected void txtVehicleNo_TextChanged(object sender, EventArgs e)
        {
            // Get the entered vehicle number and trim whitespace
            string vehicleNo = txtVehicleNo.Text.Trim();

            if (!string.IsNullOrEmpty(vehicleNo))
            {
                CheckMeetingAttendee(vehicleNo);
            }
            else
            {
                // No vehicle number entered, ensure normal behavior
                ddlArea.Enabled = true;
                lblMessage.Text = "";
            }
        }

        /// <summary>
        /// Checks if the given vehicle is registered for a meeting within the next 3 days.
        /// If yes, selects Parking Area 'F', disables the area dropdown, loads free slots, and shows a message.
        /// Otherwise, resets to normal behavior.
        /// </summary>
        private void CheckMeetingAttendee(string vehicleNo)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    string query = @"
                SELECT M.MeetingDate, M.EndTime, M.Description
                FROM MeetingAttend A
                INNER JOIN MeetingInfo M ON A.MeetingID = M.MeetingID
                WHERE A.VehicleNo = @vehicleNo
                  AND M.MeetingDate = CAST(GETDATE() AS DATE)";   // 🔥 Only today's meetings

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@vehicleNo", vehicleNo);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            bool isMeetingAttendeeToday = false;
                            string meetingTitle = "";
                            DateTime now = DateTime.Now;

                            while (reader.Read())
                            {
                                DateTime meetingDate = reader.GetDateTime(reader.GetOrdinal("MeetingDate"));

                                TimeSpan meetingEndTime = TimeSpan.Zero;
                                if (!reader.IsDBNull(reader.GetOrdinal("EndTime")))
                                {
                                    meetingEndTime = reader.GetTimeSpan(reader.GetOrdinal("EndTime"));
                                }
                                else
                                {
                                    meetingEndTime = new TimeSpan(23, 59, 59);
                                }

                                DateTime meetingEndDateTime = meetingDate.Date.Add(meetingEndTime);

                                // Check if meeting has not ended yet
                                if (now <= meetingEndDateTime)
                                {
                                    isMeetingAttendeeToday = true;
                                    meetingTitle = reader["Description"].ToString(); // 🔥 Correct: use Description
                                    break;
                                }
                            }

                            if (isMeetingAttendeeToday)
                            {
                                LoadParkingAreas(true);  // Include Area F

                                if (ddlArea.Items.FindByValue("F") != null)
                                {
                                    ddlArea.SelectedValue = "F";
                                }

                                ddlArea.Enabled = false;
                                ddlSlotNumber.Enabled = true;
                                LoadFreeSlots("F");

                                // 🛠 Updated Welcome Message
                                lblMessage.Text = $"🎉 Welcome to \"{meetingTitle}\" at Colombo Lotus Tower! 🚗 Free Parking Area F selected. Please select a slot.";
                                lblMessage.CssClass = "message success";

                                // (Optional) Beautify Message Background:
                                lblMessage.Style["background-color"] = "#d4edda"; // Light green
                                lblMessage.Style["border"] = "1px solid #28a745"; // Darker green border
                                lblMessage.Style["padding"] = "10px";
                                lblMessage.Style["border-radius"] = "5px";
                            }
                            else
                            {
                                LoadParkingAreas(false); // Normal areas
                                ddlArea.Enabled = true;
                                ddlSlotNumber.Enabled = true;
                                lblMessage.Text = "";
                                lblMessage.CssClass = ""; // Clear styles if any
                                lblMessage.Style.Clear(); // Clear background if any
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error while checking meeting attendee: " + ex.Message;
                lblMessage.CssClass = "message error";
                lblMessage.Style.Clear(); // Optional: reset any styles on error
            }
        }





        /// <summary>
        /// Loads available (free) parking slots for the specified area into the dropdown.
        /// </summary>
        private void LoadFreeSlots(string area)
        {
            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();
                    // Correct query matching your real ParkingSlots table
                    string query = @"
                SELECT PSNo
                FROM [CPMS].[ParkingSlots]
                WHERE PSArea = @area AND Status = 'Free'";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@area", area);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            ddlSlotNumber.Items.Clear();  // Clear existing slots
                            while (reader.Read())
                            {
                                string slotNo = reader["PSNo"].ToString();
                                ddlSlotNumber.Items.Add(new System.Web.UI.WebControls.ListItem(slotNo, slotNo));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error loading free slots: " + ex.Message;
                lblMessage.CssClass = "message error";
            }
        }



        // Event handler for entering vehicle information
        protected void btnEnterVehicle_Click(object sender, EventArgs e)
        {
            string vehicleNo = txtVehicleNo.Text.Trim();
            string vehicleType = ddlVehicleType.SelectedValue;
            string area = ddlArea.SelectedValue;
            string slotNo = ddlSlotNumber.SelectedValue;
            string userId = Session["LoggedInUserId"]?.ToString();

            if (string.IsNullOrEmpty(userId))
            {
                lblMessage.Text = "Error: User is not logged in.";
                lblMessage.CssClass = "message error";
                return;
            }

            if (string.IsNullOrEmpty(vehicleNo) || string.IsNullOrEmpty(vehicleType) || string.IsNullOrEmpty(area) || string.IsNullOrEmpty(slotNo))
            {
                lblMessage.Text = "Please fill in all required fields.";
                lblMessage.CssClass = "message error";
                return;
            }

            // Validate vehicle number
            if (!IsValidVehicleNo(vehicleNo))
            {
                lblMessage.Text = "Invalid vehicle number format.";
                lblMessage.CssClass = "message error";
                return;
            }

            // Check if vehicle is already inside the parking lot
            if (IsVehicleCurrentlyParked(vehicleNo))
            {
                lblMessage.Text = "This vehicle is already inside the parking lot. It must exit before re-entering.";
                lblMessage.CssClass = "message error";
                return;
            }

            string vehicleCode = GetVehicleCode(vehicleType);
            if (string.IsNullOrEmpty(vehicleCode))
            {
                lblMessage.Text = "Error: Unable to retrieve vehicle code.";
                lblMessage.CssClass = "message error";
                return;
            }

            string ticketNumber = GenerateTicketNumber(vehicleCode);
            DateTime enteredTime = DateTime.Now;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Get Vehicle ID based on vehicle type
                        int vehicleId = GetVehicleId(vehicleType, con, transaction);
                        if (vehicleId == 0)
                        {
                            throw new Exception("Vehicle not found.");
                        }

                        // Step 2: Insert the ticket record into the database
                        InsertTicket(vehicleId, vehicleNo, area, slotNo, enteredTime, userId, ticketNumber, con, transaction);

                        // Step 3: Update the parking slot status (e.g., mark it as occupied)
                        UpdateParkingSlotStatus(area, slotNo, con, transaction);

                        // Commit the transaction after all database operations
                        transaction.Commit();

                        // Step 4: Show success message to the user and update feedback checkbox
                        lblMessage.Text = "Vehicle entered successfully!";
                        lblMessage.CssClass = "message success";
                        chkEnterAndOpen.Checked = true;

                        // Ensure UI updates before proceeding with PDF generation
                        //    this.Page.ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlert", "alert('  Vehicle entered successfully!');", true);

                        // Step 5: Generate and download the PDF bill after successful entry
                        GeneratePDFBill(vehicleNo, ticketNumber, area, slotNo, enteredTime);

                        // Step 4: Show success message to the user and update feedback checkbox
                        lblMessage.Text = "Vehicle entered successfully!";
                        lblMessage.CssClass = "message success";
                        chkEnterAndOpen.Checked = true;
                    }
                    catch (Exception ex)
                    {
                        // Handle any errors here and roll back the transaction if necessary
                      
                        lblMessage.Text = "Error: " + ex.Message;
                        lblMessage.CssClass = "message error";
                    }
                }
            }
        }



        // Required for Thread.Sleep()

        private void GeneratePDFBill(string vehicleNo, string ticketNumber, string area, string slotNo, DateTime enteredTime)
        {
            string filePath = Server.MapPath("~/Bills/") + "Bill_" + ticketNumber + ".pdf";

            Document document = new Document(PageSize.A4, 50, 50, 30, 30);
            PdfWriter.GetInstance(document, new FileStream(filePath, FileMode.Create));
            document.Open();

            // Fonts
            Font titleFont = FontFactory.GetFont("Arial", 20, Font.BOLD, BaseColor.DARK_GRAY);
            Font headerFont = FontFactory.GetFont("Arial", 16, Font.BOLD, BaseColor.BLUE);
            Font labelFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.BLACK);
            Font valueFont = FontFactory.GetFont("Arial", 12, Font.NORMAL, BaseColor.BLACK);
            Font footerFont = FontFactory.GetFont("Arial", 10, Font.ITALIC, BaseColor.GRAY);

            // Logo (optional) - if you have an image at ~/Images/logo.png
            // Logo (optional) - PDF image insert
            string logoPath = Server.MapPath("~/Image/clt.png");
            if (File.Exists(logoPath))
            {
                iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath); // <== fully qualified here
                logo.ScaleToFit(100f, 100f);
                logo.Alignment = Element.ALIGN_CENTER;
                document.Add(logo);
            }


            // Title
            Paragraph title = new Paragraph("🚗 Entrance Bill - Smart Car Parking System", titleFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 10f
            };
            document.Add(title);

            // Company Name
            Paragraph companyName = new Paragraph("Colombo Lotus Tower", headerFont)
            {
                Alignment = Element.ALIGN_CENTER,
                SpacingAfter = 20f
            };
            document.Add(companyName);

            document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.BLACK, Element.ALIGN_CENTER, -2)));

            // Table
            PdfPTable table = new PdfPTable(2);
            table.WidthPercentage = 100;
            table.SpacingBefore = 20f;
            table.SpacingAfter = 20f;
            table.DefaultCell.BorderColor = BaseColor.LIGHT_GRAY;
            table.SetWidths(new float[] { 1.5f, 3f });

            // Helper method to create table cells
            PdfPCell CreateCell(string text, Font font, int alignment = Element.ALIGN_LEFT)
            {
                var cell = new PdfPCell(new Phrase(text, font))
                {
                    Padding = 8,
                    BorderColor = BaseColor.LIGHT_GRAY,
                    HorizontalAlignment = alignment,
                    VerticalAlignment = Element.ALIGN_MIDDLE
                };
                return cell;
            }

            // Add rows
            table.AddCell(CreateCell("🚘 Vehicle Number:", labelFont));
            table.AddCell(CreateCell(vehicleNo, valueFont));

            table.AddCell(CreateCell("🎟 Ticket Number:", labelFont));
            table.AddCell(CreateCell(ticketNumber, valueFont));

            table.AddCell(CreateCell("📍 Parking Area:", labelFont));
            table.AddCell(CreateCell(area, valueFont));

            table.AddCell(CreateCell("🅿 Slot Number:", labelFont));
            table.AddCell(CreateCell(slotNo, valueFont));

            table.AddCell(CreateCell("⏳ Entry Time:", labelFont));
            table.AddCell(CreateCell(enteredTime.ToString("yyyy-MM-dd HH:mm:ss"), valueFont));

            table.AddCell(CreateCell("💰 Payment Status:", labelFont));

            if (area.Trim().Equals("F", StringComparison.OrdinalIgnoreCase))
            {
                table.AddCell(CreateCell("Free of Charge - Meeting Attendee", valueFont));
            }
            else
            {
                table.AddCell(CreateCell("Unpaid", valueFont));
            }

            document.Add(table);

            document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -2)));

            // Footer
            Paragraph thankYou = new Paragraph("\n🙏 Thank you for using our service!", footerFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(thankYou);

            document.Close();

            // JS for alert and download
            string script = $@"
    alert('✅ Vehicle entered successfully!');
    window.location.href = '{ResolveUrl("~/Bills/") + "Bill_" + ticketNumber + ".pdf"}';";
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlertAndDownload", script, true);
        }


        // Method to check if a vehicle is currently inside the parking lot
        private bool IsVehicleCurrentlyParked(string vehicleNo)
        {
            string query = "SELECT COUNT(*) FROM [SCPMS].[CPMS].[Tickets] WHERE VehicleNo = @VehicleNo AND ExitDateTime IS NULL";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                    con.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0; // Returns true if vehicle is still inside
                }
            }
        }



        private bool IsValidVehicleNo(string vehicleNo)
        {
            // Updated regex to match Sri Lankan vehicle plate formats
            // Allows province code (optional), followed by optional space or hyphen, 
            // then an optional two or three-letter sequence, and finally a four-digit number.
            // Supports cases with or without a dash after the letter sequence.
            return Regex.IsMatch(vehicleNo, @"^(?:WP|CP|NP|EP|NC|SB|UV|NW|SP|BG|ND)?\s?-?\s?[A-Z]{2,3}?-?\s?\d{4}$", RegexOptions.IgnoreCase);
        }




        private int GetVehicleId(string vehicleType, SqlConnection con, SqlTransaction transaction)
        {
            string query = "SELECT TOP 1 VehicleId FROM [SCPMS].[CPMS].[Vehicles] WHERE VehicleType = @VehicleType";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@VehicleType", vehicleType);
                object result = cmd.ExecuteScalar();
                return result != null ? Convert.ToInt32(result) : 0;
            }
        }
        private void UpdateParkingSlotStatus(string area, string slotNo, SqlConnection con, SqlTransaction transaction)
        {
            string query = "UPDATE [SCPMS].[CPMS].[ParkingSlots] SET Status = 'Parked' WHERE PSArea = @PSArea AND PSNo = @PSNo";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@PSArea", area);
                cmd.Parameters.AddWithValue("@PSNo", slotNo);
                cmd.ExecuteNonQuery();
            }
        }
        private string GenerateTicketNumber(string vehicleCode)
        {
            string currentDate = DateTime.Now.ToString("yyMMdd");
            string uniqueNumber = GetNextUniqueNumber();
            return $"TKT{vehicleCode}{currentDate}{uniqueNumber}";
        }
        private void InsertTicket(int vehicleId, string vehicleNo, string area, string slotNo, DateTime enteredTime, string userId, string ticketNumber, SqlConnection con, SqlTransaction transaction)
        {
            string query = @"
                INSERT INTO [SCPMS].[CPMS].[Tickets] 
                (VehicleId, ParkingSlotPSid, VehicleNo, EnteredDateTime, EnteredGate, EnteredBy, PaymentStatus, InvoiceNo)
                VALUES 
                (@VehicleId, 
                (SELECT TOP 1 PSid FROM [SCPMS].[CPMS].[ParkingSlots] WHERE PSArea = @PSArea AND PSNo = @PSNo), 
                @VehicleNo, @EnteredDateTime, @EnteredGate, @EnteredBy, @PaymentStatus, @InvoiceNo)";
            using (SqlCommand cmd = new SqlCommand(query, con, transaction))
            {
                cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
                cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                cmd.Parameters.AddWithValue("@PSArea", area);
                cmd.Parameters.AddWithValue("@PSNo", slotNo);
                cmd.Parameters.AddWithValue("@EnteredDateTime", enteredTime);
                cmd.Parameters.AddWithValue("@EnteredGate", "C");
                cmd.Parameters.AddWithValue("@EnteredBy", userId);
                cmd.Parameters.AddWithValue("@PaymentStatus", "UnPaid");
                cmd.Parameters.AddWithValue("@InvoiceNo", ticketNumber);
                cmd.ExecuteNonQuery();
            }
        }
         // Get the VehicleCode based on the selected VehicleType
        private string GetVehicleCode(string vehicleType)
        {
            string query = "SELECT VehicleCode FROM [SCPMS].[CPMS].[Vehicles] WHERE VehicleType = @VehicleType";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VehicleType", vehicleType);
                    con.Open();
                    return cmd.ExecuteScalar()?.ToString();
                }
            }
        }
        private string GetVehicleShortCode(string vehicleType)
        {
            string shortCode = null;
            string query = "SELECT VehicleCode FROM [SCPMS].[CPMS].[Vehicles] WHERE VehicleType = @VehicleType";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VehicleType", vehicleType);
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        shortCode = result.ToString();
                    }
                }
            }
            return shortCode;
        }
        // Generate a ticket number based on vehicle number and the current date
        private string GenerateInvoiceNumber(string vehicleCode)
        {
            string currentDate = DateTime.Now.ToString("yyMMdd");
            string uniqueNumber = GetNextUniqueNumber();
            return $"INV{vehicleCode}{currentDate}{uniqueNumber}";
        }
        // Method to get the next unique number (stub for demo)
        private string GetNextUniqueNumber()
        {
            string query = "SELECT ISNULL(MAX(RIGHT(InvoiceNo, 4)), 0) + 1 AS NextValue FROM [SCPMS].[CPMS].[Tickets] WHERE InvoiceNo LIKE 'TKT%' AND InvoiceNo LIKE @CurrentDate";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CurrentDate", $"%{DateTime.Now:yyMMdd}%");
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    return result != null ? result.ToString().PadLeft(4, '0') : "0001";
                }
            }}}}