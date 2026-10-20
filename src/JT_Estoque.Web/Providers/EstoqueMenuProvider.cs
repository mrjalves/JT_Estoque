using JT_Aplicacao.Classes;
using JT_Aplicacao.UserInterface;
using JT_UserInterface.Shared.Services.Interfaces.Comum;

namespace JT_Estoque.Web.Providers
{
    public class EstoqueMenuProvider : IModuloMenuProvider, IMenuProvider
    {
        public List<SModuloMenu> GetModulos() =>
        [
            new SModuloMenu
            {
                NomeModulo = "Estoque & Logística",
                Icone = "bi-boxes",
                Modulo = (eModuloSistema)14,
                Itens =
                [
                    new SItemMenu { Url = "/estoque/dashboard", Texto = "Dashboard", Icone = "bi-speedometer2", Titulo = "Visão Geral do Estoque" },
                    new SItemMenu { Url = "/estoque/produtos", Texto = "Produtos & SKUs", Icone = "bi-box-seam", Titulo = "Catálogo de Produtos" },
                    new SItemMenu { Url = "/estoque/movimentacoes", Texto = "Kardex (Movimentações)", Icone = "bi-arrow-left-right", Titulo = "Entradas, Saídas e Transferências" },
                    new SItemMenu { Url = "/estoque/lotes", Texto = "Lotes e Validades", Icone = "bi-calendar2-check", Titulo = "Controle FEFO / Lotes" },
                    new SItemMenu { Url = "/estoque/locais", Texto = "Almoxarifados", Icone = "bi-building", Titulo = "Locais de Armazenamento" },
                    new SItemMenu { Url = "/estoque/inventarios", Texto = "Inventário Físico", Icone = "bi-clipboard-check", Titulo = "Auditoria e Contagem" },
                    new SItemMenu { Url = "/estoque/fornecedores", Texto = "Fornecedores", Icone = "bi-truck", Titulo = "Gestão de Fornecedores" }
                ]
            }
        ];

        public List<SItemMenu> GetMenu() => GetModulos().SelectMany(m => m.Itens).ToList();
    }
}
