using System.ComponentModel;
using System.Text.Json;
using JT_Classes.Acessos;
using JT_Estoque.Core.Services;
using JT_Servicos.IA.Tools;
using Microsoft.SemanticKernel;

namespace JT_Estoque.Core.IATools
{
    public class ConsultarRupturaEstoqueTool : IAToolBase
    {
        private readonly ProdutoEstoqueService _produtoService;

        public ConsultarRupturaEstoqueTool(
            CurrentUserState userState,
            ProdutoEstoqueService produtoService) : base(userState)
        {
            _produtoService = produtoService;
        }

        public override string Nome => "consultar_ruptura_estoque";
        public override string Descricao => "Lista produtos cujo saldo atual está igual ou abaixo do Estoque Mínimo ou Ponto de Pedido.";

        [KernelFunction("consultar_ruptura_estoque")]
        [Description("Lista produtos em situação de ruptura de estoque ou que atingiram ponto de reposição.")]
        public async Task<string> ConsultarRupturaAsync()
        {
            var todos = await _produtoService.GetAll();
            var criticos = todos
                .Where(p => p.Ativo && p.QuantidadeEstoque <= p.EstoqueMinimo)
                .Select(p => new
                {
                    p.Id,
                    p.CodigoSKU,
                    p.Nome,
                    SaldoAtual = p.QuantidadeEstoque,
                    p.EstoqueMinimo,
                    p.PontoPedido,
                    Deficit = p.EstoqueMinimo - p.QuantidadeEstoque,
                    p.CustoMedio
                })
                .ToList();

            return JsonSerializer.Serialize(new
            {
                TotalItensEmRuptura = criticos.Count,
                Itens = criticos
            });
        }

        public override Task<string> ExecutarAsync(Dictionary<string, object> parametros)
        {
            return ConsultarRupturaAsync();
        }
    }
}
