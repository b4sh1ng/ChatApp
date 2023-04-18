using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace GrpcServer.Migrations
{
    /// <inheritdoc />
    public partial class _1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "chats",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int(11)", nullable: false),
                    chatId = table.Column<int>(type: "int(11)", nullable: false),
                    isListed = table.Column<sbyte>(type: "tinyint(4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.userId, x.chatId });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "friendlist",
                columns: table => new
                {
                    userId1 = table.Column<int>(type: "int(11)", nullable: false),
                    userId2 = table.Column<int>(type: "int(11)", nullable: false),
                    isFriend = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "false")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.userId1, x.userId2 });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "messages",
                columns: table => new
                {
                    messageTimestamp = table.Column<long>(type: "bigint(20)", nullable: false),
                    fromId = table.Column<int>(type: "int(11)", nullable: false),
                    chatId = table.Column<int>(type: "int(11)", nullable: false),
                    message = table.Column<string>(type: "longtext", nullable: false),
                    isEdited = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0"),
                    isRead = table.Column<bool>(type: "tinyint(1)", nullable: true, defaultValueSql: "0")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => new { x.fromId, x.messageTimestamp });
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usercredentials",
                columns: table => new
                {
                    userId = table.Column<int>(type: "int(11)", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    username = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: false),
                    usernameId = table.Column<int>(type: "int(11)", nullable: false),
                    password = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    profileImgB64 = table.Column<string>(type: "longtext", nullable: true, defaultValueSql: "'NULL'")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.userId);
                })
                .Annotation("MySQL:Charset", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "chats");

            migrationBuilder.DropTable(
                name: "friendlist");

            migrationBuilder.DropTable(
                name: "messages");

            migrationBuilder.DropTable(
                name: "usercredentials");
        }
    }
}
