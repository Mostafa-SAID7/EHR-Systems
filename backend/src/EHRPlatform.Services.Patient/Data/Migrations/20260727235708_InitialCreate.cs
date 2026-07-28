using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRPlatform.Services.Patient.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Gender = table.Column<string>(type: "text", nullable: false),
                    MRN = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BloodType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    EmergencyContact = table.Column<string>(type: "text", nullable: true),
                    EmergencyPhone = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Active"),
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
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Allergen = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Severity = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_PatientAllergies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientConditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Condition = table.Column<string>(type: "text", nullable: false),
                    ICD10Code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    OnsetDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_PatientConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "Id", "AccessLevel", "ArchivedAt", "BloodType", "ChangeReason", "ContainsPII", "CorrelationId", "CreatedAt", "CreatedBy", "DateOfBirth", "DeletedAt", "DeletedBy", "Email", "EmergencyContact", "EmergencyPhone", "FirstName", "Gender", "IsEncrypted", "LastName", "MRN", "PhoneNumber", "SourceIPAddress", "Status", "TenantId", "UpdatedAt", "UpdatedBy", "Version" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), 2, null, "O+", null, true, null, new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8594), new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(1990, 1, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "john.doe@example.com", null, null, "John", "M", true, "Doe", "MRN-001", "+1-555-0123", null, "Active", null, new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8446), null, 1 });

            migrationBuilder.InsertData(
                table: "PatientAllergies",
                columns: new[] { "Id", "Allergen", "CorrelationId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Notes", "PatientId", "Severity", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Penicillin", null, new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8798), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "", new Guid("11111111-1111-1111-1111-111111111111"), "Severe", new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8787), null });

            migrationBuilder.InsertData(
                table: "PatientConditions",
                columns: new[] { "Id", "Condition", "CorrelationId", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "ICD10Code", "OnsetDate", "PatientId", "ResolvedDate", "UpdatedAt", "UpdatedBy" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), "Hypertension", null, new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8844), new Guid("00000000-0000-0000-0000-000000000000"), null, null, "I10", null, new Guid("11111111-1111-1111-1111-111111111111"), null, new DateTime(2026, 7, 27, 23, 57, 8, 2, DateTimeKind.Utc).AddTicks(8839), null });

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_CreatedAt",
                table: "PatientAllergies",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_DeletedAt",
                table: "PatientAllergies",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_PatientId",
                table: "PatientAllergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_CreatedAt",
                table: "PatientConditions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_DeletedAt",
                table: "PatientConditions",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_PatientId",
                table: "PatientConditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CreatedAt",
                table: "Patients",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_CreatedBy",
                table: "Patients",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_DeletedAt",
                table: "Patients",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Email",
                table: "Patients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_MRN",
                table: "Patients",
                column: "MRN",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "PatientAllergies");

            migrationBuilder.DropTable(
                name: "PatientConditions");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
