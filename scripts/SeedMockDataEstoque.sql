-- =========================================================================================
-- SCRIPT DE SEED / MOCK DATA — JT_ESTOQUE (GESTÃO DE ESTOQUE & LOGÍSTICA)
-- Descrição: Popula o banco de dados com dados de teste realistas e completos para o JT_Estoque.
-- Idempotente: Pode ser executado múltiplas vezes sem duplicar registros.
-- Compatibilidade: Microsoft SQL Server 2017+ / Azure SQL Database.
-- =========================================================================================

SET NOCOUNT ON;

PRINT '------------------------------------------------------------';
PRINT 'Iniciando Seed de Dados Mock para JT_Estoque...';
PRINT '------------------------------------------------------------';

-- =========================================================================================
-- 1. CRIAÇÃO OU ATUALIZAÇÃO DA VIEW VwSaldoEstoquePorLocal
-- =========================================================================================
PRINT '1. Garantindo a View VwSaldoEstoquePorLocal...';

EXEC('
CREATE OR ALTER VIEW VwSaldoEstoquePorLocal AS
SELECT 
    CAST(ROW_NUMBER() OVER(ORDER BY l.Id, p.Id) AS INT) AS Id,
    l.Id AS LocalId,
    l.Nome AS NomeLocal,
    p.Id AS ProdutoId,
    p.Nome AS NomeProduto,
    p.CodigoSKU,
    p.UnidadeMedida,
    ISNULL(SUM(CASE 
        WHEN m.LocalDestinoId = l.Id THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.LocalDestinoId IS NOT NULL THEN -m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (1, 5, 6, 9) THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (2, 3, 4, 7, 8, 10) THEN -m.Quantidade
        ELSE 0 END), 0) AS SaldoLocal,
    ISNULL(p.CustoMedio, 0) AS CustoMedio,
    ISNULL(p.CustoMedio, 0) * ISNULL(SUM(CASE 
        WHEN m.LocalDestinoId = l.Id THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.LocalDestinoId IS NOT NULL THEN -m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (1, 5, 6, 9) THEN m.Quantidade
        WHEN m.LocalOrigemId = l.Id AND m.TipoMovimentacao IN (2, 3, 4, 7, 8, 10) THEN -m.Quantidade
        ELSE 0 END), 0) AS ValorTotalEstoque
FROM LocaisEstoque l
CROSS JOIN Produtos p
LEFT JOIN EstoqueMovimentacoes m ON (m.LocalOrigemId = l.Id OR m.LocalDestinoId = l.Id) AND m.ProdutoId = p.Id
GROUP BY l.Id, l.Nome, p.Id, p.Nome, p.CodigoSKU, p.UnidadeMedida, p.CustoMedio;
');

PRINT '   View VwSaldoEstoquePorLocal atualizada.';

-- =========================================================================================
-- 2. ALMOXARIFADOS E LOCAIS DE ESTOQUE (LocaisEstoque)
-- =========================================================================================
PRINT '2. Inserindo Almoxarifados / Locais de Estoque...';

IF NOT EXISTS (SELECT 1 FROM LocaisEstoque WHERE Codigo = 'ALM-CENTRAL')
BEGIN
    INSERT INTO LocaisEstoque (Codigo, Nome, Descricao, PermiteSaldoNegativo, Ativo, UserId)
    VALUES ('ALM-CENTRAL', 'Almoxarifado Central (Matriz)', 'Depósito principal - Docas 01 a 04, Setor A/B', 0, 1, 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM LocaisEstoque WHERE Codigo = 'ALM-FILIAL')
BEGIN
    INSERT INTO LocaisEstoque (Codigo, Nome, Descricao, PermiteSaldoNegativo, Ativo, UserId)
    VALUES ('ALM-FILIAL', 'Depósito Filial Norte', 'Galpão de distribuição regional e suporte de pronta entrega', 0, 1, 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM LocaisEstoque WHERE Codigo = 'ALM-AVARIAS')
BEGIN
    INSERT INTO LocaisEstoque (Codigo, Nome, Descricao, PermiteSaldoNegativo, Ativo, UserId)
    VALUES ('ALM-AVARIAS', 'Área de Quarentena e Avarias', 'Itens com defeito, devoluções em triagem ou aguardando laudo técnico', 0, 1, 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM LocaisEstoque WHERE Codigo = 'ALM-EXPEDICAO')
BEGIN
    INSERT INTO LocaisEstoque (Codigo, Nome, Descricao, PermiteSaldoNegativo, Ativo, UserId)
    VALUES ('ALM-EXPEDICAO', 'Área de Expedição / Cross-Docking', 'Área de trânsito temporário para pedidos faturados', 1, 1, 'supervisor');
END

DECLARE @LocalCentralId INT = (SELECT TOP 1 Id FROM LocaisEstoque WHERE Codigo = 'ALM-CENTRAL');
DECLARE @LocalFilialId INT = (SELECT TOP 1 Id FROM LocaisEstoque WHERE Codigo = 'ALM-FILIAL');
DECLARE @LocalAvariasId INT = (SELECT TOP 1 Id FROM LocaisEstoque WHERE Codigo = 'ALM-AVARIAS');

PRINT '   Almoxarifados verificados/inseridos.';

-- =========================================================================================
-- 3. CATEGORIAS DE PRODUTOS (Categorias com TipoEntidade = 'ProdutoEstoque')
-- =========================================================================================
PRINT '3. Inserindo Categorias de Produtos...';

IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Hardware & Periféricos' AND TipoEntidade = 'ProdutoEstoque')
BEGIN
    INSERT INTO Categorias (Nome, Descricao, TipoEntidade, UserId)
    VALUES ('Hardware & Periféricos', 'Monitores, teclados, mouses, headsets e componentes de informática', 'ProdutoEstoque', 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Cabos & Conectividade' AND TipoEntidade = 'ProdutoEstoque')
BEGIN
    INSERT INTO Categorias (Nome, Descricao, TipoEntidade, UserId)
    VALUES ('Cabos & Conectividade', 'Cabos de rede UTP/STP, patch cords, cabos HDMI, fibra óptica e conectores', 'ProdutoEstoque', 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Equipamentos de Rede' AND TipoEntidade = 'ProdutoEstoque')
BEGIN
    INSERT INTO Categorias (Nome, Descricao, TipoEntidade, UserId)
    VALUES ('Equipamentos de Rede', 'Switches gerenciáveis, roteadores Wi-Fi mesh, access points e racks', 'ProdutoEstoque', 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Categorias WHERE Nome = 'Insumos & Suprimentos' AND TipoEntidade = 'ProdutoEstoque')
BEGIN
    INSERT INTO Categorias (Nome, Descricao, TipoEntidade, UserId)
    VALUES ('Insumos & Suprimentos', 'Pastas térmicas, abraçadeiras, pilhas, etiquetas e materiais auxiliares', 'ProdutoEstoque', 'supervisor');
END

DECLARE @CatPerifericosId INT = (SELECT TOP 1 Id FROM Categorias WHERE Nome = 'Hardware & Periféricos' AND TipoEntidade = 'ProdutoEstoque');
DECLARE @CatCabosId INT = (SELECT TOP 1 Id FROM Categorias WHERE Nome = 'Cabos & Conectividade' AND TipoEntidade = 'ProdutoEstoque');
DECLARE @CatRedesId INT = (SELECT TOP 1 Id FROM Categorias WHERE Nome = 'Equipamentos de Rede' AND TipoEntidade = 'ProdutoEstoque');
DECLARE @CatInsumosId INT = (SELECT TOP 1 Id FROM Categorias WHERE Nome = 'Insumos & Suprimentos' AND TipoEntidade = 'ProdutoEstoque');

PRINT '   Categorias verificadas/inseridas.';

-- =========================================================================================
-- 4. FORNECEDORES (Fornecedores)
-- =========================================================================================
PRINT '4. Inserindo Fornecedores Homologados...';

IF NOT EXISTS (SELECT 1 FROM Fornecedores WHERE Nome = 'Tech Distribuidora Nacional Ltda')
BEGIN
    INSERT INTO Fornecedores (Nome, Email, Telefone, Observacoes, Ativo, DataCadastro, TipoPessoa, UserId)
    VALUES ('Tech Distribuidora Nacional Ltda', 'vendas@techdistribuidora.com.br', '(11) 3456-7890', 'Fornecedor homologado para hardware, mouses e monitores. Faturamento 28 DDL.', 1, GETDATE(), 2, 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Fornecedores WHERE Nome = 'Connect Log Soluções em Conectividade S/A')
BEGIN
    INSERT INTO Fornecedores (Nome, Email, Telefone, Observacoes, Ativo, DataCadastro, TipoPessoa, UserId)
    VALUES ('Connect Log Soluções em Conectividade S/A', 'comercial@connectlog.com.br', '(19) 3210-9876', 'Distribuidor exclusivo de cabos Furukawa e conectores blindados.', 1, GETDATE(), 2, 'supervisor');
END

IF NOT EXISTS (SELECT 1 FROM Fornecedores WHERE Nome = 'MacroRedes Telecom e Informática Eireli')
BEGIN
    INSERT INTO Fornecedores (Nome, Email, Telefone, Observacoes, Ativo, DataCadastro, TipoPessoa, UserId)
    VALUES ('MacroRedes Telecom e Informática Eireli', 'pedidos@macroredes.com.br', '(41) 3344-5566', 'Importador direto de roteadores e switches corporativos.', 1, GETDATE(), 2, 'supervisor');
END

DECLARE @FornTechId INT = (SELECT TOP 1 Id FROM Fornecedores WHERE Nome = 'Tech Distribuidora Nacional Ltda');
DECLARE @FornConnectId INT = (SELECT TOP 1 Id FROM Fornecedores WHERE Nome = 'Connect Log Soluções em Conectividade S/A');
DECLARE @FornMacroId INT = (SELECT TOP 1 Id FROM Fornecedores WHERE Nome = 'MacroRedes Telecom e Informática Eireli');

PRINT '   Fornecedores verificados/inseridos.';

-- =========================================================================================
-- 5. CATÁLOGO DE PRODUTOS & SKUs (Produtos)
-- Inclui itens com estoque normal, itens controlados por lote e itens críticos (para testes de alerta)
-- =========================================================================================
PRINT '5. Inserindo Catálogo de Produtos & SKUs...';

-- Produto 1: Teclado Mecânico RGB
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'TEC-RGB-001')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Teclado Mecânico RGB Switch Blue', 'Teclado mecânico gamer padrão ABNT2 com iluminação RGB personalizável e switches azuis.',
        'TEC-RGB-001', '7891000000018', 'UN',
        249.90, 120.00, 120.00, 45.00, 10.00, 100.00, 20.00,
        0, 0, 'A-01-P1', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 2: Mouse Óptico 16000 DPI
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'MOU-OPT-002')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Mouse Óptico Ergonômico 16000 DPI', 'Mouse ergonômico para escritório e jogos, sensor Pixart 3389 e 6 botões programáveis.',
        'MOU-OPT-002', '7891000000025', 'UN',
        139.90, 65.00, 65.00, 60.00, 15.00, 150.00, 30.00,
        0, 0, 'A-01-P2', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 3: Monitor Gamer 27" 144Hz (Controla Lote & Rastreabilidade)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'MON-GAM-144')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Monitor Gamer 27" IPS 144Hz 1ms', 'Monitor com taxa de atualização 144Hz, painel IPS, resolução Full HD, conexões DP e HDMI.',
        'MON-GAM-144', '7891000000032', 'UN',
        1199.00, 680.00, 680.00, 18.00, 5.00, 40.00, 10.00,
        1, 1, 'A-02-P1', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 4: Headset Gamer Surround 7.1
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'HEA-SUR-71')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Headset Gamer Surround 7.1 USB', 'Headset com drivers de 50mm, isolamento acústico passivo e microfone removível.',
        'HEA-SUR-71', '7891000000049', 'UN',
        189.90, 95.00, 95.00, 30.00, 8.00, 80.00, 15.00,
        0, 0, 'A-02-P2', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 5: Bobina Cabo de Rede Cat6 305m (Controla Lote)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'CAB-UTP-CAT6')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Bobina Cabo UTP Cat6 305m Azul', 'Cabo de par trançado 4 pares 23AWG 100% cobre homologado Anatel.',
        'CAB-UTP-CAT6', '7891000000056', 'CX',
        449.00, 280.00, 280.00, 14.00, 4.00, 30.00, 8.00,
        1, 0, 'B-01-P1', @LocalCentralId, @FornConnectId, @CatCabosId, 1, 'supervisor'
    );
