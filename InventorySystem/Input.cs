using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InventorySystem
{
    public partial class Input : Form
    {
        public Input()
        {
            InitializeComponent();
        }
        private void itemNameLbl_Click(object sender, EventArgs e)
        {

        }

        private void Input_Load(object sender, EventArgs e)
        {
            // TODO: This line of code loads data into the 'inventoryManagementDataSet.SuppliesInventory' table. You can move, or remove it, as needed.
            this.suppliesInventoryTableAdapter.Fill(this.inventoryManagementDataSet.SuppliesInventory);

        }
    }
}
