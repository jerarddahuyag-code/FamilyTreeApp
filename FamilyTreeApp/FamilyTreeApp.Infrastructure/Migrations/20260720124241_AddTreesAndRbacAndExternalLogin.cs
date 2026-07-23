using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddTreesAndRbacAndExternalLogin : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "Samples");

        migrationBuilder.CreateTable(
            name: "trees_tree",
            columns: table => new
            {
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                is_public = table.Column<bool>(type: "boolean", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_trees_tree", x => x.tree_id);
            });

        migrationBuilder.CreateTable(
            name: "users_external_logins",
            columns: table => new
            {
                external_login_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                provider_key = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users_external_logins", x => x.external_login_id);
            });

        migrationBuilder.CreateTable(
            name: "trees_tree_rbac",
            columns: table => new
            {
                tree_rbac_id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_role = table.Column<int>(type: "integer", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_trees_tree_rbac", x => x.tree_rbac_id);
                table.ForeignKey(
                    name: "FK_trees_tree_rbac_trees_tree_tree_id",
                    column: x => x.tree_id,
                    principalTable: "trees_tree",
                    principalColumn: "tree_id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_trees_tree_rbac_users_user_user_id",
                    column: x => x.user_id,
                    principalTable: "users_user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_trees_tree_name",
            table: "trees_tree",
            column: "name");

        migrationBuilder.CreateIndex(
            name: "IX_trees_tree_rbac_tree_id_user_id",
            table: "trees_tree_rbac",
            columns: new[] { "tree_id", "user_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_trees_tree_rbac_user_id",
            table: "trees_tree_rbac",
            column: "user_id");

        migrationBuilder.CreateIndex(
            name: "IX_users_external_logins_provider_provider_key",
            table: "users_external_logins",
            columns: new[] { "provider", "provider_key" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "trees_tree_rbac");

        migrationBuilder.DropTable(
            name: "users_external_logins");

        migrationBuilder.DropTable(
            name: "trees_tree");

        migrationBuilder.CreateTable(
            name: "Samples",
            columns: table => new
            {
                SampleEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                Description = table.Column<string>(type: "text", nullable: false),
                Name = table.Column<string>(type: "text", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Samples", x => x.SampleEntityId);
            });
    }
}
