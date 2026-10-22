# Guia de Configuração e Deploy na VPS Linux (Nginx + SSL + Systemd)

Este documento descreve todo o passo a passo para hospedar o **JT_Estoque.Web** no servidor Linux de produção (`jtecsistemas.com.br`), configurando o proxy reverso Nginx com suporte a WebSockets/SignalR (Blazor InteractiveServer), serviço Systemd (.NET 10) e certificado digital SSL/TLS gratuito via Let's Encrypt (Certbot).

---

## 1. Visão Geral da Arquitetura no Servidor

- **Aplicação**: `JT_Estoque.Web` (.NET 10 Blazor Server).
- **Porta Kestrel (Interna)**: `http://127.0.0.1:5004`.
- **Diretório da Publicação**: `/var/www/jt_sistemas_estoque`.
- **Serviço Systemd**: `jt_sistemas_estoque.service`.
- **Domínio Público**: `https://estoque.jtecsistemas.com.br`.
- **Proxy Reverso**: Nginx escutando na porta `80` (redirecionamento HTTP -> HTTPS) e `443` (SSL).

---

## 2. Passo a Passo de Configuração na VPS

### 2.1. Configuração do Serviço Systemd (.NET 10)

Crie o arquivo de serviço do systemd para manter a aplicação executando em segundo plano:

```bash
sudo cat << 'EOF' > /etc/systemd/system/jt_sistemas_estoque.service
[Unit]
Description=JT Sistemas - Estoque Web Application (.NET 10)
After=network.target

[Service]
Type=simple
User=www-data
Group=www-data
WorkingDirectory=/var/www/jt_sistemas_estoque
ExecStart=/usr/bin/dotnet /var/www/jt_sistemas_estoque/JT_Estoque.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=jt_sistemas_estoque

# Variáveis de ambiente da aplicação
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false
Environment=ASPNETCORE_URLS=http://127.0.0.1:5004

[Install]
WantedBy=multi-user.target
EOF
```

Ative e inicialize o serviço:
```bash
sudo systemctl daemon-reload
sudo systemctl enable jt_sistemas_estoque
sudo systemctl start jt_sistemas_estoque
sudo systemctl status jt_sistemas_estoque --no-pager
```

---

### 2.2. Configuração do Nginx (Proxy Reverso + WebSockets)

O Blazor Server interativo depende de comunicação bidirecional contínua via **SignalR / WebSocket**. É fundamental que o bloco `map $http_upgrade` e os cabeçalhos de `Upgrade` estejam presentes.

Crie a configuração do site:

```bash
sudo cat << 'EOF' > /etc/nginx/sites-available/jt_sistemas_estoque
# /etc/nginx/sites-available/jt_sistemas_estoque
map $http_upgrade $connection_upgrade {
    default upgrade;
    ''      keep-alive;
}

server {
    listen 80;
    server_name estoque.jtecsistemas.com.br;

    client_max_body_size 50M;

    location / {
        proxy_pass http://localhost:5004;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $connection_upgrade;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;

        # Timeouts estendidos para manter o circuito Blazor ativo
        proxy_read_timeout 86400s;
        proxy_send_timeout 86400s;
    }
}
EOF
```

---

### 2.3. Ativação do Site e Remoção de Conflitos Antigos

Para garantir que apenas o arquivo padrão `jt_sistemas_estoque` fique ativo e sem avisos de conflito:

```bash
# 1. Remover eventuais arquivos antigos ou duplicados (ex: 'estoque')
sudo rm -f /etc/nginx/sites-enabled/estoque
sudo rm -f /etc/nginx/sites-available/estoque

# 2. Habilitar o novo site criando o link simbólico
sudo ln -s /etc/nginx/sites-available/jt_sistemas_estoque /etc/nginx/sites-enabled/

# 3. Testar a sintaxe do Nginx
sudo nginx -t

# 4. Recarregar o Nginx
sudo systemctl reload nginx
```

> **Verificação**: Ao rodar `ls -l /etc/nginx/sites-enabled/`, deve constar apenas:
> `jt_sistemas_estoque -> /etc/nginx/sites-available/jt_sistemas_estoque`

---

### 2.4. Instalação do Certificado SSL (HTTPS) com Let's Encrypt

Para resolver o alerta de **"Inseguro" / "Não seguro"** no navegador:

```bash
sudo certbot --nginx -d estoque.jtecsistemas.com.br
```

- **Se o certificado for novo**: O Certbot obterá a chave e adicionará as diretivas `ssl_certificate` e o redirecionamento automático de HTTP (porta 80) para HTTPS (porta 443).
- **Se o certificado já existia no servidor**: O Certbot apresentará um menu com opções. Selecione a opção **`1`** (`Attempt to reinstall this existing certificate`) e pressione `Enter`.

Após a conclusão, o Certbot altera o arquivo `/etc/nginx/sites-available/jt_sistemas_estoque` automaticamente, adicionando:
```nginx
listen 443 ssl; # managed by Certbot
ssl_certificate /etc/letsencrypt/live/estoque.jtecsistemas.com.br/fullchain.pem;
ssl_certificate_key /etc/letsencrypt/live/estoque.jtecsistemas.com.br/privkey.pem;
include /etc/letsencrypt/options-ssl-nginx.conf;
ssl_dhparam /etc/letsencrypt/ssl-dhparams.pem;
```

