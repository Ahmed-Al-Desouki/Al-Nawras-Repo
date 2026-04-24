using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Al_Nawras.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReportImportApprovalWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "ReportImports",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                table: "ReportImports",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReviewedByUserId",
                table: "ReportImports",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ReportImports",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ReportImports_ReviewedByUserId",
                table: "ReportImports",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ReportImports_Status",
                table: "ReportImports",
                column: "Status");

            migrationBuilder.AddForeignKey(
                name: "FK_ReportImports_Users_ReviewedByUserId",
                table: "ReportImports",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReportImports_Users_ReviewedByUserId",
                table: "ReportImports");

            migrationBuilder.DropIndex(
                name: "IX_ReportImports_ReviewedByUserId",
                table: "ReportImports");

            migrationBuilder.DropIndex(
                name: "IX_ReportImports_Status",
                table: "ReportImports");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "ReportImports");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                table: "ReportImports");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "ReportImports");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReportImports");
        }
    }
}
