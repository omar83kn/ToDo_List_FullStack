using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDo_List.Migrations
{
    /// <inheritdoc />
    public partial class AddListItemFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // keep: change ListItems.Title to nvarchar(200)
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ListItems",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // ❌ removed: CreateTable ListItemFiles (because it already exists)
            // ❌ removed: CreateIndex IX_ListItemFiles_ListItemId
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ❌ removed: DropTable ListItemFiles (because it already exists)

            // rollback Title column
            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "ListItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
