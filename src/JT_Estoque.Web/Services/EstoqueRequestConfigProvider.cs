using JT_Aplicacao.Classes;
using JT_Aplicacao.Config;
using Microsoft.Extensions.Configuration;

namespace JT_Estoque.Web.Services
{
    /// <summary>
    /// Provedor de configuração de banco de dados para o módulo JT_Estoque.
    /// Permite que o SchemaService e o DbSchemaMigrator executem operações locais diretamente.
    /// </summary>
    public class EstoqueRequestConfigProvider : IRequestConfigProvider
    {
        private readonly string _stringConexao;
        private readonly string _tipoBanco;
        private readonly string _clientId;
        private readonly List<eModuloSistema> _modulosAtivos;

        public EstoqueRequestConfigProvider(IConfiguration configuration)
        {
            _clientId = "JT_Estoque";

            _stringConexao = configuration["Config:Clientes:JT_Estoque:StringConexao"]
                          ?? configuration["Config:Conexao:StringConexao"]
                          ?? Config.Instancia?.StringConexao
                          ?? string.Empty;

            _tipoBanco = configuration["Config:Clientes:JT_Estoque:TipoBanco"]
                      ?? configuration["Config:Conexao:TipoBanco"]
                      ?? Config.Instancia?.TipoBanco
                      ?? "SqlServer";

            _modulosAtivos =
            [
                eModuloSistema.Comum,
                eModuloSistema.Acessos,
                eModuloSistema.Gestao,
                eModuloSistema.CRM,
                eModuloSistema.IA
            ];
        }

        public string StringConexao => _stringConexao;
        public string TipoBanco => _tipoBanco;
        public string ClientId => _clientId;
        public List<eModuloSistema> ModulosAtivos => _modulosAtivos;
    }
}