END

-- Produto 6: Switch 24 Portas Gigabit Gerenciável (Controla Lote)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'SWT-GIG-24P')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Switch Gigabit 24 Portas L2+', 'Switch gerenciável camada 2+ com 24 portas 10/100/1000 Mbps e 4 portas SFP para uplink.',
        'SWT-GIG-24P', '7891000000063', 'UN',
        980.00, 520.00, 520.00, 8.00, 3.00, 20.00, 5.00,
        1, 1, 'B-02-P1', @LocalCentralId, @FornMacroId, @CatRedesId, 1, 'supervisor'
    );
END

-- Produto 7: Roteador Mesh Wi-Fi 6 AX3000
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'ROT-WIFI6-AX')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Roteador Mesh Wi-Fi 6 Dual-Band AX3000', 'Sistema mesh de alta performance com suporte a OFDMA, Beamforming e até 150 dispositivos conectados.',
        'ROT-WIFI6-AX', '7891000000070', 'UN',
        359.90, 190.00, 190.00, 22.00, 6.00, 50.00, 12.00,
        0, 0, 'B-02-P2', @LocalCentralId, @FornMacroId, @CatRedesId, 1, 'supervisor'
    );
END

-- Produto 8: Pacote com 100 Conectores RJ45 Blindados
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'CON-RJ45-100')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Pacote 100 Conectores RJ45 Cat6 Blindados', 'Conector macho RJ45 banhado a ouro 50u para cabos sólidos e flexíveis Cat6 FTP.',
        'CON-RJ45-100', '7891000000087', 'PC',
        69.90, 32.00, 32.00, 50.00, 15.00, 100.00, 25.00,
        0, 0, 'B-03-P1', @LocalCentralId, @FornConnectId, @CatCabosId, 1, 'supervisor'
    );
