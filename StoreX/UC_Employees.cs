// Trong file UC_Employees.cs
using System;
using System.Windows.Forms;
using System.Data;
using System.Data.SqlClient;

namespace StoreX
{
    public partial class UC_Employees : UserControl
    {
        private string connectionString = "Server=LAPTOP-N6C9N5LN\\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";
        SqlDataAdapter adapter = null;
        DataTable dtEmployees = null;

        public UC_Employees()
        {
            InitializeComponent();
            LoadEmployeeData(); // <-- Thêm dòng này
        }

        private void LoadEmployeeData()
        {
            try
            {
                // Thay đổi câu lệnh SQL để lấy dữ liệu từ bảng Employees
                string query = "SELECT EmployeeID, EmployeeCode, FullName, Position, Authority FROM Employees;";

                adapter = new SqlDataAdapter(query, connectionString);
                dtEmployees = new DataTable();
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                adapter.Fill(dtEmployees);

                // Giả sử DataGridView của bạn có tên là dgvEmployees
                dgvEmployees.DataSource = dtEmployees;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu từ các TextBox
            string fullName = txtName.Text;
            string position = txtPosition.Text;
            string authority = txtAuthority.Text;

            // Kiểm tra dữ liệu đầu vào
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(position) || string.IsNullOrWhiteSpace(authority))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin nhân viên!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Tạo EmployeeCode tự động (ví dụ: EMP004)
                    string employeeCode = "EMP" + (dgvEmployees.Rows.Count + 1).ToString("D3");

                    string query = "INSERT INTO Employees (EmployeeCode, FullName, Position, Authority) VALUES (@EmployeeCode, @FullName, @Position, @Authority);";
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Position", position);
                    command.Parameters.AddWithValue("@Authority", authority);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Đã thêm nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tải lại dữ liệu để cập nhật DataGridView
                    LoadEmployeeData();
                    // Xóa dữ liệu trên các TextBox
                    txtName.Clear();
                    txtPosition.Clear();
                    txtAuthority.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có hàng nào được chọn không
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để chỉnh sửa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string fullName = txtName.Text;
            string position = txtPosition.Text;
            string authority = txtAuthority.Text;

            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(position) || string.IsNullOrWhiteSpace(authority))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin để chỉnh sửa!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                int employeeID = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Employees SET FullName = @FullName, Position = @Position, Authority = @Authority WHERE EmployeeID = @EmployeeID;";
                    SqlCommand command = new SqlCommand(query, connection);

                    command.Parameters.AddWithValue("@FullName", fullName);
                    command.Parameters.AddWithValue("@Position", position);
                    command.Parameters.AddWithValue("@Authority", authority);
                    command.Parameters.AddWithValue("@EmployeeID", employeeID);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Đã cập nhật thông tin nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadEmployeeData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi cập nhật nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvEmployees.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một nhân viên để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa nhân viên này không?",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    int employeeID = Convert.ToInt32(dgvEmployees.SelectedRows[0].Cells["EmployeeID"].Value);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string deleteQuery = "DELETE FROM Employees WHERE EmployeeID = @EmployeeID;";
                        SqlCommand command = new SqlCommand(deleteQuery, connection);
                        command.Parameters.AddWithValue("@EmployeeID", employeeID);

                        command.ExecuteNonQuery();

                        MessageBox.Show("Đã xóa nhân viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        LoadEmployeeData();
                    }
                }
                catch (SqlException ex)
                {
                    // Lỗi liên quan đến khóa ngoại (ví dụ: nhân viên này đã bán hàng)
                    MessageBox.Show("Không thể xóa nhân viên này vì có dữ liệu liên quan trong các bảng khác (ví dụ: bảng Sales).", "Lỗi ràng buộc dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa nhân viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}