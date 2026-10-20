using System.ComponentModel;
using System.Text.Json;
using JT_Classes.Acessos;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Services;
using JT_Servicos.IA.Tools;
using Microsoft.SemanticKernel;

namespace JT_Estoque.Core.IATools
{
    public class ConsultarKardexProdutoTool : IAToolBase
    {
        private readonly ProdutoEstoqueService _produtoService;
        private readonly MovimentacaoEstoqueService _movimentacaoService;

        public ConsultarKardexProdutoTool(
            CurrentUserState userState,
            ProdutoEstoqueService produtoService,
            MovimentacaoEstoqueService movimentacaoService) : base(userState)
        {
            _produtoService = produtoService;
            _movimentacaoService = movimentacaoService;
        }

        public override string Nome => "consultar_kardex_produto";
        public override string Descricao => "Consulta o extrato e histórico de movimentações (Kardex) de um produto por SKU ou ID.";

        [KernelFunction("consultar_kardex_produto")]
        [Description("Consulta o histórico de movimentações (Kardex) de um produto especificado por SKU ou ID.")]
        public async Task<string> ConsultarKardexAsync(string? sku = null, int? produtoId = null)
        {
            ProdutoEstoque? produto = null;
            if (produtoId.HasValue && produtoId.Value > 0)
            {
                produto = await _produtoService.GetById(produtoId.Value);
            }
            else if (!string.IsNullOrWhiteSpace(sku))
            {
                produto = await _produtoService.ObterPorSKUAsync(sku);
            }

            if (produto == null)
            {
                return JsonSerializer.Serialize(new { erro = "Produto não encontrado para o filtro informado." });
            }

            var movimentacoes = await _movimentacaoService.ObterKardexProdutoAsync(produto.Id);

            return JsonSerializer.Serialize(new
            {
                Produto = new
                {
                    produto.Id,
                    produto.CodigoSKU,
                    produto.Nome,
                    SaldoAtual = produto.QuantidadeEstoque,
                    produto.CustoMedio,
                    produto.UnidadeMedida
                },
                TotalMovimentacoes = movimentacoes.Count,
                UltimasMovimentacoes = movimentacoes.Take(10).Select(m => new
                {
                    m.Id,
                    Data = m.DataMovimentacao.ToString("dd/MM/yyyy HH:mm"),
                    Tipo = m.TipoMovimentacao.ToString(),
                    m.Quantidade,
                    m.CustoUnitario,
                    m.SaldoAnterior,
                    m.SaldoPosterior,
                    m.DocumentoReferencia,
                    m.Observacao
                })
            });
        }

        public override Task<string> ExecutarAsync(Dictionary<string, object> parametros)
        {
            string? sku = parametros != null && parametros.TryGetValue("sku", out var s) ? s?.ToString() : null;
            int? produtoId = null;
            if (parametros != null && parametros.TryGetValue("produtoId", out var pid) && pid != null)
            {
                if (int.TryParse(pid.ToString(), out int p)) produtoId = p;
            }

            return ConsultarKardexAsync(sku, produtoId);
        }
    }
}
