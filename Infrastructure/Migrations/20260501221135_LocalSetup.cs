using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LocalSetup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ABTestExperiments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ControlStrategy = table.Column<int>(type: "int", nullable: false),
                    TreatmentStrategy = table.Column<int>(type: "int", nullable: false),
                    TreatmentPercentage = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ABTestExperiments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserInteractions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SessionId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserInteractions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserInteractions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserInteractions_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ABTestAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExperimentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    IsTreatment = table.Column<bool>(type: "bit", nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ABTestAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ABTestAssignments_ABTestExperiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "ABTestExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ABTestAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RecommendationEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RecommendedProductId = table.Column<int>(type: "int", nullable: false),
                    SourceProductId = table.Column<int>(type: "int", nullable: true),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    Strategy = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<int>(type: "int", nullable: false),
                    ExperimentId = table.Column<int>(type: "int", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecommendationEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecommendationEvents_ABTestExperiments_ExperimentId",
                        column: x => x.ExperimentId,
                        principalTable: "ABTestExperiments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RecommendationEvents_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RecommendationEvents_Products_RecommendedProductId",
                        column: x => x.RecommendedProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ABTestAssignments_ExperimentId",
                table: "ABTestAssignments",
                column: "ExperimentId");

            migrationBuilder.CreateIndex(
                name: "IX_ABTestAssignments_UserId_ExperimentId",
                table: "ABTestAssignments",
                columns: new[] { "UserId", "ExperimentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ABTestExperiments_IsActive",
                table: "ABTestExperiments",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_ExperimentId_EventType",
                table: "RecommendationEvents",
                columns: new[] { "ExperimentId", "EventType" });

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_RecommendedProductId",
                table: "RecommendationEvents",
                column: "RecommendedProductId");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_Strategy",
                table: "RecommendationEvents",
                column: "Strategy");

            migrationBuilder.CreateIndex(
                name: "IX_RecommendationEvents_UserId_Timestamp",
                table: "RecommendationEvents",
                columns: new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractions_ProductId_Timestamp",
                table: "UserInteractions",
                columns: new[] { "ProductId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractions_SessionId",
                table: "UserInteractions",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserInteractions_UserId_Timestamp",
                table: "UserInteractions",
                columns: new[] { "UserId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ABTestAssignments");

            migrationBuilder.DropTable(
                name: "RecommendationEvents");

            migrationBuilder.DropTable(
                name: "UserInteractions");

            migrationBuilder.DropTable(
                name: "ABTestExperiments");
        }
    }
}
