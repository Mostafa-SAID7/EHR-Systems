using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRPlatform.Services.Appointment.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ScheduledEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AppointmentType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Scheduled"),
                    ReasonForVisit = table.Column<string>(type: "text", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DurationMinutes = table.Column<int>(type: "integer", nullable: false),
                    ReminderSent = table.Column<bool>(type: "boolean", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", maxLength: 250, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true),
                    ContainsPII = table.Column<bool>(type: "boolean", nullable: false),
                    AccessLevel = table.Column<int>(type: "integer", nullable: false),
                    ChangeReason = table.Column<string>(type: "text", nullable: true),
                    SourceIPAddress = table.Column<string>(type: "text", nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: true),
                    ArchivedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<string>(type: "text", nullable: false),
                    EventData = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false),
                    PublishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PublishAttempts = table.Column<int>(type: "integer", nullable: false),
                    MaxPublishAttempts = table.Column<int>(type: "integer", nullable: false),
                    ErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: true),
                    Transport = table.Column<string>(type: "text", nullable: false),
                    RoutingKey = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProviderAvailability",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotStart = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SlotEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRecurring = table.Column<bool>(type: "boolean", nullable: false),
                    RecurrencePattern = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MaxAppointmentsPerSlot = table.Column<int>(type: "integer", nullable: true),
                    CurrentBookings = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProviderAvailability", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AppointmentReminders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReminderTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsSent = table.Column<bool>(type: "boolean", nullable: false),
                    SentAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppointmentReminders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppointmentReminders_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Appointments",
                columns: new[] { "Id", "AccessLevel", "AppointmentType", "ArchivedAt", "CancellationReason", "CancelledAt", "ChangeReason", "ConfirmedAt", "ContainsPII", "CorrelationId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "DurationMinutes", "IsEncrypted", "Notes", "PatientId", "ProviderId", "ReasonForVisit", "ReminderSent", "ScheduledEnd", "ScheduledStart", "SourceIPAddress", "Status", "TenantId", "UpdatedAt", "UpdatedBy", "Version" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 2, "Office", null, null, null, null, null, true, null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, 0, true, null, new Guid("22222222-2222-2222-2222-222222222222"), new Guid("33333333-3333-3333-3333-333333333333"), null, false, new DateTime(2026, 8, 1, 10, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 1, 9, 0, 0, 0, DateTimeKind.Utc), null, "Scheduled", null, new DateTime(2026, 7, 27, 23, 57, 7, 473, DateTimeKind.Utc).AddTicks(6793), null, 1 });

            migrationBuilder.InsertData(
                table: "ProviderAvailability",
                columns: new[] { "Id", "CorrelationId", "CreatedAt", "CreatedBy", "CurrentBookings", "DeletedAt", "DeletedBy", "IsActive", "IsRecurring", "MaxAppointmentsPerSlot", "ProviderId", "RecurrencePattern", "SlotEnd", "SlotStart", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("55555555-5555-5555-5555-555555555555"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), 0, null, null, true, false, null, new Guid("33333333-3333-3333-3333-333333333333"), null, new DateTime(2026, 8, 1, 17, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 7, 27, 23, 57, 7, 473, DateTimeKind.Utc).AddTicks(7116), null });

            migrationBuilder.InsertData(
                table: "AppointmentReminders",
                columns: new[] { "Id", "AppointmentId", "CorrelationId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "IsSent", "Method", "ReminderTime", "SentAt", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("11111111-1111-1111-1111-111111111111"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, null, false, "Email", new DateTime(2026, 8, 1, 8, 0, 0, 0, DateTimeKind.Utc), null, new DateTime(2026, 7, 27, 23, 57, 7, 473, DateTimeKind.Utc).AddTicks(7071), null });

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_AppointmentId",
                table: "AppointmentReminders",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_CreatedAt",
                table: "AppointmentReminders",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_DeletedAt",
                table: "AppointmentReminders",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AppointmentReminders_ReminderTime",
                table: "AppointmentReminders",
                column: "ReminderTime");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedAt",
                table: "Appointments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CreatedBy",
                table: "Appointments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_DeletedAt",
                table: "Appointments",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_PatientId",
                table: "Appointments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProviderId",
                table: "Appointments",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ProviderId_ScheduledStart",
                table: "Appointments",
                columns: new[] { "ProviderId", "ScheduledStart" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ScheduledStart",
                table: "Appointments",
                column: "ScheduledStart",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailability_CreatedAt",
                table: "ProviderAvailability",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailability_DeletedAt",
                table: "ProviderAvailability",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailability_ProviderId",
                table: "ProviderAvailability",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ProviderAvailability_ProviderId_SlotStart_SlotEnd",
                table: "ProviderAvailability",
                columns: new[] { "ProviderId", "SlotStart", "SlotEnd" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppointmentReminders");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "ProviderAvailability");

            migrationBuilder.DropTable(
                name: "Appointments");
        }
    }
}
