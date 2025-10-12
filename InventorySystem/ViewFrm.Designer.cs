namespace InventorySystem
{
    partial class ViewFrm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.userToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changePasswordToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.logoutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.suppliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displaySuppliesTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editSuppliesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.assetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.displayAssetsTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.editAssetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.inputItemsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.suppliesInventoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.inventoryManagementDataSet = new InventorySystem.InventoryManagementDataSet();
            this.assetsInventoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.inventoryManagementDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.suppliesInventoryTableAdapter = new InventorySystem.InventoryManagementDataSetTableAdapters.SuppliesInventoryTableAdapter();
            this.assetsInventoryTableAdapter = new InventorySystem.InventoryManagementDataSetTableAdapters.AssetsInventoryTableAdapter();
            this.itemCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.locationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dateLbl = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliesInventoryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetsInventoryBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSetBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dateLbl,
            this.toolStripStatusLabel1,
            this.toolStripProgressBar1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 424);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(984, 26);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(151, 20);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 18);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.userToolStripMenuItem,
            this.suppliesToolStripMenuItem,
            this.assetsToolStripMenuItem,
            this.inputItemsToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(984, 30);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // userToolStripMenuItem
            // 
            this.userToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changePasswordToolStripMenuItem,
            this.logoutToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.userToolStripMenuItem.Name = "userToolStripMenuItem";
            this.userToolStripMenuItem.Size = new System.Drawing.Size(52, 26);
            this.userToolStripMenuItem.Text = "User";
            // 
            // changePasswordToolStripMenuItem
            // 
            this.changePasswordToolStripMenuItem.Name = "changePasswordToolStripMenuItem";
            this.changePasswordToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.changePasswordToolStripMenuItem.Text = "Change Password";
            // 
            // logoutToolStripMenuItem
            // 
            this.logoutToolStripMenuItem.Name = "logoutToolStripMenuItem";
            this.logoutToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.logoutToolStripMenuItem.Text = "Logout";
            this.logoutToolStripMenuItem.Click += new System.EventHandler(this.logoutToolStripMenuItem_Click);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(207, 26);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // suppliesToolStripMenuItem
            // 
            this.suppliesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displaySuppliesTableToolStripMenuItem,
            this.editSuppliesToolStripMenuItem});
            this.suppliesToolStripMenuItem.Name = "suppliesToolStripMenuItem";
            this.suppliesToolStripMenuItem.Size = new System.Drawing.Size(79, 26);
            this.suppliesToolStripMenuItem.Text = "Supplies";
            // 
            // displaySuppliesTableToolStripMenuItem
            // 
            this.displaySuppliesTableToolStripMenuItem.Name = "displaySuppliesTableToolStripMenuItem";
            this.displaySuppliesTableToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.displaySuppliesTableToolStripMenuItem.Text = "Display Supplies Table";
            this.displaySuppliesTableToolStripMenuItem.Click += new System.EventHandler(this.displaySuppliesTableToolStripMenuItem_Click);
            // 
            // editSuppliesToolStripMenuItem
            // 
            this.editSuppliesToolStripMenuItem.Name = "editSuppliesToolStripMenuItem";
            this.editSuppliesToolStripMenuItem.Size = new System.Drawing.Size(240, 26);
            this.editSuppliesToolStripMenuItem.Text = "Edit Supplies";
            // 
            // assetsToolStripMenuItem
            // 
            this.assetsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.displayAssetsTableToolStripMenuItem,
            this.editAssetsToolStripMenuItem});
            this.assetsToolStripMenuItem.Name = "assetsToolStripMenuItem";
            this.assetsToolStripMenuItem.Size = new System.Drawing.Size(64, 26);
            this.assetsToolStripMenuItem.Text = "Assets";
            // 
            // displayAssetsTableToolStripMenuItem
            // 
            this.displayAssetsTableToolStripMenuItem.Name = "displayAssetsTableToolStripMenuItem";
            this.displayAssetsTableToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.displayAssetsTableToolStripMenuItem.Text = "Display Assets Table";
            this.displayAssetsTableToolStripMenuItem.Click += new System.EventHandler(this.displayAssetsTableToolStripMenuItem_Click);
            // 
            // editAssetsToolStripMenuItem
            // 
            this.editAssetsToolStripMenuItem.Name = "editAssetsToolStripMenuItem";
            this.editAssetsToolStripMenuItem.Size = new System.Drawing.Size(225, 26);
            this.editAssetsToolStripMenuItem.Text = "Edit Assets";
            // 
            // inputItemsToolStripMenuItem
            // 
            this.inputItemsToolStripMenuItem.Name = "inputItemsToolStripMenuItem";
            this.inputItemsToolStripMenuItem.Size = new System.Drawing.Size(97, 26);
            this.inputItemsToolStripMenuItem.Text = "Input Items";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.itemCodeDataGridViewTextBoxColumn,
            this.itemNameDataGridViewTextBoxColumn,
            this.descriptionDataGridViewTextBoxColumn,
            this.quantityDataGridViewTextBoxColumn,
            this.unitDataGridViewTextBoxColumn,
            this.locationDataGridViewTextBoxColumn});
            this.dataGridView1.DataSource = this.suppliesInventoryBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 30);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.RowTemplate.Height = 24;
            this.dataGridView1.Size = new System.Drawing.Size(984, 394);
            this.dataGridView1.TabIndex = 2;
            this.dataGridView1.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dataGridView1_CellContentClick);
            // 
            // suppliesInventoryBindingSource
            // 
            this.suppliesInventoryBindingSource.DataMember = "SuppliesInventory";
            this.suppliesInventoryBindingSource.DataSource = this.inventoryManagementDataSet;
            // 
            // inventoryManagementDataSet
            // 
            this.inventoryManagementDataSet.DataSetName = "InventoryManagementDataSet";
            this.inventoryManagementDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // assetsInventoryBindingSource
            // 
            this.assetsInventoryBindingSource.DataMember = "AssetsInventory";
            this.assetsInventoryBindingSource.DataSource = this.inventoryManagementDataSet;
            // 
            // inventoryManagementDataSetBindingSource
            // 
            this.inventoryManagementDataSetBindingSource.DataSource = this.inventoryManagementDataSet;
            this.inventoryManagementDataSetBindingSource.Position = 0;
            // 
            // suppliesInventoryTableAdapter
            // 
            this.suppliesInventoryTableAdapter.ClearBeforeFill = true;
            // 
            // assetsInventoryTableAdapter
            // 
            this.assetsInventoryTableAdapter.ClearBeforeFill = true;
            // 
            // itemCodeDataGridViewTextBoxColumn
            // 
            this.itemCodeDataGridViewTextBoxColumn.DataPropertyName = "ItemCode";
            this.itemCodeDataGridViewTextBoxColumn.HeaderText = "ItemCode";
            this.itemCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.itemCodeDataGridViewTextBoxColumn.Name = "itemCodeDataGridViewTextBoxColumn";
            this.itemCodeDataGridViewTextBoxColumn.ReadOnly = true;
            this.itemCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // itemNameDataGridViewTextBoxColumn
            // 
            this.itemNameDataGridViewTextBoxColumn.DataPropertyName = "ItemName";
            this.itemNameDataGridViewTextBoxColumn.HeaderText = "ItemName";
            this.itemNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.itemNameDataGridViewTextBoxColumn.Name = "itemNameDataGridViewTextBoxColumn";
            this.itemNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.itemNameDataGridViewTextBoxColumn.Width = 125;
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.descriptionDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            this.descriptionDataGridViewTextBoxColumn.ReadOnly = true;
            this.descriptionDataGridViewTextBoxColumn.Width = 125;
            // 
            // quantityDataGridViewTextBoxColumn
            // 
            this.quantityDataGridViewTextBoxColumn.DataPropertyName = "Quantity";
            this.quantityDataGridViewTextBoxColumn.HeaderText = "Quantity";
            this.quantityDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.quantityDataGridViewTextBoxColumn.Name = "quantityDataGridViewTextBoxColumn";
            this.quantityDataGridViewTextBoxColumn.ReadOnly = true;
            this.quantityDataGridViewTextBoxColumn.Width = 125;
            // 
            // unitDataGridViewTextBoxColumn
            // 
            this.unitDataGridViewTextBoxColumn.DataPropertyName = "Unit";
            this.unitDataGridViewTextBoxColumn.HeaderText = "Unit";
            this.unitDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.unitDataGridViewTextBoxColumn.Name = "unitDataGridViewTextBoxColumn";
            this.unitDataGridViewTextBoxColumn.ReadOnly = true;
            this.unitDataGridViewTextBoxColumn.Width = 125;
            // 
            // locationDataGridViewTextBoxColumn
            // 
            this.locationDataGridViewTextBoxColumn.DataPropertyName = "Location";
            this.locationDataGridViewTextBoxColumn.HeaderText = "Location";
            this.locationDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.locationDataGridViewTextBoxColumn.Name = "locationDataGridViewTextBoxColumn";
            this.locationDataGridViewTextBoxColumn.ReadOnly = true;
            this.locationDataGridViewTextBoxColumn.Width = 125;
            // 
            // dateLbl
            // 
            this.dateLbl.Name = "dateLbl";
            this.dateLbl.Size = new System.Drawing.Size(41, 20);
            this.dateLbl.Text = "Date";
            // 
            // ViewFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(984, 450);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ViewFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inventory Management System";
            this.Load += new System.EventHandler(this.ViewFrm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliesInventoryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.assetsInventoryBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSetBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem userToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changePasswordToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem logoutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem suppliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displaySuppliesTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editSuppliesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem assetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem displayAssetsTableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem editAssetsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem inputItemsToolStripMenuItem;
        private System.Windows.Forms.DataGridView dataGridView1;
        private InventoryManagementDataSet inventoryManagementDataSet;
        private System.Windows.Forms.BindingSource suppliesInventoryBindingSource;
        private System.Windows.Forms.BindingSource assetsInventoryBindingSource;
        private System.Windows.Forms.BindingSource inventoryManagementDataSetBindingSource;
        private InventoryManagementDataSetTableAdapters.SuppliesInventoryTableAdapter suppliesInventoryTableAdapter;
        private InventoryManagementDataSetTableAdapters.AssetsInventoryTableAdapter assetsInventoryTableAdapter;
        private System.Windows.Forms.ToolStripStatusLabel dateLbl;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemCodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn itemNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn quantityDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn unitDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn locationDataGridViewTextBoxColumn;
    }
}