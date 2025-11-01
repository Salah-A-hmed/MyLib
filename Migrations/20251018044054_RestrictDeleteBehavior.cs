using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class RestrictDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Books_BookID",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Books_BookID",
                table: "BookCollections",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections",
                column: "CollectionID",
                principalTable: "Collections",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Books_BookID",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Books_BookID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Borrowings_Visitors_VisitorID",
                table: "Borrowings");

            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Books_BookID",
                table: "BookCollections",
                column: "BookID",
                principalTable: "Books",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections",
                column: "CollectionID",
                principalTable: "Collections",
                principalColumn: "ID");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Visitors_VisitorID",
                table: "Notifications",
                column: "VisitorID",
                principalTable: "Visitors",
                principalColumn: "ID");
        }
    }
}
