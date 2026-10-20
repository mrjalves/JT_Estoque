using JT_Estoque.Core.Entities;
using JT_Servicos;

namespace JT_Estoque.Core.Services
{
    public class LocalEstoqueService : JTServiceBase<LocalEstoque>
    {
        public LocalEstoqueService(HttpClient? httpClient = null) : base(httpClient)
        {
        }

        protected override string GetApiRoute()
        {
            return "api/estoque/locais";
        }
    }
}
