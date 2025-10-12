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

        public static string CurrentUsername { get; private set; }
        public static string CurrentRole { get; private set; }

        public LoginFrm()
        {
            InitializeComponent();
            ConfigurePasswordField();
            ConfigureEnterKeyBehavior();
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

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                // Trigger login button click
                loginBtn.PerformClick();
                e.Handled = true;
                e.SuppressKeyPress = true; // Prevent the beep sound
            }
        }

        private void loginBtn_Click(object sender, EventArgs e)
        {
            ClearErrors();

            string username = userTxtBox.Text.Trim();
            string password = passwordTxtBox.Text;

            if (string.IsNullOrEmpty(username))
            {
                ShowUsernameError("Username is required");
                userTxtBox.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowPasswordError("Password is required");
                passwordTxtBox.Focus();
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT UserId, Username, Password, Role FROM Users WHERE Username = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedPassword = reader["Password"].ToString();

                                if (password == storedPassword)
                                {
                                    CurrentUsername = reader["Username"].ToString();
                                    CurrentRole = reader["Role"].ToString();

                                    ViewFrm viewForm = new ViewFrm();
                                    viewForm.Show();
                                    this.Hide();
                                }
                                else
                                {
                                    ShowPasswordError("Invalid password");
                                    passwordTxtBox.Focus();
                                    passwordTxtBox.SelectAll();
                                }
                            }
                            else
                            {
                                ShowUsernameError("Username not found");
                                userTxtBox.Focus();
                                userTxtBox.SelectAll();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during login: {ex.Message}", "Login Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void viewerBtn_Click(object sender, EventArgs e)
        {
            CurrentUsername = "Guest";
            CurrentRole = "Guest";

            ViewFrm viewForm = new ViewFrm();
            viewForm.Show();
            this.Hide();
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
    }
}