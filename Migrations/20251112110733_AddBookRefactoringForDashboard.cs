using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class AddBookRefactoringForDashboard : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StockCount",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "CheckedOutCopies",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateAdded",
                table: "Books",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Books",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalCopies",
                table: "Books",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckedOutCopies",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "DateAdded",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Books");

            migrationBuilder.DropColumn(
                name: "TotalCopies",
                table: "Books");

            migrationBuilder.AddColumn<int>(
                name: "StockCount",
                table: "Books",
                type: "int",
                nullable: true,
                defaultValue: 1);
        }
    }
}
