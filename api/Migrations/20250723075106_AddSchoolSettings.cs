using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SchoolSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SchoolId = table.Column<int>(type: "INTEGER", nullable: false),
                    SchoolDisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    LogoImageUrl = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    NavigationType = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ThemeColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SchoolSettings_Schools_SchoolId",
                        column: x => x.SchoolId,
                        principalTable: "Schools",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SchoolSettings_SchoolId",
                table: "SchoolSettings",
                column: "SchoolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SchoolSettings");
        }
    }
}
