using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Driven.SqlLite.Migrations
{
    /// <inheritdoc />
    public partial class AddCardsAndOutboxEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Cards table
            migrationBuilder.CreateTable(
                name: "Cards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClienteId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropostaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ContaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CodigoProduto = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TokenPan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    TokenCvv = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false, defaultValue: "REQUESTED"),
                    CanalAtivacao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    CorrelacaoId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    MesValidade = table.Column<int>(type: "INTEGER", nullable: false),
                    AnoValidade = table.Column<int>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cards_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // Create indices for Cards
            migrationBuilder.CreateIndex(
                name: "IX_Card_ClienteId",
                table: "Cards",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_PropostaId",
                table: "Cards",
                column: "PropostaId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_ContaId",
                table: "Cards",
                column: "ContaId");

            migrationBuilder.CreateIndex(
                name: "IX_Card_Status",
                table: "Cards",
                column: "Status");

            // Create CardIdempotencyKeys table
            migrationBuilder.CreateTable(
                name: "CardIdempotencyKeys",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChaveIdempotencia = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CartoesIds = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CardIdempotencyKeys", x => x.Id);
                });

            // Create indices for CardIdempotencyKeys
            migrationBuilder.CreateIndex(
                name: "IX_CardIdempotencyKey_Chave",
                table: "CardIdempotencyKeys",
                column: "ChaveIdempotencia",
                unique: true);

            // Create OutboxEvents table
            migrationBuilder.CreateTable(
                name: "OutboxEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Topico = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Payload = table.Column<string>(type: "TEXT", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CriadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    AtualizadoPor = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxEvents", x => x.Id);
                });

            // Create indices for OutboxEvents
            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvent_Topico",
                table: "OutboxEvents",
                column: "Topico");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxEvent_DataEnvio",
                table: "OutboxEvents",
                column: "DataEnvio");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CardIdempotencyKeys");

            migrationBuilder.DropTable(
                name: "Cards");

            migrationBuilder.DropTable(
                name: "OutboxEvents");
        }
    }
}
