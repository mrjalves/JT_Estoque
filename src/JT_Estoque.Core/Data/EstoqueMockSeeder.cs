using JT_Classes.Comum;
using JT_Classes.CRM;
using JT_Classes.Produto;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Entities.Views;

using Microsoft.Data.SqlClient;

namespace JT_Estoque.Core.Data
{
    /// <summary>
    /// Responsável por executar o seed de dados mock para o módulo JT_Estoque.
    /// Pode ser invocado após a migração de schema (DbSchemaMigrator) ou via CLI administrativa.
    /// </summary>
    public static class EstoqueMockSeeder
    {
        private const string ScriptSeed = "SeedMockDataEstoque.sql";
        private const string ScriptReset = "ResetMockDataEstoque.sql";

        /// <summary>
        /// Executa o script de mock data no banco de dados indicado pela string de conexão.
        /// Opcionalmente executa o DbSchemaMigrator antes para assegurar a criação prévia de tabelas e colunas.
        /// </summary>
        /// <param name="connectionString">String de conexão do banco do Estoque.</param>
        /// <param name="garantirSchema">Executa a migração de schema antes do seed.</param>
        /// <param name="reset">Remove os registros mock antes de aplicar o seed.</param>
        /// <param name="dryRun">Apenas imprime os DDLs da migração; não executa reset nem seed.</param>
        /// <param name="removerOrfaos">Permite ao migrator remover colunas, FKs e tabelas fora do contexto. Padrão: false.</param>
        /// <param name="aplicarSeed">Executa o script de seed após a migração/reset.</param>
        public static async Task ExecutarAsync(
            string connectionString,
            bool garantirSchema = true,
            bool reset = false,
            bool dryRun = false,
            bool removerOrfaos = false,
            bool aplicarSeed = true)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A connection string não pode ser nula ou vazia.", nameof(connectionString));

            if (garantirSchema)
            {
                Console.WriteLine("[MOCK] Verificando e provisionando schema das entidades do Estoque...");
                var migrator = new JT_Infra_Data.Schemas.DbSchemaMigrator(connectionString);
                migrator.AplicarMigracoes(ObterTiposEstoque(), dryRun, removerOrfaos);
                Console.WriteLine("[MOCK] Schema verificado/provisionado com sucesso.");
            }

            if (dryRun)
            {
                Console.WriteLine("[MOCK] Dry-run habilitado. Reset e seed não serão executados.");
                return;
            }

            if (reset)
            {
                Console.WriteLine("[MOCK] Removendo registros mock existentes...");
                var scriptReset = await LerScriptAsync(ScriptReset);
                await ExecutarScriptSqlAsync(connectionString, scriptReset);
                Console.WriteLine("[MOCK] Registros mock removidos.");
            }

            if (!aplicarSeed)
            {
                Console.WriteLine("[MOCK] Seed não solicitado. Etapa finalizada.");
                return;
            }

            Console.WriteLine("[MOCK] Executando script SQL de seed...");
            var scriptSeed = await LerScriptAsync(ScriptSeed);
            await ExecutarScriptSqlAsync(connectionString, scriptSeed);
            Console.WriteLine("[MOCK] Script SQL de seed executado com sucesso.");

            await RegistrarContagensAsync(connectionString);
        }

        /// <summary>
        /// Tipos mínimos necessários para o schema do Estoque e para as entidades
        /// referenciadas pelo script de seed.
        /// </summary>
        public static IReadOnlyList<Type> ObterTiposEstoque() =>
        [
            typeof(Produto),
            typeof(ProdutoEstoque),
            typeof(LocalEstoque),
            typeof(LoteEstoque),
            typeof(MovimentacaoEstoque),
            typeof(InventarioEstoque),
            typeof(InventarioItem),
            typeof(VwSaldoEstoquePorLocal),
            typeof(Categoria),
            typeof(Pessoa),
            typeof(Fornecedor),
            typeof(Documento)
        ];

        /// <summary>
        /// Executa blocos de instruções SQL diretamente contra o SQL Server.
        /// </summary>
        public static async Task ExecutarScriptSqlAsync(string connectionString, string scriptSql)
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Divide por 'GO' se presente, ou executa como lote único
            var batches = System.Text.RegularExpressions.Regex.Split(
                scriptSql,
                @"^\s*GO\s*$",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);

            await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync();

            try
            {
                foreach (var batch in batches)
                {
                    var sql = batch.Trim();
                    if (string.IsNullOrWhiteSpace(sql)) continue;

                    await using var command = new SqlCommand(sql, connection, transaction);
                    command.CommandTimeout = 120;
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        private static async Task<string> LerScriptAsync(string nomeArquivo)
        {
            var scriptPath = ObterCaminhoScript(nomeArquivo);
            if (!File.Exists(scriptPath))
                throw new FileNotFoundException($"Script SQL não encontrado em '{scriptPath}'.", scriptPath);

            Console.WriteLine($"[MOCK] Lendo script: {scriptPath}");
            return await File.ReadAllTextAsync(scriptPath);
        }

        private static async Task RegistrarContagensAsync(string connectionString)
        {
            const string sql = """
                SELECT
                    (SELECT COUNT(*) FROM Produtos) AS Produtos,
                    (SELECT COUNT(*) FROM LocaisEstoque) AS Locais,
                    (SELECT COUNT(*) FROM EstoqueLotes) AS Lotes,
                    (SELECT COUNT(*) FROM EstoqueMovimentacoes) AS Movimentacoes,
                    (SELECT COUNT(*) FROM EstoqueInventarios) AS Inventarios,
                    (SELECT COUNT(*) FROM Fornecedores) AS Fornecedores;
                """;

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            await using var command = new SqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync()) return;

            Console.WriteLine(
                "[MOCK] Contagens atuais: " +
                $"Produtos={reader["Produtos"]}, " +
                $"Locais={reader["Locais"]}, " +
                $"Lotes={reader["Lotes"]}, " +
                $"Movimentações={reader["Movimentacoes"]}, " +
                $"Inventários={reader["Inventarios"]}, " +
                $"Fornecedores={reader["Fornecedores"]}");
        }

        private static string ObterCaminhoScript(string nomeArquivo)
        {
            // Tenta localizar nas pastas comuns do projeto
            var caminhosPossiveis = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "scripts", nomeArquivo),
                Path.Combine(AppContext.BaseDirectory, "Data", nomeArquivo),
                Path.Combine(AppContext.BaseDirectory, nomeArquivo),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", nomeArquivo),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "JT_Estoque.Core", "Data", nomeArquivo)
            };

            foreach (var caminho in caminhosPossiveis)
            {
                if (File.Exists(caminho))
                    return caminho;
            }

            // Fallback para caminho relativo padrão
            return caminhosPossiveis[0];
        }
    }
}
