using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using InventorySystem.Services;

namespace InventorySystem.Services
{
    public class AuthService : IDisposable
    {
        private DatabaseService databaseService;
        private LoggingService loggingService;
        private bool disposed = false;

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
                    MessageBox.Show("Current password is incorrect.", "Password Change Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Update to new password
                string updateQuery = "UPDATE Users SET Password = @NewPassword WHERE Username = @Username";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NewPassword", newPassword);
                        command.Parameters.AddWithValue("@Username", username);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            loggingService?.LogMessage("USER", $"Password changed successfully for user: {username}");
                            return true;
                        }
                        else
                        {
                            loggingService?.LogMessage("ERROR", $"Password change failed - no rows affected for user: {username}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"ChangeUserPassword error: {ex.Message}");
                MessageBox.Show($"Failed to change password: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
                        command.Parameters.AddWithValue("@Password", currentPassword);

                        int count = Convert.ToInt32(command.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"VerifyCurrentPassword error: {ex.Message}");
                MessageBox.Show($"Failed to verify current password: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
                    MessageBox.Show($"User '{username}' already exists.", "User Creation Failed",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                string createQuery = "INSERT INTO Users (Username, Password, Role) VALUES (@Username, @Password, @Role)";

                using (SqlConnection connection = databaseService.GetConnection())
                {
                    connection.Open();

                    using (SqlCommand command = new SqlCommand(createQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);
                        command.Parameters.AddWithValue("@Password", password);
                        command.Parameters.AddWithValue("@Role", role);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            loggingService?.LogMessage("USER", $"User created successfully: {username} (Role: {role})");
                            return true;
                        }
                        else
                        {
                            loggingService?.LogMessage("ERROR", $"User creation failed - no rows affected: {username}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"CreateUser error: {ex.Message}");
                MessageBox.Show($"Failed to create user: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
                    MessageBox.Show("You cannot delete your own account while logged in.", "Deletion Prevented",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                        if (rowsAffected > 0)
                        {
                            loggingService?.LogMessage("USER", $"User deleted successfully: {username}");
                            return true;
                        }
                        else
                        {
                            loggingService?.LogMessage("ERROR", $"User deletion failed - user not found: {username}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"DeleteUser error: {ex.Message}");
                MessageBox.Show($"Failed to delete user: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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

                        if (rowsAffected > 0)
                        {
                            loggingService?.LogMessage("USER", $"User role updated: {username} -> {newRole}");
                            return true;
                        }
                        else
                        {
                            loggingService?.LogMessage("ERROR", $"User role update failed - user not found: {username}");
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"UpdateUserRole error: {ex.Message}");
                MessageBox.Show($"Failed to update user role: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
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
            }
            catch (Exception ex)
            {
                loggingService?.LogMessage("ERROR", $"LogUserLogout error: {ex.Message}");
            }
        }

        // IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    databaseService?.Dispose();
                    loggingService?.Dispose();

                    // Set to null to prevent future access
                    databaseService = null;
                    loggingService = null;
                }

                disposed = true;
            }
        }

        // Finalizer - only if you have unmanaged resources
        ~AuthService()
        {
            Dispose(false);
        }
    }
}