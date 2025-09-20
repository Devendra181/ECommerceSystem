using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NotificationService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Mig1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationChannels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationChannels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationStatuses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DoNotDisturb = table.Column<bool>(type: "bit", nullable: false),
                    MaxDailyNotifications = table.Column<int>(type: "int", nullable: true),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TemplateData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "NotificationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Notifications_NotificationTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "NotificationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    TypeId = table.Column<int>(type: "int", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EffectiveTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_NotificationChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationTemplates_NotificationTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "NotificationTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationAttachments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StorageType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationAttachments_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationAttemptLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StatusId = table.Column<int>(type: "int", nullable: false),
                    ChannelId = table.Column<int>(type: "int", nullable: false),
                    ProviderResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsSuccessful = table.Column<bool>(type: "bit", nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationAttemptLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationAttemptLogs_NotificationChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "NotificationChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationAttemptLogs_NotificationStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "NotificationStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NotificationAttemptLogs_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationRecipients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    RecipientType = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationRecipients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationRecipients_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplateAudits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectTemplate = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false),
                    ChangedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplateAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationTemplateAudits_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "NotificationChannels",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Email notifications", true, "Email", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "SMS notifications", true, "SMS", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "In-App notifications", true, "InApp", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "No channel selected", true, "None", null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationStatuses",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Notification is pending", true, "Pending", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Notification sent successfully", true, "Sent", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Notification failed to deliver", true, "Failed", null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTypes",
                columns: new[] { "Id", "CreatedAt", "CreatedBy", "Description", "IsActive", "Name", "UpdatedAt", "UpdatedBy" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Order placed confirmation", true, "OrderPlaced", null, null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Payment successful confirmation", true, "PaymentSuccess", null, null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Payment failure alert", true, "PaymentFailed", null, null },
                    { 4, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Order shipment update", true, "OrderShipped", null, null },
                    { 5, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Order delivery confirmation", true, "OrderDelivered", null, null },
                    { 6, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "Order cancellation notification", true, "OrderCancelled", null, null },
                    { 99, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", "General purpose notifications", true, "General", null, null }
                });

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "ChannelId", "Content", "CreatedAt", "CreatedBy", "Description", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDefault", "SubjectTemplate", "TemplateName", "TypeId", "UpdatedAt", "UpdatedBy", "Version" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), 1, "Hello {CustomerName},<br/>Your order #{OrderNumber} has been placed successfully.<br/>Items: {Items}<br/>Total Amount: ₹{Amount}.<br/>We will notify you once it is shipped.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Your Order #{OrderNumber} is Confirmed!", "OrderPlaced_Email", 1, null, null, 1 },
                    { new Guid("11111111-1111-1111-1111-111111111112"), 2, "Hi {CustomerName}, your order #{OrderNumber} of ₹{Amount} is placed successfully.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Confirmation", "OrderPlaced_SMS", 1, null, null, 1 },
                    { new Guid("22222222-2222-2222-2222-222222222221"), 1, "Dear {CustomerName},<br/>We have received your payment of ₹{Amount} for order #{OrderNumber}.<br/>Thank you for shopping with us.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Payment Received for Order #{OrderNumber}", "PaymentSuccess_Email", 2, null, null, 1 },
                    { new Guid("22222222-2222-2222-2222-222222222222"), 2, "Hi {CustomerName}, your payment of ₹{Amount} for order #{OrderNumber} has failed. Please retry.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Payment Failed", "PaymentFailed_SMS", 3, null, null, 1 },
                    { new Guid("33333333-3333-3333-3333-333333333331"), 1, "Hello {CustomerName},<br/>Your order #{OrderNumber} has been shipped.<br/>Track your order here: {TrackingLink}.<br/>Thank you for shopping with us!", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Your Order #{OrderNumber} has been Shipped!", "OrderShipped_Email", 4, null, null, 1 },
                    { new Guid("33333333-3333-3333-3333-333333333332"), 2, "Hi {CustomerName}, your order #{OrderNumber} has been shipped. Track here: {TrackingLink}", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Shipped", "OrderShipped_SMS", 4, null, null, 1 },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 3, "Your order #{OrderNumber} has been shipped. Tap to track.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Update", "OrderShipped_InApp", 4, null, null, 1 },
                    { new Guid("44444444-4444-4444-4444-444444444441"), 1, "Dear {CustomerName},<br/>Your order #{OrderNumber} has been successfully delivered.<br/>We hope you enjoy your purchase!", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Your Order #{OrderNumber} has been Delivered", "OrderDelivered_Email", 5, null, null, 1 },
                    { new Guid("44444444-4444-4444-4444-444444444442"), 2, "Hi {CustomerName}, your order #{OrderNumber} has been delivered. Thank you for shopping with us!", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Delivered", "OrderDelivered_SMS", 5, null, null, 1 },
                    { new Guid("44444444-4444-4444-4444-444444444443"), 3, "Your order #{OrderNumber} has been delivered successfully.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Delivery Update", "OrderDelivered_InApp", 5, null, null, 1 },
                    { new Guid("55555555-5555-5555-5555-555555555551"), 1, "Dear {CustomerName},<br/>Your order #{OrderNumber} has been cancelled.<br/>If payment was made, refund of ₹{Amount} will be processed shortly.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Your Order #{OrderNumber} has been Cancelled", "OrderCancelled_Email", 6, null, null, 1 },
                    { new Guid("55555555-5555-5555-5555-555555555552"), 2, "Hi {CustomerName}, your order #{OrderNumber} has been cancelled. Refund of ₹{Amount} will be processed shortly.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Cancelled", "OrderCancelled_SMS", 6, null, null, 1 },
                    { new Guid("55555555-5555-5555-5555-555555555553"), 3, "Your order #{OrderNumber} has been cancelled. Refund will be processed soon.", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System", null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, true, true, "Order Cancelled", "OrderCancelled_InApp", 6, null, null, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttachments_NotificationId",
                table: "NotificationAttachments",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttemptLogs_AttemptedAt",
                table: "NotificationAttemptLogs",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttemptLogs_ChannelId",
                table: "NotificationAttemptLogs",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttemptLogs_NotificationId",
                table: "NotificationAttemptLogs",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationAttemptLogs_StatusId",
                table: "NotificationAttemptLogs",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationChannels_IsActive",
                table: "NotificationChannels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipients_NotificationId",
                table: "NotificationRecipients",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ChannelId",
                table: "Notifications",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ScheduledAt",
                table: "Notifications",
                column: "ScheduledAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_StatusId",
                table: "Notifications",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TypeId",
                table: "Notifications",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationStatuses_IsActive",
                table: "NotificationStatuses",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplateAudits_TemplateId",
                table: "NotificationTemplateAudits",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_ChannelId",
                table: "NotificationTemplates",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_EffectiveFrom_EffectiveTo",
                table: "NotificationTemplates",
                columns: new[] { "EffectiveFrom", "EffectiveTo" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsDefault",
                table: "NotificationTemplates",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateName",
                table: "NotificationTemplates",
                column: "TemplateName");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TypeId",
                table: "NotificationTemplates",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationAttachments");

            migrationBuilder.DropTable(
                name: "NotificationAttemptLogs");

            migrationBuilder.DropTable(
                name: "NotificationRecipients");

            migrationBuilder.DropTable(
                name: "NotificationTemplateAudits");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "NotificationStatuses");

            migrationBuilder.DropTable(
                name: "NotificationChannels");

            migrationBuilder.DropTable(
                name: "NotificationTypes");
        }
    }
}
