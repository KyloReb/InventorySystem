using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;
using InventorySystem.Services;

namespace InventorySystem.Services
{
    public class DataLoadingService
    {
        private readonly DatabaseService _databaseService;
        private readonly LoggingService _loggingService;
        private readonly bool _isInitialized = false;

        public DataLoadingService(DatabaseService databaseService, LoggingService loggingService)
        {
            _databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            _loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
            _isInitialized = true;
        }

        public bool IsInitialized => _isInitialized;

        public DataTable LoadTableData(string tableName, string tableType, Action<string, int> progressCallback = null)
        {
            ValidateInitialization();

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
            ValidateInitialization();

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
            ValidateInitialization();

            try
            {
                progressCallback?.Invoke("Testing database connection...", 15);
                _loggingService.LogMessage("DATA", "Refreshing data...");

                _databaseService.TestConnection();

                progressCallback?.Invoke("Connection successful...", 30);
                progressCallback?.Invoke("Refreshing data...", 50);

                string tableName = currentTable == "Assets" ? assetsTable : suppliesTable;
                string tableType = currentTable == "Assets" ? "Assets" : "Supplies";

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
            ValidateInitialization();

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
            ValidateInitialization();

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

        public bool TestDatabaseConnection()
        {
            ValidateInitialization();

            try
            {
                _loggingService.LogMessage("DATABASE", "Testing database connection...");
                _databaseService.TestConnection();
                _loggingService.LogMessage("DATABASE", "Database connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Database connection test failed: {ex.Message}");
                throw new Exception($"Database connection test failed: {ex.Message}", ex);
            }
        }

        public bool ValidateConfiguration()
        {
            ValidateInitialization();

            try
            {
                string suppliesTable = ConfigurationManager.AppSettings["SuppliesTable"];
                string assetsTable = ConfigurationManager.AppSettings["AssetsTable"];

                if (string.IsNullOrEmpty(suppliesTable))
                {
                    _loggingService.LogMessage("CONFIG", "SuppliesTable not found in config, using default");
                }

                if (string.IsNullOrEmpty(assetsTable))
                {
                    _loggingService.LogMessage("CONFIG", "AssetsTable not found in config, using default");
                }

                return true;
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Configuration validation failed: {ex.Message}");
                return false;
            }
        }

        public string GetSuppliesTableName()
        {
            ValidateInitialization();
            return ConfigurationManager.AppSettings["SuppliesTable"] ?? "SuppliesInventory";
        }

        public string GetAssetsTableName()
        {
            ValidateInitialization();
            return ConfigurationManager.AppSettings["AssetsTable"] ?? "AssetsInventory";
        }

        public DataTable PerformSearch(DataTable originalDataTable, string searchText)
        {
            ValidateInitialization();

            if (originalDataTable == null || string.IsNullOrWhiteSpace(searchText) || searchText == "Search...")
            {
                return originalDataTable?.Copy() ?? new DataTable();
            }

            try
            {
                searchText = searchText.ToLower();
                DataTable filteredTable = originalDataTable.Clone();

                var filteredRows = originalDataTable.AsEnumerable()
                    .Where(row => row.ItemArray.Any(field =>
                        field != null && field.ToString().ToLower().Contains(searchText)))
                    .ToArray();

                foreach (DataRow row in filteredRows)
                {
                    filteredTable.ImportRow(row);
                }

                _loggingService.LogMessage("SEARCH", $"Search performed: '{searchText}' - {filteredTable.Rows.Count} results found");
                return filteredTable;
            }
            catch (Exception ex)
            {
                _loggingService.LogMessage("ERROR", $"Search error: {ex.Message}");
                throw new Exception($"Search error: {ex.Message}", ex);
            }
        }

        public DataTable ClearSearch(DataTable originalDataTable)
        {
            ValidateInitialization();
            return originalDataTable?.Copy() ?? new DataTable();
        }

        private void ValidateInitialization()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("DataLoadingService is not properly initialized");
            }
        }
    }
}