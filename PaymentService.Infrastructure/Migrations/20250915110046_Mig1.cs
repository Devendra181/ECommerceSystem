using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PaymentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Environments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Environments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GatewayProviders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayProviders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMethodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMethodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundMethodTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundMethodTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentProviderConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GatewayProviderId = table.Column<int>(type: "int", nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiSecret = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    EndpointUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    WebhookUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EnvironmentId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentProviderConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentProviderConfigurations_Environments_EnvironmentId",
                        column: x => x.EnvironmentId,
                        principalTable: "Environments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentProviderConfigurations_GatewayProviders_GatewayProviderId",
                        column: x => x.GatewayProviderId,
                        principalTable: "GatewayProviders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserPaymentMethods",
                columns: table => new
                {
                    PaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MethodTypeId = table.Column<int>(type: "int", nullable: false),
                    MaskedDetails = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ExpiryMonth = table.Column<int>(type: "int", nullable: true),
                    ExpiryYear = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPaymentMethods", x => x.PaymentMethodId);
                    table.ForeignKey(
                        name: "FK_UserPaymentMethods_PaymentMethodTypes_MethodTypeId",
                        column: x => x.MethodTypeId,
                        principalTable: "PaymentMethodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentMethodTypeId = table.Column<int>(type: "int", nullable: false),
                    UserPaymentMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    PaymentStatusId = table.Column<int>(type: "int", nullable: false),
                    PaymentUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentMethodTypes_PaymentMethodTypeId",
                        column: x => x.PaymentMethodTypeId,
                        principalTable: "PaymentMethodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentStatuses_PaymentStatusId",
                        column: x => x.PaymentStatusId,
                        principalTable: "PaymentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_UserPaymentMethods_UserPaymentMethodId",
                        column: x => x.UserPaymentMethodId,
                        principalTable: "UserPaymentMethods",
                        principalColumn: "PaymentMethodId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GatewayResponses",
                columns: table => new
                {
                    GatewayResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RawResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ReceivedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GatewayResponses", x => x.GatewayResponseId);
                    table.ForeignKey(
                        name: "FK_GatewayResponses_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    RefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RefundMethodTypeId = table.Column<int>(type: "int", nullable: false),
                    PaymentTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefundTransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    RefundStatusId = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    InitiatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.RefundId);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_RefundMethodTypes_RefundMethodTypeId",
                        column: x => x.RefundMethodTypeId,
                        principalTable: "RefundMethodTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_RefundStatuses_RefundStatusId",
                        column: x => x.RefundStatusId,
                        principalTable: "RefundStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RefundId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransactionStatusId = table.Column<int>(type: "int", nullable: false),
                    PaymentProviderConfigurationId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_Transactions_PaymentProviderConfigurations_PaymentProviderConfigurationId",
                        column: x => x.PaymentProviderConfigurationId,
                        principalTable: "PaymentProviderConfigurations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "PaymentId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Transactions_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "RefundId");
                    table.ForeignKey(
                        name: "FK_Transactions_TransactionStatuses_TransactionStatusId",
                        column: x => x.TransactionStatusId,
                        principalTable: "TransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Environments",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Sandbox environment for testing", true, "Sandbox", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Production live environment", true, "Live", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "User Acceptance testing environment", true, "UAT", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "GatewayProviders",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Razorpay payment gateway", true, "Razorpay", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Stripe payment gateway", true, "Stripe", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Paytm payment gateway", true, "Paytm", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "Other payment gateway", true, "Other", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "PaymentMethodTypes",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "CreditCard", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "DebitCard", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "UPI", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Wallet", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 5, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "COD", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 6, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "NetBanking", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Pending", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Completed", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Failed", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Canceled", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "RefundMethodTypes",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Original", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Wallet", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "BankTransfer", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Manual", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "RefundStatuses",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Pending", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Completed", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Failed", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Rejected", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "TransactionStatuses",
                columns: new[] { "Id", "CreatedOn", "Description", "IsActive", "Name", "UpdatedOn" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Pending", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Success", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Failed", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 4, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), null, true, "Canceled", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc) }
                });

            migrationBuilder.InsertData(
                table: "PaymentProviderConfigurations",
                columns: new[] { "Id", "ApiKey", "ApiSecret", "CreatedOn", "EndpointUrl", "EnvironmentId", "GatewayProviderId", "IsActive", "UpdatedOn", "WebhookUrl" },
                values: new object[,]
                {
                    { 1, "rzp_test_sandbox_key", "rzp_test_sandbox_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://api.razorpay.com/v1/", 1, 1, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/razorpay" },
                    { 2, "rzp_live_production_key", "rzp_live_production_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://api.razorpay.com/v1/", 2, 1, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/razorpay" },
                    { 3, "sk_test_sandbox_key", "sk_test_sandbox_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://api.stripe.com/v1/", 1, 2, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/stripe" },
                    { 4, "sk_live_production_key", "sk_live_production_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://api.stripe.com/v1/", 2, 2, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/stripe" },
                    { 5, "paytm_test_sandbox_key", "paytm_test_sandbox_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://secure.paytm.in/", 1, 3, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/paytm" },
                    { 6, "paytm_live_production_key", "paytm_live_production_secret", new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://secure.paytm.in/", 2, 3, true, new DateTime(2025, 8, 6, 0, 0, 0, 0, DateTimeKind.Utc), "https://yourdomain.com/api/webhook/paytm" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_GatewayResponse_PaymentId",
                table: "GatewayResponses",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayResponse_RefundId",
                table: "GatewayResponses",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_GatewayResponse_TransactionId",
                table: "GatewayResponses",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfiguration_EnvironmentId",
                table: "PaymentProviderConfigurations",
                column: "EnvironmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentConfiguration_GatewayProviderId",
                table: "PaymentProviderConfigurations",
                column: "GatewayProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_OrderId",
                table: "Payments",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentStatusId",
                table: "Payments",
                column: "PaymentStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_UserId",
                table: "Payments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentMethodTypeId",
                table: "Payments",
                column: "PaymentMethodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_UserPaymentMethodId",
                table: "Payments",
                column: "UserPaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_Refund_PaymentId",
                table: "Refunds",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refund_RefundStatusId",
                table: "Refunds",
                column: "RefundStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Refund_TransactionId",
                table: "Refunds",
                column: "PaymentTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_RefundMethodTypeId",
                table: "Refunds",
                column: "RefundMethodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_RefundTransactionId",
                table: "Refunds",
                column: "RefundTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_PaymentId",
                table: "Transactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transaction_TransactionStatusId",
                table: "Transactions",
                column: "TransactionStatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_PaymentProviderConfigurationId",
                table: "Transactions",
                column: "PaymentProviderConfigurationId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_RefundId",
                table: "Transactions",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_MethodTypeId",
                table: "UserPaymentMethods",
                column: "MethodTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_UserId",
                table: "UserPaymentMethods",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GatewayResponses_Refunds_RefundId",
                table: "GatewayResponses",
                column: "RefundId",
                principalTable: "Refunds",
                principalColumn: "RefundId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_GatewayResponses_Transactions_TransactionId",
                table: "GatewayResponses",
                column: "TransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Refunds_Transactions_PaymentTransactionId",
                table: "Refunds",
                column: "PaymentTransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Refunds_Transactions_RefundTransactionId",
                table: "Refunds",
                column: "RefundTransactionId",
                principalTable: "Transactions",
                principalColumn: "TransactionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Refunds_Payments_PaymentId",
                table: "Refunds");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Payments_PaymentId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_Refunds_RefundId",
                table: "Transactions");

            migrationBuilder.DropTable(
                name: "GatewayResponses");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "UserPaymentMethods");

            migrationBuilder.DropTable(
                name: "PaymentMethodTypes");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "RefundMethodTypes");

            migrationBuilder.DropTable(
                name: "RefundStatuses");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "PaymentProviderConfigurations");

            migrationBuilder.DropTable(
                name: "TransactionStatuses");

            migrationBuilder.DropTable(
                name: "Environments");

            migrationBuilder.DropTable(
                name: "GatewayProviders");
        }
    }
}
