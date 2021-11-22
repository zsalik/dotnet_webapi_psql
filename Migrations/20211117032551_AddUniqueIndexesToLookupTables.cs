using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace dotnet_webapi_psql.Migrations
{
    public partial class AddUniqueIndexesToLookupTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_sizes_name",
                table: "sizes",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_body_types_name",
                table: "body_types",
                column: "name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sizes_name",
                table: "sizes");

            migrationBuilder.DropIndex(
                name: "ix_body_types_name",
                table: "body_types");
        }
    }
}
