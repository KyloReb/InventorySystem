using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using InventorySystem.Services;

namespace InventorySystem.Services
{
    public class DataLoadingService
    {
        private readonly DatabaseService _databaseService;
        private readonly LoggingService _loggingService;

        public DataLoadingService(DatabaseService databaseService, LoggingService loggingService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public DataTable LoadTableData(string tableName, string tableType, Action<string, int> progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke($"Loading {tableType.ToLower()} data...", 40);

                // Validate database service first
                if (_databaseService == null)
                {
                    throw new InvalidOperationException("Database service is not initialized");
                }

                if (!_databaseService.IsConnectionValid)
                {
                    throw new InvalidOperationException("Database connection is not valid");
                }

                // Validate table exists
                if (!_databaseService.TableExists(tableName))
                {
                    throw new InvalidOperationException($"Table '{tableName}' does not exist in the database");
                }

                // Use DatabaseService to get table data
                DataTable dataTable = _databaseService.GetTableData(tableName);

                string successMessage = $"{tableType} data loaded successfully - {dataTable.Rows.Count} records";
                progressCallback?.Invoke(successMessage, 100);
                _loggingService.LogMessage("DATA", successMessage);

                return dataTable;
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Database error loading {tableType} data: {sqlEx.Message}";
                _loggingService.LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, sqlEx);
            }
            catch (InvalidOperationException ioEx)
            {
                string errorMessage = $"Configuration error loading {tableType} data: {ioEx.Message}";
                _loggingService.LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, ioEx);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error loading {tableType} data from table '{tableName}': {ex.Message}";
                _loggingService.LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }

        public SqlDataAdapter CreateDataAdapter(string tableName)
        {
            try
            {
                return _databaseService.CreateTableDataAdapter(tableName);
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Error creating data adapter for table '{tableName}': {ex.Message}");
                throw new Exception($"Error creating data adapter: {ex.Message}", ex);
            }
        }

        public void RefreshData(string currentTable, string suppliesTable, string assetsTable, Action<string, int> progressCallback = null)
        {
            try
            {
                progressCallback?.Invoke("Testing database connection...", 15);
                _loggingService.LogMessage("DATA", "Refreshing data...");

                _databaseService.TestConnection();

                progressCallback?.Invoke("Connection successful...", 30);
                progressCallback?.Invoke("Refreshing data...", 50);

                string tableName = currentTable == "Assets" ? assetsTable : suppliesTable;
                string tableType = currentTable == "Assets" ? "Assets" : "Supplies";

                // This would typically return data, but for refresh we just validate
                progressCallback?.Invoke($"{tableType} data refreshed successfully", 100);
                _loggingService.LogMessage("DATA", "Data refresh completed successfully");
            }
            catch (SqlException sqlEx)
            {
                string errorMessage = $"Connection failed: {sqlEx.Message}";
                _loggingService.LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, sqlEx);
            }
            catch (Exception ex)
            {
                string errorMessage = $"Refresh failed: {ex.Message}";
                _loggingService.LogMessage("ERROR", errorMessage);
                throw new Exception(errorMessage, ex);
            }
        }

        public bool ValidateTableExists(string tableName)
        {
            try
            {
                return _databaseService.TableExists(tableName);
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Error validating table existence '{tableName}': {ex.Message}");
                return false;
            }
        }

        public int GetRecordCount(string tableName)
        {
            try
            {
                var dataTable = _databaseService.GetTableData(tableName);
                return dataTable.Rows.Count;
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Error getting record count for '{tableName}': {ex.Message}");
                return -1;
            }
        }
    }
}