using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Labb_3___API.Migrations
{
    /// <inheritdoc />
    public partial class _2tabeller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Intressen",
                columns: table => new
                {
                    IntresseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Beskrivning = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intressen", x => x.IntresseId);
                });

            migrationBuilder.CreateTable(
                name: "Personer",
                columns: table => new
                {
                    PersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PersonNamn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Telefonnummer = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personer", x => x.PersonId);
                });

            migrationBuilder.CreateTable(
                name: "Länkar",
                columns: table => new
                {
                    LänkId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    URL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FkIntresseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Länkar", x => x.LänkId);
                    table.ForeignKey(
                        name: "FK_Länkar_Intressen_FkIntresseId",
                        column: x => x.FkIntresseId,
                        principalTable: "Intressen",
                        principalColumn: "IntresseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Intresset",
                columns: table => new
                {
                    IntressePersonId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FkIntresseId = table.Column<int>(type: "int", nullable: false),
                    FkPersonId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Intresset", x => x.IntressePersonId);
                    table.ForeignKey(
                        name: "FK_Intresset_Intressen_FkIntresseId",
                        column: x => x.FkIntresseId,
                        principalTable: "Intressen",
                        principalColumn: "IntresseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Intresset_Personer_FkPersonId",
                        column: x => x.FkPersonId,
                        principalTable: "Personer",
                        principalColumn: "PersonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Intresset_FkIntresseId",
                table: "Intresset",
                column: "FkIntresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Intresset_FkPersonId",
                table: "Intresset",
                column: "FkPersonId");

            migrationBuilder.CreateIndex(
                name: "IX_Länkar_FkIntresseId",
                table: "Länkar",
                column: "FkIntresseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Intresset");

            migrationBuilder.DropTable(
                name: "Länkar");

            migrationBuilder.DropTable(
                name: "Personer");

            migrationBuilder.DropTable(
                name: "Intressen");
        }
    }
}
