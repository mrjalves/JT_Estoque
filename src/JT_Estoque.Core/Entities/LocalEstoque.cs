using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;

namespace JT_Estoque.Core.Entities
{
    [NomeTabela("LocaisEstoque")]
    [Modulo(eModuloSistema.Gestao)]
    public class LocalEstoque : ClasseBase
    {
        [NomeColuna("Codigo")]
        [TipoColuna("VARCHAR(30)")]
        public string Codigo { get; set; } = string.Empty;

        [NomeColuna("Nome")]
        [TipoColuna("VARCHAR(120)")]
        public string Nome { get; set; } = string.Empty;

        [NomeColuna("Descricao")]
        [TipoColuna("VARCHAR(255)")]
        public string Descricao { get; set; } = string.Empty;

        [ChaveEstrangeira("Pessoas")]
        [NomeColuna("ResponsavelId")]
        public int? ResponsavelId { get; set; }

        [NomeColuna("PermiteSaldoNegativo")]
        public bool PermiteSaldoNegativo { get; set; } = false;

        [NomeColuna("Ativo")]
        public bool Ativo { get; set; } = true;

        public override List<Filtro> CarregarFiltros()
        {
            return
            [
                new Filtro { Label = "Nome", ColumnName = "Nome", Type = "string" },
                new Filtro { Label = "Código", ColumnName = "Codigo", Type = "string" },
                new Filtro { Label = "Ativo", ColumnName = "Ativo", Type = "boolean", Value = true }
            ];
        }

        public LocalEstoque() : base() { }
        public LocalEstoque(int id) : base(id, typeof(LocalEstoque)) { }
    }
}
