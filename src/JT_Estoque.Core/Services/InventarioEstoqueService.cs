using JT_Aplicacao.Classes;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Enums;
using JT_Servicos;

namespace JT_Estoque.Core.Services
{
    public class InventarioEstoqueService : JTServiceBase<InventarioEstoque>
    {
        private readonly ProdutoEstoqueService _produtoService;
        private readonly MovimentacaoEstoqueService _movimentacaoService;

        public InventarioEstoqueService(
            ProdutoEstoqueService produtoService,
            MovimentacaoEstoqueService movimentacaoService,
            HttpClient? httpClient = null) : base(httpClient)
        {
            _produtoService = produtoService;
            _movimentacaoService = movimentacaoService;
        }

        protected override string GetApiRoute()
        {
            return "api/estoque/inventarios";
        }

        public async Task<int> AbrirSessaoInventarioAsync(int localEstoqueId, string codigo, string observacao)
        {
            var inventario = new InventarioEstoque
            {
                Codigo = codigo,
                LocalEstoqueId = localEstoqueId,
                DataAbertura = DateTime.Now,
                Status = eStatusInventario.Aberto,
                Observacoes = observacao
            };

            int invId = await SaveAsync(inventario);

            // Popula os itens com o saldo atual do sistema para os produtos cadastrados
            var produtos = await _produtoService.GetAll();
            foreach (var p in produtos.Where(p => p.Ativo))
            {
                var item = new InventarioItem
                {
                    InventarioId = invId,
                    ProdutoId = p.Id,
                    QuantidadeSistema = p.QuantidadeEstoque,
                    QuantidadeContada = p.QuantidadeEstoque,
                    Divergencia = 0
                };
                BaseRepository.Add(item);
            }

            return invId;
        }

        public async Task<List<InventarioItem>> ObterItensInventarioAsync(int inventarioId)
        {
            return BaseRepository.GetAll<InventarioItem>()
                .Where(i => i.InventarioId == inventarioId)
                .ToList();
        }

        public async Task AtualizarItemInventarioAsync(InventarioItem item)
        {
            item.Divergencia = item.QuantidadeContada - item.QuantidadeSistema;
            BaseRepository.Update(item);
        }

        public async Task ProcessarInventarioAsync(int inventarioId)
        {
            var inventario = await GetById(inventarioId)
                ?? throw new InvalidOperationException($"Inventário ID {inventarioId} não encontrado.");

            if (inventario.Status == eStatusInventario.Processado)
                throw new InvalidOperationException("Este inventário já foi processado.");

            var itens = await ObterItensInventarioAsync(inventarioId);

            foreach (var item in itens)
            {
                decimal divergencia = item.QuantidadeContada - item.QuantidadeSistema;
                if (divergencia == 0) continue;

                var produto = await _produtoService.GetById(item.ProdutoId);
                decimal custo = produto?.CustoMedio ?? 0m;

                var mov = new MovimentacaoEstoque
                {
                    ProdutoId = item.ProdutoId,
                    LocalOrigemId = inventario.LocalEstoqueId,
                    LoteId = item.LoteId,
                    TipoMovimentacao = divergencia > 0
                        ? eTipoMovimentacaoEstoque.AjusteInventarioPositivo
                        : eTipoMovimentacaoEstoque.AjusteInventarioNegativo,
                    Quantidade = Math.Abs(divergencia),
                    CustoUnitario = custo,
                    DocumentoReferencia = $"INV-{inventario.Codigo}",
                    DataMovimentacao = DateTime.Now,
                    Observacao = $"Ajuste automático de inventário físico ({inventario.Codigo}). Divergência: {divergencia:+0.00;-0.00}"
                };

                await _movimentacaoService.RegistrarMovimentacaoKardexAsync(mov);
            }

            inventario.Status = eStatusInventario.Processado;
            inventario.DataFechamento = DateTime.Now;
            await Save(inventario);
        }
    }
}
