using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CancellationPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowedCancellationDays = table.Column<int>(type: "int", nullable: false),
                    PenaltyPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CancellationStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Carts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Carts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DiscountName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscountType = table.Column<int>(type: "int", maxLength: 50, nullable: false),
                    DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    MinimumAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReasonMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReasonType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReasonText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReasonMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PolicyName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    AllowedReturnDays = table.Column<int>(type: "int", nullable: false),
                    RestockingFee = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReturnStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Taxes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaxName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AppliesToProduct = table.Column<bool>(type: "bit", nullable: false),
                    AppliesToShipping = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValidTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CartItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CartId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CartItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CartItems_Carts_CartId",
                        column: x => x.CartId,
                        principalTable: "Carts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ShippingCharges = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderStatusId = table.Column<int>(type: "int", nullable: false),
                    ShippingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingAddressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationPolicyId = table.Column<int>(type: "int", nullable: true),
                    ReturnPolicyId = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_CancellationPolicies_CancellationPolicyId",
                        column: x => x.CancellationPolicyId,
                        principalTable: "CancellationPolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_OrderStatuses_OrderStatusId",
                        column: x => x.OrderStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Orders_ReturnPolicies_ReturnPolicyId",
                        column: x => x.ReturnPolicyId,
                        principalTable: "ReturnPolicies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Cancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancellationStatusId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsPartial = table.Column<bool>(type: "bit", nullable: false),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchaseTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CancellationCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalRefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CancellationPolicyId = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cancellations_CancellationPolicies_CancellationPolicyId",
                        column: x => x.CancellationPolicyId,
                        principalTable: "CancellationPolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Cancellations_CancellationStatuses_CancellationStatusId",
                        column: x => x.CancellationStatusId,
                        principalTable: "CancellationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cancellations_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Cancellations_ReasonMasters_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "ReasonMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    InvoicePdfUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    PriceAtPurchase = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountedPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ItemStatusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderItems_OrderStatuses_ItemStatusId",
                        column: x => x.ItemStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrderStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatusId = table.Column<int>(type: "int", nullable: false),
                    NewStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_OrderStatuses_NewStatusId",
                        column: x => x.NewStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_OrderStatuses_OldStatusId",
                        column: x => x.OldStatusId,
                        principalTable: "OrderStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrderStatusHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Returns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnStatusId = table.Column<int>(type: "int", nullable: false),
                    ReasonId = table.Column<int>(type: "int", nullable: false),
                    IsPartial = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseTotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RestockingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalRefundableAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RejectionRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnPolicyId = table.Column<int>(type: "int", nullable: true),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Returns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Returns_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Returns_ReasonMasters_ReasonId",
                        column: x => x.ReasonId,
                        principalTable: "ReasonMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Returns_ReturnPolicies_ReturnPolicyId",
                        column: x => x.ReturnPolicyId,
                        principalTable: "ReturnPolicies",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Returns_ReturnStatuses_ReturnStatusId",
                        column: x => x.ReturnStatusId,
                        principalTable: "ReturnStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shipments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CarrierName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ShipmentStatusId = table.Column<int>(type: "int", nullable: false),
                    EstimatedDeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shipments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shipments_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Shipments_ShipmentStatuses_ShipmentStatusId",
                        column: x => x.ShipmentStatusId,
                        principalTable: "ShipmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CancellationItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancellationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CancellationCharge = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CancellationItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CancellationItems_Cancellations_CancellationId",
                        column: x => x.CancellationId,
                        principalTable: "Cancellations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CancellationItems_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancellationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RefundDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundStatusId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Refunds_Cancellations_CancellationId",
                        column: x => x.CancellationId,
                        principalTable: "Cancellations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Refunds_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Refunds_RefundStatuses_RefundStatusId",
                        column: x => x.RefundStatusId,
                        principalTable: "RefundStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Refunds_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ReturnItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReturnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RestockingFee = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReturnItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReturnItems_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReturnItems_Returns_ReturnId",
                        column: x => x.ReturnId,
                        principalTable: "Returns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuantityShipped = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_OrderItems_OrderItemId",
                        column: x => x.OrderItemId,
                        principalTable: "OrderItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShipmentItems_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShipmentStatusHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShipmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OldStatusId = table.Column<int>(type: "int", nullable: false),
                    NewStatusId = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShipmentStatusHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShipmentStatusHistories_ShipmentStatuses_NewStatusId",
                        column: x => x.NewStatusId,
                        principalTable: "ShipmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentStatusHistories_ShipmentStatuses_OldStatusId",
                        column: x => x.OldStatusId,
                        principalTable: "ShipmentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShipmentStatusHistories_Shipments_ShipmentId",
                        column: x => x.ShipmentId,
                        principalTable: "Shipments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CancellationPolicies",
                columns: new[] { "Id", "AllowedCancellationDays", "CreatedAt", "Description", "IsActive", "ModifiedAt", "PenaltyPercentage", "PolicyName" },
                values: new object[,]
                {
                    { 1, 3, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cancellations allowed before shipment only", true, null, 10m, "Standard Cancellation Policy" },
                    { 2, 0, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "No cancellations allowed once order is shipped", true, null, 100m, "No Cancellation After Shipment" },
                    { 3, 1, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Cancellations allowed up to 24 hours before delivery", true, null, 5m, "Flexible Cancellation" },
                    { 4, 0, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "No cancellations or returns allowed on final sale items", true, null, 100m, "No Cancellation - Final Sale" },
                    { 5, 3, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Partial cancellation allowed before shipment with 15% penalty", true, null, 15m, "Partial Cancellation Allowed" }
                });

            migrationBuilder.InsertData(
                table: "CancellationStatuses",
                columns: new[] { "Id", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Cancellation requested", "Pending" },
                    { 2, "Cancellation approved", "Approved" },
                    { 3, "Cancellation rejected", "Rejected" },
                    { 4, "Refund has been processed", "Refunded" },
                    { 5, "Partial refund has been processed", "PartiallyRefunded" }
                });

            migrationBuilder.InsertData(
                table: "Discounts",
                columns: new[] { "Id", "CreatedAt", "Description", "DiscountAmount", "DiscountName", "DiscountPercentage", "DiscountType", "EndDate", "IsActive", "MinimumAmount", "ModifiedAt", "StartDate" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "10% off on all products", null, "New Year 10% Off", 10m, 2, new DateTime(2025, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 500m, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 2, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Flat $50 off on orders above $1000", 50m, "Flat $50 Off", null, 1, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1000m, null, new DateTime(2025, 6, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 3, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Save 15% on all summer collection", null, "Summer Sale 15% Off", 15m, 2, new DateTime(2025, 7, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 0m, null, new DateTime(2025, 7, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 4, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Buy two items and get one free on select products", null, "Buy 2 Get 1 Free", 0m, 1, new DateTime(2025, 8, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 0m, null, new DateTime(2025, 8, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 5, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Flat 20% discount on orders above $1500 during festive season", null, "Festive Season Flat 20% Off", 20m, 2, new DateTime(2025, 10, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 1500m, null, new DateTime(2025, 10, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 6, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Up to 30% off on clearance items", null, "Clearance Sale 30% Off", 30m, 2, new DateTime(2025, 11, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 0m, null, new DateTime(2025, 11, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) },
                    { 7, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "$25 off on first order above $100", 25m, "New Customer $25 Off", null, 1, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), true, 100m, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) }
                });

            migrationBuilder.InsertData(
                table: "OrderStatuses",
                columns: new[] { "Id", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Order has been placed but not yet confirmed", "Pending" },
                    { 2, "Order has been confirmed", "Confirmed" },
                    { 3, "Order has been packed", "Packed" },
                    { 4, "Order has been shipped", "Shipped" },
                    { 5, "Order has been delivered", "Delivered" },
                    { 6, "Order has been cancelled", "Cancelled" },
                    { 7, "Order has been partiallly cancelled", "PartialCancelled" },
                    { 8, "Order has been returned", "Returned" },
                    { 9, "Order has been partiallly returned", "PartialReturned" }
                });

            migrationBuilder.InsertData(
                table: "ReasonMasters",
                columns: new[] { "Id", "ReasonText", "ReasonType" },
                values: new object[,]
                {
                    { 1, "Ordered by mistake", "Cancellation" },
                    { 2, "Found better price elsewhere", "Cancellation" },
                    { 3, "Shipping took too long", "Cancellation" },
                    { 4, "Changed my mind", "Cancellation" },
                    { 5, "Item no longer needed", "Cancellation" },
                    { 6, "Payment issues", "Cancellation" },
                    { 7, "Product defective", "Return" },
                    { 8, "Product not as described", "Return" },
                    { 9, "Wrong item delivered", "Return" },
                    { 10, "Damaged during shipping", "Return" },
                    { 11, "Missing parts or accessories", "Return" },
                    { 12, "Product expired or near expiry", "Return" },
                    { 13, "Received extra item", "Return" }
                });

            migrationBuilder.InsertData(
                table: "RefundStatuses",
                columns: new[] { "Id", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Refund requested but not yet processed", "Pending" },
                    { 2, "Refund is being processed", "Processing" },
                    { 3, "Refund completed successfully", "Completed" },
                    { 4, "Refund failed", "Failed" },
                    { 5, "Refund request cancelled", "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "ReturnPolicies",
                columns: new[] { "Id", "AllowedReturnDays", "CreatedAt", "Description", "IsActive", "ModifiedAt", "PolicyName", "RestockingFee" },
                values: new object[,]
                {
                    { 1, 7, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Returns accepted within 7 days", true, null, "Standard Return Policy", 5m },
                    { 2, 3, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Returns only allowed within 3 days", true, null, "No Returns After 3 Days", 0m },
                    { 3, 30, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Returns accepted within 30 days for electronics", true, null, "Extended Return Policy", 10m },
                    { 4, 0, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "No returns allowed on clearance and discount items", true, null, "No Returns on Clearance", 0m },
                    { 5, 15, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Returns allowed only if exchanged within 15 days", true, null, "Return with Exchange Only", 5m }
                });

            migrationBuilder.InsertData(
                table: "ReturnStatuses",
                columns: new[] { "Id", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Return requested", "Pending" },
                    { 2, "Return approved", "Approved" },
                    { 3, "Returned item received", "ItemReceived" },
                    { 4, "Returned item inspected", "Inspected" },
                    { 5, "Refund processed", "Refunded" },
                    { 6, "Return rejected", "Rejected" },
                    { 7, "Return completed", "Completed" }
                });

            migrationBuilder.InsertData(
                table: "ShipmentStatuses",
                columns: new[] { "Id", "Description", "StatusName" },
                values: new object[,]
                {
                    { 1, "Shipment is created but not yet dispatched", "Pending" },
                    { 2, "Shipment has been shipped from warehouse", "Shipped" },
                    { 3, "Shipment is currently in transit", "InTransit" },
                    { 4, "Shipment is out for delivery with courier", "OutForDelivery" },
                    { 5, "Shipment has been delivered to customer", "Delivered" },
                    { 6, "Shipment has been returned to sender", "Returned" },
                    { 7, "Shipment was cancelled", "Cancelled" }
                });

            migrationBuilder.InsertData(
                table: "Taxes",
                columns: new[] { "Id", "AppliesToProduct", "AppliesToShipping", "CreatedAt", "Description", "IsActive", "ModifiedAt", "TaxName", "TaxPercentage", "ValidFrom", "ValidTo" },
                values: new object[,]
                {
                    { 1, true, false, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Central GST", true, null, "CGST", 9m, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 2, true, false, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "State GST", true, null, "SGST", 9m, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null },
                    { 3, false, true, new DateTime(2025, 7, 24, 0, 0, 0, 0, DateTimeKind.Utc), "Tax on shipping charges", true, null, "Shipping Tax", 5m, new DateTime(2023, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_CancellationItems_CancellationId",
                table: "CancellationItems",
                column: "CancellationId");

            migrationBuilder.CreateIndex(
                name: "IX_CancellationItems_OrderItemId",
                table: "CancellationItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_CancellationPolicyId",
                table: "Cancellations",
                column: "CancellationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_CancellationStatusId",
                table: "Cancellations",
                column: "CancellationStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_OrderId",
                table: "Cancellations",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_ReasonId",
                table: "Cancellations",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_CartItems_CartId",
                table: "CartItems",
                column: "CartId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OrderId",
                table: "Invoices",
                column: "OrderId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_ItemStatusId",
                table: "OrderItems",
                column: "ItemStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_OrderId",
                table: "OrderItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CancellationPolicyId",
                table: "Orders",
                column: "CancellationPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderStatusId",
                table: "Orders",
                column: "OrderStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ReturnPolicyId",
                table: "Orders",
                column: "ReturnPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_NewStatusId",
                table: "OrderStatusHistories",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OldStatusId",
                table: "OrderStatusHistories",
                column: "OldStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderStatusHistories_OrderId",
                table: "OrderStatusHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_CancellationId",
                table: "Refunds",
                column: "CancellationId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_OrderId",
                table: "Refunds",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_RefundStatusId",
                table: "Refunds",
                column: "RefundStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ReturnId",
                table: "Refunds",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_OrderItemId",
                table: "ReturnItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ReturnItems_ReturnId",
                table: "ReturnItems",
                column: "ReturnId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_OrderId",
                table: "Returns",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ReasonId",
                table: "Returns",
                column: "ReasonId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ReturnPolicyId",
                table: "Returns",
                column: "ReturnPolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Returns_ReturnStatusId",
                table: "Returns",
                column: "ReturnStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_OrderItemId",
                table: "ShipmentItems",
                column: "OrderItemId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentItems_ShipmentId",
                table: "ShipmentItems",
                column: "ShipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_OrderId",
                table: "Shipments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Shipments_ShipmentStatusId",
                table: "Shipments",
                column: "ShipmentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistories_NewStatusId",
                table: "ShipmentStatusHistories",
                column: "NewStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistories_OldStatusId",
                table: "ShipmentStatusHistories",
                column: "OldStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_ShipmentStatusHistories_ShipmentId",
                table: "ShipmentStatusHistories",
                column: "ShipmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CancellationItems");

            migrationBuilder.DropTable(
                name: "CartItems");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "OrderStatusHistories");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "ReturnItems");

            migrationBuilder.DropTable(
                name: "ShipmentItems");

            migrationBuilder.DropTable(
                name: "ShipmentStatusHistories");

            migrationBuilder.DropTable(
                name: "Taxes");

            migrationBuilder.DropTable(
                name: "Carts");

            migrationBuilder.DropTable(
                name: "Cancellations");

            migrationBuilder.DropTable(
                name: "RefundStatuses");

            migrationBuilder.DropTable(
                name: "Returns");

            migrationBuilder.DropTable(
                name: "OrderItems");

            migrationBuilder.DropTable(
                name: "Shipments");

            migrationBuilder.DropTable(
                name: "CancellationStatuses");

            migrationBuilder.DropTable(
                name: "ReasonMasters");

            migrationBuilder.DropTable(
                name: "ReturnStatuses");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "ShipmentStatuses");

            migrationBuilder.DropTable(
                name: "CancellationPolicies");

            migrationBuilder.DropTable(
                name: "OrderStatuses");

            migrationBuilder.DropTable(
                name: "ReturnPolicies");
        }
    }
}
