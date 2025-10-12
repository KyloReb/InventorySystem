using static System.Net.Mime.MediaTypeNames;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;

namespace InventorySystem
{
    partial class Input
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.itemNameTxt = new System.Windows.Forms.TextBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.uSERToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quantityTxt = new System.Windows.Forms.TextBox();
            this.unitTxt = new System.Windows.Forms.TextBox();
            this.itemNameLbl = new System.Windows.Forms.Label();
            this.descriptionLbl = new System.Windows.Forms.Label();
            this.unitLbl = new System.Windows.Forms.Label();
            this.locationTxt = new System.Windows.Forms.TextBox();
            this.locationLbl = new System.Windows.Forms.Label();
            this.quantityLbl = new System.Windows.Forms.Label();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.itemCodeLbl = new System.Windows.Forms.Label();
            this.itemCodeTxt = new System.Windows.Forms.TextBox();
            this.submitBtn = new System.Windows.Forms.Button();
            this.inventoryManagementDataSet = new InventorySystem.InventoryManagementDataSet();
            this.inventoryManagementDataSetBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.suppliesInventoryBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.suppliesInventoryTableAdapter = new InventorySystem.InventoryManagementDataSetTableAdapters.SuppliesInventoryTableAdapter();
            this.itemCodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.itemNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.quantityDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.unitDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.locationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSet)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSetBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliesInventoryBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // itemNameTxt
            // 
            this.itemNameTxt.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.itemNameTxt.Location = new System.Drawing.Point(156, 94);
            this.itemNameTxt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.itemNameTxt.Name = "itemNameTxt";
            this.itemNameTxt.Size = new System.Drawing.Size(424, 34);
            this.itemNameTxt.TabIndex = 0;
            // 
            // textBox1
            // 
            this.textBox1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(156, 196);
            this.textBox1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(424, 34);
            this.textBox1.TabIndex = 1;
            // 
            // errorProvider1
            // 
            this.errorProvider1.ContainerControl = this;
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.uSERToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1387, 28);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // uSERToolStripMenuItem
            // 
            this.uSERToolStripMenuItem.Name = "uSERToolStripMenuItem";
            this.uSERToolStripMenuItem.Size = new System.Drawing.Size(58, 24);
            this.uSERToolStripMenuItem.Text = "USER";
            // 
            // quantityTxt
            // 
            this.quantityTxt.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quantityTxt.Location = new System.Drawing.Point(156, 242);
            this.quantityTxt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.quantityTxt.Name = "quantityTxt";
            this.quantityTxt.Size = new System.Drawing.Size(79, 34);
            this.quantityTxt.TabIndex = 3;
            // 
            // unitTxt
            // 
            this.unitTxt.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unitTxt.Location = new System.Drawing.Point(156, 283);
            this.unitTxt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.unitTxt.Name = "unitTxt";
            this.unitTxt.Size = new System.Drawing.Size(424, 34);
            this.unitTxt.TabIndex = 4;
            // 
            // itemNameLbl
            // 
            this.itemNameLbl.AutoSize = true;
            this.itemNameLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.itemNameLbl.Location = new System.Drawing.Point(12, 97);
            this.itemNameLbl.Name = "itemNameLbl";
            this.itemNameLbl.Size = new System.Drawing.Size(112, 28);
            this.itemNameLbl.TabIndex = 5;
            this.itemNameLbl.Text = "Item Name:";
            // 
            // descriptionLbl
            // 
            this.descriptionLbl.AutoSize = true;
            this.descriptionLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.descriptionLbl.Location = new System.Drawing.Point(12, 199);
            this.descriptionLbl.Name = "descriptionLbl";
            this.descriptionLbl.Size = new System.Drawing.Size(116, 28);
            this.descriptionLbl.TabIndex = 6;
            this.descriptionLbl.Text = "Description:";
            // 
            // unitLbl
            // 
            this.unitLbl.AutoSize = true;
            this.unitLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.unitLbl.Location = new System.Drawing.Point(12, 283);
            this.unitLbl.Name = "unitLbl";
            this.unitLbl.Size = new System.Drawing.Size(53, 28);
            this.unitLbl.TabIndex = 7;
            this.unitLbl.Text = "Unit:";
            // 
            // locationTxt
            // 
            this.locationTxt.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.locationTxt.Location = new System.Drawing.Point(156, 334);
            this.locationTxt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.locationTxt.Name = "locationTxt";
            this.locationTxt.Size = new System.Drawing.Size(424, 34);
            this.locationTxt.TabIndex = 8;
            // 
            // locationLbl
            // 
            this.locationLbl.AutoSize = true;
            this.locationLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.locationLbl.Location = new System.Drawing.Point(12, 339);
            this.locationLbl.Name = "locationLbl";
            this.locationLbl.Size = new System.Drawing.Size(91, 28);
            this.locationLbl.TabIndex = 9;
            this.locationLbl.Text = "Location:";
            // 
            // quantityLbl
            // 
            this.quantityLbl.AutoSize = true;
            this.quantityLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quantityLbl.Location = new System.Drawing.Point(12, 242);
            this.quantityLbl.Name = "quantityLbl";
            this.quantityLbl.Size = new System.Drawing.Size(92, 28);
            this.quantityLbl.TabIndex = 10;
            this.quantityLbl.Text = "Quantity:";
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
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
            this.dataGridView1.Location = new System.Drawing.Point(606, 43);
            this.dataGridView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersWidth = 51;
            this.dataGridView1.Size = new System.Drawing.Size(781, 324);
            this.dataGridView1.TabIndex = 11;
            // 
            // itemCodeLbl
            // 
            this.itemCodeLbl.AllowDrop = true;
            this.itemCodeLbl.AutoSize = true;
            this.itemCodeLbl.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.itemCodeLbl.Location = new System.Drawing.Point(12, 43);
            this.itemCodeLbl.Name = "itemCodeLbl";
            this.itemCodeLbl.Size = new System.Drawing.Size(106, 28);
            this.itemCodeLbl.TabIndex = 13;
            this.itemCodeLbl.Text = "Item Code:";
            // 
            // itemCodeTxt
            // 
            this.itemCodeTxt.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.itemCodeTxt.Location = new System.Drawing.Point(156, 41);
            this.itemCodeTxt.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.itemCodeTxt.Name = "itemCodeTxt";
            this.itemCodeTxt.Size = new System.Drawing.Size(424, 34);
            this.itemCodeTxt.TabIndex = 12;
            // 
            // submitBtn
            // 
            this.submitBtn.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.submitBtn.Location = new System.Drawing.Point(156, 381);
            this.submitBtn.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.submitBtn.Name = "submitBtn";
            this.submitBtn.Size = new System.Drawing.Size(424, 42);
            this.submitBtn.TabIndex = 14;
            this.submitBtn.Text = "SUBMIT";
            this.submitBtn.UseVisualStyleBackColor = true;
            // 
            // inventoryManagementDataSet
            // 
            this.inventoryManagementDataSet.DataSetName = "InventoryManagementDataSet";
            this.inventoryManagementDataSet.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // inventoryManagementDataSetBindingSource
            // 
            this.inventoryManagementDataSetBindingSource.DataSource = this.inventoryManagementDataSet;
            this.inventoryManagementDataSetBindingSource.Position = 0;
            // 
            // suppliesInventoryBindingSource
            // 
            this.suppliesInventoryBindingSource.DataMember = "SuppliesInventory";
            this.suppliesInventoryBindingSource.DataSource = this.inventoryManagementDataSetBindingSource;
            // 
            // suppliesInventoryTableAdapter
            // 
            this.suppliesInventoryTableAdapter.ClearBeforeFill = true;
            // 
            // itemCodeDataGridViewTextBoxColumn
            // 
            this.itemCodeDataGridViewTextBoxColumn.DataPropertyName = "ItemCode";
            this.itemCodeDataGridViewTextBoxColumn.HeaderText = "ItemCode";
            this.itemCodeDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.itemCodeDataGridViewTextBoxColumn.Name = "itemCodeDataGridViewTextBoxColumn";
            this.itemCodeDataGridViewTextBoxColumn.Width = 125;
            // 
            // itemNameDataGridViewTextBoxColumn
            // 
            this.itemNameDataGridViewTextBoxColumn.DataPropertyName = "ItemName";
            this.itemNameDataGridViewTextBoxColumn.HeaderText = "ItemName";
            this.itemNameDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.itemNameDataGridViewTextBoxColumn.Name = "itemNameDataGridViewTextBoxColumn";
            this.itemNameDataGridViewTextBoxColumn.Width = 125;
            // 
            // descriptionDataGridViewTextBoxColumn
            // 
            this.descriptionDataGridViewTextBoxColumn.DataPropertyName = "Description";
            this.descriptionDataGridViewTextBoxColumn.HeaderText = "Description";
            this.descriptionDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.descriptionDataGridViewTextBoxColumn.Name = "descriptionDataGridViewTextBoxColumn";
            this.descriptionDataGridViewTextBoxColumn.Width = 125;
            // 
            // quantityDataGridViewTextBoxColumn
            // 
            this.quantityDataGridViewTextBoxColumn.DataPropertyName = "Quantity";
            this.quantityDataGridViewTextBoxColumn.HeaderText = "Quantity";
            this.quantityDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.quantityDataGridViewTextBoxColumn.Name = "quantityDataGridViewTextBoxColumn";
            this.quantityDataGridViewTextBoxColumn.Width = 125;
            // 
            // unitDataGridViewTextBoxColumn
            // 
            this.unitDataGridViewTextBoxColumn.DataPropertyName = "Unit";
            this.unitDataGridViewTextBoxColumn.HeaderText = "Unit";
            this.unitDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.unitDataGridViewTextBoxColumn.Name = "unitDataGridViewTextBoxColumn";
            this.unitDataGridViewTextBoxColumn.Width = 125;
            // 
            // locationDataGridViewTextBoxColumn
            // 
            this.locationDataGridViewTextBoxColumn.DataPropertyName = "Location";
            this.locationDataGridViewTextBoxColumn.HeaderText = "Location";
            this.locationDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.locationDataGridViewTextBoxColumn.Name = "locationDataGridViewTextBoxColumn";
            this.locationDataGridViewTextBoxColumn.Width = 125;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 146);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 28);
            this.label1.TabIndex = 15;
            this.label1.Text = "Category:";
            // 
            // comboBox1
            // 
            this.comboBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Items.AddRange(new object[] {
            "Supplies",
            "Assets"});
            this.comboBox1.Location = new System.Drawing.Point(156, 146);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(207, 33);
            this.comboBox1.TabIndex = 16;
            // 
            // Input
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1387, 434);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.submitBtn);
            this.Controls.Add(this.itemCodeLbl);
            this.Controls.Add(this.itemCodeTxt);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.quantityLbl);
            this.Controls.Add(this.locationLbl);
            this.Controls.Add(this.locationTxt);
            this.Controls.Add(this.unitLbl);
            this.Controls.Add(this.descriptionLbl);
            this.Controls.Add(this.itemNameLbl);
            this.Controls.Add(this.unitTxt);
            this.Controls.Add(this.quantityTxt);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.itemNameTxt);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "Input";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "InventoryManagement";
            this.Load += new System.EventHandler(this.Input_Load);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSet)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.inventoryManagementDataSetBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.suppliesInventoryBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TextBox itemNameTxt;
        private TextBox textBox1;
        private ErrorProvider errorProvider1;
        private Label itemNameLbl;
        private TextBox unitTxt;
        private TextBox quantityTxt;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem uSERToolStripMenuItem;
        private Label unitLbl;
        private Label descriptionLbl;
        private TextBox locationTxt;
        private Label locationLbl;
        private Label quantityLbl;
        private Label itemCodeLbl;
        private TextBox itemCodeTxt;
        private DataGridView dataGridView1;
        private Button submitBtn;
        private BindingSource inventoryManagementDataSetBindingSource;
        private InventoryManagementDataSet inventoryManagementDataSet;
        private BindingSource suppliesInventoryBindingSource;
        private InventoryManagementDataSetTableAdapters.SuppliesInventoryTableAdapter suppliesInventoryTableAdapter;
        private DataGridViewTextBoxColumn itemCodeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn itemNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn descriptionDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn quantityDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn unitDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn locationDataGridViewTextBoxColumn;
        private Label label1;
        private ComboBox comboBox1;
    }
}
