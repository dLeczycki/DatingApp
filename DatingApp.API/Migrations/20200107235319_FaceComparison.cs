using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace DatingApp.API.Migrations
{
    public partial class FaceComparison : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Preferences",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    FacialHair = table.Column<double>(nullable: false),
                    Glasses = table.Column<string>(nullable: true),
                    MakeUp = table.Column<bool>(nullable: false),
                    Hair = table.Column<string>(nullable: true),
                    Personality = table.Column<string>(nullable: true),
                    Attitude = table.Column<string>(nullable: true),
                    Assertive = table.Column<bool>(nullable: false),
                    Patriotic = table.Column<bool>(nullable: false),
                    SelfConfident = table.Column<bool>(nullable: false),
                    WithSenseOfHumour = table.Column<bool>(nullable: false),
                    HardWorking = table.Column<bool>(nullable: false),
                    Tolerant = table.Column<bool>(nullable: false),
                    Kind = table.Column<bool>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Preferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Preferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UsersTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn)
                        .Annotation("Sqlite:Autoincrement", true),
                    FacialHair = table.Column<double>(nullable: false),
                    Glasses = table.Column<string>(nullable: true),
                    MakeUp = table.Column<bool>(nullable: false),
                    Hair = table.Column<string>(nullable: true),
                    Personality = table.Column<string>(nullable: true),
                    Attitude = table.Column<string>(nullable: true),
                    Assertive = table.Column<bool>(nullable: false),
                    Patriotic = table.Column<bool>(nullable: false),
                    SelfConfident = table.Column<bool>(nullable: false),
                    WithSenseOfHumour = table.Column<bool>(nullable: false),
                    HardWorking = table.Column<bool>(nullable: false),
                    Tolerant = table.Column<bool>(nullable: false),
                    Kind = table.Column<bool>(nullable: false),
                    UserId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsersTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsersTemplates_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Preferences_UserId",
                table: "Preferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UsersTemplates_UserId",
                table: "UsersTemplates",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Preferences");

            migrationBuilder.DropTable(
                name: "UsersTemplates");
        }
    }
}
