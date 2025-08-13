using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Linq;

namespace StoreX
{
    public partial class FormSales : Form
    {
        private string connectionString = "Server=LAPTOP-N6C9N5LN\\SQLEXPRESS01;Database=StoreX_Management;Trusted_Connection=True;";

        // Khai báo các DataTable để lưu trữ dữ liệu
        DataTable dtProducts = new DataTable();
        DataTable dtCustomers = new DataTable();
        DataTable dtEmployees = new DataTable();
        DataTable dtCart = new DataTable(); // DataTable cho giỏ hàng

        public FormSales()
        {
            InitializeComponent();
            LoadInitialData();
            SetupCartDataGridView();
        }

        // Tải dữ liệu ban đầu cho các ComboBox
        private void LoadInitialData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    // Tải danh sách khách hàng
                    SqlDataAdapter customerAdapter = new SqlDataAdapter("SELECT CustomerID, FullName FROM Customers", connection);
                    customerAdapter.Fill(dtCustomers);
                    cboCustomer.DataSource = dtCustomers;
                    cboCustomer.DisplayMember = "FullName";
                    cboCustomer.ValueMember = "CustomerID";

                    // Tải danh sách nhân viên
                    SqlDataAdapter employeeAdapter = new SqlDataAdapter("SELECT EmployeeID, FullName FROM Employees", connection);
                    employeeAdapter.Fill(dtEmployees);
                    cboEmployee.DataSource = dtEmployees;
                    cboEmployee.DisplayMember = "FullName";
                    cboEmployee.ValueMember = "EmployeeID";

                    // Tải danh sách sản phẩm
                    SqlDataAdapter productAdapter = new SqlDataAdapter("SELECT ProductID, ProductName, SellingPrice, InventoryQuantity FROM Products", connection);
                    productAdapter.Fill(dtProducts);
                    cboProduct.DataSource = dtProducts;
                    cboProduct.DisplayMember = "ProductName";
                    cboProduct.ValueMember = "ProductID";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi tải dữ liệu ban đầu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Cài đặt cấu trúc cho DataGridView giỏ hàng
        private void SetupCartDataGridView()
        {
            dtCart.Columns.Add("ProductID", typeof(int));
            dtCart.Columns.Add("ProductName", typeof(string));
            dtCart.Columns.Add("Quantity", typeof(int));
            dtCart.Columns.Add("UnitPrice", typeof(decimal));
            dtCart.Columns.Add("Total", typeof(decimal));

            dgvCart.DataSource = dtCart;
        }

