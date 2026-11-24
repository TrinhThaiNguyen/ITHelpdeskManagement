using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITHelpdesk.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTicketCommentCreatedByUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "TicketComments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketComments_CreatedByUserId",
                table: "TicketComments",
                column: "CreatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TicketComments_AspNetUsers_CreatedByUserId",
                table: "TicketComments",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TicketComments_AspNetUsers_CreatedByUserId",
                table: "TicketComments");

            migrationBuilder.DropIndex(
                name: "IX_TicketComments_CreatedByUserId",
                table: "TicketComments");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "TicketComments");
        }
    }
}
