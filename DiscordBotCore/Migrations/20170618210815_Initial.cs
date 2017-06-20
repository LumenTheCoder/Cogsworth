using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DiscordBotCore.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserInput",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    Input = table.Column<string>(type: "STRING", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInput", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInputRelationship",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "STRING", nullable: false),
                    BotOutputId = table.Column<Guid>(type: "STRING", nullable: false),
                    TimesReplied = table.Column<int>(nullable: false),
                    UserReplyId = table.Column<Guid>(type: "STRING", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInputRelationship", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInputRelationship_UserInput_BotOutputId",
                        column: x => x.BotOutputId,
                        principalTable: "UserInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserInputRelationship_UserInput_UserReplyId",
                        column: x => x.UserReplyId,
                        principalTable: "UserInput",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserInputRelationship_BotOutputId",
                table: "UserInputRelationship",
                column: "BotOutputId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInputRelationship_UserReplyId",
                table: "UserInputRelationship",
                column: "UserReplyId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserInputRelationship");

            migrationBuilder.DropTable(
                name: "UserInput");
        }
    }
}