END

-- Produto 9 (CRÍTICO!): Cabo HDMI 2.1 Ultra High Speed 2m (Estoque: 3 | Mínimo: 10)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'CAB-HDMI-4K')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Cabo HDMI 2.1 Ultra High Speed 8K/4K 2m', 'Cabo HDMI com largura de banda de 48 Gbps, suporte a eARC, Dynamic HDR e VRR.',
        'CAB-HDMI-4K', '7891000000094', 'UN',
        45.00, 18.00, 18.00, 3.00, 10.00, 80.00, 15.00,
        0, 0, 'B-03-P2', @LocalCentralId, @FornConnectId, @CatCabosId, 1, 'supervisor'
    );
END

-- Produto 10 (CRÍTICO!): Fonte ATX 650W 80 Plus Bronze (Estoque: 2 | Mínimo: 5 | Controla Lote)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'FON-ATX-650W')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Fonte de Alimentação ATX 650W 80 Plus Bronze', 'Fonte semi-modular com PFC ativo, ventoinha silenciosa de 120mm e proteções OVP/UVP/SCP.',
        'FON-ATX-650W', '7891000000100', 'UN',
        389.00, 210.00, 210.00, 2.00, 5.00, 30.00, 8.00,
        1, 1, 'A-03-P1', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 11 (CRÍTICO!): SSD NVMe M.2 1TB (Estoque: 2 | Mínimo: 6 | Controla Lote)
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'SSD-NVME-1TB')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'SSD NVMe M.2 PCIe 4.0 1TB 3500MB/s', 'Unidade de estado sólido formato 2280 com leitura rápida até 3500 MB/s e gravação até 3000 MB/s.',
        'SSD-NVME-1TB', '7891000000117', 'UN',
        329.00, 175.00, 175.00, 2.00, 6.00, 40.00, 10.00,
        1, 1, 'A-03-P2', @LocalCentralId, @FornTechId, @CatPerifericosId, 1, 'supervisor'
    );
