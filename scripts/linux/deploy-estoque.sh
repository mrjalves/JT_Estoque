#!/usr/bin/env bash
# Script de deploy para JT_Estoque no servidor Linux
set -e

APP_DIR="/var/www/jt_sistemas_estoque"
SERVICE_NAME="jt_sistemas_estoque"

echo "=== [1/4] Parando serviço $SERVICE_NAME ==="
sudo systemctl stop $SERVICE_NAME || true

echo "=== [2/4] Copiando arquivos publicados para $APP_DIR ==="
sudo mkdir -p $APP_DIR
sudo chown -R www-data:www-data $APP_DIR

echo "=== [3/4] Reiniciando serviço $SERVICE_NAME ==="
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl start $SERVICE_NAME
sudo systemctl status $SERVICE_NAME --no-pager

echo "=== [4/4] Verificando Nginx e Certificado SSL ==="
sudo nginx -t && sudo systemctl reload nginx

echo "=== Deploy concluído com sucesso! ==="
