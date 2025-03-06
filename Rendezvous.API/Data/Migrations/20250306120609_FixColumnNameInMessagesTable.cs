using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rendezvous.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNameInMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageSet",
                table: "Messages",
                newName: "MessageSent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MessageSent",
                table: "Messages",
                newName: "MessageSet");
        }
    }
}
