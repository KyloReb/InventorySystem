using System;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Imaging;
using OfficeOpenXml;
using System.Linq;
using System.ComponentModel;
using System.Reflection;

namespace InventorySystem.Services
{
    public class PrintExportService : IDisposable
    {
        private DataGridView dataGridView;
        private LoggingService loggingService;
        private PrintDocument printDocument;
        private PrintPreviewDialog printPreviewDialog;
        private PageSetupDialog pageSetupDialog;
        private PrintDialog printDialog;
        private PdfFormatterService pdfFormatterService;

        // Print settings
        private System.Drawing.Font printFont;
        private int currentPage = 0;
        private int rowsPerPage = 0;
        private DataTable printData;
        private string title = "Inventory Report";

        // Static constructor for EPPlus license initialization
        static PrintExportService()
        {
            InitializeEPPlusLicense();
        }

        private static void InitializeEPPlusLicense()
        {
            try
            {
                // Get EPPlus assembly version
                var epplusAssembly = typeof(ExcelPackage).Assembly;
                var version = epplusAssembly.GetName().Version;
                System.Diagnostics.Debug.WriteLine($"EPPlus version detected: {version}");

                // Check if EPPlus 8+ (uses License property) or older (uses LicenseContext)
                var licenseProperty = typeof(ExcelPackage).GetProperty("License", BindingFlags.Public | BindingFlags.Static);
                var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext", BindingFlags.Public | BindingFlags.Static);

                if (licenseProperty != null)
                {
                    // EPPlus 8+ uses License property
                    System.Diagnostics.Debug.WriteLine("EPPlus 8+ detected - using License property");

                    // Use reflection to set: ExcelPackage.License = LicenseType.NonCommercial;
                    var licenseTypeEnum = epplusAssembly.GetType("OfficeOpenXml.LicenseType");
                    if (licenseTypeEnum != null)
                    {
                        var nonCommercialValue = Enum.Parse(licenseTypeEnum, "NonCommercial");
                        licenseProperty.SetValue(null, nonCommercialValue);
                        System.Diagnostics.Debug.WriteLine("EPPlus License set to NonCommercial successfully");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("ERROR: Could not find LicenseType enum");
                    }
                }
                else if (licenseContextProperty != null)
                {
                    // EPPlus 5-7 uses LicenseContext property
                    System.Diagnostics.Debug.WriteLine("EPPlus 5-7 detected - using LicenseContext property");
                    ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                    System.Diagnostics.Debug.WriteLine("EPPlus LicenseContext set to NonCommercial successfully");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("WARNING: Could not find License or LicenseContext property - may be EPPlus 4 or incompatible version");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"EPPlus license initialization failed: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        public PrintExportService(DataGridView dataGridView, LoggingService loggingService)
        {
            this.dataGridView = dataGridView ?? throw new ArgumentNullException(nameof(dataGridView));
            this.loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));

            try
            {
                loggingService.LogMessage("SYSTEM", "PrintExportService: Initializing...");

                // Log EPPlus version info
                var epplusAssembly = typeof(ExcelPackage).Assembly;
                var version = epplusAssembly.GetName().Version;
                loggingService.LogMessage("INFO", $"EPPlus Version: {version}");
                loggingService.LogMessage("INFO", $"EPPlus Assembly Location: {epplusAssembly.Location}");

                InitializePrinting();
                InitializePdfFormatter();
                loggingService.LogMessage("SYSTEM", "PrintExportService: Initialized successfully");
            }
            catch (Exception ex)
            {
                loggingService.LogMessage("ERROR", $"PrintExportService initialization failed: {ex.Message}");
                loggingService.LogMessage("ERROR", $"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void InitializePrinting()
        {
            printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;
            printDocument.BeginPrint += PrintDocument_BeginPrint;
            printDocument.EndPrint += PrintDocument_EndPrint;

            printPreviewDialog = new PrintPreviewDialog
            {
                Document = printDocument,
                WindowState = FormWindowState.Maximized
            };

            pageSetupDialog = new PageSetupDialog { Document = printDocument };
            printDialog = new PrintDialog { Document = printDocument };
            printFont = new System.Drawing.Font("Arial", 10);
        }

        private void InitializePdfFormatter()
        {
            pdfFormatterService = new PdfFormatterService(dataGridView, loggingService, title);
        }

        public void ShowPrintPreview()
        {
            try
            {
                loggingService.LogMessage("INFO", "ShowPrintPreview: Starting...");
                if (!ValidateDataSource()) return;

                printData = ((DataTable)dataGridView.DataSource).Copy();
                currentPage = 0;
                loggingService.LogMessage("INFO", $"ShowPrintPreview: Showing preview with {printData.Rows.Count} rows");
                printPreviewDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                HandleError("Print preview error", ex);
            }
        }

        public void ShowPageSetup()
        {
            try
            {
                loggingService.LogMessage("INFO", "ShowPageSetup: Opening page setup dialog");
                pageSetupDialog.ShowDialog();
            }
            catch (Exception ex)
            {
                HandleError("Page setup error", ex);
            }
        }

        public void ShowPrintDialog()
        {
            try
            {
                loggingService.LogMessage("INFO", "ShowPrintDialog: Starting...");
                if (!ValidateDataSource()) return;

                printData = ((DataTable)dataGridView.DataSource).Copy();
                currentPage = 0;

                if (printDialog.ShowDialog() == DialogResult.OK)
                {
                    loggingService.LogMessage("INFO", "ShowPrintDialog: User confirmed, starting print");
                    printDocument.Print();
                }
                else
                {
                    loggingService.LogMessage("INFO", "ShowPrintDialog: User cancelled");
                }
            }
            catch (Exception ex)
            {
                HandleError("Print error", ex);
            }
        }

        private bool ValidateDataSource()
        {
            loggingService.LogMessage("INFO", "ValidateDataSource: Checking data source");

            if (dataGridView.DataSource == null)
            {
                loggingService.LogMessage("WARNING", "ValidateDataSource: DataSource is null");
                MessageBox.Show("No data to print.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (!(dataGridView.DataSource is DataTable))
            {
                loggingService.LogMessage("ERROR", $"ValidateDataSource: DataSource type is {dataGridView.DataSource.GetType().Name}, expected DataTable");
                MessageBox.Show("Data source must be a DataTable.", "Print", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            loggingService.LogMessage("INFO", "ValidateDataSource: Validation passed");
            return true;
        }

        // ... (All the printing-related methods remain the same - PrintDocument_BeginPrint, PrintDocument_EndPrint, PrintDocument_PrintPage, etc.)

        private void PrintDocument_BeginPrint(object sender, PrintEventArgs e)
        {
            currentPage = 0;
            loggingService.LogMessage("INFO", "PrintDocument_BeginPrint: Print job started");
        }

        private void PrintDocument_EndPrint(object sender, PrintEventArgs e)
        {
            loggingService.LogMessage("INFO", $"PrintDocument_EndPrint: Print job completed. Pages printed: {currentPage}");
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            try
            {
                loggingService.LogMessage("INFO", $"PrintDocument_PrintPage: Printing page {currentPage + 1}");

                Graphics g = e.Graphics;
                float leftMargin = e.MarginBounds.Left;
                float topMargin = e.MarginBounds.Top;
                float yPos = topMargin;
                float lineHeight = printFont.GetHeight(g);

                // Calculate rows per page on first page
                if (currentPage == 0)
                {
                    CalculateRowsPerPage(e, lineHeight);
                    loggingService.LogMessage("INFO", $"PrintDocument_PrintPage: Rows per page calculated: {rowsPerPage}");
                }

                // Print title
                using (System.Drawing.Font titleFont = new System.Drawing.Font(printFont.FontFamily, 14, FontStyle.Bold))
                {
                    yPos = PrintTitle(g, titleFont, e.MarginBounds.Width, leftMargin, yPos, lineHeight);
                }

                // Print date
                yPos = PrintDate(g, leftMargin, yPos, lineHeight);

                // Print column headers
                yPos = PrintColumnHeaders(g, leftMargin, yPos, lineHeight, e.MarginBounds.Width);

                // Print data rows
                int startRow = currentPage * rowsPerPage;
                int endRow = Math.Min(startRow + rowsPerPage, printData.Rows.Count);

                loggingService.LogMessage("INFO", $"PrintDocument_PrintPage: Printing rows {startRow} to {endRow - 1} of {printData.Rows.Count}");

                for (int i = startRow; i < endRow; i++)
                {
                    yPos = PrintDataRow(g, printData.Rows[i], leftMargin, yPos, lineHeight, e.MarginBounds.Width);

                    if (yPos > e.MarginBounds.Bottom - lineHeight)
                    {
                        endRow = i + 1;
                        loggingService.LogMessage("INFO", $"PrintDocument_PrintPage: Page full at row {i}");
                        break;
                    }
                }

                // Check if more pages
                currentPage++;
                e.HasMorePages = endRow < printData.Rows.Count;

                if (e.HasMorePages)
                {
                    loggingService.LogMessage("INFO", $"PrintDocument_PrintPage: More pages remaining ({printData.Rows.Count - endRow} rows left)");
                }

                PrintPageNumber(g, e.MarginBounds.Width, e.MarginBounds.Height, leftMargin, topMargin);
            }
            catch (Exception ex)
            {
                loggingService.LogMessage("ERROR", $"PrintDocument_PrintPage: Error on page {currentPage + 1}: {ex.Message}");
                loggingService.LogMessage("ERROR", $"Stack trace: {ex.StackTrace}");
                MessageBox.Show($"Print error: {ex.Message}", "Print Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.HasMorePages = false;
            }
        }

        private void CalculateRowsPerPage(PrintPageEventArgs e, float lineHeight)
        {
            float headerHeight = lineHeight * 5;
            float availableHeight = e.MarginBounds.Height - headerHeight - lineHeight;
            rowsPerPage = Math.Max(1, (int)(availableHeight / lineHeight));
        }

        private float PrintTitle(Graphics g, System.Drawing.Font titleFont, float width, float leftMargin, float yPos, float lineHeight)
        {
            string printTitle = $"{title} - {DateTime.Now:yyyy-MM-dd}";
            SizeF titleSize = g.MeasureString(printTitle, titleFont);
            float xPos = leftMargin + (width - titleSize.Width) / 2;
            g.DrawString(printTitle, titleFont, Brushes.Black, xPos, yPos);
            return yPos + titleSize.Height + lineHeight;
        }

        private float PrintDate(Graphics g, float leftMargin, float yPos, float lineHeight)
        {
            string dateString = $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}";
            g.DrawString(dateString, printFont, Brushes.Black, leftMargin, yPos);
            return yPos + lineHeight * 1.5f;
        }

        private float PrintColumnHeaders(Graphics g, float leftMargin, float yPos, float lineHeight, float width)
        {
            float xPos = leftMargin;
            var visibleColumns = dataGridView.Columns.Cast<DataGridViewColumn>().Where(col => col.Visible).ToList();
            float columnWidth = width / visibleColumns.Count;

            foreach (var column in visibleColumns)
            {
                RectangleF rect = new RectangleF(xPos, yPos, columnWidth, lineHeight * 1.5f);
                g.DrawString(column.HeaderText, printFont, Brushes.Black, rect);
                g.DrawRectangle(Pens.Black, rect.X, rect.Y, rect.Width, rect.Height);
                xPos += columnWidth;
            }

            return yPos + lineHeight * 1.5f;
        }

        private float PrintDataRow(Graphics g, DataRow row, float leftMargin, float yPos, float lineHeight, float width)
        {
            float xPos = leftMargin;
            var visibleColumns = dataGridView.Columns.Cast<DataGridViewColumn>().Where(col => col.Visible).ToList();
            float columnWidth = width / visibleColumns.Count;

            foreach (var column in visibleColumns)
            {
                string cellValue = row[column.Index]?.ToString() ?? "";
                RectangleF rect = new RectangleF(xPos, yPos, columnWidth, lineHeight);
                g.DrawString(cellValue, printFont, Brushes.Black, rect);
                g.DrawRectangle(Pens.Gray, rect.X, rect.Y, rect.Width, rect.Height);
                xPos += columnWidth;
            }

            return yPos + lineHeight;
        }

        private void PrintPageNumber(Graphics g, float width, float height, float leftMargin, float topMargin)
        {
            string pageInfo = $"Page {currentPage + 1}";
            SizeF pageSize = g.MeasureString(pageInfo, printFont);
            float xPos = leftMargin + width - pageSize.Width;
            float yPos = topMargin + height - pageSize.Height;
            g.DrawString(pageInfo, printFont, Brushes.Black, xPos, yPos);
        }

        // Export functionality
        public void ExportToImage(ImageFormat format, string filePath)
        {
            try
            {
                loggingService.LogMessage("INFO", $"ExportToImage: Starting export to {filePath}");
                loggingService.LogMessage("INFO", $"ExportToImage: Format={format.ToString()}, GridSize={dataGridView.Width}x{dataGridView.Height}");

                using (Bitmap bitmap = new Bitmap(dataGridView.Width, dataGridView.Height))
                {
                    dataGridView.DrawToBitmap(bitmap, new System.Drawing.Rectangle(0, 0, dataGridView.Width, dataGridView.Height));
                    bitmap.Save(filePath, format);
                }

                loggingService.LogMessage("INFO", "ExportToImage: Export completed successfully");
                ShowExportSuccess(filePath);
            }
            catch (Exception ex)
            {
                HandleError("Image export error", ex);
            }
        }

        public void ExportToCsv(string filePath)
        {
            try
            {
                loggingService.LogMessage("INFO", $"ExportToCsv: Starting export to {filePath}");

                int visibleColumnCount = dataGridView.Columns.Cast<DataGridViewColumn>().Count(col => col.Visible);
                int rowCount = dataGridView.Rows.Cast<DataGridViewRow>().Count(row => !row.IsNewRow);

                loggingService.LogMessage("INFO", $"ExportToCsv: Columns={visibleColumnCount}, Rows={rowCount}");

                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    // Write headers
                    var headers = dataGridView.Columns.Cast<DataGridViewColumn>()
                                        .Where(col => col.Visible)
                                        .Select(col => EscapeCsv(col.HeaderText));
                    writer.WriteLine(string.Join(",", headers));
                    loggingService.LogMessage("INFO", "ExportToCsv: Headers written");

                    // Write data
                    int exportedRows = 0;
                    foreach (DataGridViewRow row in dataGridView.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            var cells = row.Cells.Cast<DataGridViewCell>()
                                          .Where(cell => cell.OwningColumn.Visible)
                                          .Select(cell => EscapeCsv(cell.Value?.ToString() ?? ""));
                            writer.WriteLine(string.Join(",", cells));
                            exportedRows++;
                        }
                    }
                    loggingService.LogMessage("INFO", $"ExportToCsv: {exportedRows} data rows written");
                }

                loggingService.LogMessage("INFO", "ExportToCsv: Export completed successfully");
                ShowExportSuccess(filePath);
            }
            catch (Exception ex)
            {
                HandleError("CSV export error", ex);
            }
        }

        public void ExportToExcel(string filePath)
        {
            try
            {
                loggingService.LogMessage("INFO", $"ExportToExcel: Starting export to {filePath}");

                // Log EPPlus version and license status
                var epplusAssembly = typeof(ExcelPackage).Assembly;
                var version = epplusAssembly.GetName().Version;
                loggingService.LogMessage("INFO", $"ExportToExcel: EPPlus Version = {version}");

                // Check which license property exists
                var licenseProperty = typeof(ExcelPackage).GetProperty("License", BindingFlags.Public | BindingFlags.Static);
                var licenseContextProperty = typeof(ExcelPackage).GetProperty("LicenseContext", BindingFlags.Public | BindingFlags.Static);

                if (licenseProperty != null)
                {
                    var currentLicense = licenseProperty.GetValue(null);
                    loggingService.LogMessage("INFO", $"ExportToExcel: EPPlus License (v8+) = {currentLicense}");
                }
                else if (licenseContextProperty != null)
                {
                    var currentContext = licenseContextProperty.GetValue(null);
                    loggingService.LogMessage("INFO", $"ExportToExcel: EPPlus LicenseContext (v5-7) = {currentContext}");
                }
                else
                {
                    loggingService.LogMessage("WARNING", "ExportToExcel: No license property found (EPPlus v4?)");
                }

                loggingService.LogMessage("INFO", "ExportToExcel: Creating ExcelPackage...");
                using (var package = new ExcelPackage())
                {
                    loggingService.LogMessage("INFO", "ExportToExcel: ExcelPackage created successfully");

                    loggingService.LogMessage("INFO", "ExportToExcel: Adding worksheet...");
                    var worksheet = package.Workbook.Worksheets.Add("Inventory Data");
                    loggingService.LogMessage("INFO", "ExportToExcel: Worksheet added successfully");

                    // Write headers
                    int colIndex = 1;
                    int headerCount = 0;
                    foreach (DataGridViewColumn column in dataGridView.Columns)
                    {
                        if (column.Visible)
                        {
                            worksheet.Cells[1, colIndex].Value = column.HeaderText;
                            worksheet.Cells[1, colIndex].Style.Font.Bold = true;
                            colIndex++;
                            headerCount++;
                        }
                    }
                    loggingService.LogMessage("INFO", $"ExportToExcel: {headerCount} column headers written");

                    // Write data
                    int rowIndex = 2;
                    int dataRowCount = 0;
                    foreach (DataGridViewRow dataRow in dataGridView.Rows)
                    {
                        if (!dataRow.IsNewRow)
                        {
                            colIndex = 1;
                            foreach (DataGridViewColumn column in dataGridView.Columns)
                            {
                                if (column.Visible)
                                {
                                    var cellValue = dataRow.Cells[column.Index].Value;
                                    worksheet.Cells[rowIndex, colIndex].Value = cellValue?.ToString();
                                    colIndex++;
                                }
                            }
                            rowIndex++;
                            dataRowCount++;

                            if (dataRowCount % 100 == 0)
                            {
                                loggingService.LogMessage("INFO", $"ExportToExcel: {dataRowCount} rows processed...");
                            }
                        }
                    }
                    loggingService.LogMessage("INFO", $"ExportToExcel: Total {dataRowCount} data rows written");

                    // Auto-fit columns
                    if (worksheet.Dimension != null)
                    {
                        loggingService.LogMessage("INFO", "ExportToExcel: Auto-fitting columns...");
                        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                        loggingService.LogMessage("INFO", "ExportToExcel: Columns auto-fitted successfully");
                    }

                    loggingService.LogMessage("INFO", $"ExportToExcel: Saving file to {filePath}...");
                    package.SaveAs(new FileInfo(filePath));
                    loggingService.LogMessage("INFO", "ExportToExcel: File saved successfully");
                }

                ShowExportSuccess(filePath);
            }
            catch (Exception ex)
            {
                loggingService.LogMessage("ERROR", $"ExportToExcel: Exception Type = {ex.GetType().FullName}");
                loggingService.LogMessage("ERROR", $"ExportToExcel: Message = {ex.Message}");
                loggingService.LogMessage("ERROR", $"ExportToExcel: Stack Trace = {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    loggingService.LogMessage("ERROR", $"ExportToExcel: Inner Exception Type = {ex.InnerException.GetType().FullName}");
                    loggingService.LogMessage("ERROR", $"ExportToExcel: Inner Message = {ex.InnerException.Message}");
                    loggingService.LogMessage("ERROR", $"ExportToExcel: Inner Stack Trace = {ex.InnerException.StackTrace}");
                }

                HandleError("Excel export error", ex, "Please ensure EPPlus is properly installed (recommended version 7.5.0).");
            }
        }

        public void ExportToPdf(string filePath)
        {
            try
            {
                pdfFormatterService.ExportToPdf(filePath);
                ShowExportSuccess(filePath);
            }
            catch (Exception ex)
            {
                HandleError("PDF export error", ex, "Please ensure iText 7+ (namespace 'iText') is properly installed via NuGet.");
            }
        }

        public void ExportToPdf(string filePath, DataTable dataTable)
        {
            try
            {
                pdfFormatterService.ExportToPdf(filePath, dataTable);
                ShowExportSuccess(filePath);
            }
            catch (Exception ex)
            {
                HandleError("PDF export error", ex, "Please ensure iText 7+ (namespace 'iText') is properly installed via NuGet.");
            }
        }

        private string EscapeCsv(string value)
        {
            if (string.IsNullOrEmpty(value)) return "\"\"";

            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n") || value.Contains("\r"))
            {
                value = value.Replace("\"", "\"\"");
                return $"\"{value}\"";
            }

            return value;
        }

        private void HandleError(string operation, Exception ex, string additionalInfo = null)
        {
            string errorMessage = $"{operation}: {ex.Message}";
            if (!string.IsNullOrEmpty(additionalInfo))
            {
                errorMessage += $"\n\n{additionalInfo}";
            }

            loggingService.LogMessage("ERROR", $"{operation}: {ex.GetType().Name}");
            loggingService.LogMessage("ERROR", $"Message: {ex.Message}");
            loggingService.LogMessage("ERROR", $"Stack trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                loggingService.LogMessage("ERROR", $"Inner exception: {ex.InnerException.GetType().Name}");
                loggingService.LogMessage("ERROR", $"Inner message: {ex.InnerException.Message}");
            }

            MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void ShowExportSuccess(string filePath)
        {
            MessageBox.Show($"Data exported successfully to:\n{filePath}", "Export Successful",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetTitle(string newTitle)
        {
            title = newTitle ?? "Inventory Report";
            pdfFormatterService?.SetTitle(title);
            loggingService.LogMessage("INFO", $"SetTitle: Title changed to '{title}'");
        }

        public void Dispose()
        {
            loggingService.LogMessage("INFO", "Dispose: Cleaning up PrintExportService");
            printDocument?.Dispose();
            printPreviewDialog?.Dispose();
            pageSetupDialog?.Dispose();
            printDialog?.Dispose();
            printFont?.Dispose();
            pdfFormatterService?.Dispose();
        }
    }
}