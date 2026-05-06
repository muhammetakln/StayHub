using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AddHotelIdToGuest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HotelId",
                table: "AspNetUsers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_HotelId",
                table: "AspNetUsers",
                column: "HotelId");

            migrationBuilder.CreateIndex(
                name: "IX_AddOnServices_HotelId",
                table: "AddOnServices",
                column: "HotelId");

            migrationBuilder.AddForeignKey(
                name: "FK_AddOnServices_Hotels_HotelId",
                table: "AddOnServices",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Hotels_HotelId",
                table: "AspNetUsers",
                column: "HotelId",
                principalTable: "Hotels",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AddOnServices_Hotels_HotelId",
                table: "AddOnServices");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Hotels_HotelId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_HotelId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AddOnServices_HotelId",
                table: "AddOnServices");

            migrationBuilder.DropColumn(
                name: "HotelId",
                table: "AspNetUsers");
        }
    }
}
