using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotificationsModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "VisitorID",
                table: "Notifications",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Notifications",
                type: "datetime2",
                nullable: false,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true,
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AddColumn<int>(
                name: "BookID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BorrowingID",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LinkUrl",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<double>(
                name: "FineAmount",
                table: "Borrowings",
                type: "float",
                nullable: false,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true,
                oldDefaultValue: 0.0);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BookID",
                table: "Notifications",
                column: "BookID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BorrowingID",
                table: "Notifications",
                column: "BorrowingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Books_BookID",
                table: "Notifications",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Borrowings_BorrowingID",
                table: "Notifications",
                column: "BorrowingID",
                principalTable: "Borrowings",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Books_BookID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Borrowings_BorrowingID",
                table: "Notifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_BookID",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_BorrowingID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BookID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "BorrowingID",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LinkUrl",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Notifications");

            migrationBuilder.AlterColumn<int>(
                name: "VisitorID",
                table: "Notifications",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "Date",
                table: "Notifications",
                type: "datetime2",
                nullable: true,
                defaultValueSql: "GETDATE()",
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldDefaultValueSql: "GETDATE()");

            migrationBuilder.AlterColumn<double>(
                name: "FineAmount",
                table: "Borrowings",
                type: "float",
                nullable: true,
                defaultValue: 0.0,
                oldClrType: typeof(double),
                oldType: "float",
                oldDefaultValue: 0.0);

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
