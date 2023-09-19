using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrackingBugs.Data.Migrations
{
    /// <inheritdoc />
    public partial class _004_UpdatedFileTypeToFileContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "TicketAttachments",
                newName: "FileName");

            migrationBuilder.AddColumn<string>(
                name: "FileContentType",
                table: "TicketAttachments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FileContentType",
                table: "TicketAttachments");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "TicketAttachments",
                newName: "FileType");
        }
    }
}
