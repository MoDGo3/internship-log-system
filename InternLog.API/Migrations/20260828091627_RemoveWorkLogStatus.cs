using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InternLog.API.Migrations
{
    /// <inheritdoc />
    public partial class RemoveWorkLogStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "WorkLogs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$g1ogqNx9nv7.mhSU0mPhyOfrhqoJn2Y1Nd7IiZCVa0/JdE../UQPW");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "WorkLogs",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "draft");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "UserID",
                keyValue: 1,
                column: "PasswordHash",
                value: "$2a$11$ulHAsznEUHAY7zU4OICL8OuKYb.kym8lhCJF1x01Cw.O7lrHgS45a");
        }
    }
}
