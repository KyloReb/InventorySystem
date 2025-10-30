using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing.Imaging;
using InventorySystem.Services;

namespace InventorySystem
{
    public partial class ViewFrm : Form
    {
        private DatabaseService databaseService;
        private DataLoadingService dataLoadingService;
        private LoggingService loggingService;
        private PrintExportService printExportService;
        private AuthService authService; // Added AuthService

        private string currentTable = "Supplies";
        private DataTable dataTable;
        private DataTable originalDataTable;
        private SqlDataAdapter dataAdapter;
        private bool isAdminUser = false;
        private bool isGuestUser = false;
        private bool isSearchActive = false;

        private DataGridViewEditService editService;

        public ViewFrm()
        {
            InitializeComponent();
            InitializeServices();
            SetupProgressBar();
            CheckUserPermissions();
            InitializeEditService();
            InitializePrintExportService();
            UpdateMenuVisibility(); // Added to control menu visibility based on role
        }

        private void InitializeServices()
        {
            try
            {
                loggingService = new LoggingService();
                InitializeDatabaseService();
                InitializeDataLoadingService();
                InitializeAuthService(); // Added AuthService initialization
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Service Initialization Error: {ex.Message}\n\nApplication will now exit.",
                              "Initialization Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void InitializeAuthService()
        {
            try
            {
                authService = new AuthService(databaseService, loggingService);
                LogMessage("AUTH", "AuthService initialized successfully");
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"AuthService initialization failed: {ex.Message}");
                throw new Exception($"AuthService initialization failed: {ex.Message}", ex);
            }
        }

        private void InitializeDataLoadingService()
        {
            try
            {
                dataLoadingService = new DataLoadingService(databaseService, loggingService);
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Data loading service initialization failed: {ex.Message}");
                throw new Exception($"Data loading service initialization failed: {ex.Message}", ex);
            }
        }

        private bool ValidateConfiguration()
        {
            try
            {
                if (dataLoadingService == null)
                {
                    LogMessage("ERROR", "Data loading service is not initialized");
                    return false;
                }

                return dataLoadingService.ValidateConfiguration();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Configuration validation failed: {ex.Message}");
                return false;
            }
        }

        private void InitializeDatabaseService()
        {
            try
            {
                databaseService = new DatabaseService(null, loggingService);
                TestDatabaseConnectionDirect();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Database service initialization failed: {ex.Message}");

                MessageBox.Show($"Database Service Error: {ex.Message}\n\n" +
                               "Please check:\n" +
                               "1. Database server is running\n" +
                               "2. Connection string in config file\n" +
                               "3. Network connectivity\n\n" +
                               "Application will now exit.",
                              "Database Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                Environment.Exit(1);
            }
        }

        private void InitializeEditService()
        {
            editService = new DataGridViewEditService(dataGridView1, databaseService, loggingService, isAdminUser);
            editService.OnStatusUpdate += HandleServiceStatusUpdate;
            editService.OnError += HandleServiceError;
            editService.OnLogMessage += HandleServiceLogMessage;
        }

        private void InitializePrintExportService()
        {
            try
            {
                printExportService = new PrintExportService(dataGridView1, loggingService);
                printExportService.SetTitle($"{currentTable} Inventory Report");
                SetupPrintMenu();
                SetupSaveAsMenu();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Print/Export service initialization failed: {ex.Message}");
                MessageBox.Show($"Print/Export service initialization error: {ex.Message}",
                    "Initialization Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void UpdateMenuVisibility()
        {
            // Only show Manage Accounts menu for admin users
            manageAccntsToolStripMenuItem.Visible = isAdminUser;

            // Hide logout and change password for guest users
            logoutToolStripMenuItem.Visible = !isGuestUser;
            changePasswordToolStripMenuItem.Visible = !isGuestUser;
        }

        private void SetupPrintMenu()
        {
            printToolStripMenuItem.DropDownItems.Clear();

            var printPreviewItem = new ToolStripMenuItem("Print Preview");
            printPreviewItem.Click += (s, e) => PrintPreview();
            printToolStripMenuItem.DropDownItems.Add(printPreviewItem);

            var printItem = new ToolStripMenuItem("Print...");
            printItem.Click += (s, e) => Print();
            printToolStripMenuItem.DropDownItems.Add(printItem);

            var pageSetupItem = new ToolStripMenuItem("Page Setup...");
            pageSetupItem.Click += (s, e) => PageSetup();
            printToolStripMenuItem.DropDownItems.Add(pageSetupItem);
        }

        private void SetupSaveAsMenu()
        {
            saveAsToolStripMenuItem.DropDownItems.Clear();

            var excelItem = new ToolStripMenuItem("Export to Excel (.xlsx)");
            excelItem.Click += (s, e) => ExportToExcel();
            saveAsToolStripMenuItem.DropDownItems.Add(excelItem);

            var csvItem = new ToolStripMenuItem("Export to CSV (.csv)");
            csvItem.Click += (s, e) => ExportToCsv();
            saveAsToolStripMenuItem.DropDownItems.Add(csvItem);

            var pdfItem = new ToolStripMenuItem("Export to PDF (.pdf)");
            pdfItem.Click += (s, e) => ExportToPdf();
            saveAsToolStripMenuItem.DropDownItems.Add(pdfItem);

            saveAsToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());

            var imageMenu = new ToolStripMenuItem("Export to Image");

            var pngItem = new ToolStripMenuItem("PNG Image (.png)");
            pngItem.Click += (s, e) => ExportToImage(ImageFormat.Png, "PNG Image (*.png)|*.png");
            imageMenu.DropDownItems.Add(pngItem);

            var jpgItem = new ToolStripMenuItem("JPEG Image (.jpg)");
            jpgItem.Click += (s, e) => ExportToImage(ImageFormat.Jpeg, "JPEG Image (*.jpg)|*.jpg");
            imageMenu.DropDownItems.Add(jpgItem);

            var bmpItem = new ToolStripMenuItem("Bitmap Image (.bmp)");
            bmpItem.Click += (s, e) => ExportToImage(ImageFormat.Bmp, "Bitmap Image (*.bmp)|*.bmp");
            imageMenu.DropDownItems.Add(bmpItem);

            saveAsToolStripMenuItem.DropDownItems.Add(imageMenu);
        }

        private void PrintPreview()
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to print.", "Print Preview",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                UpdatePrintTitle();
                printExportService.ShowPrintPreview();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Print preview error: {ex.Message}");
                MessageBox.Show($"Print preview error: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Print()
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to print.", "Print",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                UpdatePrintTitle();
                printExportService.ShowPrintDialog();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Print error: {ex.Message}");
                MessageBox.Show($"Print error: {ex.Message}", "Print Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PageSetup()
        {
            try
            {
                printExportService.ShowPageSetup();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Page setup error: {ex.Message}");
                MessageBox.Show($"Page setup error: {ex.Message}", "Page Setup Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToExcel()
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "Excel Workbook (*.xlsx)|*.xlsx";
                    saveDialog.FileName = $"{currentTable}_Inventory_{DateTime.Now:yyyyMMdd}";
                    saveDialog.Title = "Export to Excel";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ShowLoadingState("Exporting to Excel...", 50);
                        printExportService.ExportToExcel(saveDialog.FileName);
                        ShowCompletedState("Excel export completed", 100);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Excel export error: {ex.Message}");
                ShowErrorState("Excel export failed");
                MessageBox.Show($"Excel export error: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToCsv()
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "CSV File (*.csv)|*.csv";
                    saveDialog.FileName = $"{currentTable}_Inventory_{DateTime.Now:yyyyMMdd}";
                    saveDialog.Title = "Export to CSV";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ShowLoadingState("Exporting to CSV...", 50);
                        printExportService.ExportToCsv(saveDialog.FileName);
                        ShowCompletedState("CSV export completed", 100);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"CSV export error: {ex.Message}");
                ShowErrorState("CSV export failed");
                MessageBox.Show($"CSV export error: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToPdf()
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = "PDF Document (*.pdf)|*.pdf";
                    saveDialog.FileName = $"{currentTable}_Inventory_{DateTime.Now:yyyyMMdd}";
                    saveDialog.Title = "Export to PDF";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ShowLoadingState("Exporting to PDF...", 50);
                        UpdatePrintTitle();
                        printExportService.ExportToPdf(saveDialog.FileName);
                        ShowCompletedState("PDF export completed", 100);
                    }
                }
            }
            catch (Exception ex)
            {
                ShowErrorState("PDF export failed");
                MessageBox.Show($"PDF export error: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ExportToImage(ImageFormat format, string filter)
        {
            try
            {
                if (dataGridView1.DataSource == null || dataGridView1.Rows.Count == 0)
                {
                    MessageBox.Show("No data available to export.", "Export",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using (SaveFileDialog saveDialog = new SaveFileDialog())
                {
                    saveDialog.Filter = filter;
                    saveDialog.FileName = $"{currentTable}_Inventory_{DateTime.Now:yyyyMMdd}";
                    saveDialog.Title = "Export to Image";

                    if (saveDialog.ShowDialog() == DialogResult.OK)
                    {
                        ShowLoadingState("Exporting to image...", 50);
                        printExportService.ExportToImage(format, saveDialog.FileName);
                        ShowCompletedState("Image export completed", 100);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Image export error: {ex.Message}");
                ShowErrorState("Image export failed");
                MessageBox.Show($"Image export error: {ex.Message}", "Export Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdatePrintTitle()
        {
            string searchSuffix = isSearchActive ? " (Filtered)" : "";
            printExportService.SetTitle($"{currentTable} Inventory Report{searchSuffix}");
        }

        private void HandleServiceStatusUpdate(string message)
        {
            ShowCompletedState(message, 100);
        }

        private void HandleServiceError(string errorMessage)
        {
            ShowErrorState(errorMessage);
        }

        private void HandleServiceLogMessage(string logEntry)
        {
            System.Diagnostics.Debug.WriteLine($"Service Log: {logEntry}");
        }

        private void LogMessage(string category, string message)
        {
            loggingService?.LogMessage(category, message);
        }

        private void TestDatabaseConnectionDirect()
        {
            try
            {
                if (databaseService == null)
                {
                    throw new InvalidOperationException("Database service is not initialized");
                }

                LogMessage("DATABASE", "Testing database connection...");
                databaseService.TestConnection();
                LogMessage("DATABASE", "Database connection test successful");
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Database connection test failed: {ex.Message}");
                throw new Exception($"Database connection test failed: {ex.Message}", ex);
            }
        }

        private void TestDatabaseConnection()
        {
            try
            {
                if (dataLoadingService == null)
                {
                    throw new InvalidOperationException("Data loading service is not initialized");
                }

                dataLoadingService.TestDatabaseConnection();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Database connection test failed: {ex.Message}");
                throw new Exception($"Database connection test failed: {ex.Message}", ex);
            }
        }

        private void CheckUserPermissions()
        {
            string role = LoginFrm.CurrentRole;
            string username = LoginFrm.CurrentUsername;
            isAdminUser = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                         role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);
            isGuestUser = role.Equals("Guest", StringComparison.OrdinalIgnoreCase);

            LogMessage("USER", $"User logged in: {username} (Role: {role}, IsAdmin: {isAdminUser}, IsGuest: {isGuestUser})");
        }

        private void SetupProgressBar()
        {
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar1.Minimum = 0;
            toolStripProgressBar1.Maximum = 100;
            toolStripProgressBar1.Value = 0;
            statusStrip1.Height = 30;
            statusStrip1.AutoSize = false;
        }

        private void ViewFrm_Load(object sender, EventArgs e)
        {
            CenterToScreen();
            SetupProgressBar();

            if (dataLoadingService == null || databaseService == null || authService == null)
            {
                ShowErrorState("Service initialization failed - application cannot continue");
                MessageBox.Show("Critical services failed to initialize. The application will now close.",
                               "Initialization Error",
                               MessageBoxButtons.OK,
                               MessageBoxIcon.Error);
                this.Close();
                return;
            }

            ShowLoadingState("Validating configuration...", 5);
            if (!ValidateConfiguration())
            {
                ShowErrorState("Configuration validation failed");
                return;
            }

            ShowLoadingState("Initializing application...", 10);
            LoadUserInfo();

            ShowLoadingState("Loading supplies data...", 60);
            LoadSuppliesData();

            ShowCompletedState("Application ready", 100);
            UpdateDateDisplay();
            UpdateEditModeUI();
        }

        private void ShowLoadingState(string message, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, int>(ShowLoadingState), message, progress);
                return;
            }

            toolStripStatusLabel1.Text = message;
            toolStripProgressBar1.Value = Math.Min(progress, 100);
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar1.ForeColor = Color.FromArgb(0, 120, 215);

            statusStrip1.Refresh();
            Application.DoEvents();
            System.Threading.Thread.Sleep(100);

            LogMessage("STATUS", message);
        }

        private void ShowCompletedState(string message, int progress)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string, int>(ShowCompletedState), message, progress);
                return;
            }

            toolStripStatusLabel1.Text = message;
            toolStripProgressBar1.Value = Math.Min(progress, 100);
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;
            toolStripProgressBar1.ForeColor = Color.FromArgb(0, 200, 0);

            statusStrip1.Refresh();
            Application.DoEvents();

            LogMessage("SUCCESS", message);
        }

        private void ShowErrorState(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(ShowErrorState), message);
                return;
            }

            toolStripStatusLabel1.Text = message;
            toolStripProgressBar1.ForeColor = Color.Red;
            toolStripProgressBar1.Style = ProgressBarStyle.Continuous;

            statusStrip1.Refresh();
            Application.DoEvents();

            LogMessage("ERROR", message);
        }

        private void UpdateDateDisplay()
        {
            dateLbl.Text = DateTime.Now.ToString("MM/dd/yyyy");
        }

        private void LoadUserInfo()
        {
            ShowLoadingState("Loading user information...", 30);

            string username = LoginFrm.CurrentUsername;
            string role = LoginFrm.CurrentRole;

            userToolStripMenuItem.Text = $"Welcome, {username}";

            bool isGuest = role == "Guest";
            if (editSuppliesToolStripMenuItem != null)
                editSuppliesToolStripMenuItem.Enabled = !isGuest && isAdminUser;
            if (editAssetsToolStripMenuItem != null)
                editAssetsToolStripMenuItem.Enabled = !isGuest && isAdminUser;
            if (editAccountsToolStripMenuItem != null)
                editAccountsToolStripMenuItem.Enabled = isAdminUser;

            LogMessage("USER", $"User interface configured for {username} (Role: {role})");
        }

        private void LoadSuppliesData()
        {
            if (dataLoadingService == null)
            {
                ShowErrorState("Data loading service not available");
                LogMessage("ERROR", "Cannot load supplies - service not initialized");
                return;
            }

            LogMessage("DATA", "Loading supplies data...");
            LoadTableData(dataLoadingService.GetSuppliesTableName(), "Supplies");
        }

        private void LoadAssetsData()
        {
            if (dataLoadingService == null)
            {
                ShowErrorState("Data loading service not available");
                LogMessage("ERROR", "Cannot load assets - service not initialized");
                return;
            }

            LogMessage("DATA", "Loading assets data...");
            LoadTableData(dataLoadingService.GetAssetsTableName(), "Assets");
        }

        private void LoadAccountsData()
        {
            if (dataLoadingService == null)
            {
                ShowErrorState("Data loading service not available");
                LogMessage("ERROR", "Cannot load accounts - service not initialized");
                return;
            }

            LogMessage("DATA", "Loading accounts data...");
            LoadTableData("Users", "Accounts");
        }

        private void LoadTableData(string tableName, string tableType)
        {
            if (dataLoadingService == null)
            {
                ShowErrorState("Data loading service not available");
                MessageBox.Show("Cannot load data - service not initialized",
                               "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                dataTable = dataLoadingService.LoadTableData(tableName, tableType, ShowLoadingState);
                originalDataTable = dataTable.Copy();
                dataAdapter = dataLoadingService.CreateDataAdapter(tableName);

                editService.InitializeData(dataTable, dataAdapter);

                dataGridView1.DataSource = null;
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = dataTable;
                dataGridView1.ReadOnly = true;

                currentTable = tableType;
                UpdatePrintTitle();
                AdjustFormWidth();

                string successMessage = $"{tableType} data loaded - {dataTable.Rows.Count} records";
                ShowCompletedState(successMessage, 100);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading {tableType} data: {ex.Message}";
                ShowErrorState($"Error loading {tableType} data");
                MessageBox.Show(errorMessage, "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdjustFormWidth()
        {
            if (dataGridView1.Columns.Count == 0) return;

            int totalWidth = dataGridView1.RowHeadersWidth;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                totalWidth += column.Width;
            }

            totalWidth += SystemInformation.VerticalScrollBarWidth + 20;
            this.Width = Math.Max(totalWidth, 800);
            CenterToScreen();
        }

        private void EnableEditMode()
        {
            if (editService.EnableEditMode())
            {
                UpdateEditModeUI();
            }
        }

        private void DisableEditMode()
        {
            editService.DisableEditMode();
            UpdateEditModeUI();
        }

        private void UpdateEditModeUI()
        {
            bool isEditMode = editService.IsEditMode;

            addNewToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            editSelectedToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            deleteSelectedToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            saveChangesToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            cancelChangesToolStripMenuItem.Enabled = isAdminUser && isEditMode;

            editSuppliesToolStripMenuItem.Enabled = isAdminUser;
            editAssetsToolStripMenuItem.Enabled = isAdminUser;
            editAccountsToolStripMenuItem.Enabled = isAdminUser;

            this.Text = isEditMode ?
                $"Inventory Management System - EDIT MODE ({currentTable})" :
                "Inventory Management System";
        }

        private bool SaveChangesToDatabase()
        {
            return editService.SaveChangesToDatabase();
        }

        private void DiscardChanges()
        {
            editService.DiscardChanges();
        }

        private void AddNewRecord()
        {
            editService.AddNewRecord();
        }

        private void DeleteSelectedRecord()
        {
            editService.DeleteSelectedRecord();
        }

        private void PerformSearch(string searchText)
        {
            if (dataLoadingService == null || originalDataTable == null ||
                string.IsNullOrWhiteSpace(searchText) || searchText == "Search...")
            {
                if (originalDataTable != null)
                {
                    dataTable.Clear();
                    foreach (DataRow row in originalDataTable.Rows)
                    {
                        dataTable.ImportRow(row);
                    }
                    isSearchActive = false;
                    toolStripStatusLabel1.Text = $"Displaying all {dataTable.Rows.Count} records";
                    UpdatePrintTitle();
                }
                return;
            }

            try
            {
                DataTable filteredTable = dataLoadingService.PerformSearch(originalDataTable, searchText);

                dataTable.Clear();
                foreach (DataRow row in filteredTable.Rows)
                {
                    dataTable.ImportRow(row);
                }

                isSearchActive = true;
                toolStripStatusLabel1.Text = $"Found {dataTable.Rows.Count} records matching '{searchText}'";
                UpdatePrintTitle();
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Search error: {ex.Message}");
                MessageBox.Show($"Search error: {ex.Message}", "Search Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearSearch()
        {
            searchToolStripTextBox.Text = "Search...";
            searchToolStripTextBox.ForeColor = Color.Gray;

            if (dataLoadingService != null && originalDataTable != null)
            {
                DataTable clearedTable = dataLoadingService.ClearSearch(originalDataTable);
                dataTable.Clear();
                foreach (DataRow row in clearedTable.Rows)
                {
                    dataTable.ImportRow(row);
                }
                isSearchActive = false;
                toolStripStatusLabel1.Text = $"Displaying all {dataTable.Rows.Count} records";
                UpdatePrintTitle();
            }
        }

        // ========== EVENT HANDLERS ==========

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("USER", "Logout initiated");
            if (editService.CheckForUnsavedChanges())
            {
                LogMessage("USER", "Logout cancelled due to unsaved changes");
                return;
            }

            // Log user logout
            authService?.LogUserLogout(LoginFrm.CurrentUsername);

            LoginFrm loginForm = new LoginFrm();
            loginForm.StartPosition = FormStartPosition.CenterScreen;
            loginForm.Show();
            LogMessage("USER", "User logged out successfully");
            this.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("SYSTEM", "Application exit initiated");
            if (editService.CheckForUnsavedChanges())
            {
                LogMessage("SYSTEM", "Application exit cancelled due to unsaved changes");
                return;
            }
            LogMessage("SYSTEM", "Application exiting");
            Application.Exit();
        }

        private void displaySuppliesTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("NAVIGATION", "Switching to supplies table view");
            if (editService.CheckForUnsavedChanges()) return;

            ShowLoadingState("Loading supplies table...", 10);
            DisableEditMode();
            ClearSearch();
            LoadSuppliesData();
        }

        private void displayAssetsTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("NAVIGATION", "Switching to assets table view");
            if (editService.CheckForUnsavedChanges()) return;

            ShowLoadingState("Loading assets table...", 10);
            DisableEditMode();
            ClearSearch();
            LoadAssetsData();
        }

        private void displayAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("NAVIGATION", "Switching to accounts table view");
            if (editService.CheckForUnsavedChanges()) return;

            ShowLoadingState("Loading accounts table...", 10);
            DisableEditMode();
            ClearSearch();
            LoadAccountsData();
        }

        private void editSuppliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("EDIT", "Edit supplies mode requested");
            if (currentTable != "Supplies")
            {
                if (editService.CheckForUnsavedChanges()) return;
                ClearSearch();
                LoadSuppliesData();
            }
            EnableEditMode();
        }

        private void editAssetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("EDIT", "Edit assets mode requested");
            if (currentTable != "Assets")
            {
                if (editService.CheckForUnsavedChanges()) return;
                ClearSearch();
                LoadAssetsData();
            }
            EnableEditMode();
        }

        private void editAccountsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("EDIT", "Edit accounts mode requested");
            if (currentTable != "Accounts")
            {
                if (editService.CheckForUnsavedChanges()) return;
                ClearSearch();
                LoadAccountsData();
            }
            EnableEditMode();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dataLoadingService == null)
            {
                MessageBox.Show("Data service is not available", "Error",
                               MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            LogMessage("DATA", "Manual refresh requested");
            if (editService.CheckForUnsavedChanges()) return;

            ShowLoadingState("Refreshing data...", 10);
            ClearSearch();

            try
            {
                dataLoadingService.RefreshData(currentTable,
                    dataLoadingService.GetSuppliesTableName(),
                    dataLoadingService.GetAssetsTableName(),
                    ShowLoadingState);

                if (currentTable == "Supplies")
                {
                    LoadSuppliesData();
                }
                else if (currentTable == "Assets")
                {
                    LoadAssetsData();
                }
                else if (currentTable == "Accounts")
                {
                    LoadAccountsData();
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Refresh failed: {ex.Message}");
                ShowErrorState($"Refresh failed: {ex.Message}");
                MessageBox.Show($"Refresh failed: {ex.Message}", "Refresh Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowChangePasswordDialog();
        }

        private void ShowChangePasswordDialog()
        {
            try
            {
                LogMessage("USER", "Change password dialog opened");

                using (ChangePasswordDialog dialog = new ChangePasswordDialog())
                {
                    dialog.StartPosition = FormStartPosition.CenterParent;

                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        string username = dialog.Username;
                        string currentPassword = dialog.CurrentPassword;
                        string newPassword = dialog.NewPassword;

                        if (string.IsNullOrWhiteSpace(username) ||
                            string.IsNullOrWhiteSpace(currentPassword) ||
                            string.IsNullOrWhiteSpace(newPassword))
                        {
                            MessageBox.Show("Please fill in all fields.", "Validation Error",
                                MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        ShowLoadingState("Changing password...", 50);

                        // Use AuthService for password change
                        if (authService.ChangeUserPassword(username, currentPassword, newPassword))
                        {
                            ShowCompletedState("Password changed successfully", 100);
                            MessageBox.Show("Password changed successfully!", "Success",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LogMessage("USER", $"Password changed successfully for user: {username}");
                        }
                        else
                        {
                            ShowErrorState("Password change failed");
                            MessageBox.Show("Failed to change password. Please check your current password and try again.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            LogMessage("ERROR", $"Password change failed for user: {username}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Change password error: {ex.Message}");
                ShowErrorState("Password change error");
                MessageBox.Show($"An error occurred while changing password: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ========== DATA GRID VIEW EVENT HANDLERS ==========

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            LogMessage("ERROR", $"DataGridView error: {e.Exception.Message}");
            MessageBox.Show($"Data error: {e.Exception.Message}", "Data Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            bool hasSelection = dataGridView1.SelectedRows.Count > 0;
            editSelectedToolStripMenuItem.Enabled = hasSelection && isAdminUser && editService.IsEditMode;
            deleteSelectedToolStripMenuItem.Enabled = hasSelection && isAdminUser && editService.IsEditMode;
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewRecord();
        }

        private void editSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteSelectedRecord();
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SaveChangesToDatabase())
            {
                ShowCompletedState("Changes saved successfully", 100);
                ClearSearch();
            }
        }

        private void cancelChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DiscardChanges();
            ClearSearch();
        }

        private void ViewFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                if (editService.CheckForUnsavedChanges())
                {
                    e.Cancel = true;
                    return;
                }
            }

            printExportService?.Dispose();
            LogMessage("SYSTEM", "Application closing");
        }

        private void searchToolStripTextBox_Enter(object sender, EventArgs e)
        {
            if (searchToolStripTextBox.Text == "Search...")
            {
                searchToolStripTextBox.Text = "";
                searchToolStripTextBox.ForeColor = Color.Black;
            }
        }

        private void searchToolStripTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(searchToolStripTextBox.Text))
            {
                searchToolStripTextBox.Text = "Search...";
                searchToolStripTextBox.ForeColor = Color.Gray;
            }
        }

        private void searchToolStripTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                PerformSearch(searchToolStripTextBox.Text);
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                ClearSearch();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void searchToolStripTextBox_TextChanged(object sender, EventArgs e)
        {
            if (searchToolStripTextBox.Text != "Search..." && !string.IsNullOrWhiteSpace(searchToolStripTextBox.Text))
            {
                PerformSearch(searchToolStripTextBox.Text);
            }
        }
    }

    // Change Password Dialog (unchanged from your original)
    public class ChangePasswordDialog : Form
    {
        private TextBox txtUsername;
        private TextBox txtCurrentPassword;
        private TextBox txtNewPassword;
        private Button btnConfirm;
        private Button btnCancel;
        private Label lblUsername;
        private Label lblCurrentPassword;
        private Label lblNewPassword;

        public string Username => txtUsername.Text;
        public string CurrentPassword => txtCurrentPassword.Text;
        public string NewPassword => txtNewPassword.Text;

        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(350, 250);
            this.Text = "Change Password";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            lblUsername = new Label { Text = "Username:", Location = new Point(20, 20), Size = new Size(100, 20) };
            txtUsername = new TextBox { Location = new Point(120, 20), Size = new Size(180, 20) };

            lblCurrentPassword = new Label { Text = "Current Password:", Location = new Point(20, 60), Size = new Size(100, 20) };
            txtCurrentPassword = new TextBox { Location = new Point(120, 60), Size = new Size(180, 20), UseSystemPasswordChar = true };

            lblNewPassword = new Label { Text = "New Password:", Location = new Point(20, 100), Size = new Size(100, 20) };
            txtNewPassword = new TextBox { Location = new Point(120, 100), Size = new Size(180, 20), UseSystemPasswordChar = true };

            btnConfirm = new Button { Text = "Confirm", Location = new Point(120, 150), Size = new Size(80, 30), DialogResult = DialogResult.OK };
            btnCancel = new Button { Text = "Cancel", Location = new Point(220, 150), Size = new Size(80, 30), DialogResult = DialogResult.Cancel };

            btnConfirm.Click += (s, e) => { if (ValidateInput()) this.DialogResult = DialogResult.OK; };
            btnCancel.Click += (s, e) => this.DialogResult = DialogResult.Cancel;

            this.Controls.AddRange(new Control[] {
                lblUsername, txtUsername,
                lblCurrentPassword, txtCurrentPassword,
                lblNewPassword, txtNewPassword,
                btnConfirm, btnCancel
            });

            this.AcceptButton = btnConfirm;
            this.CancelButton = btnCancel;
        }

        private bool ValidateInput()
        {
<<<<<<< HEAD
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtCurrentPassword.Text) ||
                string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
=======
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Please enter username.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtCurrentPassword.Text))
            {
                MessageBox.Show("Please enter current password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtNewPassword.Text))
            {
                MessageBox.Show("Please enter new password.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
>>>>>>> d2b31c0648712e88d0d38d3b2bf507dd2afb8d7f
                return false;
            }

            if (txtNewPassword.Text.Length < 4)
            {
<<<<<<< HEAD
                MessageBox.Show("New password must be at least 4 characters long.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
=======
                MessageBox.Show("New password must be at least 4 characters long.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
>>>>>>> d2b31c0648712e88d0d38d3b2bf507dd2afb8d7f
                return false;
            }

            return true;
        }
    }
}