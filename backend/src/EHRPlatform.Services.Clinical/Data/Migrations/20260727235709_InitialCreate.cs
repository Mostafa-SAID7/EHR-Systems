using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EHRPlatform.Services.Clinical.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClinicalNotes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EncounterType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Draft"),
                    Subjective = table.Column<string>(type: "text", nullable: false),
                    Objective = table.Column<string>(type: "text", nullable: false),
                    Assessment = table.Column<string>(type: "text", nullable: false),
                    Plan = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_ClinicalNotes", x => x.Id);
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
                name: "ClinicalDiagnoses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DiagnosisText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DiagnosisType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
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
                    table.PrimaryKey("PK_ClinicalDiagnoses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalDiagnoses_ClinicalNotes_ClinicalNoteId",
                        column: x => x.ClinicalNoteId,
                        principalTable: "ClinicalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClinicalProcedures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProcedureCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Result = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
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
                    table.PrimaryKey("PK_ClinicalProcedures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClinicalProcedures_ClinicalNotes_ClinicalNoteId",
                        column: x => x.ClinicalNoteId,
                        principalTable: "ClinicalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VitalSigns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalNoteId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Temperature = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    SystolicBP = table.Column<int>(type: "integer", nullable: false),
                    DiastolicBP = table.Column<int>(type: "integer", nullable: false),
                    HeartRate = table.Column<int>(type: "integer", nullable: false),
                    RespiratoryRate = table.Column<int>(type: "integer", nullable: false),
                    Weight = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
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
                    table.PrimaryKey("PK_VitalSigns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VitalSigns_ClinicalNotes_ClinicalNoteId",
                        column: x => x.ClinicalNoteId,
                        principalTable: "ClinicalNotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDiagnoses_ClinicalNoteId",
                table: "ClinicalDiagnoses",
                column: "ClinicalNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDiagnoses_CreatedAt",
                table: "ClinicalDiagnoses",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDiagnoses_DeletedAt",
                table: "ClinicalDiagnoses",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalDiagnoses_DiagnosisCode",
                table: "ClinicalDiagnoses",
                column: "DiagnosisCode");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_CreatedAt",
                table: "ClinicalNotes",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_CreatedBy",
                table: "ClinicalNotes",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_DeletedAt",
                table: "ClinicalNotes",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_EncounterDate",
                table: "ClinicalNotes",
                column: "EncounterDate",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PatientId",
                table: "ClinicalNotes",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_PatientId_EncounterDate",
                table: "ClinicalNotes",
                columns: new[] { "PatientId", "EncounterDate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_ProviderId",
                table: "ClinicalNotes",
                column: "ProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalNotes_Status",
                table: "ClinicalNotes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProcedures_ClinicalNoteId",
                table: "ClinicalProcedures",
                column: "ClinicalNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProcedures_CreatedAt",
                table: "ClinicalProcedures",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProcedures_DeletedAt",
                table: "ClinicalProcedures",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicalProcedures_PerformedAt",
                table: "ClinicalProcedures",
                column: "PerformedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_ClinicalNoteId",
                table: "VitalSigns",
                column: "ClinicalNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_ClinicalNoteId_RecordedAt",
                table: "VitalSigns",
                columns: new[] { "ClinicalNoteId", "RecordedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_CreatedAt",
                table: "VitalSigns",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_DeletedAt",
                table: "VitalSigns",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VitalSigns_RecordedAt",
                table: "VitalSigns",
                column: "RecordedAt",
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClinicalDiagnoses");

            migrationBuilder.DropTable(
                name: "ClinicalProcedures");

            migrationBuilder.DropTable(
                name: "OutboxEvents");

            migrationBuilder.DropTable(
                name: "VitalSigns");

            migrationBuilder.DropTable(
                name: "ClinicalNotes");
        }
    }
}
