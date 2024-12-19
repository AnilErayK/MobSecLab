using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobSecLab.Migrations
{
    /// <inheritdoc />
    public partial class AddAIresultsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIresults",
                columns: table => new
                {
                    ResultsNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileSeq = table.Column<int>(type: "integer", nullable: false),
                    File_md5 = table.Column<string>(type: "text", nullable: false),
                    File_Name = table.Column<string>(type: "text", nullable: false),
                    UserNo = table.Column<int>(type: "integer", nullable: false),
                    ResultMessage = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIresults", x => x.ResultsNo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIresults");
        }
    }
}
