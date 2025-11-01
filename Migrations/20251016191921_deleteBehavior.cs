using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class deleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings");

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
