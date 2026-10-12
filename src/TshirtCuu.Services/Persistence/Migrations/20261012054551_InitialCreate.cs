using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TshirtCuu.Services.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AdminUsers",
                columns: table => new
                {
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Role = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminUsers", x => x.Username);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Colors",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Hex = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Colors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Designs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CategoryId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Tagline = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    AccentHex = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    InkHex = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Designs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PricingRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    BaseUnitMxn = table.Column<int>(type: "int", nullable: false),
                    CatalogSideMxn = table.Column<int>(type: "int", nullable: false),
                    CustomSideMxn = table.Column<int>(type: "int", nullable: false),
                    BaseLeadDays = table.Column<int>(type: "int", nullable: false),
                    CatalogExtraDays = table.Column<int>(type: "int", nullable: false),
                    CustomExtraDays = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingRules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Quotations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    CustomerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quotations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sizes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Label = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sizes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ShirtLines",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    QuotationId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    ColorId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    ColorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ColorHex = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    SizeId = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    SizeLabel = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    FrontSource = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    FrontLabel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FrontCatalogId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    FrontDx = table.Column<double>(type: "float", nullable: false),
                    FrontDy = table.Column<double>(type: "float", nullable: false),
                    FrontScale = table.Column<double>(type: "float", nullable: false),
                    BackSource = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    BackLabel = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    BackCatalogId = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    BackDx = table.Column<double>(type: "float", nullable: false),
                    BackDy = table.Column<double>(type: "float", nullable: false),
                    BackScale = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShirtLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShirtLines_Quotations_QuotationId",
                        column: x => x.QuotationId,
                        principalTable: "Quotations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Designs_CategoryId",
                table: "Designs",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ShirtLines_QuotationId",
                table: "ShirtLines",
                column: "QuotationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AdminUsers");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Colors");

            migrationBuilder.DropTable(
                name: "Designs");

            migrationBuilder.DropTable(
                name: "PricingRules");

            migrationBuilder.DropTable(
                name: "ShirtLines");

            migrationBuilder.DropTable(
                name: "Sizes");

            migrationBuilder.DropTable(
                name: "Quotations");
        }
    }
}
