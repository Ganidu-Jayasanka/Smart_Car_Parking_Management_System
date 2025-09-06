using ClosedXML.Excel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SCPMS
{
    public partial class SaleSummaryReport : System.Web.UI.Page
    {
        private decimal totalAmount = 0;
        private decimal totalDiscount = 0;
        private decimal totalFinalAmount = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindGridView(); // Display all records initially
            }
            // Check access using username from session
            if (!HasAccess("SaleSummaryReport.aspx"))
            {
                // Redirect to Unauthorized page if the user doesn't have access (CanAccess = 0)
                Response.Redirect("~/Unauthorized.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            } }
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
        protected void btnView_Click(object sender, EventArgs e)
        {
            BindGridView(); // Refresh the grid view based on selected dates
            
        }
        private void BindGridView()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        t.InvoiceNo,
                        v.VehicleType,
                        t.VehicleNo,
                        CONCAT(ps.PSArea, ' - ', ps.PSNo) AS ParkingDetail,
                        t.EnteredDateTime,
                        u1.UserName AS EnteredBy,
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
                    ORDER BY t.InvoiceNo DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", txtStartDate.Text);
                    cmd.Parameters.AddWithValue("@EndDate", txtEndDate.Text);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                } } }
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string parkingDetail = DataBinder.Eval(e.Row.DataItem, "ParkingDetail")?.ToString();
                string areaCode = parkingDetail?.Split('-')[0]?.Trim().ToUpper();

                // Handle fields
                decimal amount = DataBinder.Eval(e.Row.DataItem, "Amount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Amount"))
                    : 0;

                decimal discount = DataBinder.Eval(e.Row.DataItem, "Discount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "Discount"))
                    : 0;

                decimal totalAmountValue = DataBinder.Eval(e.Row.DataItem, "TotalAmount") != DBNull.Value
                    ? Convert.ToDecimal(DataBinder.Eval(e.Row.DataItem, "TotalAmount"))
                    : (amount - discount);

                // If area is F, apply Free of Charge logic
                if (areaCode == "F")
                {
                    e.Row.Cells[9].Text = "Free of Charge";           // Amount
                    e.Row.Cells[10].Text = "-";                       // Discount
                    e.Row.Cells[11].Text = "Free of Charge";          // Total
                    e.Row.Cells[13].Text = "Meeting Attendee - FOC";  // Payment Status
                }
                else
                {
                    // Add to totals only if not FOC
                    totalAmount += amount;
                    totalDiscount += discount;
                    totalFinalAmount += totalAmountValue;
                }
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[8].Text = "Total:";
                e.Row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[9].Text = $"LKR {totalAmount:N2}";
                e.Row.Cells[10].Text = $"LKR {totalDiscount:N2}";
                e.Row.Cells[11].Text = $"LKR {totalFinalAmount:N2}";
                e.Row.CssClass = "gridview-footer";
            }
        }

        // Export to Excel
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (GridView1 == null || GridView1.Rows.Count == 0)
            {
                Response.Write("<script>alert('No data available to export!');</script>");
                return;
            }

            try
            {
                using (XLWorkbook wb = new XLWorkbook())
                {
                    var worksheet = wb.Worksheets.Add("SaleSummaryReport");

                    // Header
                    worksheet.Cell(1, 1).Value = "Sales Summary Report";
                    worksheet.Range("A1:N1").Merge().Style
                        .Font.SetBold()
                        .Font.SetFontSize(24)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);


                    worksheet.Cell(2, 1).Value = "Colombo Lotus Tower";
                    worksheet.Range("A2:N2").Merge().Style
                        .Font.SetBold()
                        .Font.SetFontSize(18)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                

                    string exportedBy = Session["LoggedInUserName"]?.ToString() ?? "Unknown User";
                    worksheet.Cell(3, 1).Value = $"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt} | Exported by: {exportedBy}";
                    worksheet.Range("A3:N3").Merge().Style
                        .Font.SetItalic()
                        .Font.SetBold()
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Right);

                    // Table Headers
                    string[] headers = {
                "Invoice No", "Vehicle Type", "Vehicle No", "Parking Detail", "Entered Date/Time", "Entered By",
                "Exit Date/Time", "Exit By", "Duration (hrs)", "Amount", "Discount", "Total Amount", "Payment Type", "Payment Status"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cell(5, i + 1).Value = headers[i];
                        worksheet.Cell(5, i + 1).Style.Font.SetBold();
                        worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(0, 51, 102);
                        worksheet.Cell(5, i + 1).Style.Font.FontColor = XLColor.White;
                        worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        worksheet.Cell(5, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    }

                    int currentRow = 6;
                    decimal totalAmount = 0, totalDiscount = 0, totalFinalAmount = 0;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        string parkingDetail = HttpUtility.HtmlDecode(row.Cells[3].Text.Trim());
                        string areaCode = parkingDetail.Split('-')[0].Trim().ToUpper();

                        for (int i = 0; i < headers.Length; i++)
                        {
                            string cellValue = HttpUtility.HtmlDecode(row.Cells[i].Text.Trim());
                            worksheet.Cell(currentRow, i + 1).Value = cellValue;
                            worksheet.Cell(currentRow, i + 1).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                            // Optional: highlight FOC row
                            if (areaCode == "F")
                            {
                                worksheet.Row(currentRow).Style.Fill.BackgroundColor = XLColor.White;
                            }
                        }

                        // Skip "F" (FOC) rows in totals
                        if (areaCode != "F")
                        {
                            decimal amountVal, discountVal, totalVal;
                            if (decimal.TryParse(row.Cells[9].Text.Replace("LKR", "").Trim(), out amountVal))
                                totalAmount += amountVal;
                            if (decimal.TryParse(row.Cells[10].Text.Replace("LKR", "").Trim(), out discountVal))
                                totalDiscount += discountVal;
                            if (decimal.TryParse(row.Cells[11].Text.Replace("LKR", "").Trim(), out totalVal))
                                totalFinalAmount += totalVal;
                        }

                        currentRow++;
                    }

                    // Total row
                    worksheet.Cell(currentRow, 9).Value = "TOTAL";
                    worksheet.Cell(currentRow, 10).Value = totalAmount.ToString("0,0.00");
                    worksheet.Cell(currentRow, 11).Value = totalDiscount.ToString("0,0.00");
                    worksheet.Cell(currentRow, 12).Value = totalFinalAmount.ToString("0,0.00");

                    var totalRow = worksheet.Range($"A{currentRow}:N{currentRow}");
                    totalRow.Style.Font.SetBold();
                    totalRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    totalRow.Style.Fill.BackgroundColor = XLColor.LightGray;
                    totalRow.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    worksheet.Columns().AdjustToContents();

                    using (MemoryStream ms = new MemoryStream())
                    {
                        wb.SaveAs(ms);
                        ms.Position = 0;

                        Response.Clear();
                        Response.Buffer = true;
                        Response.Charset = "";
                        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        Response.AddHeader("Content-Disposition", "attachment; filename=SaleSummaryReport.xlsx");
                        Response.BinaryWrite(ms.ToArray());
                        Response.Flush();
                        Response.SuppressContent = true;
                        HttpContext.Current.ApplicationInstance.CompleteRequest(); // Safe close
                    }
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Export Error: " + ex.Message.Replace("'", "") + "');</script>");
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
                using (MemoryStream ms = new MemoryStream())
                {
                    Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 20f, 10f);
                    PdfWriter writer = PdfWriter.GetInstance(document, ms);
                    document.Open();

                    // Company Name
                    Font headerFont = FontFactory.GetFont("Arial", 18, Font.BOLD, BaseColor.BLUE);
                    Paragraph companyName = new Paragraph("Colombo Lotus Tower", headerFont) { Alignment = Element.ALIGN_CENTER };
                    document.Add(companyName);

                    // Report Title
                    Font titleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                    Paragraph title = new Paragraph("Sales Summary Report", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10 };
                    document.Add(title);

                    // Meta Info
                    string exportedBy = Session["LoggedInUserName"] != null ? Session["LoggedInUserName"].ToString() : "Unknown User";
                    Paragraph meta = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt}  |  Exported by: {exportedBy}", FontFactory.GetFont("Arial", 10, Font.ITALIC));
                    meta.Alignment = Element.ALIGN_RIGHT;
                    document.Add(meta);
                    document.Add(new Paragraph("\n"));

                    // PDF Table
                    PdfPTable pdfTable = new PdfPTable(GridView1.HeaderRow.Cells.Count);
                    pdfTable.WidthPercentage = 100;

                    foreach (TableCell headerCell in GridView1.HeaderRow.Cells)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(headerCell.Text, FontFactory.GetFont("Arial", 10, Font.BOLD, BaseColor.WHITE)));
                        cell.BackgroundColor = new BaseColor(0, 51, 102);
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        pdfTable.AddCell(cell);
                    }

                    decimal totalAmount = 0, totalDiscount = 0, totalFinalAmount = 0;

                    foreach (GridViewRow row in GridView1.Rows)
                    {
                        for (int i = 0; i < row.Cells.Count; i++)
                        {
                            string text = Server.HtmlDecode(row.Cells[i].Text.Trim());
                            if (text == "&nbsp;" || string.IsNullOrWhiteSpace(text)) text = "-";

                            PdfPCell cell = new PdfPCell(new Phrase(text, FontFactory.GetFont("Arial", 9)));
                            cell.HorizontalAlignment = Element.ALIGN_CENTER;
                            pdfTable.AddCell(cell);
                        }

                        decimal amount = 0, discount = 0, finalAmount = 0;
                        decimal.TryParse(row.Cells[9].Text.Replace("LKR", "").Trim(), out amount);
                        decimal.TryParse(row.Cells[10].Text.Replace("LKR", "").Trim(), out discount);
                        decimal.TryParse(row.Cells[11].Text.Replace("LKR", "").Trim(), out finalAmount);

                        totalAmount += amount;
                        totalDiscount += discount;
                        totalFinalAmount += finalAmount;
                    }

                    for (int i = 0; i < GridView1.HeaderRow.Cells.Count; i++)
                    {
                        PdfPCell totalCell;
                        if (i == 8)
                        {
                            totalCell = new PdfPCell(new Phrase("TOTAL", FontFactory.GetFont("Arial", 10, Font.BOLD)));
                        }
                        else if (i == 9)
                        {
                            totalCell = new PdfPCell(new Phrase("LKR " + totalAmount.ToString("N2"), FontFactory.GetFont("Arial", 10, Font.BOLD)));
                        }
                        else if (i == 10)
                        {
                            totalCell = new PdfPCell(new Phrase("LKR " + totalDiscount.ToString("N2"), FontFactory.GetFont("Arial", 10, Font.BOLD)));
                        }
                        else if (i == 11)
                        {
                            totalCell = new PdfPCell(new Phrase("LKR " + totalFinalAmount.ToString("N2"), FontFactory.GetFont("Arial", 10, Font.BOLD)));
                        }
                        else
                        {
                            totalCell = new PdfPCell(new Phrase(""));
                        }

                        totalCell.BackgroundColor = new BaseColor(220, 220, 220);
                        totalCell.HorizontalAlignment = Element.ALIGN_CENTER;
                        pdfTable.AddCell(totalCell);
                    }

                    document.Add(pdfTable);
                    document.Close();

                    byte[] bytes = ms.ToArray();

                    // ✅ Clear before setting headers and write once
                    Response.Clear();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "attachment; filename=SaleSummaryReport.pdf");
                    Response.OutputStream.Write(bytes, 0, bytes.Length);
                    Response.Flush();
                    Response.End(); // Or: HttpContext.Current.ApplicationInstance.CompleteRequest();
                }
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error: " + ex.Message + "');</script>");
            }
        }

    }
}