END

-- Produto 12: Pasta Térmica Silver 5g
IF NOT EXISTS (SELECT 1 FROM Produtos WHERE CodigoSKU = 'PAS-TER-05G')
BEGIN
    INSERT INTO Produtos (
        Nome, Descricao, CodigoSKU, CodigoBarras, UnidadeMedida,
        Preco, PrecoCusto, CustoMedio, QuantidadeEstoque, EstoqueMinimo, EstoqueMaximo, PontoPedido,
        ControlaLote, ControlaValidade, LocalizacaoCorredor, LocalPadraoId, FornecedorPadraoId, IdCategoria, Ativo, UserId
    ) VALUES (
        'Pasta Térmica Prata Alta Condutividade 5g', 'Composto térmico de alto rendimento com partículas de prata para dissipadores de CPU e GPU.',
        'PAS-TER-05G', '7891000000124', 'UN',
        35.00, 12.00, 12.00, 40.00, 10.00, 100.00, 20.00,
        0, 0, 'C-01-P1', @LocalCentralId, @FornTechId, @CatInsumosId, 1, 'supervisor'
    );
END

PRINT '   Catálogo de Produtos verificado/inserido.';

-- =========================================================================================
-- 6. LOTES DE ESTOQUE (EstoqueLotes)
-- Lotes para rastreabilidade, controle FEFO e monitoramento de validades
-- =========================================================================================
PRINT '6. Inserindo Lotes de Estoque...';

