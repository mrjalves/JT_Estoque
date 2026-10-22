-- =========================================================================================
-- RESET DOS DADOS MOCK - JT_ESTOQUE
-- Remove apenas os registros criados pelo SeedMockDataEstoque.sql.
-- ATENÇÃO: movimentações e lotes vinculados aos SKUs mock também são removidos,
-- inclusive registros criados manualmente para esses produtos.
-- Execute com backup prévio do banco JT_Estoque.
-- =========================================================================================

SET NOCOUNT ON;

PRINT '------------------------------------------------------------';
PRINT 'Removendo dados mock do JT_Estoque...';
PRINT '------------------------------------------------------------';

DECLARE @InventarioId INT = (SELECT TOP 1 Id FROM EstoqueInventarios WHERE Codigo = 'INV-2026-Q1');

-- 1. Itens de inventário
IF @InventarioId IS NOT NULL
BEGIN
    DELETE FROM EstoqueInventarioItens WHERE InventarioId = @InventarioId;
END

-- 2. Inventários
DELETE FROM EstoqueInventarios WHERE Codigo = 'INV-2026-Q1';

-- 3. Movimentações dos produtos mock (inclui lançamentos manuais desses SKUs)
DELETE FROM EstoqueMovimentacoes
WHERE ProdutoId IN (
    SELECT Id FROM Produtos WHERE CodigoSKU IN (
        'TEC-RGB-001', 'MOU-OPT-002', 'MON-GAM-144', 'HEA-SUR-71',
        'CAB-UTP-CAT6', 'SWT-GIG-24P', 'ROT-WIFI6-AX', 'CON-RJ45-100',
        'CAB-HDMI-4K', 'FON-ATX-650W', 'SSD-NVME-1TB', 'PAS-TER-05G'
    )
)
OR DocumentoReferencia IN (
    'NFE-10240-TEC', 'PED-0012-TEC', 'NFE-10245-MON',
    'PED-0015-MON', 'NFE-9840-CAT6', 'NFE-8731-SWT'
);

-- 4. Lotes dos produtos mock
DELETE FROM EstoqueLotes
WHERE CodigoLote IN (
    'LOT-2026-MG01', 'LOT-2026-CAT6A', 'LOT-2026-SW24',
    'LOT-2026-FN65', 'LOT-2026-SSD1T', 'LOT-2026-MG-AV01'
)
OR ProdutoId IN (
    SELECT Id FROM Produtos WHERE CodigoSKU IN (
        'TEC-RGB-001', 'MOU-OPT-002', 'MON-GAM-144', 'HEA-SUR-71',
        'CAB-UTP-CAT6', 'SWT-GIG-24P', 'ROT-WIFI6-AX', 'CON-RJ45-100',
        'CAB-HDMI-4K', 'FON-ATX-650W', 'SSD-NVME-1TB', 'PAS-TER-05G'
    )
);

-- 5. Produtos mock
DELETE FROM Produtos WHERE CodigoSKU IN (
    'TEC-RGB-001', 'MOU-OPT-002', 'MON-GAM-144', 'HEA-SUR-71',
    'CAB-UTP-CAT6', 'SWT-GIG-24P', 'ROT-WIFI6-AX', 'CON-RJ45-100',
    'CAB-HDMI-4K', 'FON-ATX-650W', 'SSD-NVME-1TB', 'PAS-TER-05G'
);

-- 6. Fornecedores mock
DELETE FROM Fornecedores WHERE Nome IN (
    'Tech Distribuidora Nacional Ltda',
    'Connect Log Soluções em Conectividade S/A',
    'MacroRedes Telecom e Informática Eireli'
);

-- 7. Categorias mock
DELETE FROM Categorias
WHERE TipoEntidade = 'ProdutoEstoque'
  AND Nome IN (
      'Hardware & Periféricos',
      'Cabos & Conectividade',
      'Equipamentos de Rede',
      'Insumos & Suprimentos'
  );

-- 8. Locais de estoque mock
DELETE FROM LocaisEstoque WHERE Codigo IN (
    'ALM-CENTRAL', 'ALM-FILIAL', 'ALM-AVARIAS', 'ALM-EXPEDICAO'
);

PRINT 'Dados mock removidos com sucesso.';
PRINT '------------------------------------------------------------';
