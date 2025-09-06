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
    public partial class VehicleCount : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["ConString"].ConnectionString;
        private decimal totalAmount = 0;
        private int vehicleCount = 0;
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PopulateVehicleTypeDropdown();
                BindGridView(); // Display all records initially
            }

            // Check access using username from session
            if (!HasAccess("VehicleCount.aspx"))
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
            } }
        // Method to populate the vehicle type dropdown with data
        private void PopulateVehicleTypeDropdown()
        {
            string query = "SELECT DISTINCT VehicleType FROM [CPMS].[Vehicles]";
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                ddlvehicleType.DataSource = reader;
                ddlvehicleType.DataTextField = "VehicleType";
                ddlvehicleType.DataValueField = "VehicleType";
                ddlvehicleType.DataBind();
                ddlvehicleType.Items.Insert(0, new System.Web.UI.WebControls.ListItem("All Vehicle Types", "")); // Add a default "Select" option
                reader.Close();
            } }
        protected void btnView_Click(object sender, EventArgs e)
        {
            BindGridView(); // Refresh the grid view based on selected filters
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
                        t.BillAmount AS Amount
                    FROM [SCPMS].[CPMS].[Tickets] t
                    JOIN [SCPMS].[CPMS].[Vehicles] v ON t.VehicleId = v.VehicleId
                    INNER JOIN [SCPMS].[CPMS].[ParkingSlots] ps ON t.ParkingSlotPSid = ps.PSid
                    LEFT JOIN [SCPMS].[dbo].[SystemUser] u1 ON t.EnteredBy = u1.SUId
                    LEFT JOIN [SCPMS].[dbo].[SystemUser] u2 ON t.ExitBy = u2.SUId
                    WHERE t.EnteredDateTime BETWEEN @StartDate AND @EndDate
                    AND (@VehicleType = '' OR v.VehicleType = @VehicleType)
                    ORDER BY t.InvoiceNo DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@StartDate", txtStartDate.Text);
                    cmd.Parameters.AddWithValue("@EndDate", txtEndDate.Text);
                    cmd.Parameters.AddWithValue("@VehicleType", ddlvehicleType.SelectedValue);
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    GridView1.DataSource = dt;
                    GridView1.DataBind();
                }}}
        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                string parkingDetail = DataBinder.Eval(e.Row.DataItem, "ParkingDetail")?.ToString();
                string areaCode = parkingDetail?.Split('-')[0]?.Trim().ToUpper();

                if (areaCode == "F")
                {
                    e.Row.Cells[9].Text = "Free of Charge";
                }
                else
                {
                    // Parse amount only for non-FOC
                    if (decimal.TryParse(DataBinder.Eval(e.Row.DataItem, "Amount")?.ToString(), out decimal amount))
                    {
                        totalAmount += amount;
                    }
                }

                vehicleCount++; // Count all vehicles regardless of area
            }
            else if (e.Row.RowType == DataControlRowType.Footer)
            {
                e.Row.Cells[1].Text = "Vehicle Count:";
                e.Row.Cells[1].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[2].Text = vehicleCount.ToString("N0");
                e.Row.Cells[8].Text = "Total:";
                e.Row.Cells[8].HorizontalAlign = HorizontalAlign.Right;
                e.Row.Cells[9].Text = $"LKR {totalAmount:N2}";
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
                // Prepare the response to download the file
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("Content-Disposition", "attachment;filename=VehicleCountReport.xlsx");
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    using (ClosedXML.Excel.XLWorkbook wb = new ClosedXML.Excel.XLWorkbook())
                    {
                        // Create the worksheet
                        var worksheet = wb.Worksheets.Add("VehicleCountReport");
                        // Header part
                        worksheet.Cell(1, 1).Value = "Vehicle Count Report";
                        worksheet.Range("A1:N1").Merge().Style
                        .Font.SetBold()
                        .Font.SetFontSize(24)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Row(1).Height = 25;

                        worksheet.Cell(2, 1).Value = "Colombo Lotus Tower";
                        worksheet.Range("A2:N2").Merge().Style
                         .Font.SetBold()
                        .Font.SetFontSize(18)
                        .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
                        worksheet.Row(2).Height = 18;

                        string exportedBy = Session["LoggedInUserName"] != null ? Session["LoggedInUserName"].ToString() : "Unknown User";
                        worksheet.Cell(3, 1).Value = $"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt}  |  Exported by: {exportedBy}";
                        worksheet.Range("A3:N3").Merge().Style.Font.Bold = true;
                        worksheet.Cell(3, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                        worksheet.Row(3).Height = 20;
                        worksheet.Row(4).Height = 10;
                        // Add the column headers dynamically from GridView1
                        for (int i = 0; i < GridView1.Columns.Count; i++)
                        {
                            worksheet.Cell(5, i + 1).Value = GridView1.Columns[i].HeaderText;
                            worksheet.Cell(5, i + 1).Style.Font.Bold = true;
                            worksheet.Cell(5, i + 1).Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.CornflowerBlue;
                            worksheet.Cell(5, i + 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                            worksheet.Cell(5, i + 1).Style.Alignment.Vertical = ClosedXML.Excel.XLAlignmentVerticalValues.Center;
                            worksheet.Cell(5, i + 1).Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                            worksheet.Cell(5, i + 1).Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                        }
                        // Fill the data from GridView1 starting from row 6
                        for (int rowIndex = 0; rowIndex < GridView1.Rows.Count; rowIndex++)
                        {
                            for (int colIndex = 0; colIndex < GridView1.Columns.Count; colIndex++)
                            {
                                // Ensure missing data is handled correctly
                                string cellValue = GridView1.Rows[rowIndex].Cells[colIndex].Text.Trim();
                                var cell = worksheet.Cell(rowIndex + 6, colIndex + 1);
                                cell.Value = cellValue == "&nbsp;" ? "" : cellValue;
                                cell.Style.Border.OutsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                                cell.Style.Border.InsideBorder = ClosedXML.Excel.XLBorderStyleValues.Thin;
                            }
                        }
                        // Remove the total sales row
                        // Optionally, add footer with vehicle count
                        int rowCount = GridView1.Rows.Count + 6;
                        worksheet.Cell(rowCount + 1, 1).Value = "Vehicle Count:";
                        worksheet.Cell(rowCount + 1, 1).Style.Font.Bold = true;
                        worksheet.Cell(rowCount + 1, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Right;
                        // Calculate Vehicle Count (the number of rows)
                        int vehicleCount = GridView1.Rows.Count;
                        worksheet.Cell(rowCount + 1, 2).Value = vehicleCount;
                        worksheet.Cell(rowCount + 1, 2).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
                        // Adjust column widths to fit data dynamically
                        worksheet.Columns().AdjustToContents();
                        // Save the workbook to memory stream
                        wb.SaveAs(ms);
                        // Send the Excel file to the client
                        Response.BinaryWrite(ms.ToArray());
                        Response.End();
                    } } }
            catch (Exception ex)
            {
                // Improved error handling
                Response.Write("<script>alert('Error generating Excel: " + ex.Message + "');</script>");
            } }
        protected void btnExportPDF_Click(object sender, EventArgs e)
        {
            if (GridView1 == null || GridView1.Rows.Count == 0)
            {
                Response.Write("<script>alert('No data available to export!');</script>");
                return;}
            try
            {
                // Create a PDF document with landscape orientation
                Document document = new Document(PageSize.A4.Rotate(), 10f, 10f, 50f, 30f);
                MemoryStream ms = new MemoryStream();
                PdfWriter.GetInstance(document, ms);
                document.Open();
                // Fonts
                Font headerFont = FontFactory.GetFont("Arial", 18, Font.BOLD, BaseColor.BLUE);
                Font titleFont = FontFactory.GetFont("Arial", 14, Font.BOLD);
                Font infoFont = FontFactory.GetFont("Arial", 12, Font.ITALIC);
                Font tableHeaderFont = FontFactory.GetFont("Arial", 12, Font.BOLD, BaseColor.WHITE);
                Font tableCellFont = FontFactory.GetFont("Arial", 11, Font.NORMAL, BaseColor.BLACK);
                // Company Header
                Paragraph companyName = new Paragraph("Colombo Lotus Tower", headerFont) { Alignment = Element.ALIGN_CENTER };
                document.Add(companyName);
                // Report Title
                Paragraph title = new Paragraph("Vehicle Count Report", titleFont) { Alignment = Element.ALIGN_CENTER, SpacingAfter = 10 };
                document.Add(title);
                // Report Info (Generated date and user)
                string exportedBy = Session["LoggedInUserName"] != null ? Session["LoggedInUserName"].ToString() : "Unknown User";
                Paragraph reportInfo = new Paragraph($"Generated on: {DateTime.Now:dd/MM/yyyy hh:mm tt}  |  Exported by: {exportedBy}", infoFont)
                { Alignment = Element.ALIGN_RIGHT, SpacingAfter = 20 };
                document.Add(reportInfo);
                // Create a Table
                PdfPTable table = new PdfPTable(GridView1.Columns.Count);
                table.WidthPercentage = 100;
                // Set column widths (adjust as needed)
                float[] columnWidths = { 10f, 15f, 12f, 20f, 15f, 15f, 15f, 15f, 10f, 12f };
                table.SetWidths(columnWidths);
                // Header Row (Set Background Color)
                foreach (DataControlField col in GridView1.Columns)
                {
                    PdfPCell headerCell = new PdfPCell(new Phrase(col.HeaderText, tableHeaderFont))
                    {
                        BackgroundColor = new BaseColor(0, 51, 102), // Dark Blue
                        HorizontalAlignment = Element.ALIGN_CENTER,
                        Padding = 5
                    };
                    table.AddCell(headerCell);}
                // Data Rows
                decimal totalAmount = 0;
                int vehicleCount = 0;
                foreach (GridViewRow row in GridView1.Rows)
                {
                    for (int i = 0; i < row.Cells.Count; i++)
                    {
                        string cellText = row.Cells[i].Text.Trim() == "&nbsp;" ? "" : row.Cells[i].Text.Trim();
                        PdfPCell dataCell = new PdfPCell(new Phrase(cellText, tableCellFont))
                        {
                            HorizontalAlignment = Element.ALIGN_CENTER,
                            Padding = 4
                        };
                        table.AddCell(dataCell);}
                    // Calculate Total Amount & Vehicle Count
                    decimal amount;
                    if (decimal.TryParse(row.Cells[9].Text, out amount))
                    {
                        totalAmount += amount;
                    }
                    vehicleCount++;}
                // Footer Row (Total Count & Amount)
                PdfPCell footerLabelCell = new PdfPCell(new Phrase("Total Vehicles:", tableHeaderFont))
                {
                    Colspan = 8,
                    HorizontalAlignment = Element.ALIGN_RIGHT,
                    BackgroundColor = new BaseColor(0, 51, 102),
                    Padding = 5
                };
                table.AddCell(footerLabelCell);

                PdfPCell vehicleCountCell = new PdfPCell(new Phrase(vehicleCount.ToString("N0"), tableCellFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };
                table.AddCell(vehicleCountCell);

                PdfPCell totalAmountCell = new PdfPCell(new Phrase($"LKR {totalAmount:N2}", tableCellFont))
                {
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 4
                };
                table.AddCell(totalAmountCell);
                // Add Table to Document
                document.Add(table);
                // Close PDF
                document.Close();
                // Return PDF as Download
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=VehicleReport.pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.BinaryWrite(ms.ToArray());
                Response.End();
            }
            catch (Exception ex)
            {
                Response.Write("<script>alert('Error generating PDF: " + ex.Message + "');</script>");
            }}}}