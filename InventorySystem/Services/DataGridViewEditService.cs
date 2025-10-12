using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace InventorySystem.Services
{
    public class DataGridViewEditService
    {
        private DataGridView dataGridView;
        private DataTable dataTable;
        private SqlDataAdapter dataAdapter;
        private DatabaseService databaseService;
        private string connectionString;
        private LoggingService logger;
        private bool isEditMode = false;
        private bool isAdminUser = false;
        private bool isInitialized = false;

        public event Action<string> OnStatusUpdate;
        public event Action<string> OnError;
        public event Action<string> OnLogMessage;

        public DataGridViewEditService(DataGridView dataGridView, DatabaseService databaseService, LoggingService loggingService, bool isAdminUser)
        {
            this.dataGridView = dataGridView ?? throw new ArgumentNullException(nameof(dataGridView));
            this.databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            this.logger = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            this.isAdminUser = isAdminUser;

            // Ensure connection string is properly initialized
            ValidateDatabaseService();
        }

        // Keep for backward compatibility but mark as obsolete
        public DataGridViewEditService(DataGridView dataGridView, string connectionString, string logFilePath, bool isAdminUser)
        {
            this.dataGridView = dataGridView ?? throw new ArgumentNullException(nameof(dataGridView));

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty", nameof(connectionString));
            }

            this.connectionString = connectionString;
            this.isAdminUser = isAdminUser;

            // Create a temporary logger for backward compatibility
            this.logger = new LoggingService();
            logger.LogMessage("WARNING", "Using deprecated constructor with connection string. Use DatabaseService constructor instead.");
        }

        private void ValidateDatabaseService()
        {
            if (databaseService == null)
            {
                throw new InvalidOperationException("DatabaseService is not initialized");
            }

            if (!databaseService.IsConnectionValid)
            {
                throw new InvalidOperationException("DatabaseService does not have a valid connection");
            }

            // Set connection string from database service for fallback operations
            connectionString = databaseService.ConnectionString;
        }

        public void InitializeData(DataTable dataTable, SqlDataAdapter dataAdapter)
        {
            this.dataTable = dataTable ?? throw new ArgumentNullException(nameof(dataTable));
            this.dataAdapter = dataAdapter ?? throw new ArgumentNullException(nameof(dataAdapter));
            this.isInitialized = true;

            logger.LogMessage("INIT", "DataGridViewEditService data initialized successfully");
        }

        public bool EnableEditMode()
        {
            if (!isInitialized)
            {
                OnError?.Invoke("Service not properly initialized. Please load data first.");
                return false;
            }

            if (!isAdminUser)
            {
                string message = "Access denied. Administrator privileges required to edit data.";
                logger.LogMessage("SECURITY", "Unauthorized edit mode attempt");
                OnError?.Invoke(message);
                return false;
            }

            isEditMode = true;
            dataGridView.ReadOnly = false;
            dataGridView.AllowUserToAddRows = true;
            dataGridView.AllowUserToDeleteRows = true;

            OnStatusUpdate?.Invoke("Edit mode enabled - You can now modify data");
            logger.LogMessage("EDIT", "Edit mode enabled");
            return true;
        }

        public void DisableEditMode()
        {
            isEditMode = false;
            dataGridView.ReadOnly = true;
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;

            OnStatusUpdate?.Invoke("Edit mode disabled - Data is read-only");
            logger.LogMessage("EDIT", "Edit mode disabled");
        }

        public bool IsEditMode => isEditMode;
        public bool IsInitialized => isInitialized;

        public void AddNewRecord()
        {
            if (!ValidateEditOperation()) return;

            try
            {
                DataRow newRow = dataTable.NewRow();
                dataTable.Rows.Add(newRow);

                // Scroll to the new row and begin editing
                int newRowIndex = dataGridView.Rows.Count - 1;
                dataGridView.FirstDisplayedScrollingRowIndex = newRowIndex;
                if (dataGridView.Columns.Count > 0)
                {
                    dataGridView.CurrentCell = dataGridView.Rows[newRowIndex].Cells[0];
                }
                dataGridView.BeginEdit(true);

                OnStatusUpdate?.Invoke("New record added - Please fill in the details");
                logger.LogMessage("EDIT", "New record added to data table");
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error adding new record: {ex.Message}";
                OnError?.Invoke(errorMessage);
                logger.LogMessage("ERROR", errorMessage);
            }
        }

        public void DeleteSelectedRecord()
        {
            if (dataGridView.SelectedRows.Count == 0)
            {
                OnError?.Invoke("Please select a record to delete.");
                return;
            }

            if (!ValidateEditOperation()) return;

            var result = MessageBox.Show("Are you sure you want to delete the selected record(s)?\nThis action cannot be undone.",
                                       "Confirm Delete",
                                       MessageBoxButtons.YesNo,
                                       MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    int deleteCount = 0;
                    foreach (DataGridViewRow row in dataGridView.SelectedRows)
                    {
                        if (!row.IsNewRow)
                        {
                            dataGridView.Rows.Remove(row);
                            deleteCount++;
                        }
                    }
                    OnStatusUpdate?.Invoke("Record(s) marked for deletion - Remember to save changes");
                    logger.LogMessage("EDIT", $"{deleteCount} record(s) marked for deletion");
                }
                catch (Exception ex)
                {
                    string errorMessage = $"Error deleting record: {ex.Message}";
                    OnError?.Invoke(errorMessage);
                    logger.LogMessage("ERROR", errorMessage);
                }
            }
        }

        public bool SaveChangesToDatabase()
        {
            if (!ValidateEditOperation()) return false;

            try
            {
                if (dataAdapter == null || dataTable == null)
                {
                    string message = "No data loaded to save.";
                    logger.LogMessage("ERROR", message);
                    OnError?.Invoke(message);
                    return false;
                }

                OnStatusUpdate?.Invoke("Saving changes to database...");
                logger.LogMessage("DATABASE", "Starting save operation...");

                // Use DatabaseService if available, otherwise use the old method
                if (databaseService != null)
                {
                    return SaveChangesWithDatabaseService();
                }
                else
                {
                    return SaveChangesWithDirectConnection();
                }
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Database save error: {sqlEx.Message}";
                OnError?.Invoke($"Database save error: {sqlEx.Message}");
                logger.LogMessage("ERROR", errorMessage);
                MessageBox.Show($"Database error while saving: {sqlEx.Message}\n\nPlease check your data and try again.",
                              "Database Save Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return false;
            }
            catch (Exception ex)
            {
                string errorMessage = $"Save error: {ex.Message}";
                OnError?.Invoke($"Save error: {ex.Message}");
                logger.LogMessage("ERROR", errorMessage);
                MessageBox.Show($"Error saving changes: {ex.Message}",
                              "Save Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Error);
                return false;
            }
        }

        private bool ValidateEditOperation()
        {
            if (!isInitialized)
            {
                OnError?.Invoke("Service not properly initialized. Please load data first.");
                return false;
            }

            if (!isEditMode)
            {
                OnError?.Invoke("Please enable edit mode first.");
                return false;
            }

            return true;
        }

        private bool SaveChangesWithDatabaseService()
        {
            ValidateDatabaseService();

            int rowsAffected = databaseService.UpdateDataTable(dataTable, dataAdapter);

            string successMessage = $"Changes saved successfully - {rowsAffected} rows affected";
            OnStatusUpdate?.Invoke(successMessage);
            logger.LogMessage("DATABASE", successMessage);

            MessageBox.Show($"Changes saved successfully!\n{rowsAffected} row(s) affected.",
                          "Save Successful",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
            return true;
        }

        private bool SaveChangesWithDirectConnection()
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string is not available for direct connection");
            }

            // Ensure commands have valid connections
            EnsureCommandConnections();

            int rowsAffected = 0;

            // Use a new connection for the update operation
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Update commands with the current connection
                UpdateCommandConnection(dataAdapter.InsertCommand, connection);
                UpdateCommandConnection(dataAdapter.UpdateCommand, connection);
                UpdateCommandConnection(dataAdapter.DeleteCommand, connection);

                rowsAffected = dataAdapter.Update(dataTable);
            }

            string successMessage = $"Changes saved successfully - {rowsAffected} rows affected";
            OnStatusUpdate?.Invoke(successMessage);
            logger.LogMessage("DATABASE", successMessage);

            MessageBox.Show($"Changes saved successfully!\n{rowsAffected} row(s) affected.",
                          "Save Successful",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
            return true;
        }

        public void DiscardChanges()
        {
            if (dataTable != null)
            {
                dataTable.RejectChanges();
                OnStatusUpdate?.Invoke("Changes discarded");
                logger.LogMessage("EDIT", "Pending changes discarded");
                MessageBox.Show("All pending changes have been discarded.",
                              "Changes Discarded",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        public bool HasUnsavedChanges()
        {
            return isEditMode && dataTable != null && dataTable.GetChanges() != null;
        }

        public bool CheckForUnsavedChanges()
        {
            if (HasUnsavedChanges())
            {
                var result = MessageBox.Show("You have unsaved changes. Do you want to save before continuing?",
                                           "Unsaved Changes",
                                           MessageBoxButtons.YesNoCancel,
                                           MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    logger.LogMessage("EDIT", "User chose to save unsaved changes");
                    return !SaveChangesToDatabase(); // Return true if save failed
                }
                else if (result == DialogResult.Cancel)
                {
                    logger.LogMessage("EDIT", "User cancelled operation due to unsaved changes");
                    return true; // Return true to cancel the operation
                }
                else
                {
                    logger.LogMessage("EDIT", "User chose to discard unsaved changes");
                }
            }
            return false;
        }

        private void EnsureCommandConnections()
        {
            if (dataAdapter.InsertCommand == null ||
                dataAdapter.UpdateCommand == null ||
                dataAdapter.DeleteCommand == null)
            {
                // Recreate command builder if needed
                var commandBuilder = new SqlCommandBuilder(dataAdapter);
                dataAdapter.InsertCommand = commandBuilder.GetInsertCommand();
                dataAdapter.UpdateCommand = commandBuilder.GetUpdateCommand();
                dataAdapter.DeleteCommand = commandBuilder.GetDeleteCommand();

                logger.LogMessage("DATABASE", "Command connections reinitialized");
            }
        }

        private void UpdateCommandConnection(SqlCommand command, SqlConnection connection)
        {
            if (command != null)
            {
                command.Connection = connection;
            }
        }

        // Helper method to create data adapter using DatabaseService
        public SqlDataAdapter CreateDataAdapter(string query)
        {
            if (databaseService != null)
            {
                return databaseService.CreateDataAdapter(query);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Connection string is not available for creating data adapter");
                }
                return new SqlDataAdapter(query, connectionString);
            }
        }

        // Helper method to create table data adapter using DatabaseService
        public SqlDataAdapter CreateTableDataAdapter(string tableName)
        {
            if (databaseService != null)
            {
                return databaseService.CreateTableDataAdapter(tableName);
            }
            else
            {
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    throw new InvalidOperationException("Connection string is not available for creating table data adapter");
                }
                string query = $"SELECT * FROM {tableName}";
                return new SqlDataAdapter(query, connectionString);
            }
        }

        public LoggingService Logger
        {
            get => logger;
            set => logger = value;
        }
    }
}