        // Xử lý nút "Thêm vào giỏ hàng" (có thể là nút "Add" trên giao diện)
        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cboProduct.SelectedValue == null || string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                MessageBox.Show("Vui lòng chọn sản phẩm và nhập số lượng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra số lượng nhập vào có phải số không
            if (!int.TryParse(txtQuantity.Text, out int quantity))
            {
                MessageBox.Show("Số lượng phải là một số nguyên hợp lệ.", "Lỗi nhập liệu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy thông tin sản phẩm và số lượng
            int productID = Convert.ToInt32(cboProduct.SelectedValue);
            string productName = cboProduct.Text;

            // Tìm giá sản phẩm và số lượng tồn kho
            DataRow productRow = dtProducts.AsEnumerable().FirstOrDefault(r => r.Field<int>("ProductID") == productID);
            decimal unitPrice = productRow.Field<decimal>("SellingPrice");
            int inventoryQuantity = productRow.Field<int>("InventoryQuantity");

            // Kiểm tra số lượng tồn kho
            if (quantity > inventoryQuantity)
            {
                MessageBox.Show($"Số lượng tồn kho của sản phẩm '{productName}' chỉ còn {inventoryQuantity}. Vui lòng nhập lại.", "Lỗi tồn kho", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            decimal totalItemPrice = unitPrice * quantity;

            // Kiểm tra xem sản phẩm đã có trong giỏ hàng chưa
            DataRow cartRow = dtCart.AsEnumerable().FirstOrDefault(r => r.Field<int>("ProductID") == productID);
            if (cartRow != null)
            {
                // Nếu đã có, cập nhật số lượng và tổng tiền
                int oldQuantity = cartRow.Field<int>("Quantity");
                cartRow["Quantity"] = oldQuantity + quantity;
                cartRow["Total"] = (oldQuantity + quantity) * unitPrice;
            }
            else
            {
                // Nếu chưa có, thêm sản phẩm vào giỏ hàng
                dtCart.Rows.Add(productID, productName, quantity, unitPrice, totalItemPrice);
            }

            UpdateTotalAmount();
        }

        // Xử lý nút "Xóa khỏi giỏ hàng" (có thể là nút "Remove" trên giao diện)
        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count > 0)
            {
                dgvCart.Rows.RemoveAt(dgvCart.SelectedRows[0].Index);
                UpdateTotalAmount();
            }
        }

        // Tính toán tổng tiền và hiển thị
        private void UpdateTotalAmount()
        {
            decimal total = 0;
            foreach (DataRow row in dtCart.Rows)
            {
                total += row.Field<decimal>("Total");
            }
            lblTotalAmount.Text = $"{total:N0} VND";
        }

        // Xử lý nút "Tạo hóa đơn"
        private void btnCreateInvoice_Click(object sender, EventArgs e)
        {
            if (cboCustomer.SelectedValue == null || cboEmployee.SelectedValue == null || dtCart.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng, nhân viên và thêm sản phẩm vào giỏ hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int customerID = Convert.ToInt32(cboCustomer.SelectedValue);
            int employeeID = Convert.ToInt32(cboEmployee.SelectedValue);
            decimal totalAmount = dtCart.AsEnumerable().Sum(r => r.Field<decimal>("Total"));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 1. Thêm bản ghi mới vào bảng Sales
                        string insertSaleQuery = "INSERT INTO Sales (CustomerID, EmployeeID, SaleDate, TotalAmount) VALUES (@CustomerID, @EmployeeID, GETDATE(), @TotalAmount); SELECT SCOPE_IDENTITY();";
                        SqlCommand command = new SqlCommand(insertSaleQuery, connection, transaction);
                        command.Parameters.AddWithValue("@CustomerID", customerID);
                        command.Parameters.AddWithValue("@EmployeeID", employeeID);
                        command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        int saleID = Convert.ToInt32(command.ExecuteScalar());

                        // 2. Thêm từng sản phẩm vào bảng SaleDetails và cập nhật số lượng tồn kho

                        string insertDetailQuery = "INSERT INTO SaleDetails (SaleID, ProductID, Quantity, UnitPrice) VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice);";

                        string updateInventoryQuery = "UPDATE Products SET InventoryQuantity = InventoryQuantity - @Quantity WHERE ProductID = @ProductID;";

                        foreach (DataRow row in dtCart.Rows)
                        {
                            // Thêm vào SaleDetails
                            SqlCommand detailCommand = new SqlCommand(insertDetailQuery, connection, transaction);
                            detailCommand.Parameters.AddWithValue("@SaleID", saleID);
                            detailCommand.Parameters.AddWithValue("@ProductID", row.Field<int>("ProductID"));
                            detailCommand.Parameters.AddWithValue("@Quantity", row.Field<int>("Quantity"));
                            detailCommand.Parameters.AddWithValue("@UnitPrice", row.Field<decimal>("UnitPrice"));
                            detailCommand.ExecuteNonQuery();

                            // Cập nhật tồn kho
                            SqlCommand inventoryCommand = new SqlCommand(updateInventoryQuery, connection, transaction);
                            inventoryCommand.Parameters.AddWithValue("@Quantity", row.Field<int>("Quantity"));
                            inventoryCommand.Parameters.AddWithValue("@ProductID", row.Field<int>("ProductID"));
                            inventoryCommand.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Đã tạo hóa đơn thành công!");

                        // Dọn dẹp giao diện sau khi tạo hóa đơn thành công
                        dtCart.Clear();
                        UpdateTotalAmount();
                        LoadInitialData(); // Tải lại dữ liệu để cập nhật tồn kho
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Lỗi khi tạo hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            FormStatistics formStatistics = new FormStatistics();
            formStatistics.Show();
        }

        private void btnCreateInvoice_Click_1(object sender, EventArgs e)
        {
            // 1. Kiểm tra dữ liệu đầu vào
            if (cboCustomer.SelectedValue == null || cboEmployee.SelectedValue == null || dtCart.Rows.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn khách hàng, nhân viên và thêm sản phẩm vào giỏ hàng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int customerID = Convert.ToInt32(cboCustomer.SelectedValue);
            int employeeID = Convert.ToInt32(cboEmployee.SelectedValue);
            decimal totalAmount = dtCart.AsEnumerable().Sum(r => r.Field<decimal>("Total"));

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    // Sử dụng SqlTransaction để đảm bảo các thao tác với database được thực hiện đồng bộ
                    SqlTransaction transaction = connection.BeginTransaction();

                    try
                    {
                        // 2. Thêm bản ghi mới vào bảng Sales
                        string insertSaleQuery = "INSERT INTO Sales (CustomerID, EmployeeID, SaleDate, TotalAmount) VALUES (@CustomerID, @EmployeeID, GETDATE(), @TotalAmount); SELECT SCOPE_IDENTITY();";
                        SqlCommand command = new SqlCommand(insertSaleQuery, connection, transaction);
                        command.Parameters.AddWithValue("@CustomerID", customerID);
                        command.Parameters.AddWithValue("@EmployeeID", employeeID);
                        command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                        int saleID = Convert.ToInt32(command.ExecuteScalar());

                        // 3. Thêm từng sản phẩm vào bảng SaleDetails và cập nhật số lượng tồn kho
                        string insertDetailQuery = "INSERT INTO SaleDetails (SaleID, ProductID, Quantity, UnitPrice) VALUES (@SaleID, @ProductID, @Quantity, @UnitPrice);";
                        string updateInventoryQuery = "UPDATE Products SET InventoryQuantity = InventoryQuantity - @Quantity WHERE ProductID = @ProductID;";

                        foreach (DataRow row in dtCart.Rows)
                        {
                            // Thêm vào SaleDetails
                            SqlCommand detailCommand = new SqlCommand(insertDetailQuery, connection, transaction);
                            detailCommand.Parameters.AddWithValue("@SaleID", saleID);
                            detailCommand.Parameters.AddWithValue("@ProductID", row.Field<int>("ProductID"));
                            detailCommand.Parameters.AddWithValue("@Quantity", row.Field<int>("Quantity"));
                            detailCommand.Parameters.AddWithValue("@UnitPrice", row.Field<decimal>("UnitPrice"));
                            detailCommand.ExecuteNonQuery();

                            // Cập nhật tồn kho
                            SqlCommand inventoryCommand = new SqlCommand(updateInventoryQuery, connection, transaction);
                            inventoryCommand.Parameters.AddWithValue("@Quantity", row.Field<int>("Quantity"));
                            inventoryCommand.Parameters.AddWithValue("@ProductID", row.Field<int>("ProductID"));
                            inventoryCommand.ExecuteNonQuery();
                        }

                        // 4. Hoàn tất giao dịch
                        transaction.Commit();
                        MessageBox.Show("Đã tạo hóa đơn thành công!");

                        // 5. Dọn dẹp giao diện sau khi tạo hóa đơn thành công
                        dtCart.Clear();
                        UpdateTotalAmount();
                        LoadInitialData(); // Tải lại dữ liệu để cập nhật tồn kho
                    }
                    catch (Exception ex)
                    {
                        // Nếu có lỗi, hủy bỏ tất cả các thay đổi
                        transaction.Rollback();
                        MessageBox.Show("Lỗi khi tạo hóa đơn: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi kết nối cơ sở dữ liệu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}