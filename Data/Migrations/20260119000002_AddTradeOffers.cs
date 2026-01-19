using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SwapSmart.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTradeOffers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TradeOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OfferedItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    SenderUserId = table.Column<string>(type: "TEXT", nullable: false),
                    ReceiverUserId = table.Column<string>(type: "TEXT", nullable: false),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    IsContactOpened = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TradeOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TradeOffers_Items_OfferedItemId",
                        column: x => x.OfferedItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOffers_Items_RequestedItemId",
                        column: x => x.RequestedItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TradeOffers_AspNetUsers_ReceiverUserId",
                        column: x => x.ReceiverUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TradeOffers_AspNetUsers_SenderUserId",
                        column: x => x.SenderUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_OfferedItemId",
                table: "TradeOffers",
                column: "OfferedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_ReceiverUserId",
                table: "TradeOffers",
                column: "ReceiverUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_RequestedItemId",
                table: "TradeOffers",
                column: "RequestedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_TradeOffers_SenderUserId",
                table: "TradeOffers",
                column: "SenderUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TradeOffers");
        }
    }
}
