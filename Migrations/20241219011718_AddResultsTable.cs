using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MobSecLab.Migrations
{
    /// <inheritdoc />
    public partial class AddResultsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIresults");

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ResultsNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileSeq = table.Column<int>(type: "integer", nullable: false),
                    md5 = table.Column<string>(type: "text", nullable: false),
                    File_Name = table.Column<string>(type: "text", nullable: false),
                    UserNo = table.Column<int>(type: "integer", nullable: false),
                    TotalMalwarePermission = table.Column<int>(type: "integer", nullable: false),
                    TotalPermission = table.Column<int>(type: "integer", nullable: false),
                    SeverityHigh = table.Column<int>(type: "integer", nullable: false),
                    StatusDangerous = table.Column<int>(type: "integer", nullable: false),
                    minSdk = table.Column<string>(type: "text", nullable: false),
                    SecurityScore = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ResultsNo);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.CreateTable(
                name: "AIresults",
                columns: table => new
                {
                    ResultsNo = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileSeq = table.Column<int>(type: "integer", nullable: false),
                    File_Name = table.Column<string>(type: "text", nullable: false),
                    File_md5 = table.Column<string>(type: "text", nullable: false),
                    ResultMessage = table.Column<string>(type: "text", nullable: false),
                    UserNo = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIresults", x => x.ResultsNo);
                });
        }
    }
}
