// Toàn bộ file UC_Products.cs sau khi sửa
using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace StoreX
{
    public partial class UC_Products : UserControl
    {
        private string connectionString = "Server=LAPTOP-N6C9N5LN\\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";

        SqlDataAdapter adapter = null;
        DataTable dtProducts = null;

        public UC_Products()
        {
            InitializeComponent();
            LoadProductData();
        }

        private void LoadProductData()
        {
            try
            {
                // Câu lệnh SQL để lấy dữ liệu từ bảng Products
                string query = "SELECT ProductID, ProductCode, ProductName, SellingPrice, InventoryQuantity FROM Products;";

                adapter = new SqlDataAdapter(query, connectionString);
                dtProducts = new DataTable();
                SqlCommandBuilder builder = new SqlCommandBuilder(adapter);
                adapter.Fill(dtProducts);

                // Giả sử DataGridView của bạn có tên là dgvProducts
                dgvProducts.DataSource = dtProducts;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PHƯƠNG THỨC NÚT THÊM ĐÃ SỬA
        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Khai báo biến để lưu giá trị số đã được chuyển đổi
            decimal sellingPrice;
            int inventoryQuantity;

            // Kiểm tra và chuyển đổi giá trị từ textbox một cách an toàn
            if (decimal.TryParse(txtPrice.Text, out sellingPrice) && int.TryParse(txtQuantity.Text, out inventoryQuantity))
            {
                try
                {
                    DataRow newRow = dtProducts.NewRow();
                    newRow["ProductCode"] = "PRO" + (dtProducts.Rows.Count + 1).ToString("D3");
                    newRow["ProductName"] = txtProductName.Text;
                    newRow["SellingPrice"] = sellingPrice;
                    newRow["InventoryQuantity"] = inventoryQuantity;
                    dtProducts.Rows.Add(newRow);

                    adapter.Update(dtProducts);
                    MessageBox.Show("Đã thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi thêm sản phẩm vào cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Giá bán và Số lượng phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // PHƯƠNG THỨC NÚT SỬA ĐÃ SỬA
        private void btnEdit_Click(object sender, EventArgs e)
        {
            // Lấy dữ liệu từ các TextBox
            string productName = txtProductName.Text;
            string priceText = txtPrice.Text;
            string quantityText = txtQuantity.Text;

            // Kiểm tra và chuyển đổi dữ liệu
            if (string.IsNullOrWhiteSpace(productName) || string.IsNullOrWhiteSpace(priceText) || string.IsNullOrWhiteSpace(quantityText))
            {
                MessageBox.Show("Vui lòng điền đầy đủ thông tin sản phẩm!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                decimal price = Convert.ToDecimal(priceText);
                int quantity = Convert.ToInt32(quantityText);

                if (price <= 0 || quantity < 0)
                {
                    MessageBox.Show("Giá bán và Số lượng phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Thực hiện thêm sản phẩm vào database
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Products SET ProductName = @ProductName, SellingPrice = @SellingPrice, InventoryQuantity = @InventoryQuantity WHERE ProductID = @ProductID;";
                    SqlCommand command = new SqlCommand(query, connection);

                    // Tạo ProductCode tự động (ví dụ: P00x)
                    string productCode = "P" + (dgvProducts.Rows.Count + 1).ToString("D3");
                    command.Parameters.AddWithValue("@ProductCode", productCode);
                    command.Parameters.AddWithValue("@ProductName", productName);
                    command.Parameters.AddWithValue("@SellingPrice", price);
                    command.Parameters.AddWithValue("@InventoryQuantity", quantity);

                    command.ExecuteNonQuery();

                    MessageBox.Show("Đã thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tải lại dữ liệu để cập nhật DataGridView
                    LoadProductData();

                    // Xóa dữ liệu trên các TextBox
                    txtProductName.Clear();
                    txtPrice.Clear();
                    txtQuantity.Clear();
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Giá bán và Số lượng phải là số hợp lệ!", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi thêm sản phẩm vào cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // PHƯƠNG THỨC NÚT XÓA ĐÃ SỬA
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.SelectedRows.Count > 0)
            {
                try
                {
                    // Lấy dòng đang chọn và xóa nó khỏi DataTable
                    DataRowView rowToDelete = (DataRowView)dgvProducts.SelectedRows[0].DataBoundItem;
                    rowToDelete.Delete();

                    // Cập nhật thay đổi vào database
                    adapter.Update(dtProducts);
                    MessageBox.Show("Đã xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi khi xóa sản phẩm: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}