DECLARE @ProdMonitorId INT = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'MON-GAM-144');
DECLARE @ProdCaboCat6Id INT = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'CAB-UTP-CAT6');
DECLARE @ProdSwitchId INT = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'SWT-GIG-24P');
DECLARE @ProdFonteId INT = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'FON-ATX-650W');
DECLARE @ProdSsdId INT = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'SSD-NVME-1TB');

-- Lote 1: Monitores (18 unidades disponíveis no Almoxarifado Central)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-MG01')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdMonitorId, @LocalCentralId, 'LOT-2026-MG01', DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, 22, GETDATE()),
        20.00, 18.00, 680.00, 1, 'supervisor'
    );
END

-- Lote 2: Bobinas Cat6 (14 bobinas no Almoxarifado Central)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-CAT6A')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdCaboCat6Id, @LocalCentralId, 'LOT-2026-CAT6A', DATEADD(MONTH, -1, GETDATE()), DATEADD(MONTH, 35, GETDATE()),
        15.00, 14.00, 280.00, 1, 'supervisor'
    );
END

-- Lote 3: Switches 24P (8 unidades no Almoxarifado Central)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-SW24')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdSwitchId, @LocalCentralId, 'LOT-2026-SW24', DATEADD(MONTH, -3, GETDATE()), DATEADD(MONTH, 33, GETDATE()),
        10.00, 8.00, 520.00, 1, 'supervisor'
    );
END

-- Lote 4: Fontes 650W (2 unidades - Crítico!)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-FN65')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdFonteId, @LocalCentralId, 'LOT-2026-FN65', DATEADD(MONTH, -4, GETDATE()), DATEADD(MONTH, 20, GETDATE()),
        10.00, 2.00, 210.00, 1, 'supervisor'
    );
END

-- Lote 5: SSDs 1TB (2 unidades - Crítico!)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-SSD1T')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdSsdId, @LocalCentralId, 'LOT-2026-SSD1T', DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, 34, GETDATE()),
        12.00, 2.00, 175.00, 1, 'supervisor'
    );
END

