using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceIsDeletedWithDeletedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tasks");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Tasks",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Tasks");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tasks",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
