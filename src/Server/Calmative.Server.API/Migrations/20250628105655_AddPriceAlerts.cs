using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calmative.Server.API.Migrations
{
    public partial class AddPriceAlerts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8193));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8196));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8197));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8198));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8199));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8200));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Timestamp",
                value: new DateTime(2025, 6, 28, 10, 56, 55, 98, DateTimeKind.Utc).AddTicks(8201));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
