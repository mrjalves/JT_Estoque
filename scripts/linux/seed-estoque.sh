#!/usr/bin/env bash
# =========================================================================================
# Executa migração de schema, reset dos dados mock e seed no JT_Estoque (Linux/VPS).
# Uso: sudo ./seed-estoque.sh [--dry-run]
# Variáveis opcionais:
#   APP_DIR       (default: /var/www/jt_sistemas_estoque)
#   SERVICE_NAME  (default: jt_sistemas_estoque)
#   DOTNET_BIN    (default: /usr/bin/dotnet)
# =========================================================================================
set -euo pipefail

APP_DIR="${APP_DIR:-/var/www/jt_sistemas_estoque}"
SERVICE_NAME="${SERVICE_NAME:-jt_sistemas_estoque}"
DOTNET_BIN="${DOTNET_BIN:-/usr/bin/dotnet}"
APP_DLL="${APP_DLL:-$APP_DIR/JT_Estoque.Web.dll}"

if [[ ! -f "$APP_DLL" ]]; then
    echo "[ERRO] Aplicação não encontrada em: $APP_DLL" >&2
    echo "       Ajuste APP_DIR ou APP_DLL." >&2
    exit 1
fi

if [[ $EUID -eq 0 ]]; then
    SUDO=""
    RUN_AS_WWW_DATA=(runuser -u www-data --)
else
    SUDO="sudo"
    RUN_AS_WWW_DATA=(sudo -u www-data --)
fi

echo "=== [1/4] Parando serviço $SERVICE_NAME ==="
$SUDO systemctl stop "$SERVICE_NAME" || true

echo "=== [2/4] Limpando cache SQLite local ==="
$SUDO find "$APP_DIR" -maxdepth 3 -name 'jt_local_cache.db3*' -delete 2>/dev/null || true
$SUDO find /var/www/.local/share /var/lib/www-data/.local/share -maxdepth 2 -name 'jt_local_cache.db3*' -delete 2>/dev/null || true

echo "=== [3/4] Executando migração, reset e seed ==="
cd "$APP_DIR"
"${RUN_AS_WWW_DATA[@]}" "$DOTNET_BIN" "$APP_DLL" \
    --migrate-estoque --reset-estoque --seed-estoque "$@"

echo "=== [4/4] Reiniciando serviço $SERVICE_NAME ==="
$SUDO systemctl start "$SERVICE_NAME"
$SUDO systemctl status "$SERVICE_NAME" --no-pager || true

echo "=== Concluído. Verifique os logs com: journalctl -u $SERVICE_NAME -n 100 --no-pager ==="
