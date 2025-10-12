using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;
using System.Drawing;
using System.ComponentModel;
using System.IO;
using System.Text;
using InventorySystem.Services;

namespace InventorySystem
{
    public partial class ViewFrm : Form
    {
        private DatabaseService databaseService;
        private DataLoadingService dataLoadingService;
        private LoggingService loggingService;
        private string currentTable = "Supplies";
        private DataTable dataTable;
        private SqlDataAdapter dataAdapter;
        private bool isAdminUser = false;

        // Service instances
        private DataGridViewEditService editService;

        // Read table names from config
        private string SuppliesTable => ConfigurationManager.AppSettings["SuppliesTable"] ?? "SuppliesInventory";
        private string AssetsTable => ConfigurationManager.AppSettings["AssetsTable"] ?? "AssetsInventory";

        public ViewFrm()
        {
            InitializeComponent();
            InitializeServices();
            SetupProgressBar();
            CheckUserPermissions();
            InitializeEditService();
        }

        private void InitializeServices()
        {
            try
            {
                // Initialize logging service first
                loggingService = new LoggingService();
                LogMessage("SYSTEM", "Logging service initialized successfully");

                // Then initialize database service
                InitializeDatabaseService();

                // Initialize data loading service
                InitializeDataLoadingService();
            }
            catch (Exception ex)
            {
                string errorMessage = $"Service initialization failed: {ex.Message}";
                MessageBox.Show($"Service Initialization Error: {ex.Message}\n\nApplication will now exit.",
                              "Initialization Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                Application.Exit();
                throw;
            }
        }

        private void InitializeDataLoadingService()
        {
            try
            {
                dataLoadingService = new DataLoadingService(databaseService, loggingService);
                LogMessage("SYSTEM", "Data loading service initialized successfully");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Data loading service initialization failed: {ex.Message}";
                LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }

        private bool ValidateConfiguration()
        {
            try
            {
                // Check if table names are configured
                string suppliesTable = ConfigurationManager.AppSettings["SuppliesTable"];
                string assetsTable = ConfigurationManager.AppSettings["AssetsTable"];

                if (string.IsNullOrEmpty(suppliesTable))
                {
                    LogMessage("CONFIG", "SuppliesTable not found in config, using default");
                }

                if (string.IsNullOrEmpty(assetsTable))
                {
                    LogMessage("CONFIG", "AssetsTable not found in config, using default");
                }

                return true;
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
                LogMessage("CONFIG", "Initializing database service...");

                // Test configuration first
                string testConnectionString = ConfigurationManager.ConnectionStrings["InventoryManagementConnection"]?.ConnectionString;
                if (string.IsNullOrEmpty(testConnectionString))
                {
                    LogMessage("CONFIG", "No connection string found in config, using fallback");
                }

                // ✅ CORRECTED: Pass logging service to DatabaseService
                databaseService = new DatabaseService(null, loggingService);

                // Test connection immediately with better error handling
                TestDatabaseConnection();
                LogMessage("CONFIG", "Database service initialized successfully");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Database service initialization failed: {ex.Message}";
                LogMessage("ERROR", errorMessage);

                // More detailed error message
                MessageBox.Show($"Database Service Error: {ex.Message}\n\n" +
                               "Please check:\n" +
                               "1. Database server is running\n" +
                               "2. Connection string in config file\n" +
                               "3. Network connectivity\n\n" +
                               "Application will now exit.",
                              "Database Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);

                // Close the application if database cannot be initialized
                Application.Exit();
                throw;
            }
        }

        private void InitializeEditService()
        {
            editService = new DataGridViewEditService(dataGridView1, databaseService, loggingService, isAdminUser);
            editService.OnStatusUpdate += HandleServiceStatusUpdate;
            editService.OnError += HandleServiceError;
            editService.OnLogMessage += HandleServiceLogMessage;
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
            // Optional: Handle log messages from service if needed
            System.Diagnostics.Debug.WriteLine($"Service Log: {logEntry}");
        }

        private void LogMessage(string category, string message)
        {
            loggingService?.LogMessage(category, message);
        }

        private void TestDatabaseConnection()
        {
            try
            {
                LogMessage("DATABASE", "Testing database connection...");
                databaseService.TestConnection();
                LogMessage("DATABASE", "Database connection test successful");
            }
            catch (Exception ex)
            {
                LogMessage("ERROR", $"Database connection test failed: {ex.Message}");
                throw new Exception($"Database connection test failed: {ex.Message}");
            }
        }

        private void CheckUserPermissions()
        {
            string role = LoginFrm.CurrentRole;
            string username = LoginFrm.CurrentUsername;
            isAdminUser = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ||
                         role.Equals("Administrator", StringComparison.OrdinalIgnoreCase);

            LogMessage("USER", $"User logged in: {username} (Role: {role}, IsAdmin: {isAdminUser})");
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
            LogMessage("SYSTEM", "Application initialization started");
            CenterToScreen();
            SetupProgressBar();
            toolStripProgressBar1.Visible = true;
            toolStripProgressBar1.Value = 0;

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
            LogMessage("SYSTEM", "Application initialization completed successfully");
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

            // ✅ Set greeting as the main menu item text
            userToolStripMenuItem.Text = $"Welcome, {username}";

            // Disable edit menus for Guest users
            bool isGuest = role == "Guest";
            if (editSuppliesToolStripMenuItem != null)
                editSuppliesToolStripMenuItem.Enabled = !isGuest && isAdminUser;
            if (editAssetsToolStripMenuItem != null)
                editAssetsToolStripMenuItem.Enabled = !isGuest && isAdminUser;

            LogMessage("USER", $"User interface configured for {username} (Role: {role})");
        }

        private void LoadSuppliesData()
        {
            LogMessage("DATA", "Loading supplies data...");
            LoadTableData(SuppliesTable, "Supplies");
        }

        private void LoadAssetsData()
        {
            LogMessage("DATA", "Loading assets data...");
            LoadTableData(AssetsTable, "Assets");
        }

        private void LoadTableData(string tableName, string tableType)
        {
            try
            {
                // Use DataLoadingService to load table data
                dataTable = dataLoadingService.LoadTableData(tableName, tableType, ShowLoadingState);

                // Create data adapter for updates using DataLoadingService
                dataAdapter = dataLoadingService.CreateDataAdapter(tableName);

                // Initialize the edit service with the loaded data
                editService.InitializeData(dataTable, dataAdapter);

                // Set up DataGridView
                dataGridView1.DataSource = null;
                dataGridView1.AutoGenerateColumns = true;
                dataGridView1.DataSource = dataTable;
                dataGridView1.ReadOnly = true; // Start in read-only mode

                currentTable = tableType;
                AdjustFormWidth();

                string successMessage = $"{tableType} data loaded successfully - {dataTable.Rows.Count} records";
                ShowCompletedState(successMessage, 100);
                LogMessage("DATA", successMessage);
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Database error loading {tableType} data: {sqlEx.Message}";
                ShowErrorState($"Database error: {sqlEx.Message}");
                LogMessage("ERROR", errorMessage);
                MessageBox.Show(errorMessage, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (InvalidOperationException ioEx)
            {
                string errorMessage = $"Configuration error loading {tableType} data: {ioEx.Message}";
                ShowErrorState($"Configuration error: {ioEx.Message}");
                LogMessage("ERROR", errorMessage);
                MessageBox.Show(errorMessage, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading {tableType} data from table '{tableName}': {ex.Message}";
                ShowErrorState($"Error loading {tableType} data");
                LogMessage("ERROR", errorMessage);
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

            // Update context menu items based on permissions and mode
            addNewToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            editSelectedToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            deleteSelectedToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            saveChangesToolStripMenuItem.Enabled = isAdminUser && isEditMode;
            cancelChangesToolStripMenuItem.Enabled = isAdminUser && isEditMode;

            // Update main menu items
            editSuppliesToolStripMenuItem.Enabled = isAdminUser;
            editAssetsToolStripMenuItem.Enabled = isAdminUser;

            // Update form title to indicate edit mode
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

        // Event Handlers
        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("USER", "Logout initiated");
            if (editService.CheckForUnsavedChanges())
            {
                LogMessage("USER", "Logout cancelled due to unsaved changes");
                return;
            }

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
            LoadSuppliesData();
        }

        private void displayAssetsTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("NAVIGATION", "Switching to assets table view");
            if (editService.CheckForUnsavedChanges()) return;

            ShowLoadingState("Loading assets table...", 10);
            DisableEditMode();
            LoadAssetsData();
        }

        private void editSuppliesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("EDIT", "Edit supplies mode requested");
            if (currentTable != "Supplies")
            {
                if (editService.CheckForUnsavedChanges()) return;
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
                LoadAssetsData();
            }
            EnableEditMode();
        }

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("DATA", "Manual refresh requested");
            if (editService.CheckForUnsavedChanges()) return;
            RefreshData();
        }

        private void RefreshData()
        {
            try
            {
                // Use DataLoadingService for refresh operation
                dataLoadingService.RefreshData(currentTable, SuppliesTable, AssetsTable, ShowLoadingState);

                // Reload the current table data
                if (currentTable == "Assets")
                {
                    LoadAssetsData();
                }
                else
                {
                    LoadSuppliesData();
                }

                UpdateDateDisplay();
                ShowCompletedState("Data refreshed successfully", 100);
                LogMessage("DATA", "Data refresh completed successfully");
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Connection failed: {sqlEx.Message}";
                ShowErrorState($"Connection failed: {sqlEx.Message}");
                LogMessage("ERROR", errorMessage);
                MessageBox.Show($"Database connection error: {sqlEx.Message}",
                              "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Refresh failed: {ex.Message}";
                ShowErrorState($"Refresh failed: {ex.Message}");
                LogMessage("ERROR", errorMessage);
                MessageBox.Show($"Error during refresh: {ex.Message}",
                              "Refresh Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void changePasswordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("FEATURE", "Change password feature accessed (not implemented)");
            MessageBox.Show("Password change functionality to be implemented.",
                          "Feature Coming Soon",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }

        // Context Menu Event Handlers
        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("CONTEXT", "Add new record via context menu");
            AddNewRecord();
        }

        private void editSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("CONTEXT", "Edit selected via context menu");
            if (dataGridView1.SelectedRows.Count > 0 && !editService.IsEditMode)
            {
                EnableEditMode();
            }
        }

        private void deleteSelectedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("CONTEXT", "Delete selected via context menu");
            DeleteSelectedRecord();
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("CONTEXT", "Save changes via context menu");
            SaveChangesToDatabase();
        }

        private void cancelChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LogMessage("CONTEXT", "Cancel changes via context menu");
            DiscardChanges();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            // Update context menu item states based on current selection and permissions
            bool hasSelection = dataGridView1.SelectedRows.Count > 0;
            bool hasChanges = editService.HasUnsavedChanges();

            editSelectedToolStripMenuItem.Enabled = isAdminUser && editService.IsEditMode && hasSelection;
            deleteSelectedToolStripMenuItem.Enabled = isAdminUser && editService.IsEditMode && hasSelection;
            saveChangesToolStripMenuItem.Enabled = isAdminUser && editService.IsEditMode && hasChanges;
            cancelChangesToolStripMenuItem.Enabled = isAdminUser && editService.IsEditMode && hasChanges;
        }

        private void dataGridView1_SelectionChanged(object sender, EventArgs e)
        {
            int selectedCount = dataGridView1.SelectedRows.Count;
            if (selectedCount > 0)
            {
                toolStripStatusLabel1.Text = $"{selectedCount} row(s) selected";
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            string errorMessage = $"Data error in cell: {e.Exception.Message}";
            ShowErrorState($"Data error in cell: {e.Exception.Message}");
            LogMessage("ERROR", errorMessage);
            MessageBox.Show($"Data error: {e.Exception.Message}\n\nPlease check your input and try again.",
                          "Data Error",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Error);
            e.ThrowException = false;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle any special cell content clicks if needed
        }

        private void ViewFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            LogMessage("SYSTEM", "Form closing event triggered");
            if (editService.CheckForUnsavedChanges())
            {
                e.Cancel = true; // Cancel closing if there are unsaved changes
                LogMessage("SYSTEM", "Form closing cancelled due to unsaved changes");
                return;
            }

            // Log application closure
            if (!e.Cancel)
            {
                LogMessage("SYSTEM", "Application closed successfully");
                loggingService?.LogSessionEnd();

                // Dispose services
                databaseService?.Dispose();
                dataLoadingService = null; // DataLoadingService doesn't implement IDisposable
                loggingService?.Dispose();
            }
        }

        private void viewLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                loggingService?.OpenLogFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening log file: {ex.Message}", "Error",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}