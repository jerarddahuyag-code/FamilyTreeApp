using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTreeApp.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddCanvas : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "canvas_treenode",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                node_type = table.Column<int>(type: "integer", nullable: false),
                x = table.Column<double>(type: "double precision", nullable: false),
                y = table.Column<double>(type: "double precision", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
            name: "canvas_treeedge",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                tree_id = table.Column<Guid>(type: "uuid", nullable: false),
                source_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                target_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_canvas_treeedge", x => x.id);
                table.ForeignKey(
                    name: "FK_canvas_treeedge_canvas_treenode_source_node_id",
                    column: x => x.source_node_id,
                    principalTable: "canvas_treenode",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_canvas_treeedge_canvas_treenode_target_node_id",
                    column: x => x.target_node_id,
                    principalTable: "canvas_treenode",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
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
                tree_node_id = table.Column<Guid>(type: "uuid", nullable: false),
                family_member_id = table.Column<Guid>(type: "uuid", nullable: false)
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
                    name: "FK_canvas_treenode_member_roster_family_members_family_member_~",
                    column: x => x.family_member_id,
                    principalTable: "roster_family_members",
                    principalColumn: "family_member_id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_roster_family_members_claimed_by_user_id",
            table: "roster_family_members",
            column: "claimed_by_user_id");

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

        migrationBuilder.AddForeignKey(
            name: "FK_roster_family_members_users_user_claimed_by_user_id",
            table: "roster_family_members",
            column: "claimed_by_user_id",
            principalTable: "users_user",
            principalColumn: "user_id",
            onDelete: ReferentialAction.SetNull);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_roster_family_members_users_user_claimed_by_user_id",
            table: "roster_family_members");

        migrationBuilder.DropTable(
            name: "canvas_treeedge");

        migrationBuilder.DropTable(
            name: "canvas_treenode_member");

        migrationBuilder.DropTable(
            name: "canvas_treenode");

        migrationBuilder.DropIndex(
            name: "IX_roster_family_members_claimed_by_user_id",
            table: "roster_family_members");
    }
}
