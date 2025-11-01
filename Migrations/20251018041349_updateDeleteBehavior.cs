using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class updateDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_AspNetUsers_UserId",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_AspNetUsers_UserId",
                table: "BookCollections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections",
                column: "CollectionID",
                principalTable: "Collections",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_AspNetUsers_UserId",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_AspNetUsers_UserId",
                table: "BookCollections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections",
                column: "CollectionID",
                principalTable: "Collections",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
