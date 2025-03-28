using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace worker_service.Migrations
{
    /// <inheritdoc />
    public partial class CreateCoinsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Coins",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Symbol = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MarketCap = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MarketCapRank = table.Column<int>(type: "int", nullable: false),
                    TotalSupply = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceChange24h = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceInUsd = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceInEuro = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PriceInAed = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coins", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Coins");
        }
    }
}
