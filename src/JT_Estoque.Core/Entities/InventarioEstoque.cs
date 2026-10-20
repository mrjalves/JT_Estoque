using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Estoque.Core.Enums;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("EstoqueInventarios")]
    [Modulo(eModuloSistema.Gestao)]
    public class InventarioEstoque : ClasseBase
    {
        [NomeColuna("Codigo")]
        [TipoColuna("VARCHAR(40)")]
        public string Codigo { get; set; } = string.Empty;

        [ChaveEstrangeira("LocaisEstoque")]
        [NomeColuna("LocalEstoqueId")]
        public int LocalEstoqueId { get; set; }

        [Relacionamento(TipoRelacionamento.Agregacao, "LocalEstoqueId")]
        public LocalEstoque? LocalEstoque { get; set; }

        [NomeColuna("DataAbertura")]
        public DateTime DataAbertura { get; set; } = DateTime.Now;

        [NomeColuna("DataFechamento")]
        public DateTime? DataFechamento { get; set; }

        [NomeColuna("Status")]
        public new eStatusInventario Status { get; set; } = eStatusInventario.Aberto;

        [NomeColuna("Observacoes")]
        [TipoColuna("VARCHAR(500)")]
        public string Observacoes { get; set; } = string.Empty;

        [Relacionamento(TipoRelacionamento.Composicao, "Id")]
        public List<InventarioItem> Itens { get; set; } = [];

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Código", ColumnName = "Codigo", Type = "string" },
                new Filtro { Label = "Local", ColumnName = "LocalEstoqueId", Type = "int" }
            ];
        }

        public InventarioEstoque() : base() { }
        public InventarioEstoque(int id) : base(id, typeof(InventarioEstoque)) { }
    }
}
