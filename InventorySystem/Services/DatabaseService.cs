using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;

namespace InventorySystem.Services
{
    public class DatabaseService : IDisposable
    {
        private string _connectionString;
        private SqlConnection _connection;
        private bool _disposed = false;
        private readonly object _lockObject = new object();
        private LoggingService _logger;

        // Configuration constants from app.config
        private const string DEFAULT_CONNECTION_STRING_NAME = "InventoryManagementConnection";
        private const string SUPPLIES_TABLE_KEY = "SuppliesTable";
        private const string ASSETS_TABLE_KEY = "AssetsTable";

        public DatabaseService(string connectionString = null, LoggingService loggingService = null)
        {
            _logger = loggingService;
            InitializeConnection(connectionString);
        }

        #region Connection Management

        private void InitializeConnection(string connectionString = null)
        {
            lock (_lockObject)
            {
                try
                {
                    _connectionString = connectionString ?? GetValidatedConnectionString();

                    if (string.IsNullOrWhiteSpace(_connectionString))
                    {
                        throw new InvalidOperationException("Connection string cannot be null or empty after initialization");
                    }

                    // Test connection immediately to validate
                    TestConnection();
                    _logger?.LogMessage("DATABASE", "Database service initialized successfully with configuration");
                }
                catch (Exception ex)
                {
                    _logger?.LogMessage("ERROR", $"DatabaseService initialization failed: {ex.Message}");
                    throw new InvalidOperationException($"DatabaseService initialization failed: {ex.Message}", ex);
                }
            }
        }

        public SqlConnection GetConnection()
        {
            if (_connection == null)
            {
                _connection = new SqlConnection(_connectionString);
            }

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            return _connection;
        }

        public SqlConnection CreateNewConnection()
        {
            var connection = new SqlConnection(_connectionString);
            connection.Open();
            return connection;
        }

        public void CloseConnection()
        {
            if (_connection?.State == ConnectionState.Open)
            {
                _connection.Close();
                _logger?.LogMessage("DATABASE", "Database connection closed");
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (var connection = CreateNewConnection())
                {
                    bool isConnected = connection.State == ConnectionState.Open;
                    _logger?.LogMessage("DATABASE", $"Database connection test: {(isConnected ? "SUCCESS" : "FAILED")}");
                    return isConnected;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Database connection test failed: {ex.Message}");
                throw new Exception($"Database connection test failed: {ex.Message}", ex);
            }
        }

        public bool TestConnectionWithTimeout(int timeoutSeconds = 30)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new SqlCommand("SELECT 1", connection))
                    {
                        command.CommandTimeout = timeoutSeconds;
                        bool result = Convert.ToInt32(command.ExecuteScalar()) == 1;
                        _logger?.LogMessage("DATABASE", $"Database connection test with timeout: {(result ? "SUCCESS" : "FAILED")}");
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Database connection test with timeout failed: {ex.Message}");
                throw new Exception($"Database connection test with timeout failed: {ex.Message}", ex);
            }
        }

        #endregion

        #region Query Operations

        public DataTable ExecuteQuery(string query, SqlParameter[] parameters = null, int timeoutSeconds = 30)
        {
            ValidateConnectionString();

            try
            {
                _logger?.LogMessage("DATABASE", $"Executing query: {GetSafeQueryLog(query)}");

                using (var connection = CreateNewConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        _logger?.LogMessage("DATABASE", $"Query executed successfully - {dataTable.Rows.Count} rows returned");
                        return dataTable;
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                _logger?.LogMessage("ERROR", $"SQL error executing query: {sqlEx.Message}");
                throw new Exception($"SQL error executing query: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Error executing query: {ex.Message}");
                throw new Exception($"Error executing query: {ex.Message}", ex);
            }
        }

        public DataTable GetTableData(string tableName, int timeoutSeconds = 30)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty");

            ValidateTableExists(tableName);

            string query = $"SELECT * FROM {tableName}";
            _logger?.LogMessage("DATA", $"Loading table data: {tableName}");
            return ExecuteQuery(query, null, timeoutSeconds);
        }

