using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class InitialSQLServer : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "trees_tree",
            columns: table => new
            {
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                is_public = table.Column<bool>(type: "bit", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_trees_tree", x => x.tree_id);
            });

        migrationBuilder.CreateTable(
            name: "users_user",
            columns: table => new
            {
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                is_public = table.Column<bool>(type: "bit", nullable: false),
                first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                gender = table.Column<int>(type: "int", maxLength: 50, nullable: true),
                bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users_user", x => x.user_id);
            });

        migrationBuilder.CreateTable(
            name: "canvas_treenode",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                node_type = table.Column<int>(type: "int", nullable: false),
                x = table.Column<double>(type: "float", nullable: false),
                y = table.Column<double>(type: "float", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_canvas_treenode", x => x.id);
                table.ForeignKey(
                    name: "FK_canvas_treenode_trees_tree_tree_id",
                    column: x => x.tree_id,
                    principalTable: "trees_tree",
                    principalColumn: "tree_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "roster_family_members",
            columns: table => new
            {
                family_member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                claimed_by_user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                first_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                last_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                date_of_birth = table.Column<DateTime>(type: "datetime2", nullable: true),
                avatar_url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                phone_number = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                gender = table.Column<int>(type: "int", maxLength: 50, nullable: true),
                bio = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                visibility_status = table.Column<int>(type: "int", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_roster_family_members", x => x.family_member_id);
                table.ForeignKey(
                    name: "FK_roster_family_members_users_user_claimed_by_user_id",
                    column: x => x.claimed_by_user_id,
                    principalTable: "users_user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "trees_tree_rbac",
            columns: table => new
            {
                tree_rbac_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_role = table.Column<int>(type: "int", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
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

        migrationBuilder.CreateTable(
            name: "users_external_logins",
            columns: table => new
            {
                external_login_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                user_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                provider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                provider_key = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users_external_logins", x => x.external_login_id);
                table.ForeignKey(
                    name: "FK_users_external_logins_users_user_user_id",
                    column: x => x.user_id,
                    principalTable: "users_user",
                    principalColumn: "user_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "canvas_treeedge",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                source_node_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                target_node_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_canvas_treeedge", x => x.id);
                table.ForeignKey(
                    name: "FK_canvas_treeedge_canvas_treenode_source_node_id",
                    column: x => x.source_node_id,
                    principalTable: "canvas_treenode",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_canvas_treeedge_canvas_treenode_target_node_id",
                    column: x => x.target_node_id,
                    principalTable: "canvas_treenode",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_canvas_treeedge_trees_tree_tree_id",
                    column: x => x.tree_id,
                    principalTable: "trees_tree",
                    principalColumn: "tree_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "canvas_treenode_member",
            columns: table => new
            {
                tree_node_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                family_member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_canvas_treenode_member", x => new { x.tree_node_id, x.family_member_id });
                table.ForeignKey(
                    name: "FK_canvas_treenode_member_canvas_treenode_tree_node_id",
                    column: x => x.tree_node_id,
                    principalTable: "canvas_treenode",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_canvas_treenode_member_roster_family_members_family_member_id",
                    column: x => x.family_member_id,
                    principalTable: "roster_family_members",
                    principalColumn: "family_member_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "roster_family_member_relationships",
            columns: table => new
            {
                family_member_relationship_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                tree_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                base_family_member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                related_family_member_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                relationship_type = table.Column<int>(type: "int", nullable: false),
                created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                updated_at = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_roster_family_member_relationships", x => x.family_member_relationship_id);
                table.ForeignKey(
                    name: "FK_roster_family_member_relationships_roster_family_members_base_family_member_id",
                    column: x => x.base_family_member_id,
                    principalTable: "roster_family_members",
                    principalColumn: "family_member_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_canvas_treeedge_source_node_id",
            table: "canvas_treeedge",
            column: "source_node_id");

        migrationBuilder.CreateIndex(
            name: "IX_canvas_treeedge_target_node_id",
            table: "canvas_treeedge",
            column: "target_node_id");

        migrationBuilder.CreateIndex(
            name: "IX_canvas_treeedge_tree_id",
            table: "canvas_treeedge",
            column: "tree_id");

        migrationBuilder.CreateIndex(
            name: "IX_canvas_treenode_tree_id",
            table: "canvas_treenode",
            column: "tree_id");

        migrationBuilder.CreateIndex(
            name: "IX_canvas_treenode_member_family_member_id",
            table: "canvas_treenode_member",
            column: "family_member_id");

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_member_relationships_base_family_member_id_related_family_member_id_relationship_type",
            table: "roster_family_member_relationships",
            columns: new[] { "base_family_member_id", "related_family_member_id", "relationship_type" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_member_relationships_tree_id_base_family_member_id",
            table: "roster_family_member_relationships",
            columns: new[] { "tree_id", "base_family_member_id" });

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_members_claimed_by_user_id",
            table: "roster_family_members",
            column: "claimed_by_user_id");

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

        migrationBuilder.CreateIndex(
            name: "IX_users_external_logins_user_id",
            table: "users_external_logins",
            column: "user_id");

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
            name: "canvas_treeedge");

        migrationBuilder.DropTable(
            name: "canvas_treenode_member");

        migrationBuilder.DropTable(
            name: "roster_family_member_relationships");

        migrationBuilder.DropTable(
            name: "trees_tree_rbac");

        migrationBuilder.DropTable(
            name: "users_external_logins");

        migrationBuilder.DropTable(
            name: "canvas_treenode");

        migrationBuilder.DropTable(
            name: "roster_family_members");

        migrationBuilder.DropTable(
            name: "trees_tree");

        migrationBuilder.DropTable(
            name: "users_user");
    }
}
