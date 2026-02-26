using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpotifyStats.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpotifyAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SpotifyUserId = table.Column<string>(type: "TEXT", nullable: false),
                    RefreshTokenCiphertext = table.Column<byte[]>(type: "BLOB", nullable: false),
                    RefreshTokenKeyId = table.Column<string>(type: "TEXT", nullable: true),
                    AccessTokenCiphertext = table.Column<byte[]>(type: "BLOB", nullable: true),
                    AccessTokenExpiresAtUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Scope = table.Column<string>(type: "TEXT", nullable: false),
                    TokenType = table.Column<string>(type: "TEXT", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SpotifyAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SpotifyAccounts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyAccounts_SpotifyUserId",
                table: "SpotifyAccounts",
                column: "SpotifyUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SpotifyAccounts_UserId",
                table: "SpotifyAccounts",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SpotifyAccounts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
