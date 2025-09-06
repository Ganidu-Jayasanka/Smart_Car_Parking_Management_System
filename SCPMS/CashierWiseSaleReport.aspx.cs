using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class CashierWiseSaleReport : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private decimal totalAmount = 0;
        private decimal totalDiscount = 0;
        private decimal totalFinalAmount = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateUserDropdowns();
            }
            // Check access using username from session
            if (!HasAccess("CashierWiseSaleReport.aspx"))
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
        // Method to populate the EnteredBy and ExitBy dropdowns with active users
        private void PopulateUserDropdowns()
        {
            string query = "SELECT SUId, UserName FROM SystemUser WHERE ActiveStatus = 1"; // Assuming ActiveStatus = 1 means active users
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                // Bind the ExitBy dropdown
                ddlExitBy.DataSource = reader;
                ddlExitBy.DataTextField = "UserName";
                ddlExitBy.DataValueField = "SUId";
                ddlExitBy.DataBind();
                ddlExitBy.Items.Insert(0, new System.Web.UI.WebControls.ListItem("Select All ", "")); // Add a default "Select" option
            }
        }
        // Button click event to refresh the data based on selected filters
        protected void btnView_Click(object sender, EventArgs e)
        {
            BindGridView(); // Refresh the grid view based on selected dates and filters       
        }
        // Method to bind the GridView with filtered data based on the selected filters
        private void BindGridView()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
        SELECT 
            t.InvoiceNo,
            v.VehicleType,
            t.VehicleNo,
            ps.PSArea, -- needed for FOC logic
            CONCAT(ps.PSArea, ' - ', ps.PSNo) AS ParkingDetail,
            t.EnteredDateTime,
            t.ExitDateTime,
            u2.UserName AS ExitBy,
            CEILING(DATEDIFF(MINUTE, t.EnteredDateTime, t.ExitDateTime) / 60.0) AS Duration,
            t.BillAmount AS Amount,
            t.Discount,
            t.TotBillAmount AS TotalAmount,
            t.PaymentType,
            t.PaymentStatus
        FROM [SCPMS].[CPMS].[Tickets] t
        JOIN [SCPMS].[CPMS].[Vehicles] v ON t.VehicleId = v.VehicleId
        INNER JOIN [SCPMS].[CPMS].[ParkingSlots] ps ON t.ParkingSlotPSid = ps.PSid
        LEFT JOIN [SCPMS].[dbo].[SystemUser] u1 ON t.EnteredBy = u1.SUId
        LEFT JOIN [SCPMS].[dbo].[SystemUser] u2 ON t.ExitBy = u2.SUId
        WHERE t.EnteredDateTime BETWEEN @StartDate AND @EndDate 
        AND (@ExitBy IS NULL OR t.ExitBy = @ExitBy)
        ORDER BY t.InvoiceNo DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    DateTime startDate;
                    if (DateTime.TryParse(txtStartDate.Text, out startDate) && startDate >= new DateTime(1753, 1, 1))
                        cmd.Parameters.AddWithValue("@StartDate", startDate);
                    else
                        cmd.Parameters.AddWithValue("@StartDate", new DateTime(1753, 1, 1));

                    DateTime endDate;
                    if (DateTime.TryParse(txtEndDate.Text, out endDate) && endDate <= new DateTime(9999, 12, 31))
                        cmd.Parameters.AddWithValue("@EndDate", endDate);
                    else
                        cmd.Parameters.AddWithValue("@EndDate", new DateTime(9999, 12, 31));

                    cmd.Parameters.AddWithValue("@ExitBy", string.IsNullOrEmpty(ddlExitBy.SelectedValue) ? (object)DBNull.Value : ddlExitBy.SelectedValue);

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }
            }
        }

        // Event handler for the GridView's RowDataBound to calculate the totals in the footer
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string psArea = DataBinder.Eval(e.Row.DataItem, "PSArea")?.ToString()?.Trim().ToUpper();

                decimal amount = DataBinder.Eval(e.Row.DataItem, "Amount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Amount"))
                    : 0;

                decimal discount = DataBinder.Eval(e.Row.DataItem, "Discount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Discount"))
                    : 0;

                decimal totalAmountValue = DataBinder.Eval(e.Row.DataItem, "TotalAmount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TotalAmount"))
                    : (amount - discount);

                if (psArea == "F")
                {
                    // Override display for FOC records
                    e.Row.Cells[8].Text = "Free of Charge";         // Amount
                    e.Row.Cells[10].Text = "Free of Charge";        // Total
                    e.Row.Cells[12].Text = "Meeting Attendee - FOC"; // PaymentStatus

                    // Optional: Clear discount display or keep it as 0
                    e.Row.Cells[9].Text = "-";
                }
                else
                {
                    // Sum only non-FOC records
                    totalAmount += amount;
                    totalDiscount += discount;
                    totalFinalAmount += totalAmountValue;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[7].Text = "Total:";
                e.Row.Cells[7].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[8].Text = $"LKR {totalAmount:N2}";
                e.Row.Cells[9].Text = $"LKR {totalDiscount:N2}";
                e.Row.Cells[10].Text = $"LKR {totalFinalAmount:N2}";
                e.Row.CssClass = "gridview-footer";
            }
        }

        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (GridView1 == null || GridView1.Rows.Count == 0)
            {
                Response.Write("<script>alert('No data available to export!');</script>");
                return;
            }

            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    using (XLWorkbook wb = new XLWorkbook())
                    {
                        var worksheet = wb.Worksheets.Add("CashierWiseSalesReport");

                        // Headers

                        worksheet.Cell(1, 1).Value = "Cashier Wise Sales Report";
                        worksheet.Range("A1:M1").Merge().Style
                            .Font.SetBold()
                            .Font.SetFontSize(24)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                      
                        worksheet.Cell(2, 1).Value = "Colombo Lotus Tower";
                        worksheet.Range("A2:M2").Merge().Style
                            .Font.SetBold()
                            .Font.SetFontSize(18)
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                      

                        string exportedBy = Session["LoggedInUserName"]?.ToString() ?? "Unknown User";
                        worksheet.Cell(3, 1).Value = $"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt} | Exported by: {exportedBy}";
                        worksheet.Range("A3:M3").Merge().Style
                            .Font.SetItalic()
                             .Font.SetBold()
                            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                        worksheet.Row(4).Height = 5;

                        // Table Headers
                        string[] headers = {
                    "Invoice No", "Vehicle Type", "Vehicle No", "Parking Detail", "Entered Date/Time", "Exit Date/Time",
                    "Exit By", "Duration (hrs)", "Amount", "Discount", "Total Amount", "Payment Type", "Payment Status"
                };

                        for (int col = 0; col < headers.Length; col++)
                        {
                            worksheet.Cell(5, col + 1).Value = headers[col];
                        }

                        var headerRow = worksheet.Range("A5:M5");
                        headerRow.Style.Font.SetBold();
                        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(0, 51, 102);
                        headerRow.Style.Font.FontColor = XLColor.White;
                        headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        headerRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        headerRow.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                        // Data Rows
                        int currentRow = 6;
                        decimal totalAmount = 0, totalDiscount = 0, totalFinalAmount = 0;

                        foreach (GridViewRow row in GridView1.Rows)
                        {
                            for (int col = 0; col < headers.Length; col++)
                            {
                                string cellValue = HttpUtility.HtmlDecode(row.Cells[col].Text.Trim());
                                worksheet.Cell(currentRow, col + 1).Value = cellValue;
                                worksheet.Cell(currentRow, col + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            }

                            // Try parse amounts (skip "Free of Charge")
                            decimal amountVal, discountVal, totalVal;
                            if (Decimal.TryParse(row.Cells[8].Text.Replace("LKR", "").Trim(), out amountVal))
                                totalAmount += amountVal;
                            if (Decimal.TryParse(row.Cells[9].Text.Replace("LKR", "").Trim(), out discountVal))
                                totalDiscount += discountVal;
                            if (Decimal.TryParse(row.Cells[10].Text.Replace("LKR", "").Trim(), out totalVal))
                                totalFinalAmount += totalVal;

                            currentRow++;
                        }

                        // Total Row
                        worksheet.Cell(currentRow, 8).Value = "TOTAL";
                        worksheet.Cell(currentRow, 9).Value = totalAmount;
                        worksheet.Cell(currentRow, 10).Value = totalDiscount;
                        worksheet.Cell(currentRow, 11).Value = totalFinalAmount;

                        var totalRow = worksheet.Range($"A{currentRow}:M{currentRow}");
                        totalRow.Style.Font.SetBold();
                        totalRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                        totalRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        totalRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                        worksheet.Columns().AdjustToContents();
                        wb.SaveAs(ms);
                    }

                    // Write to response
                    Response.Clear();
                    Response.Buffer = true;
                    Response.Charset = "";
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", "attachment;filename=CashierWiseSalesReport.xlsx");

                    ms.Position = 0;
                    ms.WriteTo(Response.OutputStream);
                    Response.Flush();
                    Response.SuppressContent = true;
                    HttpContext.Current.ApplicationInstance.CompleteRequest(); // safer than Response.End()
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error: " + ex.Message.Replace("'", "") + "');</script>");
            }
        }

        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            if (GridView1 == null || GridView1.Rows.Count == 0)
            {
                Response.Write("<script>alert('No data available to export!');</script>");
                return;
            }

            try
            {
                Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 50f, 30f);
                MemoryStream ms = new MemoryStream();
                PdfWriter.GetInstance(document, ms);
                document.Open();

                // **Header Section**
                Font headerFont = FontFactory.GetFont("Arial", 18, Font.BOLD, BaseColor.BLUE);
                Paragraph companyName = new Paragraph("Colombo Lotus Tower", headerFont) { Alignment = Element.ALIGN_CENTER };
                document.Add(companyName);
                Font titleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                Paragraph title = new Paragraph("Cashier Wise Sales Report", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10 };
                document.Add(title);
                string exportedBy = Session["LoggedInUserName"] != null ? Session["LoggedInUserName"].ToString() : "Unknown User";
                Font infoFont = FontFactory.GetFont("Arial", 12, Font.ITALIC);
                Paragraph reportInfo = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt}  |  Exported by: {exportedBy}", infoFont)
                { Alignment = Element.ALIGN_RIGHT, SpacingAfter = 20 };
                document.Add(reportInfo);
                // **Table Section**
                PdfPTable table = new PdfPTable(13);
                table.WidthPercentage = 100;

                string[] headers = { "Invoice No", "Vehicle Type", "Vehicle No", "Parking Detail", "Entered Date/Time", "Exit Date/Time",
                             "Exit By", "Duration (hrs)", "Amount", "Discount", "Total Amount", "Payment Type", "Payment Status" };

                foreach (string header in headers)
                {
                    PdfPCell cell = new PdfPCell(new Phrase(header, FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE)))
                    {
                        BackgroundColor = new BaseColor(0, 51, 102),
                        HorizontalAlignment = Element.ALIGN_CENTER
                    };
                    table.AddCell(cell);
                }
                decimal totalAmount = 0, totalDiscount = 0, totalFinalAmount = 0;

                foreach (GridViewRow row in GridView1.Rows)
                {
                    for (int col = 0; col < 13; col++)
                    {
                        string cellText = row.Cells[col].Text.Trim();
                        if (cellText == "&nbsp;") cellText = "0";

                        PdfPCell cell = new PdfPCell(new Phrase(string.IsNullOrEmpty(cellText) ? "-" : cellText, FontFactory.GetFont("Arial", 9, Font.NORMAL)))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER
                        };
                        table.AddCell(cell);
                    }

                    // Ensure correct indexing for amount, discount, and final total
                    decimal amount = 0, discount = 0, finalAmount = 0;
                    decimal.TryParse(row.Cells[8].Text.Trim().Replace("LKR", "").Trim(), out amount);
                    decimal.TryParse(row.Cells[9].Text.Trim().Replace("LKR", "").Trim(), out discount);
                    decimal.TryParse(row.Cells[10].Text.Trim().Replace("LKR", "").Trim(), out finalAmount);

                    totalAmount += amount;
                    totalDiscount += discount;
                    totalFinalAmount += finalAmount;
                }

                // **Adding Total Row**
                PdfPCell totalLabelCell = new PdfPCell(new Phrase("TOTAL:", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                {
                    Colspan = 8,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = new BaseColor(220, 220, 220)
                };
                table.AddCell(totalLabelCell);

                table.AddCell(new PdfPCell(new Phrase($"LKR {totalAmount:N2}", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = new BaseColor(220, 220, 220) });

                table.AddCell(new PdfPCell(new Phrase($"LKR {totalDiscount:N2}", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = new BaseColor(220, 220, 220) });

                table.AddCell(new PdfPCell(new Phrase($"LKR {totalFinalAmount:N2}", FontFactory.GetFont("Arial", 10, Font.BOLD)))
                { HorizontalAlignment = Element.ALIGN_CENTER, BackgroundColor = new BaseColor(220, 220, 220) });

                table.AddCell(new PdfPCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.BOLD))) { BackgroundColor = new BaseColor(220, 220, 220) });
                table.AddCell(new PdfPCell(new Phrase("", FontFactory.GetFont("Arial", 10, Font.BOLD))) { BackgroundColor = new BaseColor(220, 220, 220) });
                document.Add(table);
                document.Close();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=CashierWiseSalesReport.pdf");
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
            }}}}