using Microsoft.EntityFrameworkCore.Migrations;

namespace EFProject.Migrations
{
    public partial class RenamedAttributes_PostTitleMadeUnique : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Blog_BlogID",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "BlogID",
                table: "Posts",
                newName: "BlogId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Posts",
                newName: "PostId");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_BlogID",
                table: "Posts",
                newName: "IX_Posts_BlogId");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "Blog",
                newName: "BlogId");

            migrationBuilder.CreateIndex(
                name: "IX_Posts_Title",
                table: "Posts",
                column: "Title",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Blog_BlogId",
                table: "Posts",
                column: "BlogId",
                principalTable: "Blog",
                principalColumn: "BlogId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Blog_BlogId",
                table: "Posts");

            migrationBuilder.DropIndex(
                name: "IX_Posts_Title",
                table: "Posts");

            migrationBuilder.RenameColumn(
                name: "BlogId",
                table: "Posts",
                newName: "BlogID");

            migrationBuilder.RenameColumn(
                name: "PostId",
                table: "Posts",
                newName: "ID");

            migrationBuilder.RenameIndex(
                name: "IX_Posts_BlogId",
                table: "Posts",
                newName: "IX_Posts_BlogID");

            migrationBuilder.RenameColumn(
                name: "BlogId",
                table: "Blog",
                newName: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Blog_BlogID",
                table: "Posts",
                column: "BlogID",
                principalTable: "Blog",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
