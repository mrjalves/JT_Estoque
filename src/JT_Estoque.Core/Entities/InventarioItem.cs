using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("EstoqueInventarioItens")]
    [Modulo(eModuloSistema.Gestao)]
    public class InventarioItem : ClasseBase
    {
        [ChaveEstrangeira("EstoqueInventarios")]
        [NomeColuna("InventarioId")]
        public int InventarioId { get; set; }

        [ChaveEstrangeira("Produtos")]
        [NomeColuna("ProdutoId")]
        public int ProdutoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "ProdutoId")]
        public ProdutoEstoque? Produto { get; set; }

        [ChaveEstrangeira("EstoqueLotes")]
        [NomeColuna("LoteId")]
        public int? LoteId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LoteId")]
        public LoteEstoque? Lote { get; set; }

        [NomeColuna("QuantidadeSistema")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal QuantidadeSistema { get; set; }

        [NomeColuna("QuantidadeContada")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal QuantidadeContada { get; set; }

        [NomeColuna("Divergencia")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal Divergencia { get; set; }

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Inventário", ColumnName = "InventarioId", Type = "int" },
                new Filtro { Label = "Produto", ColumnName = "ProdutoId", Type = "int" }
            ];
        }

        public InventarioItem() : base() { }
        public InventarioItem(int id) : base(id, typeof(InventarioItem)) { }
    }
}
