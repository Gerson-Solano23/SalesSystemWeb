-- =====================================================================
-- SalesSystemWeb - creates the MySalesCompany database from scratch:
-- tables (generated from the EF Core model), the stored procedures used by
-- the dashboard and initial data (roles, menus, admin user, a category and
-- the document counter).
--
-- Usage (SQL Server 2019+, e.g. the mcr.microsoft.com/mssql/server Docker image):
--   sqlcmd -S <server> -U sa -P <password> -C -i 000_create_database.sql
-- On Azure SQL, create the database in the portal and run the script from the
-- "Tables" section on, connected to that database (USE is not supported there).
--
-- Only for a NEW, EMPTY database: it stops without changes if the tables
-- already exist. 001_password_hash.sql is not needed after this script.
--
-- Initial login: admin@greateshop.com / Admin123!   -> change it after the first login.
-- =====================================================================

IF DB_ID(N'MySalesCompany') IS NULL
    CREATE DATABASE MySalesCompany;
GO

USE MySalesCompany;
GO

IF OBJECT_ID(N'dbo.Usuario', N'U') IS NOT NULL
BEGIN
    RAISERROR(N'MySalesCompany already has tables; nothing was changed.', 16, 1);
    SET NOEXEC ON;
END
GO

-- ---------------------------------------------------------------------
-- Tables
-- ---------------------------------------------------------------------
CREATE TABLE [dbo].[Category] (
    [idCategory] int NOT NULL IDENTITY,
    [name] varchar(50) NULL,
    [status] bit NULL DEFAULT (((1))),
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Category__79D361B6EB79E9DE] PRIMARY KEY ([idCategory])
);
GO

CREATE TABLE [dbo].[DocumentNumber] (
    [idDocumentNumber] int NOT NULL IDENTITY,
    [last_Number] int NOT NULL,
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Document__BB80F59066FEB40A] PRIMARY KEY ([idDocumentNumber])
);
GO

CREATE TABLE [dbo].[Menu] (
    [idMenu] int NOT NULL IDENTITY,
    [name] varchar(50) NULL,
    [icono] varchar(50) NULL,
    [url] varchar(50) NULL,
    CONSTRAINT [PK__Menu__C26AF48334F11DDA] PRIMARY KEY ([idMenu])
);
GO

CREATE TABLE [dbo].[Rol] (
    [idRol] int NOT NULL IDENTITY,
    [name] varchar(50) NULL,
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Rol__3C872F76C69DC798] PRIMARY KEY ([idRol])
);
GO

CREATE TABLE [dbo].[S3DetailsFile] (
    [Id] int NOT NULL IDENTITY,
    [FileDate] datetime2 NULL,
    [FileName] nvarchar(max) NOT NULL,
    [FileType] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_S3DetailsFile] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [dbo].[Sale] (
    [idSale] int NOT NULL IDENTITY,
    [numberDocument] varchar(50) NULL,
    [paymentType] varchar(50) NULL,
    [total] decimal(10,2) NULL,
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Sale__C4AEB198EAFCA875] PRIMARY KEY ([idSale])
);
GO

CREATE TABLE [dbo].[Product] (
    [idProduct] int NOT NULL IDENTITY,
    [name] varchar(100) NULL,
    [idCategory] int NULL,
    [stock] int NULL,
    [price] decimal(10,2) NULL,
    [status] bit NULL DEFAULT (((1))),
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    [img] varchar(1000) NULL,
    CONSTRAINT [PK__Product__5EEC79D12DA06D7F] PRIMARY KEY ([idProduct]),
    CONSTRAINT [FK__Product__idCateg__75A278F5] FOREIGN KEY ([idCategory]) REFERENCES [dbo].[Category] ([idCategory])
);
GO

CREATE TABLE [dbo].[MenuRol] (
    [idMenuRol] int NOT NULL IDENTITY,
    [idMenu] int NULL,
    [idRol] int NULL,
    CONSTRAINT [PK__MenuRol__9D6D61A41067E3C5] PRIMARY KEY ([idMenuRol]),
    CONSTRAINT [FK__MenuRol__idMenu__693CA210] FOREIGN KEY ([idMenu]) REFERENCES [dbo].[Menu] ([idMenu]),
    CONSTRAINT [FK__MenuRol__idRol__6A30C649] FOREIGN KEY ([idRol]) REFERENCES [dbo].[Rol] ([idRol])
);
GO

