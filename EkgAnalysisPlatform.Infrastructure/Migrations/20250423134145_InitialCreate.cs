using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EkgAnalysisPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientCode = table.Column<string>(type: "TEXT", nullable: false),
                    Age = table.Column<int>(type: "INTEGER", nullable: false),
                    Gender = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EkgSignals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PatientId = table.Column<int>(type: "INTEGER", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataPoints = table.Column<string>(type: "TEXT", nullable: false),
                    SamplingRate = table.Column<double>(type: "REAL", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EkgSignals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EkgSignals_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AnalysisResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EkgSignalId = table.Column<int>(type: "INTEGER", nullable: false),
                    HeartRate = table.Column<double>(type: "REAL", nullable: false),
                    HasArrhythmia = table.Column<bool>(type: "INTEGER", nullable: false),
                    QRSDuration = table.Column<double>(type: "REAL", nullable: true),
                    PRInterval = table.Column<double>(type: "REAL", nullable: true),
                    QTInterval = table.Column<double>(type: "REAL", nullable: true),
                    AnalyzedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisResults_EkgSignals_EkgSignalId",
                        column: x => x.EkgSignalId,
                        principalTable: "EkgSignals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisResults_EkgSignalId",
                table: "AnalysisResults",
                column: "EkgSignalId");

            migrationBuilder.CreateIndex(
                name: "IX_EkgSignals_PatientId",
                table: "EkgSignals",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AnalysisResults");

            migrationBuilder.DropTable(
                name: "EkgSignals");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
