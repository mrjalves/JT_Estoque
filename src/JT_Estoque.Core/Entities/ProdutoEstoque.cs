using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Classes.Comum;
using JT_Classes.CRM;
using JT_Classes.Produto;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("Produtos")]
    [Modulo(eModuloSistema.Gestao)]
    [GerenciaCategoria("CategoriaProduto", "Categorias de Produtos (Estoque)")]
    public class ProdutoEstoque : Produto
    {
        [NomeColuna("CodigoSKU")]
        [TipoColuna("VARCHAR(60)")]
        public string CodigoSKU { get; set; } = string.Empty;

        [NomeColuna("CodigoBarras")]
        [TipoColuna("VARCHAR(60)")]
        public string CodigoBarras { get; set; } = string.Empty;

        [NomeColuna("UnidadeMedida")]
        [TipoColuna("VARCHAR(10)")]
        public string UnidadeMedida { get; set; } = "UN";

        [ChaveEstrangeira("Categorias")]
        [NomeColuna("IdCategoria")]
        public int? IdCategoria { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "IdCategoria")]
        public Categoria? Categoria { get; set; }

        [ChaveEstrangeira("Fornecedores")]
        [NomeColuna("FornecedorPadraoId")]
        public int? FornecedorPadraoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "FornecedorPadraoId")]
        public Fornecedor? FornecedorPadrao { get; set; }

        [ChaveEstrangeira("LocaisEstoque")]
        [NomeColuna("LocalPadraoId")]
        public int? LocalPadraoId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LocalPadraoId")]
        public LocalEstoque? LocalPadrao { get; set; }

        [NomeColuna("PrecoCusto")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal PrecoCusto { get; set; }

        [NomeColuna("CustoMedio")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal CustoMedio { get; set; }

        [NomeColuna("EstoqueMinimo")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal EstoqueMinimo { get; set; }

        [NomeColuna("EstoqueMaximo")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal EstoqueMaximo { get; set; }

        [NomeColuna("PontoPedido")]
        [TipoColuna("DECIMAL(18,4)")]
        public decimal PontoPedido { get; set; }

        [NomeColuna("ControlaLote")]
        public bool ControlaLote { get; set; } = false;

        [NomeColuna("ControlaValidade")]
        public bool ControlaValidade { get; set; } = false;

        [NomeColuna("LocalizacaoCorredor")]
        [TipoColuna("VARCHAR(50)")]
        public string LocalizacaoCorredor { get; set; } = string.Empty;

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Nome do Produto", ColumnName = "Nome", Type = "string" },
                new Filtro { Label = "SKU", ColumnName = "CodigoSKU", Type = "string" },
                new Filtro { Label = "Código de Barras", ColumnName = "CodigoBarras", Type = "string" },
                new Filtro { Label = "Ativo", ColumnName = "Ativo", Type = "boolean", Value = true }
            ];
        }

        public ProdutoEstoque() : base() { }
        public ProdutoEstoque(int id) : base(id) { }
    }
}
