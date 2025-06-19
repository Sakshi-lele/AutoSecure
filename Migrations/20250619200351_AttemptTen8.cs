using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auto_Insurance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AttemptTen8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeleteReason",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Policies",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeletedByUserId",
                table: "Policies",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Policies",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Policies_DeletedByUserId",
                table: "Policies",
                column: "DeletedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Policies_AspNetUsers_DeletedByUserId",
                table: "Policies",
                column: "DeletedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Policies_AspNetUsers_DeletedByUserId",
                table: "Policies");

            migrationBuilder.DropIndex(
                name: "IX_Policies_DeletedByUserId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DeleteReason",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Policies");
        }
    }
}
