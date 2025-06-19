using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Auto_Insurance_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class AttemptTen4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claim_Policies_PolicyId",
                table: "Claim");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claim",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "ClaimNumber",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "ClaimStatus",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "IncidentType",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "ProcessedBy",
                table: "Claim");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Claim");

            migrationBuilder.RenameTable(
                name: "Claim",
                newName: "Claims");

            migrationBuilder.RenameColumn(
                name: "DateProcessed",
                table: "Claims",
                newName: "DateVerified");

            migrationBuilder.RenameColumn(
                name: "DateFiled",
                table: "Claims",
                newName: "DateOfSubmission");

            migrationBuilder.RenameColumn(
                name: "ClaimAmount",
                table: "Claims",
                newName: "ClaimAmountRequested");

            migrationBuilder.RenameIndex(
                name: "IX_Claim_PolicyId",
                table: "Claims",
                newName: "IX_Claims_PolicyId");

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByAdminId",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClaimType",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "DamageEstimate",
                table: "Claims",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateApproved",
                table: "Claims",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IncidentDescription",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncidentLocation",
                table: "Claims",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<TimeSpan>(
                name: "IncidentTime",
                table: "Claims",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<string>(
                name: "OtherClaimType",
                table: "Claims",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Claims",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerifiedByAgentId",
                table: "Claims",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claims",
                table: "Claims",
                column: "ClaimId");

            migrationBuilder.CreateTable(
                name: "ClaimDocuments",
                columns: table => new
                {
                    DocumentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClaimId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClaimDocuments", x => x.DocumentId);
                    table.ForeignKey(
                        name: "FK_ClaimDocuments_Claims_ClaimId",
                        column: x => x.ClaimId,
                        principalTable: "Claims",
                        principalColumn: "ClaimId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Claims_ApprovedByAdminId",
                table: "Claims",
                column: "ApprovedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId",
                table: "Claims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_UserId1",
                table: "Claims",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Claims_VerifiedByAgentId",
                table: "Claims",
                column: "VerifiedByAgentId");

            migrationBuilder.CreateIndex(
                name: "IX_ClaimDocuments_ClaimId",
                table: "ClaimDocuments",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_ApprovedByAdminId",
                table: "Claims",
                column: "ApprovedByAdminId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_UserId",
                table: "Claims",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_UserId1",
                table: "Claims",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_AspNetUsers_VerifiedByAgentId",
                table: "Claims",
                column: "VerifiedByAgentId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_ApprovedByAdminId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_UserId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_UserId1",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_AspNetUsers_VerifiedByAgentId",
                table: "Claims");

            migrationBuilder.DropForeignKey(
                name: "FK_Claims_Policies_PolicyId",
                table: "Claims");

            migrationBuilder.DropTable(
                name: "ClaimDocuments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Claims",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_ApprovedByAdminId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_UserId",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_UserId1",
                table: "Claims");

            migrationBuilder.DropIndex(
                name: "IX_Claims_VerifiedByAgentId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ApprovedByAdminId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "ClaimType",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DamageEstimate",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DateApproved",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentDescription",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentLocation",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "IncidentTime",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "OtherClaimType",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "VerifiedByAgentId",
                table: "Claims");

            migrationBuilder.RenameTable(
                name: "Claims",
                newName: "Claim");

            migrationBuilder.RenameColumn(
                name: "DateVerified",
                table: "Claim",
                newName: "DateProcessed");

            migrationBuilder.RenameColumn(
                name: "DateOfSubmission",
                table: "Claim",
                newName: "DateFiled");

            migrationBuilder.RenameColumn(
                name: "ClaimAmountRequested",
                table: "Claim",
                newName: "ClaimAmount");

            migrationBuilder.RenameIndex(
                name: "IX_Claims_PolicyId",
                table: "Claim",
                newName: "IX_Claim_PolicyId");

            migrationBuilder.AddColumn<string>(
                name: "ClaimNumber",
                table: "Claim",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ClaimStatus",
                table: "Claim",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Claim",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "IncidentType",
                table: "Claim",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProcessedBy",
                table: "Claim",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Claim",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Claim",
                table: "Claim",
                column: "ClaimId");

            migrationBuilder.AddForeignKey(
                name: "FK_Claim_Policies_PolicyId",
                table: "Claim",
                column: "PolicyId",
                principalTable: "Policies",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
