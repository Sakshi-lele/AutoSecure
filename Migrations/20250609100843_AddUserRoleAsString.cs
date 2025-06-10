using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auto_Insurance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleAsString : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_UserId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claims",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_UserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "Claims",
                newName: "Claim");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_PolicyId",
                table: "Claim",
                newName: "IX_Claim_PolicyId");

            migrationBuilder.AddColumn<string>(
                name: "LicensePlate",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleMake",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "VehicleModel",
                table: "Policies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "VehicleYear",
                table: "Policies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claim",
                table: "Claim",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Policies_PolicyId",
                table: "Claim",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Policies_PolicyId",
                table: "Claim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claim",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "LicensePlate",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VehicleMake",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VehicleModel",
                table: "Policies");

            migrationBuilder.DropColumn(
                name: "VehicleYear",
                table: "Policies");

            migrationBuilder.RenameTable(
                name: "Claim",
                newName: "Claims");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_PolicyId",
                table: "Claims",
                newName: "IX_Claims_PolicyId");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claims",
                table: "Claims",
                column: "ClaimId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_UserId",
                table: "Claims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "PolicyId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
