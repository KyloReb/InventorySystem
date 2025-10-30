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
        private AuthService _authService;
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
            ConfigureForgotPasswordLabel();
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

                // Initialize auth service
                _authService = new AuthService(_databaseService, _loggingService);

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

        private void ConfigureForgotPasswordLabel()
        {
            // Ensure the label has hand cursor to indicate it's clickable
            lblForgotPassword.Cursor = Cursors.Hand;
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
                    // Use AuthService for authentication
                    if (_authService == null)
                    {
                        throw new InvalidOperationException("Authentication service is not available");
                    }

                    // Validate user credentials using AuthService - run on background thread
                    bool isValidUser = await Task.Run(() => _authService.ValidateUserCredentials(username, password));

                    if (isValidUser)
                    {
                        // Get user role using AuthService - run on background thread
                        string userRole = await Task.Run(() => _authService.GetUserRole(username));

                        CurrentUsername = username;
                        CurrentRole = userRole;

                        // Log successful login using AuthService
                        await Task.Run(() => _authService.LogUserLogin(username, userRole));

                        // Simulate loading time
                        await Task.Delay(1000);

                        ViewFrm viewForm = new ViewFrm();
                        viewForm.Show();
                        this.Hide();
                        return true;
                    }
                    else
                    {
                        ShowPasswordError("Invalid username or password");
                        passwordTxtBox.Focus();
                        passwordTxtBox.SelectAll();
                        return false;
                    }
                }
                catch (Exception ex)
                {
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

                // Log guest access using AuthService - run on background thread
                if (_authService != null)
                {
                    await Task.Run(() => _authService.LogUserLogin("Guest", "Guest"));
                }

                // Simulate loading time
                await Task.Delay(800);

                ViewFrm viewForm = new ViewFrm();
                viewForm.Show();
                this.Hide();
                return true;
            });
        }

        private async void lblForgotPassword_Click(object sender, EventArgs e)
        {
            if (isLoading) return;

            await ShowForgotPasswordDialogAsync();
        }

        private async Task ShowForgotPasswordDialogAsync()
        {
            try
            {
                using (var dialog = new Form()
                {
                    Width = 500,
                    Height = 400,
                    Text = "Change Password",
                    StartPosition = FormStartPosition.CenterParent,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    // Create controls for the dialog
                    var lblUsername = new Label() { Text = "Username:", Left = 20, Top = 20, Width = 150, Font = new Font("Microsoft Sans Serif", 10F) };
                    var txtUsername = new TextBox() { Left = 180, Top = 20, Width = 280, Font = new Font("Microsoft Sans Serif", 10F) };

                    var lblCurrentPassword = new Label() { Text = "Current Password:", Left = 20, Top = 70, Width = 150, Font = new Font("Microsoft Sans Serif", 10F) };
                    var txtCurrentPassword = new TextBox() { Left = 180, Top = 70, Width = 280, Font = new Font("Microsoft Sans Serif", 10F), UseSystemPasswordChar = true };

                    var lblNewPassword = new Label() { Text = "New Password:", Left = 20, Top = 120, Width = 150, Font = new Font("Microsoft Sans Serif", 10F) };
                    var txtNewPassword = new TextBox() { Left = 180, Top = 120, Width = 280, Font = new Font("Microsoft Sans Serif", 10F), UseSystemPasswordChar = true };

                    var lblConfirmPassword = new Label() { Text = "Confirm Password:", Left = 20, Top = 170, Width = 150, Font = new Font("Microsoft Sans Serif", 10F) };
                    var txtConfirmPassword = new TextBox() { Left = 180, Top = 170, Width = 280, Font = new Font("Microsoft Sans Serif", 10F), UseSystemPasswordChar = true };

                    var btnChange = new Button() { Text = "Change Password", Left = 180, Top = 230, Width = 120, Font = new Font("Microsoft Sans Serif", 10F, FontStyle.Bold) };
                    var btnCancel = new Button() { Text = "Cancel", Left = 320, Top = 230, Width = 80, Font = new Font("Microsoft Sans Serif", 10F) };

                    var lblMessage = new Label() { Left = 20, Top = 280, Width = 440, Height = 50, ForeColor = Color.Red, Font = new Font("Microsoft Sans Serif", 9F) };

                    // Add controls to dialog
                    dialog.Controls.AddRange(new Control[] {
                        lblUsername, txtUsername,
                        lblCurrentPassword, txtCurrentPassword,
                        lblNewPassword, txtNewPassword,
                        lblConfirmPassword, txtConfirmPassword,
                        btnChange, btnCancel, lblMessage
                    });

                    // Create a TaskCompletionSource to handle the dialog result asynchronously
                    var tcs = new TaskCompletionSource<DialogResult>();

                    // Set up event handlers
                    btnChange.Click += async (s, ev) =>
                    {
                        await HandlePasswordChangeAsync(txtUsername, txtCurrentPassword, txtNewPassword, txtConfirmPassword, lblMessage, dialog, tcs);
                    };

                    btnCancel.Click += (s, ev) =>
                    {
                        dialog.DialogResult = DialogResult.Cancel;
                        tcs.TrySetResult(DialogResult.Cancel);
                    };

                    // Handle form closing
                    dialog.FormClosed += (s, ev) =>
                    {
                        tcs.TrySetResult(dialog.DialogResult);
                    };

                    // Set default button and accept/close behaviors
                    dialog.AcceptButton = btnChange;
                    dialog.CancelButton = btnCancel;

                    // Show dialog
                    dialog.Show(this);

                    // Wait for the dialog to complete asynchronously
                    var result = await tcs.Task;

                    if (result == DialogResult.OK)
                    {
                        MessageBox.Show("Password changed successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing password change dialog: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task HandlePasswordChangeAsync(TextBox txtUsername, TextBox txtCurrentPassword,
            TextBox txtNewPassword, TextBox txtConfirmPassword, Label lblMessage, Form dialog, TaskCompletionSource<DialogResult> tcs)
        {
            try
            {
                // Clear previous messages
                lblMessage.Text = "";
                lblMessage.ForeColor = Color.Red;

                // Validate inputs
                string username = txtUsername.Text.Trim();
                string currentPassword = txtCurrentPassword.Text;
                string newPassword = txtNewPassword.Text;
                string confirmPassword = txtConfirmPassword.Text;

                if (string.IsNullOrEmpty(username))
                {
                    lblMessage.Text = "Username is required.";
                    return;
                }

                if (string.IsNullOrEmpty(currentPassword))
                {
                    lblMessage.Text = "Current password is required.";
                    return;
                }

                if (string.IsNullOrEmpty(newPassword))
                {
                    lblMessage.Text = "New password is required.";
                    return;
                }

                if (newPassword != confirmPassword)
                {
                    lblMessage.Text = "New password and confirmation do not match.";
                    return;
                }

                if (newPassword.Length < 4)
                {
                    lblMessage.Text = "New password must be at least 4 characters long.";
                    return;
                }

                // Use AuthService to change password
                if (_authService == null)
                {
                    lblMessage.Text = "Authentication service is not available.";
                    return;
                }

                // Show loading state
                await SetLoadingStateAsync(true);

                try
                {
                    // Perform password change asynchronously on background thread
                    bool success = await Task.Run(() =>
                        _authService.ChangeUserPassword(username, currentPassword, newPassword));

                    if (success)
                    {
                        lblMessage.ForeColor = Color.Green;
                        lblMessage.Text = "Password changed successfully!";

                        // Close dialog after successful change with delay
                        await Task.Delay(1000);
                        dialog.DialogResult = DialogResult.OK;
                        tcs.TrySetResult(DialogResult.OK);
                        dialog.Close();
                    }
                    else
                    {
                        lblMessage.Text = "Failed to change password. Please check your current password.";
                    }
                }
                finally
                {
                    await SetLoadingStateAsync(false);
                }
            }
            catch (Exception ex)
            {
                await SetLoadingStateAsync(false);
                lblMessage.Text = $"Error: {ex.Message}";
            }
        }

        private async Task PerformLoginAction(Func<Task<bool>> action)
        {
            if (isLoading) return;

            try
            {
                await SetLoadingStateAsync(true);

                // Show progress bar
                toolStripProgressBar1.Visible = true;
                toolStripProgressBar1.Style = ProgressBarStyle.Marquee;

                bool success = await action();

                if (!success)
                {
                    await SetLoadingStateAsync(false);
                }
                // If successful, the form will be hidden so we don't need to reset loading state
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                await SetLoadingStateAsync(false);
            }
        }

        private async Task SetLoadingStateAsync(bool loading)
        {
            if (this.InvokeRequired)
            {
                await this.InvokeAsync(() => SetLoadingState(loading));
                return;
            }

            SetLoadingState(loading);
        }

        private void SetLoadingState(bool loading)
        {
            isLoading = loading;

            // Enable/disable controls
            loginBtn.Enabled = !loading;
            viewerBtn.Enabled = !loading;
            userTxtBox.Enabled = !loading;
            passwordTxtBox.Enabled = !loading;
            lblForgotPassword.Enabled = !loading;

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

        // Helper method for safe Invoke operations
        private async Task InvokeAsync(Action action)
        {
            if (this.InvokeRequired)
            {
                await Task.Run(() => this.Invoke(action));
            }
            else
            {
                action();
            }
        }

        private void ShowUsernameError(string message)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(ShowUsernameError), message);
                return;
            }

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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<string>(ShowPasswordError), message);
                return;
            }

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
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(ClearErrors));
                return;
            }

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

            // Dispose services properly
            _authService?.Dispose();
            _databaseService?.Dispose();
            _loggingService?.Dispose();

            base.OnFormClosing(e);
        }
    }
}