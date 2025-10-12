using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Configuration;

namespace InventorySystem
{
    public partial class ViewFrm : Form
    {
        private string connectionString;
        private string currentTable = "Supplies";

        // Read table names from config
        private string SuppliesTable => ConfigurationManager.AppSettings["SuppliesTable"] ?? "SuppliesInventory";
        private string AssetsTable => ConfigurationManager.AppSettings["AssetsTable"] ?? "AssetsInventory";

        public ViewFrm()
        {
            InitializeComponent();
            InitializeConnectionString();
        }

        private void InitializeConnectionString()
        {
            try
            {
                connectionString = ConfigurationManager.ConnectionStrings["InventoryManagementConnection"]?.ConnectionString;

                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new Exception("Connection string not found in configuration.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Configuration Error: {ex.Message}\n\nUsing fallback connection string.",
                              "Configuration Error",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Warning);
                // Fallback connection string
                connectionString = "Data Source=DESKTOP-TK48S3J;Initial Catalog=InventoryManagement;Integrated Security=True;Encrypt=True;TrustServerCertificate=True";
            }
        }

        private void ViewFrm_Load(object sender, EventArgs e)
        {
            UpdateStatus("Loading user information...", 25);
            LoadUserInfo();

            UpdateStatus("Loading supplies data...", 50);
            LoadSuppliesData();

            UpdateStatus("Ready", 100);
        }

        private void UpdateStatus(string message, int progress)
        {
            toolStripStatusLabel1.Text = message;
            toolStripProgressBar1.Value = progress;
            Application.DoEvents();
        }

        private void LoadUserInfo()
        {
            string username = LoginFrm.CurrentUsername;
            string role = LoginFrm.CurrentRole;

            userToolStripMenuItem.Text = $"User: {username} ({role})";

            if (userToolStripMenuItem.DropDownItems.Count > 0)
            {
                userToolStripMenuItem.DropDownItems[0].Text = $"Welcome, {username}";
            }
            else
            {
                userToolStripMenuItem.DropDownItems.Add($"Welcome, {username}");
            }

            // Disable edit menus for Guest users
            bool isGuest = role == "Guest";
            if (editSuppliesToolStripMenuItem != null)
                editSuppliesToolStripMenuItem.Enabled = !isGuest;
            if (editAssetsToolStripMenuItem != null)
                editAssetsToolStripMenuItem.Enabled = !isGuest;
        }

        private void LoadSuppliesData()
        {
            LoadTableData(SuppliesTable, "Supplies");
        }

        private void LoadAssetsData()
        {
            LoadTableData(AssetsTable, "Assets");
        }

        private void LoadTableData(string tableName, string tableType)
        {
            try
            {
                UpdateStatus($"Loading {tableType.ToLower()} data...", 50);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $"SELECT * FROM {tableName}";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGridView1.DataSource = null;
                    dataGridView1.AutoGenerateColumns = true;
                    dataGridView1.DataSource = dataTable;

                    currentTable = tableType;
                    AdjustFormWidth();

                    UpdateStatus($"{tableType} data loaded successfully", 100);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus($"Error loading {tableType} data: {ex.Message}", 0);
                MessageBox.Show($"Error loading {tableType} data from table '{tableName}': {ex.Message}",
                              "Data Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AdjustFormWidth()
        {
            int totalWidth = dataGridView1.RowHeadersWidth;
            foreach (DataGridViewColumn column in dataGridView1.Columns)
            {
                totalWidth += column.Width;
            }

            totalWidth += SystemInformation.VerticalScrollBarWidth + 20;
            this.Width = Math.Max(totalWidth, 800);
        }

        private void logoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoginFrm loginForm = new LoginFrm();
            loginForm.Show();
            this.Close();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void displaySuppliesTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateStatus("Loading supplies table...", 0);
            LoadSuppliesData();
        }

        private void displayAssetsTableToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UpdateStatus("Loading assets table...", 0);
            LoadAssetsData();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Handle cell content click if needed
        }
    }
}