---

## 3. Publicação e Atualização de Novas Versões

### 3.1. Publicação Local (Windows)

Na máquina de desenvolvimento:
```powershell
dotnet publish src/JT_Estoque.Web/JT_Estoque.Web.csproj -c Release -o ./publish
```

Em seguida, envie a pasta `./publish` para a VPS via SCP, SFTP ou Rsync:
```bash
scp -r ./publish/* root@jtecsistemas.com.br:/var/www/jt_sistemas_estoque/
```

### 3.2. Script de Deploy na VPS (`deploy-estoque.sh`)

Para automatizar a reinicialização e permissões na VPS:

```bash
sudo chmod +x /var/www/jt_sistemas_estoque/scripts/linux/deploy-estoque.sh
sudo /var/www/jt_sistemas_estoque/scripts/linux/deploy-estoque.sh
```

Conteúdo do script (`scripts/linux/deploy-estoque.sh`):
```bash
#!/usr/bin/env bash
set -e

APP_DIR="/var/www/jt_sistemas_estoque"
SERVICE_NAME="jt_sistemas_estoque"

echo "=== [1/4] Parando serviço $SERVICE_NAME ==="
sudo systemctl stop $SERVICE_NAME || true

echo "=== [2/4] Ajustando permissões em $APP_DIR ==="
sudo chown -R www-data:www-data $APP_DIR

echo "=== [3/4] Reiniciando serviço $SERVICE_NAME ==="
sudo systemctl daemon-reload
sudo systemctl enable $SERVICE_NAME
sudo systemctl start $SERVICE_NAME
sudo systemctl status $SERVICE_NAME --no-pager

echo "=== [4/4] Recarregando Nginx ==="
sudo nginx -t && sudo systemctl reload nginx

echo "=== Deploy concluído com sucesso! ==="
```

### 3.3. Migração, Reset e Seed dos Dados Mock

Antes de executar, ajuste o `appsettings.Production.json` na VPS:

```json
{
  "Config": {
    "Conexao": {
      "TipoConexao": "REDE",
      "StringConexao": "Data Source=179.198.119.165;Initial Catalog=JT_Estoque;User ID=...;Password=...;TrustServerCertificate=True;"
    },
    "Clientes": {
      "JT_Estoque": {
        "StringConexao": "Data Source=179.198.119.165;Initial Catalog=JT_Estoque;User ID=...;Password=...;TrustServerCertificate=True;"
      }
    },
    "Executor": "Sistema",
    "JwtSecretKey": "uma-chave-com-pelo-menos-32-caracteres",
    "Migration": {
      "RemoverOrfaos": false
    }
  }
}
```

> `JwtSecretKey` é obrigatória no modo local (`TipoConexao=REDE`) e deve ter no mínimo 32 caracteres.

Execute o script (para o serviço, limpa o cache SQLite, migra, reseta e aplica o seed):

```bash
sudo chmod +x /var/www/jt_sistemas_estoque/scripts/linux/seed-estoque.sh
sudo /var/www/jt_sistemas_estoque/scripts/linux/seed-estoque.sh
```

Para apenas validar o DDL sem alterar dados:

```bash
sudo /var/www/jt_sistemas_estoque/scripts/linux/seed-estoque.sh --dry-run
```

Também é possível chamar a CLI diretamente:

```bash
cd /var/www/jt_sistemas_estoque
sudo -u www-data dotnet JT_Estoque.Web.dll --migrate-estoque             # só migra
sudo -u www-data dotnet JT_Estoque.Web.dll --reset-estoque               # só remove mock
sudo -u www-data dotnet JT_Estoque.Web.dll --seed-estoque                # migra + seed (sem reset)
sudo -u www-data dotnet JT_Estoque.Web.dll --migrate-estoque --reset-estoque --seed-estoque
```

> O reset remove os registros mock e também movimentações/lotes vinculados aos SKUs mock, inclusive lançamentos manuais desses produtos. Faça backup antes.

---

## 4. Comandos de Diagnóstico e Resolução de Problemas

| Comando | Descrição |
| :--- | :--- |
| `sudo systemctl status jt_sistemas_estoque` | Verifica se a aplicação .NET está ativa e rodando |
| `sudo journalctl -u jt_sistemas_estoque -f -n 50` | Exibe os logs em tempo real da aplicação .NET |
| `sudo nginx -t` | Valida sintaxe de todos os arquivos de configuração do Nginx |
| `sudo systemctl reload nginx` | Aplica alterações do Nginx sem derrubar conexões ativas |
| `sudo certbot certificates` | Lista todos os certificados SSL instalados e validades |
| `sudo certbot renew --dry-run` | Testa a renovação automática dos certificados Let's Encrypt |
| `curl -I https://estoque.jtecsistemas.com.br` | Testa a resposta HTTP 200 do domínio com SSL |
