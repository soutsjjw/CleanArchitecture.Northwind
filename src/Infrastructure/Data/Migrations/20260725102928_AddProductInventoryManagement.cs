using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CleanArchitecture.Northwind.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProductInventoryManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Suppliers",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Picture",
                table: "Products",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PictureContentType",
                table: "Products",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Products",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "InventoryTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    TransactionType = table.Column<int>(type: "int", nullable: false),
                    QuantityBefore = table.Column<short>(type: "smallint", nullable: false),
                    QuantityDelta = table.Column<short>(type: "smallint", nullable: false),
                    QuantityAfter = table.Column<short>(type: "smallint", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    SourceDocumentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceDocumentId = table.Column<int>(type: "int", nullable: true),
                    IsDelete = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InventoryTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InventoryTransactions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InventoryTransactions_ProductId_Created",
                table: "InventoryTransactions",
                columns: new[] { "ProductId", "Created" });

            migrationBuilder.Sql("""
                INSERT INTO [InventoryTransactions]
                    ([ProductId], [TransactionType], [QuantityBefore], [QuantityDelta], [QuantityAfter], [Reason], [SourceDocumentType], [SourceDocumentId], [IsDelete], [Created], [CreatedBy], [LastModified], [LastModifiedBy])
                SELECT [ProductID], 0, COALESCE([UnitsInStock], 0), 0, COALESCE([UnitsInStock], 0), N'Initial inventory balance', NULL, NULL, 0, SYSUTCDATETIME(), N'system:initial-balance', NULL, NULL
                FROM [Products];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InventoryTransactions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Picture",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "PictureContentType",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");

        }
    }
}
