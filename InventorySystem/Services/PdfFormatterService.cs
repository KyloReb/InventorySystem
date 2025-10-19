using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.IO.Font.Constants;

namespace InventorySystem.Services
{
    public class PdfFormatterService : IDisposable
    {
        private LoggingService _loggingService;
        private string _title;
        private DataGridView _dataGridView;

        public PdfFormatterService(DataGridView dataGridView, LoggingService loggingService, string title = "Inventory Report")
        {
            _dataGridView = dataGridView ?? throw new ArgumentNullException(nameof(dataGridView));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _title = title;
        }

        public void SetTitle(string title)
        {
            _title = title ?? "Inventory Report";
            _loggingService.LogMessage("INFO", $"PdfFormatterService: Title changed to '{_title}'");
        }

        public void ExportToPdf(string filePath, DataTable dataTable = null)
        {
            try
            {
                _loggingService.LogMessage("INFO", $"ExportToPdf: Starting export to {filePath}");

                var directory = System.IO.Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !System.IO.Directory.Exists(directory))
                {
                    _loggingService.LogMessage("INFO", $"ExportToPdf: Creating directory {directory}");
                    System.IO.Directory.CreateDirectory(directory);
                }

                // Get data source
                var dataSource = dataTable ?? GetDataFromGridView();
                var visibleColumns = _dataGridView.Columns.Cast<DataGridViewColumn>()
                    .Where(col => col.Visible)
                    .ToList();

                int dataRowCount = dataSource.Rows.Count;
                _loggingService.LogMessage("INFO", $"ExportToPdf: Columns={visibleColumns.Count}, Rows={dataRowCount}");

                _loggingService.LogMessage("INFO", "ExportToPdf: Creating PdfWriter...");
                using (var writer = new PdfWriter(filePath))
                {
                    _loggingService.LogMessage("INFO", "ExportToPdf: Creating PdfDocument...");
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = CreateDocument(pdf);
                        AddHeader(document);
                        AddDataTable(document, dataSource, visibleColumns);
                        _loggingService.LogMessage("INFO", "ExportToPdf: Document completed, closing...");
                    }
                }

                _loggingService.LogMessage("INFO", "ExportToPdf: PDF created successfully");
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"ExportToPdf: Failed - {ex.GetType().Name}: {ex.Message}");
                _loggingService.LogMessage("ERROR", $"ExportToPdf: Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    _loggingService.LogMessage("ERROR", $"ExportToPdf: Inner exception: {ex.InnerException.Message}");
                }
                throw;
            }
        }

        private Document CreateDocument(PdfDocument pdf)
        {
            _loggingService.LogMessage("INFO", "ExportToPdf: Creating Document with landscape A4...");
            var document = new Document(pdf, iText.Kernel.Geom.PageSize.A4.Rotate());
            document.SetMargins(20, 20, 20, 20);
            return document;
        }

        private void AddHeader(Document document)
        {
            // Fonts
            _loggingService.LogMessage("INFO", "ExportToPdf: Loading fonts...");
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            _loggingService.LogMessage("INFO", "ExportToPdf: Fonts loaded successfully");

            // Title
            _loggingService.LogMessage("INFO", "ExportToPdf: Adding title...");
            Paragraph titleParagraph = new Paragraph($"{_title} - {DateTime.Now:yyyy-MM-dd}")
                .SetFont(boldFont)
                .SetFontSize(16)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(10);
            document.Add(titleParagraph);

            // Date
            _loggingService.LogMessage("INFO", "ExportToPdf: Adding date...");
            Paragraph dateParagraph = new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}")
                .SetFont(normalFont)
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(15);
            document.Add(dateParagraph);
        }

        private void AddDataTable(Document document, DataTable dataSource, System.Collections.Generic.List<DataGridViewColumn> visibleColumns)
        {
            // Create table with proportional widths
            _loggingService.LogMessage("INFO", "ExportToPdf: Creating table structure...");
            float[] columnWidths = new float[visibleColumns.Count];
            for (int i = 0; i < visibleColumns.Count; i++)
            {
                columnWidths[i] = 1f;
            }

            Table table = new Table(UnitValue.CreatePercentArray(columnWidths));
            table.SetWidth(UnitValue.CreatePercentValue(100));
            _loggingService.LogMessage("INFO", $"ExportToPdf: Table created with {visibleColumns.Count} columns");

            // Add headers and data
            AddTableHeaders(table, visibleColumns);
            AddTableData(table, dataSource, visibleColumns);

            _loggingService.LogMessage("INFO", "ExportToPdf: Adding table to document...");
            document.Add(table);
        }

        private void AddTableHeaders(Table table, System.Collections.Generic.List<DataGridViewColumn> visibleColumns)
        {
            _loggingService.LogMessage("INFO", "ExportToPdf: Adding table headers...");
            PdfFont boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

            foreach (var column in visibleColumns)
            {
                Cell headerCell = new Cell()
                    .Add(new Paragraph(column.HeaderText).SetFont(boldFont).SetFontSize(10))
                    .SetBackgroundColor(WebColors.GetRGBColor("#F0F0F0"))
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetPadding(5);
                table.AddHeaderCell(headerCell);
            }
        }

        private void AddTableData(Table table, DataTable dataSource, System.Collections.Generic.List<DataGridViewColumn> visibleColumns)
        {
            _loggingService.LogMessage("INFO", "ExportToPdf: Adding data rows...");
            PdfFont normalFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            int addedRows = 0;

            foreach (DataRow dataRow in dataSource.Rows)
            {
                foreach (var column in visibleColumns)
                {
                    string cellValue = dataRow[column.DataPropertyName]?.ToString() ?? "";
                    Cell dataCell = new Cell()
                        .Add(new Paragraph(cellValue).SetFont(normalFont).SetFontSize(9))
                        .SetPadding(4)
                        .SetTextAlignment(GetTextAlignment(column.ValueType));
                    table.AddCell(dataCell);
                }
                addedRows++;
            }

            _loggingService.LogMessage("INFO", $"ExportToPdf: {addedRows} data rows added to table");
        }

        private DataTable GetDataFromGridView()
        {
            if (_dataGridView.DataSource is DataTable dataTable)
            {
                return dataTable;
            }
            else if (_dataGridView.DataSource is BindingSource bindingSource && bindingSource.DataSource is DataTable)
            {
                return (DataTable)bindingSource.DataSource;
            }
            else
            {
                throw new InvalidOperationException("DataGridView data source is not a DataTable");
            }
        }

        private TextAlignment GetTextAlignment(Type valueType)
        {
            if (valueType == typeof(int) || valueType == typeof(decimal) ||
                valueType == typeof(double) || valueType == typeof(float))
            {
                return TextAlignment.RIGHT;
            }
            return TextAlignment.LEFT;
        }

        public void Dispose()
        {
            _loggingService.LogMessage("INFO", "PdfFormatterService: Disposing");
            // Clean up resources if needed
        }
    }
}