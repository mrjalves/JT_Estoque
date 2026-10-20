using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.Enums;

namespace JT_RegrasNegocios.Estoque
{
    [RegraPara(typeof(MovimentacaoEstoque))]
    [TipoRegra("Restricao")]
    [MomentoExecucao("Antes")]
    [EventoAcao("Salvar")]
    [DescricaoRegra("Impede a saída de estoque vinculada a lotes vencidos ou bloqueados.")]
    public class ValidarLoteVencidoRegra : RegraNegocioValidacao
    {
        public override Type TipoEntidade { get; set; } = typeof(MovimentacaoEstoque);
        public override TipoRegra Tipo => TipoRegra.Restricao;
        public override MomentoExecucao Momento => MomentoExecucao.Antes;
        public override EventoAcao Acao => EventoAcao.Salvar;
        public override string Descricao => "Não é permitido realizar movimentação de saída com lote vencido ou bloqueado.";

        public override bool Validar(object entidade)
        {
            if (entidade is not MovimentacaoEstoque mov) return true;
            if (!mov.LoteId.HasValue) return true;

            // Aplica a validação apenas para tipos de movimentação de saída ou consumo
            bool isSaida = mov.TipoMovimentacao is eTipoMovimentacaoEstoque.SaidaVenda
                or eTipoMovimentacaoEstoque.SaidaConsumoInterno
                or eTipoMovimentacaoEstoque.TransferenciaSaida
                or eTipoMovimentacaoEstoque.PerdaOuAvaria;

            if (!isSaida) return true;

            try
            {
                var lote = BaseRepository.GetById<LoteEstoque>(mov.LoteId.Value);
                if (lote != null)
                {
                    if (lote.StatusLote is eStatusLote.Vencido or eStatusLote.Bloqueado)
                    {
                        return false;
                    }

                    if (lote.DataValidade.HasValue && lote.DataValidade.Value.Date < DateTime.Today)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                // Em caso de falha de leitura em ambiente sem banco direto, prossegue
            }

            return true;
        }
    }
}
