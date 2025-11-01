using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Biblio.Migrations
{
    /// <inheritdoc />
    public partial class ll : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "BookCollections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookCollections_UserId",
                table: "BookCollections",
                column: "UserId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_AspNetUsers_UserId",
                table: "BookCollections");

            migrationBuilder.DropForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections");

            migrationBuilder.DropIndex(
                name: "IX_BookCollections_UserId",
                table: "BookCollections");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "BookCollections");

            migrationBuilder.AddForeignKey(
                name: "FK_BookCollections_Collections_CollectionID",
                table: "BookCollections",
                column: "CollectionID",
                principalTable: "Collections",
                principalColumn: "ID");
        }
    }
}
