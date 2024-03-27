using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankingSystem.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class Transaction_Entity_Update : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LoggedInTeller",
                table: "Transactions",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LoggedInTeller",
                table: "Transactions");
        }
    }
}
