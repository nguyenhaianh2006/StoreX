using System;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace StoreX
{
    public partial class UC_Customers : UserControl
    {
        // Chuỗi kết nối đến cơ sở dữ liệu của bạn
        // Tên server của bạn: LAPTOP-N6C9NSLW\SQLEXPRESS01
        private string connectionString = "Server=LAPTOP-N6C9N5LN\\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";

        // Khai báo SqlDataAdapter và DataTable để dễ dàng quản lý dữ liệu
        SqlDataAdapter adapter = null;
        DataTable dtCustomers = null;

        public UC_Customers()
        {
            InitializeComponent();
            LoadCustomerData(); // Gọi phương thức tải dữ liệu khi UserControl được tạo
        }

        // Phương thức để tải dữ liệu từ database vào DataGridView
        private void LoadCustomerData()
        {
            try
            {
                // Câu lệnh SQL để lấy dữ liệu từ bảng Customers
                string query = "SELECT CustomerID, CustomerCode, FullName, Phone, Address FROM Customers;";

                // Khởi tạo DataAdapter và DataTable
                adapter = new SqlDataAdapter(query, connectionString);
                dtCustomers = new DataTable();

                // Dùng SqlCommandBuilder để tự động tạo các câu lệnh INSERT, UPDATE, DELETE
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

                // Lấp đầy dữ liệu vào DataTable
                adapter.Fill(dtCustomers);

                // Gán DataTable làm nguồn dữ liệu cho DataGridView
                dgvCustomers.DataSource = dtCustomers;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu khách hàng: " + ex.Message);
            }
        }

        // Cải thiện nút Thêm
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Thêm một dòng mới vào DataTable trong bộ nhớ
                DataRow newRow = dtCustomers.NewRow();
                newRow["CustomerCode"] = "CUST" + (dtCustomers.Rows.Count + 1).ToString("D3"); // Tạo mã khách hàng tự động
                newRow["FullName"] = txtCustomerName.Text;
                newRow["Phone"] = txtPhone.Text;
                newRow["Address"] = txtAddress.Text;
                dtCustomers.Rows.Add(newRow);

                // Cập nhật thay đổi vào database
                adapter.Update(dtCustomers);
                MessageBox.Show("Đã thêm khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm khách hàng: " + ex.Message);
            }
        }

        // Cải thiện nút Sửa
        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvCustomers.SelectedRows.Count > 0)
            {
                try
                {
                    // Lấy dòng đang chọn trong DataGridView
                    int selectedIndex = dgvCustomers.SelectedRows[0].Index;
                    DataRow rowToEdit = dtCustomers.Rows[selectedIndex];

                    // Cập nhật dữ liệu cho dòng đó trong DataTable
                    rowToEdit["FullName"] = txtCustomerName.Text;
                    rowToEdit["Phone"] = txtPhone.Text;
                    rowToEdit["Address"] = txtAddress.Text;

                    // Cập nhật thay đổi vào database
                    adapter.Update(dtCustomers);
                    MessageBox.Show("Đã sửa thông tin khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi sửa khách hàng: " + ex.Message);
                }
            }
        }

        // Cải thiện nút Xóa
        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có hàng nào được chọn không
            if (dgvCustomers.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một khách hàng để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa khách hàng này không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    // Lấy CustomerID của khách hàng đã chọn
                    int customerID = Convert.ToInt32(dgvCustomers.SelectedRows[0].Cells["CustomerID"].Value);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string deleteQuery = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
                        SqlCommand command = new SqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@CustomerID", customerID);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Đã xóa khách hàng thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadCustomerData(); // Tải lại dữ liệu để cập nhật DataGridView
                        }
                        else
                        {
                            MessageBox.Show("Không tìm thấy khách hàng để xóa. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa khách hàng: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UC_Customers_Load(object sender, EventArgs e)
        {

        }
    }
}