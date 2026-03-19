using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COLManager.Web.Migrations
{
    /// <inheritdoc />
    public partial class updateusagelogtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProtocolName",
                table: "Column_Usage_Log");

            migrationBuilder.CreateIndex(
                name: "IX_Column_Usage_Log_ColumnID",
                table: "Column_Usage_Log",
                column: "ColumnID");

            migrationBuilder.AddForeignKey(
                name: "FK_Column_Usage_Log_Column_Master_ColumnID",
                table: "Column_Usage_Log",
                column: "ColumnID",
                principalTable: "Column_Master",
                principalColumn: "ColumnID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Column_Usage_Log_Column_Master_ColumnID",
                table: "Column_Usage_Log");

            migrationBuilder.DropIndex(
                name: "IX_Column_Usage_Log_ColumnID",
                table: "Column_Usage_Log");

            migrationBuilder.AddColumn<string>(
                name: "ProtocolName",
                table: "Column_Usage_Log",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
