using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using InventorySystem.Services;

namespace InventorySystem.Services
{
    public class AuthService
    {
        private DatabaseService databaseService;
        private LoggingService loggingService;

        public AuthService(DatabaseService databaseService, LoggingService loggingService)
        {
            this.databaseService = databaseService ?? throw new ArgumentNullException(nameof(databaseService));
            this.loggingService = loggingService ?? throw new ArgumentNullException(nameof(loggingService));
        }

        public bool ChangeUserPassword(string username, string currentPassword, string newPassword)
        {
            try
            {
                // First verify the current password
                if (!VerifyCurrentPassword(username, currentPassword))
                {
                    return false;
                }

                // Update to new password
                string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NewPassword", newPassword); // In production, hash this password
                        command.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"ChangeUserPassword error: {ex.Message}");
                throw new Exception($"Failed to change password: {ex.Message}", ex);
            }
        }

        public bool VerifyCurrentPassword(string username, string currentPassword)
        {
            try
            {
                string verifyQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(verifyQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", currentPassword); // In production, compare hashed passwords

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"VerifyCurrentPassword error: {ex.Message}");
                throw new Exception($"Failed to verify current password: {ex.Message}", ex);
            }
        }

        public bool ValidateUserCredentials(string username, string password)
        {
            try
            {
                string validateQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username AND Password = @Password";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(validateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"ValidateUserCredentials error: {ex.Message}");
                throw new Exception($"Failed to validate user credentials: {ex.Message}", ex);
            }
        }

        public string GetUserRole(string username)
        {
            try
            {
                string roleQuery = "SELECT Role FROM Users WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(roleQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        object result = command.ExecuteScalar();
                        return result?.ToString() ?? "Guest";
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"GetUserRole error: {ex.Message}");
                return "Guest"; // Default to Guest role on error
            }
        }

        public bool CreateUser(string username, string password, string role)
        {
            try
            {
                // Check if user already exists
                if (UserExists(username))
                {
                    loggingService?.LogMessage("WARNING", $"User creation failed - user already exists: {username}");
                    return false;
                }

                string createQuery = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(createQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password); // In production, hash this password
                        command.Parameters.AddWithValue("@Role", role);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"CreateUser error: {ex.Message}");
                throw new Exception($"Failed to create user: {ex.Message}", ex);
            }
        }

        public bool DeleteUser(string username)
        {
            try
            {
                // Prevent deletion of current user if needed
                if (username.Equals(LoginFrm.CurrentUsername, StringComparison.OrdinalIgnoreCase))
                {
                    loggingService?.LogMessage("WARNING", $"User deletion prevented - cannot delete current user: {username}");
                    return false;
                }

                string deleteQuery = "DELETE FROM Users WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"DeleteUser error: {ex.Message}");
                throw new Exception($"Failed to delete user: {ex.Message}", ex);
            }
        }

        public bool UpdateUserRole(string username, string newRole)
        {
            try
            {
                string updateQuery = "UPDATE Users SET Role = @Role WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Role", newRole);
                        command.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = command.ExecuteNonQuery();
                        return rowsAffected > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"UpdateUserRole error: {ex.Message}");
                throw new Exception($"Failed to update user role: {ex.Message}", ex);
            }
        }

        public bool UserExists(string username)
        {
            try
            {
                string existsQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(existsQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"UserExists error: {ex.Message}");
                throw new Exception($"Failed to check if user exists: {ex.Message}", ex);
            }
        }

        public void LogUserLogin(string username, string role)
        {
            try
            {
                loggingService?.LogMessage("USER", $"User logged in: {username} (Role: {role})");
<<<<<<< HEAD
=======

                // Optional: Log to database audit table
                LogToAuditTable(username, "LOGIN", $"User {username} logged in with role {role}");
>>>>>>> 89eb856876213f20bfc10b19e31b453b529961f6
            }
            catch (Exception ex)
            {
                // Don't throw exception for logging failures, but log them
                loggingService?.LogMessage("ERROR", $"LogUserLogin error: {ex.Message}");
            }
        }

        public void LogUserLogout(string username)
        {
            try
            {
                loggingService?.LogMessage("USER", $"User logged out: {username}");
<<<<<<< HEAD
=======

                // Optional: Log to database audit table
                LogToAuditTable(username, "LOGOUT", $"User {username} logged out");
>>>>>>> 89eb856876213f20bfc10b19e31b453b529961f6
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"LogUserLogout error: {ex.Message}");
            }
        }
<<<<<<< HEAD
=======

        private void LogToAuditTable(string username, string action, string description)
        {
            try
            {
                // Optional: Implement database audit logging
                string auditQuery = @"INSERT INTO UserAuditLog (Username, Action, Description, Timestamp) 
                                     VALUES (@Username, @Action, @Description, @Timestamp)";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(auditQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Action", action);
                        command.Parameters.AddWithValue("@Description", description);
                        command.Parameters.AddWithValue("@Timestamp", DateTime.Now);

                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Silent fail for audit logging - don't disrupt main functionality
                loggingService?.LogMessage("WARNING", $"Audit logging failed: {ex.Message}");
            }
        }
>>>>>>> 89eb856876213f20bfc10b19e31b453b529961f6
    }
}