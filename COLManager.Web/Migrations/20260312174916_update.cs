using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COLManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Audit_Trail",
                columns: table => new
                {
                    AuditID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecordID = table.Column<int>(type: "int", nullable: false),
                    OperationType = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    OldData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChangedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ChangedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audit_Trail", x => x.AuditID);
                });

            migrationBuilder.CreateTable(
                name: "Column_Maintenance_Log",
                columns: table => new
                {
                    MaintenanceID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnID = table.Column<int>(type: "int", nullable: false),
                    MaintenanceType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PerformedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Column_Maintenance_Log", x => x.MaintenanceID);
                });

            migrationBuilder.CreateTable(
                name: "Column_Usage_Log",
                columns: table => new
                {
                    UsageID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnID = table.Column<int>(type: "int", nullable: false),
                    RuntimeHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NumberOfInjections = table.Column<int>(type: "int", nullable: false),
                    PreUseBackPressureBar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PostUseBackPressureBar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxPressureObservedBar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    FlowRateMLPerMin = table.Column<decimal>(type: "decimal(10,3)", nullable: true),
                    InjectionVolumeML = table.Column<decimal>(type: "decimal(10,4)", nullable: true),
                    RunDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Column_Usage_Log", x => x.UsageID);
                });

            migrationBuilder.CreateTable(
                name: "Measurement_Type",
                columns: table => new
                {
                    MeasurementTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Measurement_Type", x => x.MeasurementTypeID);
                });

            migrationBuilder.CreateTable(
                name: "Protocol",
                columns: table => new
                {
                    ProtocolID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProtocolName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MaxAllowedPressureBar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxAllowedUsageHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MaxAllowedInjections = table.Column<int>(type: "int", nullable: false),
                    OperatingTemperatureC = table.Column<decimal>(type: "decimal(6,2)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Protocol", x => x.ProtocolID);
                });

            migrationBuilder.CreateTable(
                name: "Status_Master",
                columns: table => new
                {
                    StatusID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Status_Master", x => x.StatusID);
                });

            migrationBuilder.CreateTable(
                name: "Unit_Master",
                columns: table => new
                {
                    UnitID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MeasurementTypeID = table.Column<int>(type: "int", nullable: false),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UnitSymbol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsBaseUnit = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unit_Master", x => x.UnitID);
                });

            migrationBuilder.CreateTable(
                name: "Column_Master",
                columns: table => new
                {
                    ColumnID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ColumnName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LengthMM = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    InternalDiameterMM = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ParticleSizeMicron = table.Column<decimal>(type: "decimal(6,2)", nullable: false),
                    MaxPressureBar = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ProtocolID = table.Column<int>(type: "int", nullable: true),
                    StatusID = table.Column<int>(type: "int", nullable: false),
                    TotalRuntimeHours = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    TotalInjections = table.Column<int>(type: "int", nullable: false),
                    InstalledOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RetiredOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Column_Master", x => x.ColumnID);
                    table.ForeignKey(
                        name: "FK_Column_Master_Protocol_ProtocolID",
                        column: x => x.ProtocolID,
                        principalTable: "Protocol",
                        principalColumn: "ProtocolID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Column_Master_ProtocolID",
                table: "Column_Master",
                column: "ProtocolID");

            migrationBuilder.CreateIndex(
                name: "IX_Column_Master_SerialNumber",
                table: "Column_Master",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Measurement_Type_MeasurementName",
                table: "Measurement_Type",
                column: "MeasurementName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Status_Master_StatusName",
                table: "Status_Master",
                column: "StatusName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Audit_Trail");

            migrationBuilder.DropTable(
                name: "Column_Maintenance_Log");

            migrationBuilder.DropTable(
                name: "Column_Master");

            migrationBuilder.DropTable(
                name: "Column_Usage_Log");

            migrationBuilder.DropTable(
                name: "Measurement_Type");

            migrationBuilder.DropTable(
                name: "Status_Master");

            migrationBuilder.DropTable(
                name: "Unit_Master");

            migrationBuilder.DropTable(
                name: "Protocol");
        }
    }
}
