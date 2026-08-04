using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class NormalizeRosterTableNames : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "roster_family_members",
            columns: table => new
            {
                family_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                claimed_by_user_id = table.Column<Guid>(type: "uuid", nullable: true),
                first_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                avatar_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                phone_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                gender = table.Column<int>(type: "integer", maxLength: 50, nullable: true),
                bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                visibility_status = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_roster_family_members", x => x.family_member_id);
            });

        migrationBuilder.CreateTable(
            name: "roster_family_member_relationships",
            columns: table => new
            {
                family_member_relationship_id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                base_family_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                related_family_member_id = table.Column<Guid>(type: "uuid", nullable: false),
                relationship_type = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_roster_family_member_relationships", x => x.family_member_relationship_id);
                table.ForeignKey(
                    name: "FK_roster_family_member_relationships_roster_family_members_ba~",
                    column: x => x.base_family_member_id,
                    principalTable: "roster_family_members",
                    principalColumn: "family_member_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_users_external_logins_user_id",
            table: "users_external_logins",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_member_relationships_base_family_member_id_re~",
            table: "roster_family_member_relationships",
            columns: new[] { "base_family_member_id", "related_family_member_id", "relationship_type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_member_relationships_tree_id_base_family_memb~",
            table: "roster_family_member_relationships",
            columns: new[] { "tree_id", "base_family_member_id" });

        migrationBuilder.AddForeignKey(
            name: "FK_users_external_logins_users_user_user_id",
            table: "users_external_logins",
            column: "user_id",
            principalTable: "users_user",
            principalColumn: "user_id",
            onDelete: ReferentialAction.Cascade);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_users_external_logins_users_user_user_id",
            table: "users_external_logins");

        migrationBuilder.DropTable(
            name: "roster_family_member_relationships");

        migrationBuilder.DropTable(
            name: "roster_family_members");

        migrationBuilder.DropIndex(
            name: "IX_users_external_logins_user_id",
            table: "users_external_logins");
    }
}
