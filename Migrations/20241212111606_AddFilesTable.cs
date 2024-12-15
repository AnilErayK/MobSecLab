using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobSecLab.Migrations
{
    /// <inheritdoc />
    public partial class AddFilesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Files",
                columns: table => new
                {
                    FileNum = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserNo = table.Column<int>(type: "integer", nullable: false),
                    FileSeq = table.Column<int>(type: "integer", nullable: false),
                    File_Name = table.Column<string>(type: "text", nullable: false),
                    File_md5 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Files", x => x.FileNum);
                    table.ForeignKey(
                        name: "FK_Files_Users_UserNo",
                        column: x => x.UserNo,
                        principalTable: "Users",
                        principalColumn: "UserNo",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Files_UserNo",
                table: "Files",
                column: "UserNo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Files");
        }
    }
}
