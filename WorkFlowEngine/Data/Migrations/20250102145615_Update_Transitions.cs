using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FundManagerStateMachine.Data.Migrations
{
    /// <inheritdoc />
    public partial class Update_Transitions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "Administration");

            migrationBuilder.EnsureSchema(
                name: "Project");

            migrationBuilder.EnsureSchema(
                name: "Registration");

            migrationBuilder.CreateTable(
                name: "States",
                schema: "Registration",
                columns: table => new
                {
                    StateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedById = table.Column<int>(type: "int", nullable: true),
                    DateStamp = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedById = table.Column<int>(type: "int", nullable: true),
                    ModifiedDateTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => x.StateId);
                });

            migrationBuilder.CreateTable(
                name: "RollBackStates",
                schema: "Project",
                columns: table => new
                {
                    RollBackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrentStateId = table.Column<int>(type: "int", nullable: false),
                    RollbackStateId = table.Column<int>(type: "int", nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    DateStamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    ModifiedBy = table.Column<int>(type: "int", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RollBackStates", x => x.RollBackId);
                    table.ForeignKey(
                        name: "FK_RollBackStates_States_CurrentStateId",
                        column: x => x.CurrentStateId,
                        principalSchema: "Registration",
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RollBackStates_States_RollbackStateId",
                        column: x => x.RollbackStateId,
                        principalSchema: "Registration",
                        principalTable: "States",
                        principalColumn: "StateId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FilterConditions",
                schema: "Administration",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Operator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ObjectTypeId = table.Column<int>(type: "int", nullable: true),
                    ObjectId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true),
                    EvaluatedLinkedId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    LogicalOperator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransitionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FilterConditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FilterConditions_FilterConditions_ParentId",
                        column: x => x.ParentId,
                        principalSchema: "Administration",
                        principalTable: "FilterConditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FilterConditions_RollBackStates_TransitionId",
                        column: x => x.TransitionId,
                        principalSchema: "Project",
                        principalTable: "RollBackStates",
                        principalColumn: "RollBackId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FilterConditions_ParentId",
                schema: "Administration",
                table: "FilterConditions",
                column: "ParentId");

            migrationBuilder.CreateIndex(
                name: "IX_FilterConditions_TransitionId",
                schema: "Administration",
                table: "FilterConditions",
                column: "TransitionId");

            migrationBuilder.CreateIndex(
                name: "IX_RollBackStates_CurrentStateId",
                schema: "Project",
                table: "RollBackStates",
                column: "CurrentStateId");

            migrationBuilder.CreateIndex(
                name: "IX_RollBackStates_RollbackStateId",
                schema: "Project",
                table: "RollBackStates",
                column: "RollbackStateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FilterConditions",
                schema: "Administration");

            migrationBuilder.DropTable(
                name: "RollBackStates",
                schema: "Project");

            migrationBuilder.DropTable(
                name: "States",
                schema: "Registration");
        }
    }
}
