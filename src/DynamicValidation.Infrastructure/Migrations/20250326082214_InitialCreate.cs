using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DynamicValidation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DynamicModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DynamicModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ModelFields",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DataType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    MaxLength = table.Column<int>(type: "int", nullable: true),
                    DefaultValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false),
                    ModelId = table.Column<int>(type: "int", nullable: false),
                    NestedModelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelFields", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModelFields_DynamicModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DynamicModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ModelFields_DynamicModels_NestedModelId",
                        column: x => x.NestedModelId,
                        principalTable: "DynamicModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValidationRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RuleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ErrorMessage = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsModelLevel = table.Column<bool>(type: "bit", nullable: false),
                    ModelId = table.Column<int>(type: "int", nullable: true),
                    FieldId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidationRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidationRules_DynamicModels_ModelId",
                        column: x => x.ModelId,
                        principalTable: "DynamicModels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ValidationRules_ModelFields_FieldId",
                        column: x => x.FieldId,
                        principalTable: "ModelFields",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DynamicModels_Name",
                table: "DynamicModels",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModelFields_ModelId",
                table: "ModelFields",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelFields_NestedModelId",
                table: "ModelFields",
                column: "NestedModelId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationRules_FieldId",
                table: "ValidationRules",
                column: "FieldId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidationRules_ModelId",
                table: "ValidationRules",
                column: "ModelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ValidationRules");

            migrationBuilder.DropTable(
                name: "ModelFields");

            migrationBuilder.DropTable(
                name: "DynamicModels");
        }
    }
}
