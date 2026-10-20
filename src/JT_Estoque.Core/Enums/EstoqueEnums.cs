namespace JT_Estoque.Core.Enums
{
    public enum eTipoMovimentacaoEstoque
    {
        EntradaCompra = 1,
        SaidaVenda = 2,
        SaidaConsumoInterno = 3,
        TransferenciaSaida = 4,
        TransferenciaEntrada = 5,
        AjusteInventarioPositivo = 6,
        AjusteInventarioNegativo = 7,
        PerdaOuAvaria = 8,
        DevolucaoCliente = 9,
        DevolucaoFornecedor = 10
    }

    public enum eStatusLote
    {
        Disponivel = 1,
        Quarentena = 2,
        Bloqueado = 3,
        Vencido = 4,
        Esgotado = 5
    }

    public enum eStatusInventario
    {
        Aberto = 1,
        EmContagem = 2,
        Processado = 3,
        Cancelado = 4
    }
}