CREATE TABLE [dbo].[Usuario] (
    [idUser] int NOT NULL IDENTITY,
    [fullName] varchar(100) NULL,
    [email] varchar(50) NULL,
    [idRol] int NULL,
    [password] varchar(256) NULL,
    [status] bit NULL DEFAULT (((1))),
    [dateRegistry] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__Usuario__3717C982F22FEC6F] PRIMARY KEY ([idUser]),
    CONSTRAINT [FK__Usuario__idRol__6D0D32F4] FOREIGN KEY ([idRol]) REFERENCES [dbo].[Rol] ([idRol])
);
GO

CREATE TABLE [dbo].[SaleDetail] (
    [idSaleDetail] int NOT NULL IDENTITY,
    [idSale] int NULL,
    [idProduct] int NULL,
    [quantity] int NULL,
    [price] decimal(10,2) NULL,
    [total] decimal(10,2) NULL,
    CONSTRAINT [PK__SaleDeta__B23385CD63712103] PRIMARY KEY ([idSaleDetail]),
    CONSTRAINT [FK__SaleDetai__idPro__01142BA1] FOREIGN KEY ([idProduct]) REFERENCES [dbo].[Product] ([idProduct]),
    CONSTRAINT [FK__SaleDetai__idSal__00200768] FOREIGN KEY ([idSale]) REFERENCES [dbo].[Sale] ([idSale])
);
GO

CREATE INDEX [IX_MenuRol_idMenu] ON [dbo].[MenuRol] ([idMenu]);
GO

CREATE INDEX [IX_MenuRol_idRol] ON [dbo].[MenuRol] ([idRol]);
GO

CREATE INDEX [IX_Product_idCategory] ON [dbo].[Product] ([idCategory]);
GO

CREATE INDEX [IX_SaleDetail_idProduct] ON [dbo].[SaleDetail] ([idProduct]);
GO

CREATE INDEX [IX_SaleDetail_idSale] ON [dbo].[SaleDetail] ([idSale]);
GO

CREATE INDEX [IX_Usuario_idRol] ON [dbo].[Usuario] ([idRol]);
GO


-- ---------------------------------------------------------------------
-- Stored procedures
-- Result columns must match SalesSystem.DTO.TopProductsDTO:
-- topNumber, name, price, salequantity, totalSale.
-- ---------------------------------------------------------------------

-- Top 10 products by units sold for sales in [@start, @endExclusive).
CREATE OR ALTER PROCEDURE [dbo].[CSP_TopProducts_Between]
    @start DATETIME,
    @endExclusive DATETIME
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP (10)
        CAST(ROW_NUMBER() OVER (ORDER BY SUM(ISNULL(sd.[quantity], 0)) DESC, SUM(ISNULL(sd.[total], 0)) DESC) AS INT) AS [topNumber],
        CAST(ISNULL(p.[name], N'') AS NVARCHAR(100)) AS [name],
        CAST(ISNULL(p.[price], 0) AS DECIMAL(18, 2)) AS [price],
        CAST(SUM(ISNULL(sd.[quantity], 0)) AS INT) AS [salequantity],
        CAST(SUM(ISNULL(sd.[total], 0)) AS DECIMAL(18, 2)) AS [totalSale]
    FROM [dbo].[SaleDetail] sd
        INNER JOIN [dbo].[Sale] s ON s.[idSale] = sd.[idSale]
        INNER JOIN [dbo].[Product] p ON p.[idProduct] = sd.[idProduct]
    WHERE s.[dateRegistry] >= @start
      AND s.[dateRegistry] < @endExclusive
    GROUP BY p.[idProduct], p.[name], p.[price]
    ORDER BY [topNumber];
END
GO

-- Last 7 days, today included (same window as the dashboard summary).
CREATE OR ALTER PROCEDURE [dbo].[CSP_TopProducts_LastWeek]
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @today DATE = CAST(GETDATE() AS DATE);
    DECLARE @start DATETIME = DATEADD(DAY, -6, @today);
    DECLARE @endExclusive DATETIME = DATEADD(DAY, 1, @today);
    EXEC [dbo].[CSP_TopProducts_Between] @start, @endExclusive;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[CSP_TopProducts_Monthly]
    @month INT,
    @year INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @start DATETIME = DATEFROMPARTS(@year, @month, 1);
    DECLARE @endExclusive DATETIME = DATEADD(MONTH, 1, @start);
    EXEC [dbo].[CSP_TopProducts_Between] @start, @endExclusive;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[CSP_TopProducts_Yearly]
    @year INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @start DATETIME = DATEFROMPARTS(@year, 1, 1);
    DECLARE @endExclusive DATETIME = DATEADD(YEAR, 1, @start);
    EXEC [dbo].[CSP_TopProducts_Between] @start, @endExclusive;
