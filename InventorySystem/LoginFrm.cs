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

namespace InventorySystem
{
    public partial class LoginFrm : Form
    {
        private string connectionString = "Data Source=DESKTOP-TK48S3J;Initial Catalog=InventoryManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
        private bool isLoading = false;

        public static string CurrentUsername { get; private set; }
        public static string CurrentRole { get; private set; }

        public LoginFrm()
        {
            InitializeComponent();
            ConfigurePasswordField();
            ConfigureEnterKeyBehavior();
            ConfigureProgressBar();
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
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        string query = "SELECT UserId, Username, Password, Role FROM Users WHERE Username = @Username";

                        using (SqlCommand command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@Username", username);

                            using (SqlDataReader reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    string storedPassword = reader["Password"].ToString();

                                    if (password == storedPassword)
                                    {
                                        CurrentUsername = reader["Username"].ToString();
                                        CurrentRole = reader["Role"].ToString();

                                        // Simulate loading time
                                        await Task.Delay(1000);

                                        ViewFrm viewForm = new ViewFrm();
                                        viewForm.Show();
                                        this.Hide();
                                        return true;
                                    }
                                    else
                                    {
                                        ShowPasswordError("Invalid password");
                                        passwordTxtBox.Focus();
                                        passwordTxtBox.SelectAll();
                                        return false;
                                    }
                                }
                                else
                                {
                                    ShowUsernameError("Username not found");
                                    userTxtBox.Focus();
                                    userTxtBox.SelectAll();
                                    return false;
                                }
                            }
                        }
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
            base.OnFormClosing(e);
        }
    }
}