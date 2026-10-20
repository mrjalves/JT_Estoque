using JT_Aplicacao.Atributos;
using JT_Aplicacao.Classes;
using JT_Estoque.Core.Entities;

namespace JT_RegrasNegocios.Estoque
{
    [RegraPara(typeof(MovimentacaoEstoque))]
    [TipoRegra("Restricao")]
    [MomentoExecucao("Antes")]
    [EventoAcao("Salvar")]
    [DescricaoRegra("Impede o registro de movimentações de estoque com quantidade zerada ou negativa.")]
    public class ValidarSaldoEstoqueRegra : RegraNegocioValidacao
    {
        public override Type TipoEntidade { get; set; } = typeof(MovimentacaoEstoque);
        public override TipoRegra Tipo => TipoRegra.Restricao;
        public override MomentoExecucao Momento => MomentoExecucao.Antes;
        public override EventoAcao Acao => EventoAcao.Salvar;
        public override string Descricao => "A quantidade movimentada no estoque deve ser maior que zero.";

        public override bool Validar(object entidade)
        {
            if (entidade is not MovimentacaoEstoque mov) return true;
            return mov.Quantidade > 0;
        }
    }
}
