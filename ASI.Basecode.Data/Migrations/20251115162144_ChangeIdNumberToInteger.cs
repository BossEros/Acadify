using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASI.Basecode.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeIdNumberToInteger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL requires dropping default first, then converting type
            migrationBuilder.Sql("ALTER TABLE users ALTER COLUMN id_number DROP DEFAULT;");

            // Update empty strings or NULL values to 0 before conversion
            migrationBuilder.Sql("UPDATE users SET id_number = '0' WHERE id_number IS NULL OR id_number = '';");

            // Convert VARCHAR to INTEGER
            migrationBuilder.Sql("ALTER TABLE users ALTER COLUMN id_number TYPE INTEGER USING id_number::integer;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "id_number",
                table: "users",
                type: "VARCHAR(50)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }
    }
}
