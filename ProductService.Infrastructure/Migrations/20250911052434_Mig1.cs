using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProductService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ParentCategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Categories_Categories_ParentCategoryId",
                        column: x => x.ParentCategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Discounts_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    AltText = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductImages_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Rating = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "ModifiedOn", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { new Guid("b1c2e1d5-4f3b-4a96-82d3-1a234567890a"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Electronic devices and gadgets", true, null, "Electronics", null },
                    { new Guid("d7f8e3c4-6a7b-4a12-9c9f-2b3456789abc"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Smartphones and mobile devices", true, null, "Mobile Phones", new Guid("b1c2e1d5-4f3b-4a96-82d3-1a234567890a") }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedOn", "Description", "IsActive", "ModifiedOn", "Name", "Price", "SKU", "StockQuantity" },
                values: new object[,]
                {
                    { new Guid("e2f3d4c5-7a8b-4b23-8d9e-3c456789abcd"), new Guid("b1c2e1d5-4f3b-4a96-82d3-1a234567890a"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Noise-cancelling wireless headphones", true, null, "Wireless Headphones Pro", 199.99m, "ELE-WIR-2025-BCDE", 150 },
                    { new Guid("f3a4b5c6-8b9c-4c34-9e0f-4d56789abcde"), new Guid("b1c2e1d5-4f3b-4a96-82d3-1a234567890a"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "55 inch 4K UHD Smart TV", true, null, "Smart TV 55 inch", 799.99m, "ELE-SMA-2025-BCDE", 80 }
                });

            migrationBuilder.InsertData(
                table: "Discounts",
                columns: new[] { "Id", "CreatedOn", "Description", "DiscountAmount", "DiscountPercentage", "EndDate", "IsActive", "ModifiedOn", "Name", "ProductId", "StartDate" },
                values: new object[] { new Guid("a1b2c3d4-1234-5678-9abc-def012345678"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "15% off on Wireless Headphones Pro", null, 15m, new DateTime(2025, 7, 31, 23, 59, 59, 0, DateTimeKind.Utc), true, null, "Summer Sale 15% OFF", new Guid("e2f3d4c5-7a8b-4b23-8d9e-3c456789abcd"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "AltText", "CreatedOn", "ImageUrl", "IsPrimary", "ModifiedOn", "ProductId" },
                values: new object[,]
                {
                    { new Guid("c6d7e8f9-be2f-4f67-c032-789abcdef012"), null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://example.com/images/headphones_pro.jpg", true, null, new Guid("e2f3d4c5-7a8b-4b23-8d9e-3c456789abcd") },
                    { new Guid("d7e8f901-cf30-5068-d143-89abcdef0123"), null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://example.com/images/smart_tv_55.jpg", true, null, new Guid("f3a4b5c6-8b9c-4c34-9e0f-4d56789abcde") }
                });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "CreatedOn", "Description", "IsActive", "ModifiedOn", "Name", "Price", "SKU", "StockQuantity" },
                values: new object[,]
                {
                    { new Guid("a4b5c6d7-9c0d-4d45-af10-5e6789abcdef"), new Guid("d7f8e3c4-6a7b-4a12-9c9f-2b3456789abc"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Latest Super Smartphone X1 with OLED display", true, null, "Super Smartphone X1", 699.99m, "MOB-SUP-2025-BCDE", 100 },
                    { new Guid("b5c6d7e8-ad1e-4e56-bf21-6f789abcdef0"), new Guid("d7f8e3c4-6a7b-4a12-9c9f-2b3456789abc"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Affordable smartphone with great features", true, null, "Budget Smartphone Z5", 149.99m, "MOB-BUD-2025-BCDE", 200 }
                });

            migrationBuilder.InsertData(
                table: "Discounts",
                columns: new[] { "Id", "CreatedOn", "Description", "DiscountAmount", "DiscountPercentage", "EndDate", "IsActive", "ModifiedOn", "Name", "ProductId", "StartDate" },
                values: new object[] { new Guid("b2c3d4e5-2345-6789-abcd-ef0123456789"), new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Flat $50 off on Super Smartphone X1", 50m, 0m, new DateTime(2025, 8, 15, 23, 59, 59, 0, DateTimeKind.Utc), true, null, "Special Offer $50 OFF", new Guid("a4b5c6d7-9c0d-4d45-af10-5e6789abcdef"), new DateTime(2025, 7, 15, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "ProductImages",
                columns: new[] { "Id", "AltText", "CreatedOn", "ImageUrl", "IsPrimary", "ModifiedOn", "ProductId" },
                values: new object[,]
                {
                    { new Guid("e8f901a2-d041-6179-e254-9abcdef01234"), null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://example.com/images/smartphone_x1_front.jpg", true, null, new Guid("a4b5c6d7-9c0d-4d45-af10-5e6789abcdef") },
                    { new Guid("f901a2b3-e152-728a-f365-abcedf012345"), null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Utc), "https://example.com/images/budget_smartphone_z5.jpg", true, null, new Guid("b5c6d7e8-ad1e-4e56-bf21-6f789abcdef0") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_ProductId",
                table: "Discounts",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_ProductId",
                table: "Reviews",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "ProductImages");

            migrationBuilder.DropTable(
                name: "Reviews");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
