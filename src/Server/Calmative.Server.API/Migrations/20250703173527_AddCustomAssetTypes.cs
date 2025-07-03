using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Calmative.Server.API.Migrations
{
    public partial class AddCustomAssetTypes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomAssetTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomAssetTypes", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9520));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9522));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9523));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9523));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9524));

            migrationBuilder.UpdateData(
                table: "PriceHistories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Timestamp",
                value: new DateTime(2025, 7, 3, 17, 35, 27, 713, DateTimeKind.Utc).AddTicks(9525));

            migrationBuilder.CreateIndex(
                name: "IX_CustomAssetTypes_Name",
                table: "CustomAssetTypes",
                column: "Name",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomAssetTypes");

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
    }
}