-- Lote 6: Em Quarentena / Avarias (1 Monitor avariado na Doca)
IF NOT EXISTS (SELECT 1 FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-MG-AV01')
BEGIN
    INSERT INTO EstoqueLotes (
        ProdutoId, LocalEstoqueId, CodigoLote, DataFabricacao, DataValidade,
        QuantidadeInicial, QuantidadeSaldo, CustoUnitario, StatusLote, UserId
    ) VALUES (
        @ProdMonitorId, @LocalAvariasId, 'LOT-2026-MG-AV01', DATEADD(MONTH, -2, GETDATE()), DATEADD(MONTH, 22, GETDATE()),
        1.00, 1.00, 680.00, 2, 'supervisor' -- Status 2 = Quarentena
    );
END

DECLARE @LoteMonitorId INT = (SELECT TOP 1 Id FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-MG01');
DECLARE @LoteCat6Id INT = (SELECT TOP 1 Id FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-CAT6A');
DECLARE @LoteSwitchId INT = (SELECT TOP 1 Id FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-SW24');
DECLARE @LoteFonteId INT = (SELECT TOP 1 Id FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-FN65');
DECLARE @LoteSsdId INT = (SELECT TOP 1 Id FROM EstoqueLotes WHERE CodigoLote = 'LOT-2026-SSD1T');

PRINT '   Lotes de estoque verificados/inseridos.';

-- =========================================================================================
-- 7. HISTÓRICO DE MOVIMENTAÇÕES KARDEX (EstoqueMovimentacoes)
-- Registra a formação dos saldos com tipos:
-- 1 = EntradaCompra | 2 = SaidaVenda | 4 = TransferenciaSaida
-- =========================================================================================
PRINT '7. Inserindo Histórico de Movimentações Kardex...';

-- Entrada por Compra: Teclados Mecânicos (50 unidades recebidas via NF-e 10240)
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'NFE-10240-TEC')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'TEC-RGB-001'),
        @LocalCentralId, NULL, NULL, @FornTechId,
        1, 50.00, 120.00, 0.00, 50.00,
        'NFE-10240-TEC', DATEADD(DAY, -15, GETDATE()), 'Entrada de compra inicial via NF-e 10240 da Tech Distribuidora',
        1, 'JT_Estoque', 'supervisor'
    );
END

-- Saída por Venda: 5 Teclados Mecânicos
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'PED-0012-TEC')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'TEC-RGB-001'),
        @LocalCentralId, NULL, NULL, NULL,
        2, 5.00, 120.00, 50.00, 45.00,
        'PED-0012-TEC', DATEADD(DAY, -5, GETDATE()), 'Saída faturada no Pedido de Venda #0012',
        1, 'JT_Estoque', 'supervisor'
    );
END

-- Entrada por Compra: Monitores Gamer 144Hz (20 unidades com Lote via NF-e 10245)
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'NFE-10245-MON')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        @ProdMonitorId, @LocalCentralId, NULL, @LoteMonitorId, @FornTechId,
        1, 20.00, 680.00, 0.00, 20.00,
        'NFE-10245-MON', DATEADD(DAY, -20, GETDATE()), 'Recebimento de lote de monitores gamer via NF-e 10245',
        1, 'JT_Estoque', 'supervisor'
    );
END

-- Saída por Venda: 2 Monitores Gamer
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'PED-0015-MON')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        @ProdMonitorId, @LocalCentralId, NULL, @LoteMonitorId, NULL,
        2, 2.00, 680.00, 20.00, 18.00,
        'PED-0015-MON', DATEADD(DAY, -3, GETDATE()), 'Atendimento de pedido faturado #0015',
        1, 'JT_Estoque', 'supervisor'
    );
END

-- Entrada por Compra: Bobinas Cat6 (15 caixas via NF-e 9840)
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'NFE-9840-CAT6')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        @ProdCaboCat6Id, @LocalCentralId, NULL, @LoteCat6Id, @FornConnectId,
        1, 15.00, 280.00, 0.00, 15.00,
        'NFE-9840-CAT6', DATEADD(DAY, -12, GETDATE()), 'Entrada de reposição de cabos UTP Cat6',
        1, 'JT_Estoque', 'supervisor'
    );
END

-- Entrada por Compra: Switches 24P (10 unidades via NF-e 8731)
IF NOT EXISTS (SELECT 1 FROM EstoqueMovimentacoes WHERE DocumentoReferencia = 'NFE-8731-SWT')
BEGIN
    INSERT INTO EstoqueMovimentacoes (
        ProdutoId, LocalOrigemId, LocalDestinoId, LoteId, FornecedorId,
        TipoMovimentacao, Quantidade, CustoUnitario, SaldoAnterior, SaldoPosterior,
        DocumentoReferencia, DataMovimentacao, Observacao, UsuarioId, TenantId, UserId
    ) VALUES (
        @ProdSwitchId, @LocalCentralId, NULL, @LoteSwitchId, @FornMacroId,
        1, 10.00, 520.00, 0.00, 10.00,
        'NFE-8731-SWT', DATEADD(DAY, -18, GETDATE()), 'Aquisição corporativa de switches gerenciáveis',
        1, 'JT_Estoque', 'supervisor'
    );