END
GO

-- The API sends both dates as 'yyyy-MM-dd'; @endDate is inclusive.
CREATE OR ALTER PROCEDURE [dbo].[CSP_TopProducts_RangeDates]
    @startDate DATE,
    @endDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @start DATETIME = @startDate;
    DECLARE @endExclusive DATETIME = DATEADD(DAY, 1, @endDate);
    EXEC [dbo].[CSP_TopProducts_Between] @start, @endExclusive;
END
GO

-- Revenue per month of the current year (used by backend/ModelPrediction/model.py).
CREATE OR ALTER PROCEDURE [dbo].[CSP_GET_TotalsPerMonth]
AS
BEGIN
    SET NOCOUNT ON;
    SELECT
        MONTH(s.[dateRegistry]) AS [month],
        CAST(SUM(ISNULL(s.[total], 0)) AS DECIMAL(18, 2)) AS [total]
    FROM [dbo].[Sale] s
    WHERE YEAR(s.[dateRegistry]) = YEAR(GETDATE())
    GROUP BY MONTH(s.[dateRegistry])
    ORDER BY [month];
END
GO

-- ---------------------------------------------------------------------
-- Initial data
-- Role ids must stay 1 = Admin, 2 = Employee, 3 = Supervisor (the API
-- authorization policies use these ids). Menu URLs are the Angular routes;
-- the "Reports" menu name is used by the frontend.
-- ---------------------------------------------------------------------
BEGIN TRANSACTION;

SET IDENTITY_INSERT [dbo].[Rol] ON;
INSERT INTO [dbo].[Rol] ([idRol], [name]) VALUES
    (1, 'Admin'),
    (2, 'Employee'),
    (3, 'Supervisor');
SET IDENTITY_INSERT [dbo].[Rol] OFF;

SET IDENTITY_INSERT [dbo].[Menu] ON;
INSERT INTO [dbo].[Menu] ([idMenu], [name], [icono], [url]) VALUES
    (1, 'Dashboard',    'dashboard',     '/pages/dashboard'),
    (2, 'Users',        'group',         '/pages/users'),
    (3, 'Products',     'inventory_2',   '/pages/products'),
    (4, 'Sale',         'point_of_sale', '/pages/sale'),
    (5, 'Sale History', 'receipt_long',  '/pages/history_sale'),
    (6, 'Reports',      'summarize',     '/pages/reports'),
    (7, 'Files',        'folder',        '/pages/files'),
    (8, 'Prediction',   'insights',      '/pages/dataPrediction');
SET IDENTITY_INSERT [dbo].[Menu] OFF;

-- Same access rules as the API policies and the Angular route guards.
INSERT INTO [dbo].[MenuRol] ([idMenu], [idRol]) VALUES
    (1, 1), (2, 1), (3, 1), (4, 1), (5, 1), (6, 1), (7, 1), (8, 1),   -- Admin: everything
    (3, 3), (4, 3), (5, 3), (6, 3), (7, 3), (8, 3),                   -- Supervisor
    (4, 2), (5, 2);                                                   -- Employee: sale and history

INSERT INTO [dbo].[DocumentNumber] ([last_Number]) VALUES (0);

INSERT INTO [dbo].[Category] ([name], [status]) VALUES ('General', 1);

-- Password: Admin123! (PBKDF2 hash generated by SalesSystem.Utility.PasswordHasher)
INSERT INTO [dbo].[Usuario] ([fullName], [email], [idRol], [password], [status]) VALUES
    ('Administrator', 'admin@greateshop.com', 1,
     'PBKDF2$100000$C4J6qYf/ymIuYq1Unfe0KA==$umnFRCw2rcfTwlulvZxNq1Fy0K7upWdQ0vLUGaHfHVY=', 1);

COMMIT TRANSACTION;
GO

SET NOEXEC OFF;
GO
