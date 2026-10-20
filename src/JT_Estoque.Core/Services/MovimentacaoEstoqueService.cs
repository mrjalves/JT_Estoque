using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Enums;
using JT_Servicos;
using JT_Servicos.Interfaces;

namespace JT_Estoque.Core.Services
{
    public class MovimentacaoEstoqueService : JTServiceBase<MovimentacaoEstoque>
    {
        private readonly ProdutoEstoqueService _produtoService;
        private readonly LoteEstoqueService _loteService;
        private readonly IMGestaoConfiguracaoService _gestaoConfigService;

        public MovimentacaoEstoqueService(
            ProdutoEstoqueService produtoService,
            LoteEstoqueService loteService,
            IMGestaoConfiguracaoService gestaoConfigService,
            HttpClient? httpClient = null) : base(httpClient)
        {
            _produtoService = produtoService;
            _loteService = loteService;
            _gestaoConfigService = gestaoConfigService;
        }

        protected override string GetApiRoute()
        {
            return "api/estoque/movimentacoes";
        }

        public async Task<int> RegistrarMovimentacaoKardexAsync(MovimentacaoEstoque mov)
        {
            var config = await _gestaoConfigService.GetAtivaAsync();
            var produto = await _produtoService.GetById(mov.ProdutoId)
                ?? throw new InvalidOperationException($"Produto ID {mov.ProdutoId} não encontrado.");

            bool isEntrada = mov.TipoMovimentacao is eTipoMovimentacaoEstoque.EntradaCompra
                or eTipoMovimentacaoEstoque.TransferenciaEntrada
                or eTipoMovimentacaoEstoque.AjusteInventarioPositivo
                or eTipoMovimentacaoEstoque.DevolucaoCliente;

            decimal saldoAtual = produto.QuantidadeEstoque;
            decimal fator = isEntrada ? 1m : -1m;
            decimal delta = mov.Quantidade * fator;
            decimal novoSaldo = saldoAtual + delta;

            // Respeita a regra de negócio/configuração do sistema para estoque negativo
            if (!isEntrada && !(config?.PermitirVendaEstoqueNegativo ?? false) && novoSaldo < 0)
            {
                throw new InvalidOperationException(
                    $"Saldo insuficiente para o produto '{produto.Nome}'. Saldo atual: {saldoAtual}, Quantidade solicitada: {mov.Quantidade}.");
            }

            // Atualiza Custo Médio Ponderado Móvel (CMP) nas compras
            if (mov.TipoMovimentacao == eTipoMovimentacaoEstoque.EntradaCompra && mov.CustoUnitario > 0)
            {
                if (saldoAtual > 0 && (saldoAtual + mov.Quantidade) > 0)
                {
                    produto.CustoMedio = ((saldoAtual * produto.CustoMedio) + (mov.Quantidade * mov.CustoUnitario))
                                         / (saldoAtual + mov.Quantidade);
                }
                else
                {
                    produto.CustoMedio = mov.CustoUnitario;
                }
                produto.PrecoCusto = mov.CustoUnitario;
            }

            // Atualiza saldo do lote se especificado
            if (mov.LoteId.HasValue)
            {
                var lote = await _loteService.GetById(mov.LoteId.Value);
                if (lote != null)
                {
                    lote.QuantidadeSaldo += delta;
                    if (lote.QuantidadeSaldo <= 0)
                    {
                        lote.QuantidadeSaldo = 0;
                        lote.StatusLote = eStatusLote.Esgotado;
                    }
                    await _loteService.Save(lote);
                }
            }

            mov.SaldoAnterior = saldoAtual;
            mov.SaldoPosterior = novoSaldo;
            if (mov.DataMovimentacao == default)
            {
                mov.DataMovimentacao = DateTime.Now;
            }

            int movId = await SaveAsync(mov);

            produto.QuantidadeEstoque = novoSaldo;
            await _produtoService.Save(produto);

            return movId;
        }

        public async Task<List<MovimentacaoEstoque>> ObterKardexProdutoAsync(int produtoId, DateTime? inicio = null, DateTime? fim = null)
        {
            var todas = await GetAll();
            var query = todas.Where(m => m.ProdutoId == produtoId);

            if (inicio.HasValue)
                query = query.Where(m => m.DataMovimentacao >= inicio.Value);

            if (fim.HasValue)
                query = query.Where(m => m.DataMovimentacao <= fim.Value);

            return query.OrderByDescending(m => m.DataMovimentacao).ToList();
        }
    }
}