END

PRINT '   Histórico Kardex verificado/inserido.';

-- =========================================================================================
-- 8. SESSÃO DE INVENTÁRIO FÍSICO E AUDITORIA (EstoqueInventarios & EstoqueInventarioItens)
-- =========================================================================================
PRINT '8. Inserindo Sessão de Inventário Físico...';

IF NOT EXISTS (SELECT 1 FROM EstoqueInventarios WHERE Codigo = 'INV-2026-Q1')
BEGIN
    INSERT INTO EstoqueInventarios (
        Codigo, LocalEstoqueId, DataAbertura, DataFechamento, Status, Observacoes, UserId
    ) VALUES (
        'INV-2026-Q1', @LocalCentralId, DATEADD(DAY, -2, GETDATE()), NULL, 2, -- Status 2 = EmContagem
        'Inventário geral de fechamento do 1º trimestre no Almoxarifado Central (Docas 01 a 04).', 'supervisor'
    );
END

DECLARE @InventarioId INT = (SELECT TOP 1 Id FROM EstoqueInventarios WHERE Codigo = 'INV-2026-Q1');

IF @InventarioId IS NOT NULL
BEGIN
    -- Item 1: Teclado Mecânico (Sistema: 45 | Contado: 45 -> Sem divergência)
    IF NOT EXISTS (SELECT 1 FROM EstoqueInventarioItens WHERE InventarioId = @InventarioId AND ProdutoId = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'TEC-RGB-001'))
    BEGIN
        INSERT INTO EstoqueInventarioItens (InventarioId, ProdutoId, LoteId, QuantidadeSistema, QuantidadeContada, Divergencia, UserId)
        VALUES (@InventarioId, (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'TEC-RGB-001'), NULL, 45.00, 45.00, 0.00, 'supervisor');
    END

    -- Item 2: Mouse Óptico (Sistema: 60 | Contado: 61 -> Sobra de 1)
    IF NOT EXISTS (SELECT 1 FROM EstoqueInventarioItens WHERE InventarioId = @InventarioId AND ProdutoId = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'MOU-OPT-002'))
    BEGIN
        INSERT INTO EstoqueInventarioItens (InventarioId, ProdutoId, LoteId, QuantidadeSistema, QuantidadeContada, Divergencia, UserId)
        VALUES (@InventarioId, (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'MOU-OPT-002'), NULL, 60.00, 61.00, 1.00, 'supervisor');
    END

    -- Item 3: Monitor Gamer 144Hz (Sistema: 18 | Contado: 18 -> Sem divergência)
    IF NOT EXISTS (SELECT 1 FROM EstoqueInventarioItens WHERE InventarioId = @InventarioId AND ProdutoId = @ProdMonitorId)
    BEGIN
        INSERT INTO EstoqueInventarioItens (InventarioId, ProdutoId, LoteId, QuantidadeSistema, QuantidadeContada, Divergencia, UserId)
        VALUES (@InventarioId, @ProdMonitorId, @LoteMonitorId, 18.00, 18.00, 0.00, 'supervisor');
    END

    -- Item 4: Cabo HDMI 2.1 (Sistema: 3 | Contado: 2 -> Falta de 1)
    IF NOT EXISTS (SELECT 1 FROM EstoqueInventarioItens WHERE InventarioId = @InventarioId AND ProdutoId = (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'CAB-HDMI-4K'))
    BEGIN
        INSERT INTO EstoqueInventarioItens (InventarioId, ProdutoId, LoteId, QuantidadeSistema, QuantidadeContada, Divergencia, UserId)
        VALUES (@InventarioId, (SELECT TOP 1 Id FROM Produtos WHERE CodigoSKU = 'CAB-HDMI-4K'), NULL, 3.00, 2.00, -1.00, 'supervisor');
    END
END

PRINT '   Sessão de Inventário e itens verificados/inseridos.';

PRINT '------------------------------------------------------------';
PRINT 'Seed de Dados Mock para JT_Estoque CONCLUÍDO COM SUCESSO!';
PRINT '------------------------------------------------------------';
