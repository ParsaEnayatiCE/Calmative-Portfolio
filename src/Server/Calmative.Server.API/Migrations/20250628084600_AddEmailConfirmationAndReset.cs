using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calmative.Server.API.Migrations
{
    public partial class AddEmailConfirmationAndReset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ConfirmationTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailConfirmed",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiryTime",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7833));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7835));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7836));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7837));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7838));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7839));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 8, 45, 59, 845, DateTimeKind.Utc).AddTicks(7840));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConfirmationTokenExpiryTime",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsEmailConfirmed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiryTime",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3260));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3262));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3263));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3264));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3265));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3266));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Timestamp",
                value: new DateTime(2025, 6, 25, 14, 37, 8, 632, DateTimeKind.Utc).AddTicks(3267));
        }
    }
}
