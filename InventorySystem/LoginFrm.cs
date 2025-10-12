using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using InventorySystem.Services;
using System.Configuration;

namespace InventorySystem
{
    public partial class LoginFrm : Form
    {
        private DatabaseService _databaseService;
        private LoggingService _loggingService;
        private bool isLoading = false;

        public static string CurrentUsername { get; private set; }
        public static string CurrentRole { get; private set; }

        public LoginFrm()
        {
            InitializeComponent();
            InitializeServices();
            ConfigurePasswordField();
            ConfigureEnterKeyBehavior();
            ConfigureProgressBar();
        }

        private void InitializeServices()
        {
            try
            {
                // Initialize logging service first
                _loggingService = new LoggingService();

                // Initialize database service with connection string from configuration
                string connectionString = GetConnectionString();
                _databaseService = new DatabaseService(connectionString, _loggingService);

                // Test connection on startup
                if (!_databaseService.TestConnection())
                {
                    MessageBox.Show("Database connection failed. Please check configuration.",
                        "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Service initialization failed: {ex.Message}",
                    "Initialization Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Continue without services - will handle errors in login methods
            }
        }

        private string GetConnectionString()
        {
            // Try to get from app.config first, fall back to hardcoded if not found
            try
            {
                var connectionString = ConfigurationManager.ConnectionStrings["InventoryManagementConnection"]?.ConnectionString;
                if (!string.IsNullOrEmpty(connectionString))
                {
                    return connectionString;
                }
            }
            catch
            {
                // Fall through to hardcoded connection string
            }

            // Fallback connection string (same as your original)
            return "Data Source=DESKTOP-TK48S3J;Initial Catalog=InventoryManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        }

        private void ConfigurePasswordField()
        {
            // Set password character to mask input
            passwordTxtBox.UseSystemPasswordChar = true;
            passwordTxtBox.PasswordChar = '•'; // Bullet character for masking
        }

        private void ConfigureEnterKeyBehavior()
        {
            // Set AcceptButton for form to trigger login when Enter is pressed
            this.AcceptButton = loginBtn;

            // Alternatively, you can handle KeyDown events for specific controls
            userTxtBox.KeyDown += TextBox_KeyDown;
            passwordTxtBox.KeyDown += TextBox_KeyDown;
        }

        private void ConfigureProgressBar()
        {
            // Configure progress bar to be properly sized and centered
            toolStripProgressBar1.Alignment = ToolStripItemAlignment.Right;
            toolStripProgressBar1.Margin = new Padding(50, 3, 50, 3);
            toolStripProgressBar1.Width = 600; // Adjust based on your form size
            toolStripProgressBar1.Visible = false;
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !isLoading)
            {
                // Trigger login button click
                loginBtn.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent the beep sound
            }
        }

        private async void loginBtn_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            await PerformLoginAction(async () =>
            {
                ClearErrors();

                string username = userTxtBox.Text.Trim();
                string password = passwordTxtBox.Text;

                if (string.IsNullOrEmpty(username))
                {
                    ShowUsernameError("Username is required");
                    userTxtBox.Focus();
                    return false;
                }

                if (string.IsNullOrEmpty(password))
                {
                    ShowPasswordError("Password is required");
                    passwordTxtBox.Focus();
                    return false;
                }

                try
                {
                    // Use DatabaseService instead of raw SQL connection
                    if (_databaseService == null)
                    {
                        throw new InvalidOperationException("Database service is not available");
                    }

                    string query = "SELECT UserId, Username, Password, Role FROM Users WHERE Username = @Username";
                    var parameters = new SqlParameter[]
                    {
                        new SqlParameter("@Username", username)
                    };

                    // Execute query using DatabaseService
                    DataTable result = await Task.Run(() => _databaseService.ExecuteQuery(query, parameters));

                    if (result.Rows.Count > 0)
                    {
                        DataRow userRow = result.Rows[0];
                        string storedPassword = userRow["Password"].ToString();

                        if (password == storedPassword)
                        {
                            CurrentUsername = userRow["Username"].ToString();
                            CurrentRole = userRow["Role"].ToString();

                            // Log successful login
                            _loggingService?.LogMessage("AUTH", $"User {username} logged in successfully as {CurrentRole}");

                            // Simulate loading time
                            await Task.Delay(1000);

                            ViewFrm viewForm = new ViewFrm();
                            viewForm.Show();
                            this.Hide();
                            return true;
                        }
                        else
                        {
                            _loggingService?.LogMessage("AUTH", $"Failed login attempt for user {username} - invalid password");
                            ShowPasswordError("Invalid password");
                            passwordTxtBox.Focus();
                            passwordTxtBox.SelectAll();
                            return false;
                        }
                    }
                    else
                    {
                        _loggingService?.LogMessage("AUTH", $"Failed login attempt - username not found: {username}");
                        ShowUsernameError("Username not found");
                        userTxtBox.Focus();
                        userTxtBox.SelectAll();
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    _loggingService?.LogMessage("ERROR", $"Login error for user {username}: {ex.Message}");
                    MessageBox.Show($"Error during login: {ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            });
        }

        private async void viewerBtn_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            await PerformLoginAction(async () =>
            {
                CurrentUsername = "Guest";
                CurrentRole = "Guest";

                // Log guest access
                _loggingService?.LogMessage("AUTH", "Guest user accessed the system");

                // Simulate loading time
                await Task.Delay(800);

                ViewFrm viewForm = new ViewFrm();
                viewForm.Show();
                this.Hide();
                return true;
            });
        }

        private async Task PerformLoginAction(Func<Task<bool>> action)
        {
            if (isLoading) return;

            try
            {
                SetLoadingState(true);

                // Show progress bar
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                bool success = await action();

                if (!success)
                {
                    SetLoadingState(false);
                }
                // If successful, the form will be hidden so we don't need to reset loading state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                SetLoadingState(false);
            }
        }

        private void SetLoadingState(bool loading)
        {
            isLoading = loading;

            // Enable/disable controls
            loginBtn.Enabled = !loading;
            viewerBtn.Enabled = !loading;
            userTxtBox.Enabled = !loading;
            passwordTxtBox.Enabled = !loading;

            // Show/hide progress bar
            toolStripProgressBar1.Visible = loading;

            // Update button texts to show loading state
            if (loading)
            {
                loginBtn.Text = "Loading...";
                viewerBtn.Text = "Loading...";
            }
            else
            {
                loginBtn.Text = "Login";
                viewerBtn.Text = "Continue as viewer only";
            }

            // Refresh the form to ensure UI updates
            this.Refresh();
        }

        private void ShowUsernameError(string message)
        {
            // Remove existing error label if any
            Control existingError = this.Controls.Find("usernameErrorLabel", true).FirstOrDefault();
            if (existingError != null)
            {
                this.Controls.Remove(existingError);
                existingError.Dispose();
            }

            Label errorLabel = new Label();
            errorLabel.Name = "usernameErrorLabel";
            errorLabel.Text = message;
            errorLabel.ForeColor = Color.Red;
            errorLabel.Location = new Point(userTxtBox.Left, userTxtBox.Bottom + 2);
            errorLabel.Size = new Size(userTxtBox.Width, 20);
            errorLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular);
            this.Controls.Add(errorLabel);
            errorLabel.BringToFront();
        }

        private void ShowPasswordError(string message)
        {
            // Remove existing error label if any
            Control existingError = this.Controls.Find("passwordErrorLabel", true).FirstOrDefault();
            if (existingError != null)
            {
                this.Controls.Remove(existingError);
                existingError.Dispose();
            }

            Label errorLabel = new Label();
            errorLabel.Name = "passwordErrorLabel";
            errorLabel.Text = message;
            errorLabel.ForeColor = Color.Red;
            errorLabel.Location = new Point(passwordTxtBox.Left, passwordTxtBox.Bottom + 2);
            errorLabel.Size = new Size(passwordTxtBox.Width, 20);
            errorLabel.Font = new Font("Microsoft Sans Serif", 8F, FontStyle.Regular);
            this.Controls.Add(errorLabel);
            errorLabel.BringToFront();
        }

        private void ClearErrors()
        {
            // Remove username error label if exists
            Control usernameError = this.Controls.Find("usernameErrorLabel", true).FirstOrDefault();
            if (usernameError != null)
            {
                this.Controls.Remove(usernameError);
                usernameError.Dispose();
            }

            // Remove password error label if exists
            Control passwordError = this.Controls.Find("passwordErrorLabel", true).FirstOrDefault();
            if (passwordError != null)
            {
                this.Controls.Remove(passwordError);
                passwordError.Dispose();
            }
        }

        private void userTxtBox_TextChanged(object sender, EventArgs e)
        {
            ClearErrors();
        }

        private void passwordTxtBox_TextChanged(object sender, EventArgs e)
        {
            ClearErrors();
        }

        // Optional: Add a method to toggle password visibility
        private void TogglePasswordVisibility()
        {
            passwordTxtBox.UseSystemPasswordChar = !passwordTxtBox.UseSystemPasswordChar;
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Clean up resources
            if (isLoading)
            {
                SetLoadingState(false);
            }

            // Dispose services
            _databaseService?.Dispose();
            _loggingService?.Dispose();

            base.OnFormClosing(e);
        }
    }
}