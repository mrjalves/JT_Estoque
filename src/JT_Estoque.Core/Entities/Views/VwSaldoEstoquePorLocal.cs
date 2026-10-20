using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;

namespace JT_Estoque.Core.Entities.Views
{
    [NomeTabela("VwSaldoEstoquePorLocal")]
    [Modulo(eModuloSistema.Gestao)]
    [MapearComoView(@"
CREATE OR ALTER VIEW VwSaldoEstoquePorLocal AS
SELECT 
    CAST(ROW_NUMBER() OVER(ORDER BY l.Id, p.Id) AS INT) AS Id,
    l.Id AS LocalId,
    l.Nome AS NomeLocal,
    p.Id AS ProdutoId,
    p.Nome AS NomeProduto,
    p.CodigoSKU,
    p.UnidadeMedida,
    ISNULL(SUM(CASE 
        WHEN m.LocalDestinoId = l.Id THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.LocalDestinoId IS NOT NULL THEN -m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (1, 5, 6, 9) THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (2, 3, 4, 7, 8, 10) THEN -m.Quantidade
        ELSE 0 END), 0) AS SaldoLocal,
    ISNULL(p.CustoMedio, 0) AS CustoMedio,
    ISNULL(p.CustoMedio, 0) * ISNULL(SUM(CASE 
        WHEN m.LocalDestinoId = l.Id THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.LocalDestinoId IS NOT NULL THEN -m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (1, 5, 6, 9) THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (2, 3, 4, 7, 8, 10) THEN -m.Quantidade
        ELSE 0 END), 0) AS ValorTotalEstoque
FROM LocaisEstoque l
CROSS JOIN Produtos p
LEFT JOIN EstoqueMovimentacoes m ON (m.LocalOrigemId = l.Id OR m.LocalDestinoId = l.Id) AND m.ProdutoId = p.Id
GROUP BY l.Id, l.Nome, p.Id, p.Nome, p.CodigoSKU, p.UnidadeMedida, p.CustoMedio;")]
    public class VwSaldoEstoquePorLocal : ClasseBase
    {
        [NomeColuna("LocalId")]
        public int LocalId { get; set; }

        [NomeColuna("NomeLocal")]
        public string NomeLocal { get; set; } = string.Empty;

        [NomeColuna("ProdutoId")]
        public int ProdutoId { get; set; }

        [NomeColuna("NomeProduto")]
        public string NomeProduto { get; set; } = string.Empty;

        [NomeColuna("CodigoSKU")]
        public string CodigoSKU { get; set; } = string.Empty;

        [NomeColuna("UnidadeMedida")]
        public string UnidadeMedida { get; set; } = string.Empty;

        [NomeColuna("SaldoLocal")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal SaldoLocal { get; set; }

        [NomeColuna("CustoMedio")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal CustoMedio { get; set; }

        [NomeColuna("ValorTotalEstoque")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal ValorTotalEstoque { get; set; }

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Local", ColumnName = "NomeLocal", Type = "string" },
                new Filtro { Label = "Produto", ColumnName = "NomeProduto", Type = "string" },
                new Filtro { Label = "SKU", ColumnName = "CodigoSKU", Type = "string" }
            ];
        }

        public VwSaldoEstoquePorLocal() : base() { }
        public VwSaldoEstoquePorLocal(int id) : base(id, typeof(VwSaldoEstoquePorLocal)) { }
    }
}
