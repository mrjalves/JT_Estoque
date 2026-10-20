using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Enums;
using JT_Servicos;

namespace JT_Estoque.Core.Services
{
    public class LoteEstoqueService : JTServiceBase<LoteEstoque>
    {
        public LoteEstoqueService(HttpClient? httpClient = null) : base(httpClient)
        {
        }

        protected override string GetApiRoute()
        {
            return "api/estoque/lotes";
        }

        public async Task<List<LoteEstoque>> ObterLotesPorProdutoAsync(int produtoId)
        {
            var todos = await GetAll();
            return todos.Where(l => l.ProdutoId == produtoId).ToList();
        }

        public async Task<List<LoteEstoque>> ObterLotesAVencerAsync(int diasAviso = 30)
        {
            var limite = DateTime.Today.AddDays(diasAviso);
            var todos = await GetAll();
            return todos
                .Where(l => l.StatusLote != eStatusLote.Esgotado &&
                            l.DataValidade.HasValue &&
                            l.DataValidade.Value.Date <= limite)
                .OrderBy(l => l.DataValidade)
                .ToList();
        }
    }
}