        // Convenience method for configured tables
        public DataTable GetSuppliesTableData(int timeoutSeconds = 30)
        {
            string tableName = GetSuppliesTableName();
            return GetTableData(tableName, timeoutSeconds);
        }

        public DataTable GetAssetsTableData(int timeoutSeconds = 30)
        {
            string tableName = GetAssetsTableName();
            return GetTableData(tableName, timeoutSeconds);
        }

        public DataRow GetRecordById(string tableName, string idColumn, int id, int timeoutSeconds = 30)
        {
            ValidateTableExists(tableName);

            string query = $"SELECT * FROM {tableName} WHERE {idColumn} = @Id";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@Id", id)
            };

            _logger?.LogMessage("DATABASE", $"Getting record by ID: {tableName}.{idColumn} = {id}");
            var dataTable = ExecuteQuery(query, parameters, timeoutSeconds);
            return dataTable.Rows.Count > 0 ? dataTable.Rows[0] : null;
        }

        #endregion

        #region Non-Query Operations

        public int ExecuteNonQuery(string commandText, SqlParameter[] parameters = null, int timeoutSeconds = 30)
        {
            ValidateConnectionString();

            try
            {
                _logger?.LogMessage("DATABASE", $"Executing non-query: {GetSafeQueryLog(commandText)}");

                using (var connection = CreateNewConnection())
                using (var command = new SqlCommand(commandText, connection))
                {
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    int rowsAffected = command.ExecuteNonQuery();
                    _logger?.LogMessage("DATABASE", $"Non-query executed successfully - {rowsAffected} rows affected");
                    return rowsAffected;
                }
            }
            catch (SqlException sqlEx)
            {
                _logger?.LogMessage("ERROR", $"SQL error executing command: {sqlEx.Message}");
                throw new Exception($"SQL error executing command: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Error executing command: {ex.Message}");
                throw new Exception($"Error executing command: {ex.Message}", ex);
            }
        }

        public int InsertRecord(string tableName, Dictionary<string, object> values, int timeoutSeconds = 30)
        {
            ValidateTableExists(tableName);

            if (values == null || values.Count == 0)
                throw new ArgumentException("Values cannot be null or empty");

            var columns = string.Join(", ", values.Keys);
            var parameters = string.Join(", ", values.Keys.Select(k => "@" + k));

            string commandText = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            var sqlParameters = values.Select(v => new SqlParameter("@" + v.Key, v.Value ?? DBNull.Value)).ToArray();

            _logger?.LogMessage("DATABASE", $"Inserting record into {tableName}");
            return ExecuteNonQuery(commandText, sqlParameters, timeoutSeconds);
        }

        // Convenience method for configured tables
        public int InsertSuppliesRecord(Dictionary<string, object> values, int timeoutSeconds = 30)
        {
            return InsertRecord(GetSuppliesTableName(), values, timeoutSeconds);
        }

        public int InsertAssetsRecord(Dictionary<string, object> values, int timeoutSeconds = 30)
        {
            return InsertRecord(GetAssetsTableName(), values, timeoutSeconds);
        }

        public int UpdateRecord(string tableName, Dictionary<string, object> values, string whereCondition, SqlParameter[] whereParameters = null, int timeoutSeconds = 30)
        {
            ValidateTableExists(tableName);

            if (values == null || values.Count == 0)
                throw new ArgumentException("Values cannot be null or empty");
            if (string.IsNullOrWhiteSpace(whereCondition))
                throw new ArgumentException("Where condition cannot be null or empty");

            var setClause = string.Join(", ", values.Keys.Select(k => $"{k} = @{k}"));
            string commandText = $"UPDATE {tableName} SET {setClause} WHERE {whereCondition}";

            var setParameters = values.Select(v => new SqlParameter("@" + v.Key, v.Value ?? DBNull.Value)).ToArray();
            var allParameters = setParameters.Concat(whereParameters ?? new SqlParameter[0]).ToArray();

            _logger?.LogMessage("DATABASE", $"Updating record in {tableName}");
            return ExecuteNonQuery(commandText, allParameters, timeoutSeconds);
        }

        public int DeleteRecord(string tableName, string whereCondition, SqlParameter[] parameters = null, int timeoutSeconds = 30)
        {
            ValidateTableExists(tableName);

            if (string.IsNullOrWhiteSpace(whereCondition))
                throw new ArgumentException("Where condition cannot be null or empty");

            string commandText = $"DELETE FROM {tableName} WHERE {whereCondition}";
            _logger?.LogMessage("DATABASE", $"Deleting record from {tableName}");
            return ExecuteNonQuery(commandText, parameters, timeoutSeconds);
        }

        #endregion

        #region Data Adapter Operations

        public SqlDataAdapter CreateDataAdapter(string query, int timeoutSeconds = 30)
        {
            ValidateConnectionString();

            var adapter = new SqlDataAdapter(query, _connectionString);

            // Configure command builder for automatic command generation
            var commandBuilder = new SqlCommandBuilder(adapter);

            // Set timeout for all commands
            if (adapter.SelectCommand != null)
                adapter.SelectCommand.CommandTimeout = timeoutSeconds;
            if (adapter.InsertCommand != null)
                adapter.InsertCommand.CommandTimeout = timeoutSeconds;
            if (adapter.UpdateCommand != null)
                adapter.UpdateCommand.CommandTimeout = timeoutSeconds;
            if (adapter.DeleteCommand != null)
                adapter.DeleteCommand.CommandTimeout = timeoutSeconds;

            _logger?.LogMessage("DATABASE", $"Data adapter created for query: {GetSafeQueryLog(query)}");
            return adapter;
        }

        public SqlDataAdapter CreateTableDataAdapter(string tableName, int timeoutSeconds = 30)
        {
            ValidateTableExists(tableName);

            string query = $"SELECT * FROM {tableName}";
            _logger?.LogMessage("DATABASE", $"Table data adapter created for: {tableName}");
            return CreateDataAdapter(query, timeoutSeconds);
        }

        // Convenience methods for configured tables
        public SqlDataAdapter CreateSuppliesDataAdapter(int timeoutSeconds = 30)
        {
            return CreateTableDataAdapter(GetSuppliesTableName(), timeoutSeconds);
        }

        public SqlDataAdapter CreateAssetsDataAdapter(int timeoutSeconds = 30)
        {
            return CreateTableDataAdapter(GetAssetsTableName(), timeoutSeconds);
        }

        public int UpdateDataTable(DataTable dataTable, SqlDataAdapter adapter)
        {
            try
            {
                _logger?.LogMessage("DATABASE", "Starting data table update...");
                int rowsAffected = adapter.Update(dataTable);
                _logger?.LogMessage("DATABASE", $"Data table update completed - {rowsAffected} rows affected");
                return rowsAffected;
            }
            catch (SqlException sqlEx)
            {
                _logger?.LogMessage("ERROR", $"SQL error updating data: {sqlEx.Message}");
                throw new Exception($"SQL error updating data: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Error updating data: {ex.Message}");
                throw new Exception($"Error updating data: {ex.Message}", ex);
            }
        }

        #endregion

        #region Configuration Methods

        private string GetValidatedConnectionString()
        {
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings[DEFAULT_CONNECTION_STRING_NAME]?.ConnectionString;

                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    _logger?.LogMessage("CONFIG", $"Connection string '{DEFAULT_CONNECTION_STRING_NAME}' not found in configuration");
                    throw new InvalidOperationException($"Connection string '{DEFAULT_CONNECTION_STRING_NAME}' not found in configuration");
                }

                _logger?.LogMessage("CONFIG", "Using connection string from configuration");
                return connectionString;
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Failed to get connection string: {ex.Message}");
                throw new InvalidOperationException($"Failed to get connection string: {ex.Message}", ex);
            }
        }

        public string GetSuppliesTableName()
        {
            return GetAppSetting(SUPPLIES_TABLE_KEY, "SuppliesInventory");
        }

        public string GetAssetsTableName()
        {
            return GetAppSetting(ASSETS_TABLE_KEY, "AssetsInventory");
        }

        public string GetApplicationName()
        {
            return GetAppSetting("ApplicationName", "Inventory Management System");
        }

        private string GetAppSetting(string key, string defaultValue = "")
        {
            try
            {
                var value = ConfigurationManager.AppSettings[key];
                if (string.IsNullOrWhiteSpace(value))
                {
                    _logger?.LogMessage("CONFIG", $"AppSetting '{key}' not found, using default: {defaultValue}");
                    return defaultValue;
                }
                return value;
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Error reading AppSetting '{key}': {ex.Message}");
                return defaultValue;
            }
        }

        #endregion

        #region Validation Methods

        private void ValidateConnectionString()
        {
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                _logger?.LogMessage("ERROR", "ConnectionString validation failed - not initialized");
                throw new InvalidOperationException("ConnectionString has not been initialized. DatabaseService is not properly configured.");
            }
        }

        private void ValidateTableExists(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                throw new ArgumentException("Table name cannot be null or empty");

            if (!TableExists(tableName))
            {
                _logger?.LogMessage("ERROR", $"Table validation failed: {tableName} does not exist");
                throw new ArgumentException($"Table '{tableName}' does not exist in the database.");
            }
        }

        #endregion

        #region Utility Methods

        public List<string> GetTableNames()
        {
            var tableNames = new List<string>();
            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";

            _logger?.LogMessage("DATABASE", "Retrieving table names from database");
            var dataTable = ExecuteQuery(query);
            foreach (DataRow row in dataTable.Rows)
            {
                tableNames.Add(row["TABLE_NAME"].ToString());
            }

            _logger?.LogMessage("DATABASE", $"Retrieved {tableNames.Count} table names");
            return tableNames;
        }

        public bool TableExists(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
                return false;

            string query = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @TableName";
            var parameters = new SqlParameter[]
            {
                new SqlParameter("@TableName", tableName)
            };

            try
            {
                var result = ExecuteScalar(query, parameters);
                bool exists = Convert.ToInt32(result) > 0;
                _logger?.LogMessage("DATABASE", $"Table existence check: {tableName} = {exists}");
                return exists;
            }
            catch
            {
                _logger?.LogMessage("ERROR", $"Table existence check failed for: {tableName}");
                return false;
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null, int timeoutSeconds = 30)
        {
            ValidateConnectionString();

            try
            {
                _logger?.LogMessage("DATABASE", $"Executing scalar query: {GetSafeQueryLog(query)}");

                using (var connection = CreateNewConnection())
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandTimeout = timeoutSeconds;

                    if (parameters != null)
                    {
                        command.Parameters.AddRange(parameters);
                    }

                    object result = command.ExecuteScalar();
                    _logger?.LogMessage("DATABASE", "Scalar query executed successfully");
                    return result;
                }
            }
            catch (SqlException sqlEx)
            {
                _logger?.LogMessage("ERROR", $"SQL error executing scalar: {sqlEx.Message}");
                throw new Exception($"SQL error executing scalar: {sqlEx.Message}", sqlEx);
            }
            catch (Exception ex)
            {
                _logger?.LogMessage("ERROR", $"Error executing scalar: {ex.Message}");
                throw new Exception($"Error executing scalar: {ex.Message}", ex);
            }
        }

        private string GetSafeQueryLog(string query)
        {
            // For security, truncate long queries and remove sensitive data in logs
            if (string.IsNullOrEmpty(query)) return query;

            string safeQuery = query.Length > 200 ? query.Substring(0, 200) + "..." : query;
            return safeQuery.Replace(Environment.NewLine, " ");
        }

        #endregion

        #region Properties

        public string ConnectionString
        {
            get
            {
                ValidateConnectionString();
                return _connectionString;
            }
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    _connectionString = value;
                    // Close existing connection if any
                    CloseConnection();
                    _connection = null;

                    // Test new connection string
                    TestConnection();
                    _logger?.LogMessage("DATABASE", "Connection string updated and validated");
                }
            }
        }

        public bool IsConnectionValid
        {
            get
            {
                try
                {
                    return TestConnection();
                }
                catch
                {
                    return false;
                }
            }
        }

        public LoggingService Logger
        {
            get => _logger;
            set => _logger = value;
        }

        #endregion

        #region IDisposable Implementation

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    CloseConnection();
                    _connection?.Dispose();
                    _logger?.LogMessage("DATABASE", "Database service disposed");
                }
                _disposed = true;
            }
        }

        ~DatabaseService()
        {
            Dispose(false);
        }

        #endregion
    }
}