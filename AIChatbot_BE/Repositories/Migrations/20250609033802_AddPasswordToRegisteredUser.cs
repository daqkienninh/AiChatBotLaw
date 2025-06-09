using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Repositories.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordToRegisteredUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registered_user",
                columns: table => new
                {
                    user_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    user_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    user_email = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    user_status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    role = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Register__B9BE370F009903D9", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "Question",
                columns: table => new
                {
                    question_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    user_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    question_content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ques_create_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Question__2EC21549E2006468", x => x.question_id);
                    table.ForeignKey(
                        name: "FK__Question__user_i__4E88ABD4",
                        column: x => x.user_id,
                        principalTable: "Registered_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "Answer",
                columns: table => new
                {
                    answer_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: false),
                    question_id = table.Column<string>(type: "nvarchar(36)", maxLength: 36, nullable: true),
                    ans_content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ans_create_at = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    legalclause_id = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Answer__33724318783AAB78", x => x.answer_id);
                    table.ForeignKey(
                        name: "FK__Answer__question__52593CB8",
                        column: x => x.question_id,
                        principalTable: "Question",
                        principalColumn: "question_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Answer_question_id",
                table: "Answer",
                column: "question_id");

            migrationBuilder.CreateIndex(
                name: "IX_Question_user_id",
                table: "Question",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "UQ__Register__B0FBA2122283A44E",
                table: "Registered_user",
                column: "user_email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Answer");

            migrationBuilder.DropTable(
                name: "Question");

            migrationBuilder.DropTable(
                name: "Registered_user");
        }
    }
}
