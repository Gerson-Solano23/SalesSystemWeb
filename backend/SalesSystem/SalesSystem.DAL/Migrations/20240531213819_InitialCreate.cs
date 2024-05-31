using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SalesSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    idCategory = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    status = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Category__79D361B6EB79E9DE", x => x.idCategory);
                });

            migrationBuilder.CreateTable(
                name: "DocumentNumber",
                columns: table => new
                {
                    idDocumentNumber = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    lastNumber = table.Column<int>(name: "last_Number", type: "int", nullable: false),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Document__BB80F59066FEB40A", x => x.idDocumentNumber);
                });

            migrationBuilder.CreateTable(
                name: "Menu",
                columns: table => new
                {
                    idMenu = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    icono = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    url = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Menu__C26AF48334F11DDA", x => x.idMenu);
                });

            migrationBuilder.CreateTable(
                name: "Rol",
                columns: table => new
                {
                    idRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Rol__3C872F76C69DC798", x => x.idRol);
                });

            migrationBuilder.CreateTable(
                name: "Sale",
                columns: table => new
                {
                    idSale = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    numeroDocumento = table.Column<string>(name: "numero_Documento", type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    paymentType = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    total = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sale__C4AEB198EAFCA875", x => x.idSale);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    idProduct = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    idCategory = table.Column<int>(type: "int", nullable: true),
                    stock = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    status = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Product__5EEC79D12DA06D7F", x => x.idProduct);
                    table.ForeignKey(
                        name: "FK__Product__idCateg__75A278F5",
                        column: x => x.idCategory,
                        principalTable: "Category",
                        principalColumn: "idCategory");
                });

            migrationBuilder.CreateTable(
                name: "MenuRol",
                columns: table => new
                {
                    idMenuRol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idMenu = table.Column<int>(type: "int", nullable: true),
                    idRol = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__MenuRol__9D6D61A41067E3C5", x => x.idMenuRol);
                    table.ForeignKey(
                        name: "FK__MenuRol__idMenu__693CA210",
                        column: x => x.idMenu,
                        principalTable: "Menu",
                        principalColumn: "idMenu");
                    table.ForeignKey(
                        name: "FK__MenuRol__idRol__6A30C649",
                        column: x => x.idRol,
                        principalTable: "Rol",
                        principalColumn: "idRol");
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                columns: table => new
                {
                    idUser = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fullName = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true),
                    email = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    idRol = table.Column<int>(type: "int", nullable: true),
                    password = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    status = table.Column<bool>(type: "bit", nullable: true, defaultValueSql: "((1))"),
                    dateRegistry = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Usuario__3717C982F22FEC6F", x => x.idUser);
                    table.ForeignKey(
                        name: "FK__Usuario__idRol__6D0D32F4",
                        column: x => x.idRol,
                        principalTable: "Rol",
                        principalColumn: "idRol");
                });

            migrationBuilder.CreateTable(
                name: "SaleDetail",
                columns: table => new
                {
                    idSaleDetail = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idSale = table.Column<int>(type: "int", nullable: true),
                    idProduct = table.Column<int>(type: "int", nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: true),
                    price = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    total = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SaleDeta__B23385CD63712103", x => x.idSaleDetail);
                    table.ForeignKey(
                        name: "FK__SaleDetai__idPro__01142BA1",
                        column: x => x.idProduct,
                        principalTable: "Product",
                        principalColumn: "idProduct");
                    table.ForeignKey(
                        name: "FK__SaleDetai__idSal__00200768",
                        column: x => x.idSale,
                        principalTable: "Sale",
                        principalColumn: "idSale");
                });

            migrationBuilder.CreateIndex(
                name: "IX_MenuRol_idMenu",
                table: "MenuRol",
                column: "idMenu");

            migrationBuilder.CreateIndex(
                name: "IX_MenuRol_idRol",
                table: "MenuRol",
                column: "idRol");

            migrationBuilder.CreateIndex(
                name: "IX_Product_idCategory",
                table: "Product",
                column: "idCategory");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetail_idProduct",
                table: "SaleDetail",
                column: "idProduct");

            migrationBuilder.CreateIndex(
                name: "IX_SaleDetail_idSale",
                table: "SaleDetail",
                column: "idSale");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_idRol",
                table: "Usuario",
                column: "idRol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DocumentNumber");

            migrationBuilder.DropTable(
                name: "MenuRol");

            migrationBuilder.DropTable(
                name: "SaleDetail");

            migrationBuilder.DropTable(
                name: "Usuario");

            migrationBuilder.DropTable(
                name: "Menu");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "Sale");

            migrationBuilder.DropTable(
                name: "Rol");

            migrationBuilder.DropTable(
                name: "Category");
        }
    }
}
