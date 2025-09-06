
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace SCPMS
{
    public partial class Exit : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            Response.Cache.SetExpires(DateTime.UtcNow.AddHours(-1));

            if (Session["LoggedInUserId"] == null)
            {
                Response.Redirect("~/Login.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Check access using username from session
            if (!HasAccess("Exit.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
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

        private bool IsValidInvoiceNo(string invoiceNo)
        {
            return Regex.IsMatch(invoiceNo, "^TKT[A-Z]{3}\\d{10}$"); // Example: TKT followed by 3 letters and 10 digits
        }

        private bool IsValidVehicleNo(string vehicleNo)
        {
            // Updated regex to match Sri Lankan vehicle plate formats
            // Allows province code (optional), followed by optional space or hyphen, 
            // then an optional two or three-letter sequence, and finally a four-digit number.
            // Supports cases with or without a dash after the letter sequence.
            return Regex.IsMatch(vehicleNo, @"^(?:WP|CP|NP|EP|NC|SB|UV|NW|SP|BG|ND)?\s?-?\s?[A-Z]{2,3}?-?\s?\d{4}$", RegexOptions.IgnoreCase);
        }
        protected void btnSearch_Click(object sender, EventArgs e)
        {
            lblSearchError.Visible = false; // Hide previous error messages
            string searchInput = txtSearch.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchInput))
            {
                lblSearchError.Text = "Please enter an Invoice No or Vehicle No to search.";
                lblSearchError.CssClass = "message error";
                lblSearchError.Visible = true;
                return;
            }

            // Validate Input Format
            if (!IsValidInvoiceNo(searchInput) && !IsValidVehicleNo(searchInput))
            {
                lblSearchError.Text = "Invalid format for Invoice No or Vehicle No.";
                lblSearchError.CssClass = "message error";
                lblSearchError.Visible = true;
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();

                // ✅ Check if there is an active ticket (i.e., ExitDateTime is NULL)
                string activeTicketQuery = @"
            SELECT COUNT(*) 
            FROM [CPMS].[Tickets] 
            WHERE (InvoiceNo = @search OR VehicleNo = @search)
              AND ExitDateTime IS NULL";

                using (SqlCommand cmdActive = new SqlCommand(activeTicketQuery, con))
                {
                    cmdActive.Parameters.AddWithValue("@search", searchInput);
                    int activeCount = (int)cmdActive.ExecuteScalar();

                    if (activeCount == 0)
                    {
                        lblSearchError.Text = "This vehicle has already exited and cannot be processed again.";
                        lblSearchError.CssClass = "message error";
                        lblSearchError.Visible = true;
                        return;
                    }
                }

                // ✅ Fetch active ticket details
                string query = @"
               SELECT T.InvoiceNo, T.VehicleNo, V.VehicleType, T.EnteredDateTime, PS.PSArea
               FROM [CPMS].[Tickets] T 
               JOIN [CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
               JOIN [CPMS].[ParkingSlots] PS ON T.ParkingSlotPSid = PS.PSid
               WHERE (T.InvoiceNo = @search OR T.VehicleNo = @search)
               AND T.ExitDateTime IS NULL";


                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@search", searchInput);
                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        txtInvoiceNo.Text = reader["InvoiceNo"].ToString();
                        txtVehicleNo.Text = reader["VehicleNo"].ToString();
                        txtVehicleType.Text = reader["VehicleType"].ToString();

                        if (reader["EnteredDateTime"] != DBNull.Value)
                        {
                            DateTime enteredDateTime = Convert.ToDateTime(reader["EnteredDateTime"]);
                            txtInTime.Text = enteredDateTime.ToString("yyyy-MM-dd HH:mm:ss");

                            // 🛠 Calculate Duration
                            DateTime now = DateTime.Now;
                            TimeSpan duration = now - enteredDateTime;
                            double parkedHours = Math.Round(duration.TotalHours, 2); // example: 1.25 hours
                            txtParkedDuration.Text = parkedHours.ToString("0.00");
                        }
                        else
                        {
                            txtInTime.Text = "N/A";
                            txtParkedDuration.Text = "0.00"; // Safe default
                        }

                        // 🛠 Here add Meeting Attendee Checking
                        string parkingArea = reader["PSArea"].ToString();
                        bool isMeetingAttendee = (parkingArea == "F");

                        if (isMeetingAttendee)
                        {
                            // ✅ Real duration already calculated above
                            txtBillAmount.Text = "0.00";
                            txtDiscount.Text = "0.00";
                            txtTotal.Text = "0.00";
                            ddlPaymentType.SelectedValue = "Free"; // Set Free Payment
                            paymentDetailsCard.Visible = true;    // Show Payment Section
                        }
                        else
                        {
                            paymentDetailsCard.Visible = false;  // Hide Payment Section until Proceed to Pay
                        }

                        vehicleInfoCard.Visible = true; // after all
                    }

                    else
                    {
                        lblSearchError.Text = "This Vehicle No or Invoice No is invalid or has not entered.";
                        lblSearchError.CssClass = "message error";
                        lblSearchError.Visible = true;
                        vehicleInfoCard.Visible = false;
                    }
                }
            }
        }

        protected void btnProceedToPay_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInvoiceNo.Text) ||
          string.IsNullOrWhiteSpace(txtVehicleNo.Text) ||
          string.IsNullOrWhiteSpace(txtVehicleType.Text) ||
          string.IsNullOrWhiteSpace(txtInTime.Text))
            {
                lblProceedToPayError.Text = "Please ensure Invoice No, Vehicle No, Vehicle Type, and In Time fields are filled before proceeding to payment.";
                lblProceedToPayError.CssClass = "message error";
                lblProceedToPayError.Visible = true;
                return;
            }

            if (!IsValidInvoiceNo(txtInvoiceNo.Text) || !IsValidVehicleNo(txtVehicleNo.Text))
            {
                lblProceedToPayError.Text = "Invalid Invoice No or Vehicle No format.";
                lblProceedToPayError.CssClass = "message error";
                lblProceedToPayError.Visible = true;
                return;
            }
            try
            {
                int ticketId = GetCurrentTicketId(txtInvoiceNo.Text, txtVehicleNo.Text);
                if (ticketId == 0) return;

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // Retrieve Entry Time
                    DateTime enteredTime;
                    SqlCommand cmdGetEntryTime = new SqlCommand("SELECT EnteredDateTime FROM [CPMS].[Tickets] WHERE TicketID = @TicketID", con);
                    cmdGetEntryTime.Parameters.AddWithValue("@TicketID", ticketId);
                    object entryTimeObj = cmdGetEntryTime.ExecuteScalar();
                    if (entryTimeObj == null || entryTimeObj == DBNull.Value) return;
                    enteredTime = Convert.ToDateTime(entryTimeObj);

                    // Calculate Parked Duration (in hours)
                    DateTime exitTime = DateTime.Now;
                    double parkedHours = (exitTime - enteredTime).TotalHours;
                    parkedHours = Math.Ceiling(parkedHours); // Round up to the next full hour

                    // Get Vehicle Type and Hourly Rate
                    decimal hourlyRate = 0;
                    SqlCommand cmdGetRate = new SqlCommand(@"
                SELECT V.HourlyRate 
                FROM [CPMS].[Tickets] T
                JOIN [CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
                WHERE T.TicketID = @TicketID", con);
                    cmdGetRate.Parameters.AddWithValue("@TicketID", ticketId);
                    object rateObj = cmdGetRate.ExecuteScalar();
                    if (rateObj != null && rateObj != DBNull.Value)
                    {
                        hourlyRate = Convert.ToDecimal(rateObj);
                    }

                    // Calculate Bill Amount
                    decimal billAmount = hourlyRate * (decimal)parkedHours;

                    // Get existing Discount (default 0.00)
                    decimal discount = 0;
                    SqlCommand cmdGetDiscount = new SqlCommand("SELECT ISNULL(Discount, 0) FROM [CPMS].[Tickets] WHERE TicketID = @TicketID", con);
                    cmdGetDiscount.Parameters.AddWithValue("@TicketID", ticketId);
                    object discountObj = cmdGetDiscount.ExecuteScalar();
                    if (discountObj != null && discountObj != DBNull.Value)
                    {
                        discount = Convert.ToDecimal(discountObj);
                    }

                    // Calculate Total Amount
                    decimal totalAmount = billAmount - discount;

                    // Update the Ticket with the calculated values
                    // DO NOT update ExitDateTime here!
                    SqlCommand cmdUpdateTicket = new SqlCommand(@"
                      UPDATE [CPMS].[Tickets] 
                      SET DurationHours = @Duration, BillAmount = @BillAmount
                      WHERE TicketID = @TicketID", con);

                    cmdUpdateTicket.Parameters.AddWithValue("@Duration", parkedHours);
                    cmdUpdateTicket.Parameters.AddWithValue("@BillAmount", billAmount);
                    cmdUpdateTicket.Parameters.AddWithValue("@Total", totalAmount);
                    cmdUpdateTicket.Parameters.AddWithValue("@TicketID", ticketId);
                    cmdUpdateTicket.ExecuteNonQuery();

                    // Auto-fill UI fields
                    txtParkedDuration.Text = parkedHours.ToString("0.00");
                    txtBillAmount.Text = billAmount.ToString("0.00");
                    txtDiscount.Text = discount.ToString("0.00");
                    txtTotal.Text = totalAmount.ToString("0.00");

                    // Make payment section visible
                    paymentDetailsCard.Visible = true;
                }
            }
            catch (Exception ex)
            {
                LogError("Error in btnProceedToPay_Click: " + ex.Message);
            }
            lblProceedToPayError.Visible = false; // Hide error label if validation passes
        }


        // Method to execute the stored procedure for updating the ExitDateTime and calculating the BillAmount
        private void ExecuteSP_UpdateVehicleExit(int ticketId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlCommand cmdUpdateExit = new SqlCommand("[SCPMS].[SP_UpdateVehicleExit]", con);
                cmdUpdateExit.CommandType = CommandType.StoredProcedure;

                // Add TicketId as parameter
                cmdUpdateExit.Parameters.AddWithValue("@Ticketid", ticketId);

                con.Open();
                LogDebug("Executing SP_UpdateVehicleExit");
                cmdUpdateExit.ExecuteNonQuery(); // Executes the stored procedure
                LogDebug("Stored procedure executed successfully");
            }
        }
        protected void txtDiscount_TextChanged(object sender, EventArgs e)
        {
            try
            {
                decimal billAmount = Convert.ToDecimal(txtBillAmount.Text);
                decimal discount = !string.IsNullOrEmpty(txtDiscount.Text) ? Convert.ToDecimal(txtDiscount.Text) : 0;
                decimal totalAmount = billAmount - discount;
                txtTotal.Text = totalAmount.ToString("0.00");  // Update the total dynamically
                LogDebug("Total amount updated successfully based on discount.");
            }
            catch (Exception ex)
            {
                LogError("Error in txtDiscount_TextChanged: " + ex.Message);
            }
        }
        protected void btnExitVehicle_Click(object sender, EventArgs e)
        {
            lblExitVehicleError.Visible = false;
            lblMessage.Text = "";
            lblMessage.CssClass = "";

            if (string.IsNullOrWhiteSpace(txtParkedDuration.Text) ||
                string.IsNullOrWhiteSpace(txtBillAmount.Text) ||
                string.IsNullOrWhiteSpace(txtDiscount.Text) ||
                string.IsNullOrWhiteSpace(txtTotal.Text) ||
                string.IsNullOrWhiteSpace(ddlPaymentType.SelectedValue))
            {
                lblExitVehicleError.Text = "Ensure all fields (Parked Duration, Amount, Discount, Total, and Payment Type) are filled before exiting the vehicle.";
                lblExitVehicleError.CssClass = "message error";
                lblExitVehicleError.Visible = true;
                return;
            }
            if (Session["LoggedInUserId"] == null)
            {
                lblExitVehicleError.Text = "Session expired. Please log in again.";
                lblExitVehicleError.CssClass = "message error";
                lblExitVehicleError.Visible = true;
                return;
            }
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        int ticketId = GetCurrentTicketId(txtInvoiceNo.Text.Trim(), txtVehicleNo.Text.Trim());
                        if (ticketId == 0)
                        {
                            throw new Exception("Ticket not found.");
                        }
                        // Convert inputs to proper data types
                        decimal discount = Convert.ToDecimal(txtDiscount.Text);
                        decimal totalAmount = Convert.ToDecimal(txtTotal.Text);
                        string paymentType = ddlPaymentType.SelectedValue;
                        int userId = Convert.ToInt32(Session["LoggedInUserId"]);
                        using (SqlCommand cmdUpdate = new SqlCommand(@"
                    UPDATE [CPMS].[Tickets] 
                    SET 
                        ExitDateTime = GETDATE(), Discount = @Discount, TotBillAmount = @Total, PaymentType = @PaymentType, 
                        PaymentStatus = 'Paid', ExitGate = 'D', ExitBy = @UserId 
                    WHERE TicketID = @TicketID", con, transaction))
                        {
                            cmdUpdate.Parameters.AddWithValue("@Discount", discount);
                            cmdUpdate.Parameters.AddWithValue("@Total", totalAmount);
                            cmdUpdate.Parameters.AddWithValue("@PaymentType", paymentType);
                            cmdUpdate.Parameters.AddWithValue("@UserId", userId);
                            cmdUpdate.Parameters.AddWithValue("@TicketID", ticketId);

                            int rowsAffected = cmdUpdate.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("Vehicle exit update failed.");
                            }
                        }
                        // Retrieve the assigned parking slot ID (PSid)
                        int parkingSlotId = 0;
                        using (SqlCommand cmdGetSlot = new SqlCommand(@"
                    SELECT ParkingSlotPSid FROM [CPMS].[Tickets] WHERE TicketID = @TicketID", con, transaction))
                        {
                            cmdGetSlot.Parameters.AddWithValue("@TicketID", ticketId);
                            object slotObj = cmdGetSlot.ExecuteScalar();
                            if (slotObj != null && slotObj != DBNull.Value)
                            {
                                parkingSlotId = Convert.ToInt32(slotObj);
                            }
                        }
                        // Update parking slot status to 'Free' if a valid slot is found
                        if (parkingSlotId > 0)
                        {
                            using (SqlCommand cmdUpdateSlot = new SqlCommand(@"
                        UPDATE [CPMS].[ParkingSlots] 
                        SET Status = 'Free' 
                        WHERE PSid = @PSid", con, transaction))
                            {
                                cmdUpdateSlot.Parameters.AddWithValue("@PSid", parkingSlotId);
                                cmdUpdateSlot.ExecuteNonQuery();
                            }
                        }
                        // Commit transaction
                        // Commit transaction
                        transaction.Commit();

                        // Show Success Message
                        lblMessage.Text = "Vehicle exited successfully! Parking slot is now available.";
                        lblMessage.CssClass = "message success";

                        // Fetch entry and exit times
                        DateTime enteredTime = GetEnteredDateTime(txtInvoiceNo.Text.Trim(), txtVehicleNo.Text.Trim());
                        DateTime exitedTime = DateTime.Now;
                        double parkedDuration = Convert.ToDouble(txtParkedDuration.Text);

                        string area = "N/A";
                        string slotNo = "N/A";

                        // Get Area and Slot Number from ParkingSlots table
                        using (SqlCommand cmdGetSlotDetails = new SqlCommand(@"
                        SELECT PS.PSArea, PS.PSNo
                        FROM [CPMS].[ParkingSlots] PS
                        JOIN [CPMS].[Tickets] T ON T.ParkingSlotPSid = PS.PSid
                        WHERE T.TicketID = @TicketID", con, transaction))
                        {
                            cmdGetSlotDetails.Parameters.AddWithValue("@TicketID", ticketId);
                            using (SqlDataReader reader = cmdGetSlotDetails.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    area = reader["PSArea"].ToString();
                                    slotNo = reader["PSNo"].ToString();
                                }
                            }
                        }

                        // Generate Exit PDF Bill
                        GenerateExitPDFBill(
                         vehicleNo: txtVehicleNo.Text.Trim(),
                         invoiceNo: txtInvoiceNo.Text.Trim(),
                         vehicleType: txtVehicleType.Text.Trim(),
                         area: area,
                         slotNo: slotNo,
                         enteredTime: enteredTime,
                         exitedTime: exitedTime,
                         parkedDuration: parkedDuration,
                          totalAmount: totalAmount

                        );


                        // Clear all fields after success
                        ClearAllFields();
                    }
                    catch (Exception ex)
                    {
                        // Rollback transaction in case of error
                        transaction.Rollback();
                        // Show Error Message
                        lblMessage.Text = $"Error: {ex.Message}";
                        lblMessage.CssClass = "message error"; // Red error message
                    }
                }
            }
        }

        private void GenerateExitPDFBill(string vehicleNo, string invoiceNo, string vehicleType, string area, string slotNo, DateTime enteredTime, DateTime exitedTime, double parkedDuration, decimal totalAmount)
        {
            string filePath = Server.MapPath("~/Bills/") + "ExitBill_" + invoiceNo + ".pdf";

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
            string logoPath = Server.MapPath("~/Image/clt.png");
            if (File.Exists(logoPath))
            {
                Image logo = Image.GetInstance(logoPath);
                logo.ScaleToFit(100f, 100f);
                logo.Alignment = Element.ALIGN_CENTER;
                document.Add(logo);
            }

            // Title
            Paragraph title = new Paragraph("🏁 Exit Bill - Smart Car Parking System", titleFont)
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

            // Helper method to create cells
            PdfPCell CreateCell(string text, Font font, int alignment = Element.ALIGN_LEFT, bool isBold = false)
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

            // Add table rows
            table.AddCell(CreateCell("🚘 Vehicle Number:", labelFont));
            table.AddCell(CreateCell(vehicleNo, valueFont));

            table.AddCell(CreateCell("📄 Invoice Number:", labelFont));
            table.AddCell(CreateCell(invoiceNo, valueFont));

            table.AddCell(CreateCell("🚗 Vehicle Type:", labelFont));
            table.AddCell(CreateCell(vehicleType, valueFont));

            table.AddCell(CreateCell("📍 Area / Slot:", labelFont));
            table.AddCell(CreateCell($"{area} / {slotNo}", valueFont));

            table.AddCell(CreateCell("🕒 Entry Time:", labelFont));
            table.AddCell(CreateCell(enteredTime.ToString("yyyy-MM-dd HH:mm:ss"), valueFont));

            table.AddCell(CreateCell("🏁 Exit Time:", labelFont));
            table.AddCell(CreateCell(exitedTime.ToString("yyyy-MM-dd HH:mm:ss"), valueFont));

            table.AddCell(CreateCell("⏳ Duration:", labelFont));
            table.AddCell(CreateCell($"{parkedDuration:0.00} hours", valueFont));

            table.AddCell(CreateCell("💵 Total Amount:", labelFont));
            if (area.Trim().Equals("F", StringComparison.OrdinalIgnoreCase))
            {
                table.AddCell(CreateCell("Free of Charge - Meeting Attendee", valueFont));
            }
            else
            {
                table.AddCell(CreateCell($"LKR {totalAmount:0.00}", valueFont));
            }

            document.Add(table);

            // Footer
            document.Add(new Chunk(new LineSeparator(1f, 100f, BaseColor.LIGHT_GRAY, Element.ALIGN_CENTER, -2)));

            Paragraph footer = new Paragraph("\n🙏 Thank you for visiting Colombo Lotus Tower!", footerFont)
            {
                Alignment = Element.ALIGN_CENTER
            };
            document.Add(footer);

            document.Close();

            // JavaScript for download prompt
            string script = $@"
    alert('✅ Vehicle exited successfully!!! 🅿 Parking slot is now available.');
    window.location.href = '{ResolveUrl("~/Bills/") + "ExitBill_" + invoiceNo + ".pdf"}';";
            this.Page.ClientScript.RegisterStartupScript(this.GetType(), "SuccessAlertAndDownload", script, true);
        }


        private bool HasVehicleExited(string invoiceNo, string vehicleNo)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                LogDebug("Database connection opened successfully");
                SqlCommand cmd = new SqlCommand("SELECT ExitDateTime FROM [SCPMS].[CPMS].[Tickets] WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo", con);
                cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                object exitDateTime = cmd.ExecuteScalar();
                LogDebug($"ExitDateTime: {exitDateTime}");
                return exitDateTime != DBNull.Value;
            }
        }
        private bool UpdateExitDateTime(string invoiceNo, string vehicleNo)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                con.Open();
                LogDebug("Database connection opened successfully");
                SqlCommand cmd = new SqlCommand("UPDATE [CPMS].[Tickets] SET ExitDateTime = GETDATE() WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo", con);
                cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                int rowsAffected = cmd.ExecuteNonQuery();
                LogDebug($"Rows affected: {rowsAffected}");
                return rowsAffected > 0;
            }}
        private decimal GetBillAmount(int ticketId)
        {
            decimal billAmount = 0.0m;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT BillAmount FROM [CPMS].[Tickets] WHERE TicketID = @TicketID";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TicketID", ticketId);
                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            billAmount = Convert.ToDecimal(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error retrieving bill amount", ex);
                    }}}
            return billAmount;}
        private int GetCurrentTicketId(string invoiceNo, string vehicleNo)
        {
            int ticketId = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TicketID FROM [CPMS].[Tickets] WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);
                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            ticketId = Convert.ToInt32(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error retrieving ticket ID", ex);
                    }}}
            return ticketId;}
        private DateTime GetExitDateTime(string invoiceNo, string vehicleNo)
        {
            DateTime exitDateTime = DateTime.MinValue;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT ExitDateTime FROM [SCPMS].[CPMS].[Tickets]
                       WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            exitDateTime = Convert.ToDateTime(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error retrieving exit date and time", ex);
                    }
                }
            }

            return exitDateTime;
        }

        private DateTime GetEnteredDateTime(string invoiceNo, string vehicleNo)
        {
            DateTime enteredDateTime = DateTime.MinValue;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT EnteredDateTime FROM [SCPMS].[CPMS].[Tickets]
                       WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            enteredDateTime = Convert.ToDateTime(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error retrieving entered date and time", ex);
                    }
                }
            }

            return enteredDateTime;
        }

        private bool SaveDataToDB(string invoiceNo, string vehicleNo, decimal amount, decimal discount, decimal total, string paymentType)
        {
            bool success = false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE [SCPMS].[CPMS].[Tickets]
                       SET TotBillAmount = @TotBillAmount, PaymentType = @PaymentType
                       WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo AND ExitTime IS NOT NULL";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@TotBillAmount", total);
                    cmd.Parameters.AddWithValue("@PaymentType", paymentType);
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        success = rowsAffected > 0;
                    }
                    catch (Exception ex)
                    {
                        LogError("Error saving data to database", ex);
                    }
                }
            }

            return success;
        }


        private double GetDurationHours(string invoiceNo, string vehicleNo)
        {
            double durationHours = 0.0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT DurationHours FROM [SCPMS].[CPMS].[Tickets]
                       WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            durationHours = Convert.ToDouble(result);
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error retrieving duration hours", ex);
                    }
                }
            }

            return durationHours;
        }

        private int GetCurrentVehicleId(string invoiceNo, string vehicleNo)
        {
            VehicleInfo vehicleInfo = GetVehicleInfo(invoiceNo); // Or however you're getting it
            return vehicleInfo?.VehicleId ?? -1; // Return -1 or handle the case if vehicle not found
        }

        private void SearchVehicle(string invoiceNo, string vehicleNo)
        {

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT T.TicketId, T.InvoiceNo, T.VehicleNo, T.EnteredTime, V.VehicleId, V.VehicleType
                         FROM [SCPMS].[CPMS].[Tickets] T
                         JOIN [SCPMS].[CPMS].[Vehicles] V ON T.VehicleId = V.VehicleId
                         WHERE T.InvoiceNo = @InvoiceNo AND T.VehicleNo = @VehicleNo AND T.ExitTime IS NULL";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        VehicleInfo vehicleInfo = new VehicleInfo
                        {
                            TicketId = Convert.ToInt32(reader["TicketId"]),
                            InvoiceNo = reader["InvoiceNo"].ToString(),
                            VehicleNo = reader["VehicleNo"].ToString(),
                            EnteredTime = Convert.ToDateTime(reader["EnteredTime"]),
                            VehicleId = Convert.ToInt32(reader["VehicleId"]),
                            VehicleType = reader["VehicleType"].ToString()
                        };

                        DisplayVehicleInfo(vehicleInfo);
                    }
                }
            }
        }

        private void DisplayVehicleInfo(VehicleInfo vehicleInfo)
        {
            // Assuming you have labels or textboxes on your ASPX page to display the vehicle info
            lblInvoiceNo.Text = vehicleInfo.InvoiceNo;
            lblVehicleNo.Text = vehicleInfo.VehicleNo;
            lblInTime.Text = vehicleInfo.EnteredTime.ToString("yyyy-MM-dd HH:mm:ss");
            lblVehicleType.Text = vehicleInfo.VehicleType;

            // You may need to add more fields based on your requirements
        }


        private double CalculateParkingFee(string invoiceNo, DateTime exitTime)
        {
            double hourlyRate = 0.0;
            double totalHours = GetTotalHoursParked(invoiceNo, exitTime); // Ensure this is accurate
            double totalFee = 0.0;


            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT HourlyRate FROM [SCPMS].[CPMS].[Vehicles] WHERE VehicleId = @VehicleId"; // Adjust this line
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", GetCurrentVehicleId(invoiceNo, txtVehicleNo.Text)); // Pass vehicleId
                    con.Open();
                    var result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        double.TryParse(result.ToString(), out hourlyRate);
                    }
                }
            }

            totalFee = hourlyRate * totalHours;
            Console.WriteLine($"Hourly Rate: {hourlyRate}, Total Hours: {totalHours}, Total Fee: {totalFee}"); // Debug line
            return totalFee;
        }

        public double GetTotalHoursParked(string invoiceNo, DateTime exitTime)
        {
            LogDebug($"GetTotalHoursParked started - InvoiceNo: {invoiceNo}, ExitTime: {exitTime}");

            TimeSpan entryTime = GetEntryTimeFromDatabase(invoiceNo);
            LogDebug($"Entry time from database: {entryTime}");

            if (entryTime == TimeSpan.Zero)
            {
                LogError($"Entry time not found for InvoiceNo: {invoiceNo}");
                return 0;
            }

            DateTime entryDateTime = new DateTime(exitTime.Year, exitTime.Month, exitTime.Day, entryTime.Hours, entryTime.Minutes, entryTime.Seconds);
            if (entryDateTime > exitTime)
            {
                entryDateTime = entryDateTime.AddDays(-1);
            }

            TimeSpan duration = exitTime - entryDateTime;
            double totalHours = duration.TotalHours;

            LogDebug($"Calculated duration: {duration}, Total hours: {totalHours}");

            return Math.Max(totalHours, 0); // Ensure non-negative value
        }

        private TimeSpan GetEntryTimeFromDatabase(string invoiceNo)
        {
            LogDebug($"GetEntryTimeFromDatabase started - InvoiceNo: {invoiceNo}");

            TimeSpan entryTime = TimeSpan.Zero;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT EnteredTime FROM [SCPMS].[CPMS].[Tickets] WHERE InvoiceNo = @InvoiceNo AND ExitTime IS NULL";
                LogDebug($"SQL Query: {query}");

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@InvoiceNo", invoiceNo);

                    try
                    {
                        connection.Open();
                        LogDebug("Database connection opened successfully");

                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            entryTime = (TimeSpan)result;
                            LogDebug($"Entry time retrieved: {entryTime}");
                        }
                        else
                        {
                            LogError($"No entry time found for InvoiceNo: {invoiceNo}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error in GetEntryTimeFromDatabase", ex);
                    }
                    finally
                    {
                        if (connection.State == ConnectionState.Open)
                        {
                            connection.Close();
                            LogDebug("Database connection closed");
                        }
                    }
                }
            }

            return entryTime;
        }

        private VehicleInfo GetVehicleInfo(string searchValue)
        {
            VehicleInfo vehicle = null;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 1 [Ticketid], [InvoiceNo], [VehicleNo], [EnteredTime], [VehicleId]
                FROM [SCPMS].[CPMS].[Tickets]
                WHERE ([InvoiceNo] = @SearchValue OR [VehicleNo] = @SearchValue)
                AND [ExitTime] IS NULL
                ORDER BY [EnteredTime] DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SearchValue", searchValue);
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                vehicle = new VehicleInfo
                                {
                                    TicketId = reader.GetInt32(reader.GetOrdinal("Ticketid")),
                                    InvoiceNo = reader.GetString(reader.GetOrdinal("InvoiceNo")),
                                    VehicleNo = reader.GetString(reader.GetOrdinal("VehicleNo")),
                                    EnteredTime = reader.GetDateTime(reader.GetOrdinal("EnteredTime")),
                                    VehicleId = reader.GetInt32(reader.GetOrdinal("VehicleId"))
                                };

                                vehicle.VehicleType = GetVehicleType(vehicle.VehicleId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error in GetVehicleInfo", ex);
                    }
                }
            }

            return vehicle;
        }

        private string GetVehicleType(int vehicleId)
        {
            string vehicleType = "Unknown";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT [VehicleType] FROM [SCPMS].[CPMS].[VehicleTypes] WHERE VehicleId = @VehicleId";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@VehicleId", vehicleId);

                    try
                    {
                        connection.Open();
                        var result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            vehicleType = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error in GetVehicleType", ex);
                    }
                }
            }

            return vehicleType;
        }

        private TimeSpan CalculateParkedDuration(string inTime)
        {
            DateTime inDateTime = DateTime.Parse(inTime);
            TimeSpan duration = DateTime.Now - inDateTime;

            // Apply 30-minute grace period for the first hour
            if (duration.TotalHours < 1.5)
            {
                duration = TimeSpan.FromHours(1);
            }
            else
            {
                duration = TimeSpan.FromHours(Math.Ceiling(duration.TotalHours));
            }

            return duration;
        }


        private double CalculateParkingAmount(int vehicleId, TimeSpan parkedDuration)
        {
            double hourlyRate = GetHourlyRate(vehicleId);
            double totalHours = Math.Ceiling(parkedDuration.TotalHours);
            double amount = hourlyRate * totalHours;

            // Apply discount
            double discountPercentage = 0.0; // Define a constant for discount percentage
            double discount = amount * discountPercentage;
            amount -= discount;

            return amount;
        }

        private double GetHourlyRate(int vehicleId)
        {
            double hourlyRate = 0.0;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT HourlyRate FROM [SCPMS].[CPMS].[Vehicles] WHERE VehicleId = @VehicleId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@VehicleId", vehicleId);
                    try
                    {
                        con.Open();
                        var result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            if (double.TryParse(result.ToString(), out hourlyRate))
                            {
                                LogDebug($"Hourly rate for vehicleId {vehicleId}: {hourlyRate}");
                            }
                            else
                            {
                                LogError($"Failed to parse hourly rate for vehicleId {vehicleId}");
                            }
                        }
                        else
                        {
                            LogError($"No hourly rate found for vehicleId {vehicleId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError($"Error retrieving hourly rate for vehicleId {vehicleId}", ex);
                    }
                }
            }

            return hourlyRate;
        }

        private bool ProcessVehicleExit(string invoiceNo, string vehicleNo, decimal total, string paymentType)
        {
            LogDebug($"Starting ProcessVehicleExit - InvoiceNo: {invoiceNo}, VehicleNo: {vehicleNo}, Total: {total}, PaymentType: {paymentType}");

            bool success = false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"UPDATE [SCPMS].[CPMS].[Tickets]
                       SET ExitTime = GETDATE(), TotBillAmount = @Total, PaymentType = @PaymentType
                       WHERE InvoiceNo = @InvoiceNo AND VehicleNo = @VehicleNo AND ExitTime IS NULL";

                LogDebug($"SQL Query: {query}");

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Total", total);
                    cmd.Parameters.AddWithValue("@PaymentType", paymentType);
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);
                    cmd.Parameters.AddWithValue("@VehicleNo", vehicleNo);

                    LogDebug($"SQL Parameters: Total={total}, PaymentType={paymentType}, InvoiceNo={invoiceNo}, VehicleNo={vehicleNo}");

                    try
                    {
                        con.Open();
                        LogDebug("Database connection opened successfully");

                        int rowsAffected = cmd.ExecuteNonQuery();
                        LogDebug($"Rows affected: {rowsAffected}");

                        if (rowsAffected == 0)
                        {
                            LogError("Vehicle exit has already been processed. Cannot proceed to pay.");
                            return false;
                        }

                        success = rowsAffected > 0;

                        if (success)
                        {
                            LogDebug("Vehicle exit processed successfully");
                        }
                        else
                        {
                            LogError("No rows were updated in the database");
                        }
                    }
                    catch (SqlException ex)
                    {
                        LogError($"SQL Error: {ex.Message}", ex);
                        LogError($"Error Number: {ex.Number}");
                        LogError($"Procedure: {ex.Procedure}");
                        LogError($"Line Number: {ex.LineNumber}");
                        success = false;
                    }
                    catch (Exception ex)
                    {
                        LogError("Error in ProcessVehicleExit", ex);
                        success = false;
                    }
                    finally
                    {
                        if (con.State == ConnectionState.Open)
                        {
                            con.Close();
                            LogDebug("Database connection closed");
                        }
                    }
                }
            }

            LogDebug($"ProcessVehicleExit completed. Success: {success}");
            return success;
        }

        private void BindLastExitedVehicles()
        {
            List<ExitedVehicle> exitedVehicles = GetLastExitedVehicles();
            gvLastExitedVehicles.DataSource = exitedVehicles;
            gvLastExitedVehicles.DataBind();
        }

        private List<ExitedVehicle> GetLastExitedVehicles()
        {
            List<ExitedVehicle> exitedVehicles = new List<ExitedVehicle>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 5 [VehicleNo], [TotBillAmount]
                                 FROM [SCPMS].[CPMS].[Tickets]
                                 WHERE [ExitTime] IS NOT NULL
                                 ORDER BY [ExitTime] DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                exitedVehicles.Add(new ExitedVehicle
                                {
                                    VehicleNo = reader.GetString(reader.GetOrdinal("VehicleNo")),
                                    Amount = reader.GetDecimal(reader.GetOrdinal("TotBillAmount")).ToString("C")
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError("Error in GetLastExitedVehicles", ex);
                    }
                }
            }

            return exitedVehicles;
        }

        private void ClearVehicleInfo()
        {
            txtInvoiceNo.Text = "";
            txtVehicleNo.Text = "";
            txtVehicleType.Text = "";
            txtInTime.Text = "";
        }

        private void ClearAllFields()
        {
            txtInvoiceNo.Text = "";
            txtVehicleNo.Text = "";
            txtVehicleType.Text = "";
            txtInTime.Text = "";
            txtParkedDuration.Text = "";
            txtBillAmount.Text = "";
            txtDiscount.Text = "";
            txtTotal.Text = "";
            
        }

        private void LogDebug(string message)
        {
            // Implement your logging mechanism here
            System.Diagnostics.Debug.WriteLine($"DEBUG: {message}");
        }

        private void LogError(string message, Exception ex = null)
        {
            // Implement your logging mechanism here
            System.Diagnostics.Debug.WriteLine($"ERROR: {message}");
            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"Exception: {ex}");
            }
        }
    }

    public class VehicleInfo
    {
        public int TicketId { get; set; }
        public string InvoiceNo { get; set; }
        public string VehicleNo { get; set; }
        public DateTime EnteredTime { get; set; }
        public int VehicleId { get; set; }
        public string VehicleType { get; set; }
    }

    public class ExitedVehicle
    {
        public string VehicleNo { get; set; }
        public string Amount { get; set; }
    }

    public class PaymentCardDetails
    {
        public string ParkedDuration { get; set; }
        public decimal Amount { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        public string PaymentType { get; set; }
    }
   
 }
