using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Aplicacao.Classes.Interfaces;
using JT_Classes.CRM;
using JT_Estoque.Core.Enums;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("EstoqueMovimentacoes")]
    [Modulo(eModuloSistema.Gestao)]
    public class MovimentacaoEstoque : ClasseBase, IDataSegregada
    {
        [ChaveEstrangeira("Produtos")]
        [NomeColuna("ProdutoId")]
        public int ProdutoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "ProdutoId")]
        public ProdutoEstoque? Produto { get; set; }

        [ChaveEstrangeira("LocaisEstoque")]
        [NomeColuna("LocalOrigemId")]
        public int LocalOrigemId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LocalOrigemId")]
        public LocalEstoque? LocalOrigem { get; set; }

        [ChaveEstrangeira("LocaisEstoque")]
        [NomeColuna("LocalDestinoId")]
        public int? LocalDestinoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LocalDestinoId")]
        public LocalEstoque? LocalDestino { get; set; }

        [ChaveEstrangeira("EstoqueLotes")]
        [NomeColuna("LoteId")]
        public int? LoteId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LoteId")]
        public LoteEstoque? Lote { get; set; }

        [ChaveEstrangeira("Fornecedores")]
        [NomeColuna("FornecedorId")]
        public int? FornecedorId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "FornecedorId")]
        public Fornecedor? Fornecedor { get; set; }

        [NomeColuna("TipoMovimentacao")]
        public eTipoMovimentacaoEstoque TipoMovimentacao { get; set; }

        [NomeColuna("Quantidade")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal Quantidade { get; set; }

        [NomeColuna("CustoUnitario")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal CustoUnitario { get; set; }

        [NomeColuna("SaldoAnterior")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal SaldoAnterior { get; set; }

        [NomeColuna("SaldoPosterior")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal SaldoPosterior { get; set; }

        [NomeColuna("DocumentoReferencia")]
        [TipoColuna("VARCHAR(100)")]
        public string DocumentoReferencia { get; set; } = string.Empty;

        [NomeColuna("DataMovimentacao")]
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;

        [NomeColuna("Observacao")]
        [TipoColuna("VARCHAR(500)")]
        public string Observacao { get; set; } = string.Empty;

        [NomeColuna("UsuarioId")]
        public int UsuarioId { get; set; }

        [NomeColuna("TenantId")]
        [TipoColuna("VARCHAR(50)")]
        public string TenantId { get; set; } = string.Empty;

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Produto", ColumnName = "ProdutoId", Type = "int" },
                new Filtro { Label = "Almoxarifado Origem", ColumnName = "LocalOrigemId", Type = "int" },
                new Filtro { Label = "Documento", ColumnName = "DocumentoReferencia", Type = "string" }
            ];
        }

        public MovimentacaoEstoque() : base() { }
        public MovimentacaoEstoque(int id) : base(id, typeof(MovimentacaoEstoque)) { }
    }
}
