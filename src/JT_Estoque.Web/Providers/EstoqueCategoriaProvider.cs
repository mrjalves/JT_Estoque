using JT_Aplicacao.Classes;
using JT_Classes.CRM;
using JT_Estoque.Core.Entities;
using JT_UserInterface.Shared.Services.Interfaces.Comum;

namespace JT_Estoque.Web.Providers
{
    public class EstoqueCategoriaProvider : IModuloCategoriaProvider
    {
        public string NomeModulo => "Estoque & Logística";
        public eModuloSistema Modulo => eModuloSistema.Gestao;

        public List<Type> GetEntidadesComCategoria() =>
        [
            typeof(ProdutoEstoque),
            typeof(Fornecedor)
        ];
    }
}
