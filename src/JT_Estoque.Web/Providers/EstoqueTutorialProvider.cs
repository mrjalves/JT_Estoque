using JT_UserInterface.Shared.Models;
using JT_UserInterface.Shared.Services.Interfaces.Comum;

namespace JT_Estoque.Web.Providers
{
    /// <summary>
    /// Provedor de tutoriais interativos guiados para as páginas do JT_Estoque.
    /// Registrado via <see cref="ITutorialProvider"/> e gerenciado pelo TutorialService / TutorialHost.
    /// </summary>
    public class EstoqueTutorialProvider : ITutorialProvider
    {
        public List<TutorialDefinicao> ObterTutoriais() => new()
        {
            // ──────────────────────────────────────────────
            // 1. Dashboard de Estoque & Logística
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-dashboard",
                Rotas = new List<string> { "/estoque/dashboard", "/", "/home" },
                Titulo = "Dashboard de Estoque & Logística",
                Descricao = "Visão panorâmica com indicadores em tempo real, rupturas, validade de lotes e giro do estoque.",
                Icone = "bi-speedometer2",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Visão Geral e Ações Rápidas",
                        Descricao = "Acesse atalhos rápidos para cadastrar produtos imediatamente ou registrar movimentações manuais no Kardex.",
                        Seletor = "[data-tutorial='dashboard-header']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Valor Total Imobilizado",
                        Descricao = "Consolidação financeira do custo dos produtos estocados em todos os seus almoxarifados.",
                        Seletor = "[data-tutorial='kpi-valor-total']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Alertas de Ruptura de Estoque",
                        Descricao = "Indica produtos cujo saldo físico atual está igual ou abaixo do estoque mínimo de segurança definido.",
                        Seletor = "[data-tutorial='kpi-rupturas']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Controle FEFO & Lotes a Vencer",
                        Descricao = "Monitore mercadorias perecíveis que expiram nos próximos 30 dias para evitar perdas financeiras.",
                        Seletor = "[data-tutorial='kpi-lotes-vencer']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Volume Total em Saldo",
                        Descricao = "Quantidade total de unidades físicas armazenadas e o total de almoxarifados em operação.",
                        Seletor = "[data-tutorial='kpi-volume-saldo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Curva ABC - Maiores Ativos",
                        Descricao = "Gráfico interativo destacando os SKUs de maior relevância patrimonial na sua operação.",
                        Seletor = "[data-tutorial='curva-abc-chart']",
                        Posicao = "top"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Distribuição de Movimentações",
                        Descricao = "Divisão proporcional das operações de entrada, saída, transferência e ajustes realizadas no estoque.",
                        Seletor = "[data-tutorial='movimentacoes-chart']",
                        Posicao = "top"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Alertas de Reposição Imediata",
                        Descricao = "Tabela com os itens mais críticos que demandam nova compra, com atalho direto para dar entrada no Kardex.",
                        Seletor = "[data-tutorial='tabela-criticos']",
                        Posicao = "top"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Últimos Lançamentos no Kardex",
                        Descricao = "Histórico recente com as últimas operações físicas realizadas e suas quantidades.",
                        Seletor = "[data-tutorial='tabela-kardex-recente']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 2. Catálogo de Produtos & SKUs
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-produtos",
                Rotas = new List<string> { "/estoque/produtos" },
                Titulo = "Catálogo de Produtos & SKUs",
                Descricao = "Gestão de itens estocáveis, fichas técnicas de SKU, ponto de pedido e custos médios.",
                Icone = "bi-box-seam",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Cadastrar Novo Produto",
                        Descricao = "Clique aqui para cadastrar um novo SKU com unidade de medida, código de barras EAN, estoque mínimo e preços.",
                        Seletor = "[data-tutorial='produtos-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Filtros de Saúde do Estoque",
                        Descricao = "Filtre com agilidade entre todos os itens, apenas produtos Críticos (abaixo do mínimo) ou produtos com controle de Lote.",
                        Seletor = "[data-tutorial='produtos-filtros-saude']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Busca Inteligente",
                        Descricao = "Digite o código SKU, código de barras ou o nome do produto para filtrar instantaneamente a listagem.",
                        Seletor = "[data-tutorial='produtos-busca']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Tabela de Saldos e Indicadores",
                        Descricao = "Acompanhe saldo físico atual, indicador visual de saúde (Normal, Ponto de Pedido, Crítico), custo médio e valor imobilizado.",
                        Seletor = "[data-tutorial='produtos-tabela']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 3. Livro-Razão Kardex (Movimentações)
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-movimentacoes",
                Rotas = new List<string> { "/estoque/movimentacoes" },
                Titulo = "Livro-Razão Kardex",
                Descricao = "Extrato oficial de movimentações com rastreamento completo de entradas, saídas, transferências e custos.",
                Icone = "bi-arrow-left-right",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Registrar Operação no Kardex",
                        Descricao = "Lance compras, devoluções, requisições de consumo, perdas ou transferências entre almoxarifados.",
                        Seletor = "[data-tutorial='movimentacoes-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Filtro por Produto",
                        Descricao = "Isole o extrato histórico de um SKU específico para conferência detalhada de saldos.",
                        Seletor = "[data-tutorial='movimentacoes-filtro-produto']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Filtro por Almoxarifado",
                        Descricao = "Consulte lançamentos ocorridos em um almoxarifado ou centro de distribuição específico.",
                        Seletor = "[data-tutorial='movimentacoes-filtro-local']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Extrato e Rastreabilidade do Kardex",
                        Descricao = "Audite a data/hora, tipo de operação, evolução do saldo (anterior vs posterior) e documentos de origem.",
                        Seletor = "[data-tutorial='movimentacoes-tabela']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 4. Controle de Lotes & Validades (FEFO)
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-lotes",
                Rotas = new List<string> { "/estoque/lotes" },
                Titulo = "Controle de Lotes & Validades (FEFO)",
                Descricao = "Rastreabilidade de lotes, datas de fabricação, controle de perecíveis e quarentena.",
                Icone = "bi-calendar2-check",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Novo Lote",
                        Descricao = "Cadastre um novo lote vinculando ao produto correspondente, data de fabricação, validade e almoxarifado.",
                        Seletor = "[data-tutorial='lotes-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Filtros FEFO & Alertas",
                        Descricao = "Isole rapidamente lotes A Vencer nos próximos 30 dias, lotes já Vencidos ou itens retidos em Quarentena.",
                        Seletor = "[data-tutorial='lotes-filtros-status']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Busca de Lote",
                        Descricao = "Localize um lote pelo código para procedimentos de rastreabilidade ou recall.",
                        Seletor = "[data-tutorial='lotes-busca']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Gestão de Saldos & Vencimentos",
                        Descricao = "Visualize a contagem regressiva de dias para expirar, saldo remanescente e altere o status para quarentena.",
                        Seletor = "[data-tutorial='lotes-tabela']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 5. Almoxarifados e Locais de Armazenamento
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-locais",
                Rotas = new List<string> { "/estoque/locais" },
                Titulo = "Almoxarifados & Locais de Armazenamento",
                Descricao = "Cadastro de almoxarifados, centros de distribuição e políticas de saldo negativo.",
                Icone = "bi-building",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Cadastrar Almoxarifado",
                        Descricao = "Adicione novos galpões, depósitos centrais, estoques avançados ou estoques de frente de loja.",
                        Seletor = "[data-tutorial='locais-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Busca de Almoxarifados",
                        Descricao = "Pesquise por código interno ou nome do almoxarifado cadastrado.",
                        Seletor = "[data-tutorial='locais-busca']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Lista de Almoxarifados e Regras",
                        Descricao = "Consulte os locais ativos e verifique as regras de bloqueio contra saldo negativo por depósito.",
                        Seletor = "[data-tutorial='locais-tabela']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 6. Inventário Físico & Auditoria
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-inventarios",
                Rotas = new List<string> { "/estoque/inventarios" },
                Titulo = "Inventário Físico & Auditoria",
                Descricao = "Abertura de contagem cega, apuração de divergências e homologação com ajustes automáticos.",
                Icone = "bi-clipboard-check",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Nova Sessão de Inventário",
                        Descricao = "Inicie uma sessão de auditoria para um almoxarifado, registrando o congelamento dos saldos do sistema.",
                        Seletor = "[data-tutorial='inventarios-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Sessões e Status da Auditoria",
                        Descricao = "Acompanhe as sessões abertas, em contagem, em apuração de divergências ou já processadas.",
                        Seletor = "[data-tutorial='inventarios-tabela']",
                        Posicao = "top"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Contagem Física e Homologação",
                        Descricao = "Abra a planilha de contagem para preencher as quantidades apuradas fisicamente e homologue os ajustes no Kardex.",
                        Seletor = "[data-tutorial='inventarios-acoes']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 7. Gestão de Fornecedores
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-fornecedores",
                Rotas = new List<string> { "/estoque/fornecedores" },
                Titulo = "Gestão de Fornecedores",
                Descricao = "Cadastro centralizado de fornecedores, fabricantes e distribuidores homologados.",
                Icone = "bi-truck",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Novo Fornecedor",
                        Descricao = "Cadastre novos fornecedores informando razão social, e-mail de pedidos e telefone de contato.",
                        Seletor = "[data-tutorial='fornecedores-novo']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Busca de Fornecedores",
                        Descricao = "Encontre fornecedores digitando nome, razão social ou e-mail de contato.",
                        Seletor = "[data-tutorial='fornecedores-busca']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Listagem e Contato Ágil",
                        Descricao = "Acesse links rápidos para envio de e-mails, telefone e edição dos dados cadastrais.",
                        Seletor = "[data-tutorial='fornecedores-tabela']",
                        Posicao = "top"
                    }
                }
            },

            // ──────────────────────────────────────────────
            // 8. Configurações do Sistema
            // ──────────────────────────────────────────────
            new TutorialDefinicao
            {
                Chave = "estoque-configuracoes",
                Rotas = new List<string> { "/configuracoes" },
                Titulo = "Configurações Gerais & Parâmetros",
                Descricao = "Personalize regras de estoque, integrações de IA, notificações e preferências operacionais.",
                Icone = "bi-gear-fill",
                MostrarUmaVez = true,
                Passos = new List<TutorialPasso>
                {
                    new TutorialPasso
                    {
                        Titulo = "Painel de Configurações",
                        Descricao = "Central de controle para ajustar regras de estoque, integração de IA, parâmetros e preferências operacionais.",
                        Seletor = "[data-tutorial='configuracoes-pagina']",
                        Posicao = "top"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Abas Modulares",
                        Descricao = "Navegue entre as abas para gerenciar parâmetros gerais, inteligência artificial, regras operacionais e atalhos.",
                        Seletor = ".nav-tabs, .nav-pills, [role='tablist']",
                        Posicao = "bottom"
                    },
                    new TutorialPasso
                    {
                        Titulo = "Salvar Alterações",
                        Descricao = "Após ajustar as opções desejadas, clique aqui para gravar todas as configurações com segurança.",
                        Seletor = "button[type='submit']",
                        Posicao = "top"
                    }
                }
            }
        };
    }
}
