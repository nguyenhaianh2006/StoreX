using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using StoreX; 

namespace StoreX
{
    public partial class Form2 : Form
    {
        string connectionString = @"Server=LAPTOP-N6C9N5LN\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";

        public Form2()
        {
            InitializeComponent();
        }

        private void LoadUserControl(UserControl uc)
        {
            // Xóa control cũ
            panelMain.Controls.Clear();

            // Dock full màn hình trong panel
            uc.Dock = DockStyle.Fill;

            // Thêm UC mới vào panel
            panelMain.Controls.Add(uc);
        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Products());
        }

        private void btnEmployees_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Employees());
        }

        private void btnCustomers_Click(object sender, EventArgs e)
        {
            LoadUserControl(new UC_Customers());
        }

        private void btnSales_Click(object sender, EventArgs e)
        {
            FormSales formSales = new FormSales();
            formSales.Show();
        }

        private void btnStatitics_Click(object sender, EventArgs e)
        {
            FormStatistics formStatistics = new FormStatistics();
            formStatistics.Show(); // Hoặc formStatistics.ShowDialog();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            FormSales formSales = new FormSales();
            formSales.Show();
        }
    }
}