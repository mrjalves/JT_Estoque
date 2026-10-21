using Microsoft.Data.SqlClient;

namespace JT_Estoque.Core.Data
{
    /// <summary>
    /// Responsável por executar o seed de dados mock para o módulo JT_Estoque.
    /// Pode ser invocado após a migração de schema (DbSchemaMigrator) ou via endpoint administrativo.
    /// </summary>
    public static class EstoqueMockSeeder
    {
        /// <summary>
        /// Executa o script de mock data no banco de dados indicado pela string de conexão.
        /// Opcionalmente executa o DbSchemaMigrator antes para assegurar a criação prévia de tabelas e colunas.
        /// </summary>
        public static async Task ExecutarAsync(string connectionString, bool garantirSchema = true)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new ArgumentException("A connection string não pode ser nula ou vazia.", nameof(connectionString));

            if (garantirSchema)
            {
                try
                {
                    Console.WriteLine("[MOCK] Verificando e provisionando schema das entidades antes do seed...");
                    var migrator = new JT_Infra_Data.Schemas.DbSchemaMigrator(connectionString);
                    migrator.AplicarMigracoes(dryRun: false);
                    Console.WriteLine("[MOCK] Schema do banco verificado/provisionado com sucesso.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MOCK][AVISO] Não foi possível verificar schema via DbSchemaMigrator: {ex.Message}");
                }
            }

            var scriptPath = ObterCaminhoScript();
            if (!File.Exists(scriptPath))
            {
                throw new FileNotFoundException($"Script de mock não encontrado em '{scriptPath}'.");
            }

            Console.WriteLine($"[MOCK] Executando script SQL de seed a partir de: {scriptPath}");
            var scriptSql = await File.ReadAllTextAsync(scriptPath);
            await ExecutarScriptSqlAsync(connectionString, scriptSql);
            Console.WriteLine("[MOCK] Script SQL de seed executado com sucesso.");
        }

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

            foreach (var batch in batches)
            {
                var sql = batch.Trim();
                if (string.IsNullOrWhiteSpace(sql)) continue;

                await using var command = new SqlCommand(sql, connection);
                command.CommandTimeout = 120;
                await command.ExecuteNonQueryAsync();
            }
        }

        private static string ObterCaminhoScript()
        {
            // Tenta localizar nas pastas comuns do projeto
            var caminhosPossiveis = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "scripts", "SeedMockDataEstoque.sql"),
                Path.Combine(AppContext.BaseDirectory, "Data", "SeedMockDataEstoque.sql"),
                Path.Combine(AppContext.BaseDirectory, "SeedMockDataEstoque.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "scripts", "SeedMockDataEstoque.sql"),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "JT_Estoque.Core", "Data", "SeedMockDataEstoque.sql")
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
