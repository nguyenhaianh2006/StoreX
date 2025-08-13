using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;
using StoreX;

namespace StoreX
{
    public partial class FormStatistics : Form
    {
        private string connectionString = "Server=LAPTOP-N6C9N5LN\\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";

        // Khai báo DataTable để lưu trữ dữ liệu gốc
        private DataTable dtSales = new DataTable();
        // Khai báo DataView để lọc dữ liệu
        private DataView dvSales;

        public FormStatistics()
        {
            InitializeComponent();
            LoadSaleData();
        }

        private void LoadSaleData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT SaleID, SaleDate, TotalAmount FROM Sales;";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    // Xóa dữ liệu cũ và đổ dữ liệu mới vào DataTable
                    dtSales.Clear();
                    adapter.Fill(dtSales);

                    // Khởi tạo DataView từ DataTable
                    dvSales = new DataView(dtSales);

                    // Gán DataView vào DataSource của DataGridView
                    dgvStatistics.DataSource = dvSales;

                    UpdateTotal();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu thống kê: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            try
            {
                // Lấy ngày bắt đầu và kết thúc từ DateTimePicker
                DateTime fromDate = dtpFromDate.Value.Date;
                // Ngày kết thúc là ngày được chọn, cộng thêm 1 ngày rồi trừ đi 1 giây để lấy toàn bộ thời gian của ngày đó
                DateTime toDate = dtpToDate.Value.Date.AddDays(1).AddSeconds(-1);

                // Sử dụng thuộc tính RowFilter của DataView để lọc dữ liệu
                string filterExpression = $"SaleDate >= '{fromDate.ToString("yyyy-MM-dd HH:mm:ss")}' AND SaleDate <= '{toDate.ToString("yyyy-MM-dd HH:mm:ss")}'";
                dvSales.RowFilter = filterExpression;

                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi lọc dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateTotal()
        {
            decimal total = 0;

            // Tính tổng từ các hàng trong DataView đã được lọc
            foreach (DataRowView row in dvSales)
            {
                // Kiểm tra xem cột "TotalAmount" có tồn tại và không rỗng không
                if (row["TotalAmount"] != DBNull.Value)
                {
                    total += Convert.ToDecimal(row["TotalAmount"]);
                }
            }

            lblTotalRevenue.Text = $"Tổng doanh thu: {total:N0} VND";
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            // Kiểm tra xem có hàng nào được chọn không
            if (dgvStatistics.SelectedRows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một hóa đơn để xóa.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Xác nhận từ người dùng trước khi xóa
            DialogResult confirmResult = MessageBox.Show(
                "Bạn có chắc chắn muốn xóa hóa đơn này không? Thao tác này không thể hoàn tác.",
                "Xác nhận xóa",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirmResult == DialogResult.Yes)
            {
                try
                {
                    // Lấy SaleID của hóa đơn đã chọn
                    int saleID = Convert.ToInt32(dgvStatistics.SelectedRows[0].Cells["SaleID"].Value);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        SqlTransaction transaction = connection.BeginTransaction();

                        try
                        {
                            // Xóa các bản ghi chi tiết hóa đơn (SaleDetails) trước
                            string deleteDetailsQuery = "DELETE FROM SaleDetails WHERE SaleID = @SaleID;";
                            SqlCommand deleteDetailsCommand = new SqlCommand(deleteDetailsQuery, connection, transaction);
                            deleteDetailsCommand.Parameters.AddWithValue("@SaleID", saleID);
                            deleteDetailsCommand.ExecuteNonQuery();

                            // Xóa bản ghi hóa đơn chính (Sales)
                            string deleteSaleQuery = "DELETE FROM Sales WHERE SaleID = @SaleID;";

                            SqlCommand deleteSaleCommand = new SqlCommand(deleteSaleQuery, connection, transaction);
                            deleteSaleCommand.Parameters.AddWithValue("@SaleID", saleID);
                            deleteSaleCommand.ExecuteNonQuery();

                            // Hoàn tất giao dịch
                            transaction.Commit();
                            MessageBox.Show("Đã xóa hóa đơn thành công.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            // Tải lại dữ liệu để cập nhật hiển thị
                            LoadSaleData();
                        }
                        catch (Exception ex)
                        {
                            // Nếu có lỗi, hủy bỏ tất cả các thay đổi
                            transaction.Rollback();
                            MessageBox.Show("Lỗi khi xóa hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}