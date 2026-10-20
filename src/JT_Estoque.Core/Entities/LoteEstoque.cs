using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Estoque.Core.Enums;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("EstoqueLotes")]
    [Modulo(eModuloSistema.Gestao)]
    public class LoteEstoque : ClasseBase
    {
        [ChaveEstrangeira("Produtos")]
        [NomeColuna("ProdutoId")]
        public int ProdutoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "ProdutoId")]
        public ProdutoEstoque? Produto { get; set; }

        [ChaveEstrangeira("LocaisEstoque")]
        [NomeColuna("LocalEstoqueId")]
        public int LocalEstoqueId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LocalEstoqueId")]
        public LocalEstoque? LocalEstoque { get; set; }

        [NomeColuna("CodigoLote")]
        [TipoColuna("VARCHAR(80)")]
        public string CodigoLote { get; set; } = string.Empty;

        [NomeColuna("DataFabricacao")]
        public DateTime? DataFabricacao { get; set; }

        [NomeColuna("DataValidade")]
        public DateTime? DataValidade { get; set; }

        [NomeColuna("QuantidadeInicial")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal QuantidadeInicial { get; set; }

        [NomeColuna("QuantidadeSaldo")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal QuantidadeSaldo { get; set; }

        [NomeColuna("CustoUnitario")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal CustoUnitario { get; set; }

        [NomeColuna("StatusLote")]
        public eStatusLote StatusLote { get; set; } = eStatusLote.Disponivel;

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Código do Lote", ColumnName = "CodigoLote", Type = "string" },
                new Filtro { Label = "Produto", ColumnName = "ProdutoId", Type = "int" }
            ];
        }

        public LoteEstoque() : base() { }
        public LoteEstoque(int id) : base(id, typeof(LoteEstoque)) { }
    }
}
