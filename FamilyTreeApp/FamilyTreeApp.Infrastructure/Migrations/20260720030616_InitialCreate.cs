using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Samples",
            columns: table => new
            {
                SampleEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Samples", x => x.SampleEntityId);
            });

        migrationBuilder.CreateTable(
            name: "users_user",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                is_public = table.Column<bool>(type: "boolean", nullable: false),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                gender = table.Column<int>(type: "integer", maxLength: 50, nullable: true),
                bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users_user", x => x.user_id);
            });

        migrationBuilder.CreateIndex(
            name: "IX_users_user_email",
            table: "users_user",
            column: "email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Samples");

        migrationBuilder.DropTable(
            name: "users_user");
    }
}
