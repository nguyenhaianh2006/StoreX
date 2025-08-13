CREATE DATABASE StoreX_Management;
-- T?o b?ng UserGroups
CREATE TABLE UserGroups (
    GroupID INT PRIMARY KEY IDENTITY,
    GroupName NVARCHAR(50) NOT NULL
);
-- T?o b?ng Users (dang nh?p h? th?ng)
CREATE TABLE Users (
    UserID INT PRIMARY KEY IDENTITY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    GroupID INT NOT NULL,
    FOREIGN KEY (GroupID) REFERENCES UserGroups(GroupID)
);
-- T?o b?ng Employees
CREATE TABLE Employees (
    EmployeeID INT PRIMARY KEY IDENTITY,
    EmployeeCode NVARCHAR(20) UNIQUE NOT NULL,
    FullName NVARCHAR(100),
    Position NVARCHAR(50),
    Authority NVARCHAR(50),
    UserID INT UNIQUE,  -- liên k?t v?i tài kho?n dang nh?p
    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
-- T?o b?ng Customers
CREATE TABLE Customers (
    CustomerID INT PRIMARY KEY IDENTITY,
    CustomerCode NVARCHAR(20) UNIQUE NOT NULL,
    FullName NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255)
);
-- T?o b?ng Products
CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY,
    ProductCode NVARCHAR(20) UNIQUE NOT NULL,
    ProductName NVARCHAR(100),
    SellingPrice DECIMAL(18, 2),
    InventoryQuantity INT DEFAULT 0
);
-- T?o b?ng ProductImages
CREATE TABLE ProductImages (
    ImageID INT PRIMARY KEY IDENTITY,
    ProductID INT,
    ImagePath NVARCHAR(255),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
-- T?o b?ng Sales (luu l?ch s? mua hàng)
CREATE TABLE Sales (
    SaleID INT PRIMARY KEY IDENTITY,
    CustomerID INT,
    EmployeeID INT,
    SaleDate DATE,
    TotalAmount DECIMAL(18, 2),
    FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID),
    FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID)
);
-- T?o b?ng SaleDetails (chi ti?t s?n ph?m trong m?i don hàng)
CREATE TABLE SaleDetails (
    SaleDetailID INT PRIMARY KEY IDENTITY,
    SaleID INT,
    ProductID INT,
    Quantity INT,
    UnitPrice DECIMAL(18, 2),
    FOREIGN KEY (SaleID) REFERENCES Sales(SaleID),
    FOREIGN KEY (ProductID) REFERENCES Products(ProductID)
);
-- Thêm d? li?u nhóm ngu?i dùng (UserGroups)
INSERT INTO UserGroups (GroupName) VALUES
('Admin'),
('Sales'),
('Warehouse');
-- Thêm tài kho?n ngu?i dùng (Users)
INSERT INTO Users (Username, PasswordHash, GroupID) VALUES
('admin01', 'adminpass123', 1),       -- Admin
('sales01', 'salespass123', 2),       -- Sales
('warehouse01', 'warehousepass123', 3); -- Warehouse
--  Thêm nhân viên (Employees)
INSERT INTO Employees (EmployeeCode, FullName, Position, Authority, UserID) VALUES
('EMP001', 'Nguyen Hai Anh', 'Manager', 'Full', 1),
('EMP002', 'Nguyen Quynh Anh', 'Sales Staff', 'Limited', 2),
('EMP003', 'Bui Xuan Binh', 'Warehouse Staff', 'Limited', 3);
-- Thêm khách hàng (Customers)
INSERT INTO Customers (CustomerCode, FullName, Phone, Address) VALUES
('CUST001', 'Nguyen Duc Minh', '0912345678', 'Ha Noi'),
('CUST002', 'Tran Minh Quang', '0987654321', 'Vinh Phuc');
-- Thêm s?n ph?m (Products)
INSERT INTO Products (ProductCode, ProductName, SellingPrice, InventoryQuantity) VALUES
('P001', 'Ban hoc sinh', 1500000, 20),
('P002', 'Ghe xoay van phong', 850000, 15),
('P003', 'Tu sach mini', 1200000, 10);
-- Thêm ?nh s?n ph?m (ProductImages)
INSERT INTO ProductImages (ProductID, ImagePath) VALUES
(1, 'images/ban-hoc.jpg'),
(2, 'images/ghe-xoay.jpg'),
(3, 'images/tu-sach.jpg');
-- Them don ban hàng (Sales)
INSERT INTO Sales (CustomerID, EmployeeID, SaleDate, TotalAmount) VALUES
(1, 2, '2025-06-15', 3000000),
(2, 2, '2025-06-16', 850000);
-- Thêm chi tiet don hang (SaleDetails)
INSERT INTO SaleDetails (SaleID, ProductID, Quantity, UnitPrice) VALUES
(1, 1, 2, 1500000),  -- 2 Bàn hoc sinh x 1.5tr
(2, 2, 1, 850000);   -- 1 Ghe xoay x 850k

SELECT * FROM UserGroups;
SELECT * FROM Users;
SELECT * FROM Employees;
SELECT * FROM Customers;
SELECT * FROM Products;
SELECT * FROM ProductImages;
SELECT * FROM Sales;
SELECT * FROM SaleDetails;

-- Users ? Khóa ph? d?n UserGroups(GroupID)
ALTER TABLE Users
ADD CONSTRAINT FK_Users_UserGroups
FOREIGN KEY (GroupID) REFERENCES UserGroups(GroupID);

--  Employees ? Khóa ph? d?n Users(UserID)
ALTER TABLE Employees
ADD CONSTRAINT FK_Employees_Users
FOREIGN KEY (UserID) REFERENCES Users(UserID);

--  ProductImages ? Khóa ph? d?n Products(ProductID)
ALTER TABLE ProductImages
ADD CONSTRAINT FK_ProductImages_Products
FOREIGN KEY (ProductID) REFERENCES Products(ProductID);

-- Sales ? Khóa ph? d?n Customers(CustomerID) và Employees(EmployeeID)
ALTER TABLE Sales
ADD CONSTRAINT FK_Sales_Customers
FOREIGN KEY (CustomerID) REFERENCES Customers(CustomerID);

ALTER TABLE Sales
ADD CONSTRAINT FK_Sales_Employees
FOREIGN KEY (EmployeeID) REFERENCES Employees(EmployeeID);

-- SaleDetails ? Khóa ph? d?n Sales(SaleID) và Products(ProductID)
ALTER TABLE SaleDetails
ADD CONSTRAINT FK_SaleDetails_Sales
FOREIGN KEY (SaleID) REFERENCES Sales(SaleID);

ALTER TABLE SaleDetails
ADD CONSTRAINT FK_SaleDetails_Products
FOREIGN KEY (ProductID) REFERENCES Products(ProductID);