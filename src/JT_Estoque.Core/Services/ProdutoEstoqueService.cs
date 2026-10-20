using JT_Aplicacao.Classes;
using JT_Estoque.Core.Entities;
using JT_Servicos;

namespace JT_Estoque.Core.Services
{
    public class ProdutoEstoqueService : JTServiceBase<ProdutoEstoque>
    {
        public ProdutoEstoqueService(HttpClient? httpClient = null) : base(httpClient)
        {
        }

        protected override string GetApiRoute()
        {
            return "api/estoque/produtos";
        }

        public async Task<ProdutoEstoque?> ObterPorSKUAsync(string sku)
        {
            var todos = await GetAll();
            return todos.FirstOrDefault(p => string.Equals(p.CodigoSKU, sku, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<List<ProdutoEstoque>> ObterProdutosCriticosAsync()
        {
            var todos = await GetAll();
            return todos.Where(p => p.Ativo && p.QuantidadeEstoque <= p.EstoqueMinimo).ToList();
        }
    }
}
