# Checklist de Implantação do JT_Estoque na VPS

Passo a passo para publicar o web, configurar o `appsettings.Production.json` e aplicar o seed/reset dos dados mock.

Substitua antes de executar:

| Variável | Exemplo |
|---|---|
| `VPS_USER` | `deploy` |
| `VPS_HOST` | `179.198.119.165` |
| `VPS_PORT` | `22` |
| `APP_DIR` | `/var/www/jt_sistemas_estoque` |
| `SERVICE` | `jt_sistemas_estoque` |
| `SQL_USER` | usuário do SQL Server |
| `SQL_PASS` | senha do SQL Server |

> O banco usado é o `JT_Estoque` em `179.198.119.165`. A senha **não** pode conter `;`, pois quebra a string de conexão.

No terminal local (bash: Linux/macOS, WSL ou Git Bash), ajuste e exporte os valores usados nos comandos abaixo (não commite este bloco com senhas reais):

```bash
export VPS_USER="deploy"
export VPS_HOST="179.198.119.165"
export VPS_PORT="22"
export APP_DIR="/var/www/jt_sistemas_estoque"
export SERVICE="jt_sistemas_estoque"
export SQL_USER="USUARIO"
export SQL_PASS="SENHA"
```

---

## 0. Pré-requisitos

```bash
# Framework já commitado/pushado em Master (fix de herança, persistência e migrador)
git -C ../JT_ApplicationLayer log -1 --oneline origin/Master

# Publicar o web: push em main/master dispara o GitHub Actions "Deploy Estoque"
git push origin main
```

Aguarde o workflow e confirme que o serviço subiu:

```bash
ssh -p "$VPS_PORT" "$VPS_USER@$VPS_HOST" "systemctl is-active $SERVICE"
```

---

## 1. Criar/ajustar o `appsettings.Production.json` na VPS

O deploy faz `rsync --delete` **excluindo** `appsettings.json` e `appsettings.Production.json`, então esse arquivo persiste entre deploys.

```bash
# Backup do arquivo atual (se existir)
sudo cp APP_DIR/appsettings.Production.json APP_DIR/appsettings.Production.json.bak 2>/dev/null || true

# Criar a partir do exemplo do repositório
sudo nano APP_DIR/appsettings.Production.json
```

Base para colar (copie de `scripts/linux/appsettings.Production.example.json` e troque `USUARIO`/`SENHA`/`JwtSecretKey`):

```json
{
  "Config": {
    "Ambiente": "Producao",
    "Executor": "Sistema",
    "Conexao": {
      "TipoConexao": "REDE",
      "TipoBanco": "SqlServer",
      "StringConexao": "Data Source=179.198.119.165;Initial Catalog=JT_Estoque;Persist Security Info=True;User ID=USUARIO;Password=SENHA;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;"
    },
    "Clientes": {
      "JT_Estoque": {
        "TipoBanco": "SqlServer",
        "StringConexao": "Data Source=179.198.119.165;Initial Catalog=JT_Estoque;Persist Security Info=True;User ID=USUARIO;Password=SENHA;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;",
        "ModulosAtivos": ["Comum", "Acessos", "Gestao", "CRM", "IA"]
      }
    },
    "JwtSecretKey": "TROQUE-POR-UMA-CHAVE-COM-32-CARACTERES-OU-MAIS",
    "Migration": { "RemoverOrfaos": false },
    "LogAtivo": true,
    "DiretorioLogs": "Logs",
    "CapturaErroAtiva": true,
    "TimeoutPadrao": 30,
    "TipoApp": "WEB"
  }
}
```

Ajuste permissões e valide o JSON (o workflow também valida antes do restart):

```bash
sudo chown www-data:www-data APP_DIR/appsettings.Production.json
sudo chmod 600 APP_DIR/appsettings.Production.json
sudo python3 -m json.tool APP_DIR/appsettings.Production.json > /dev/null && echo "JSON OK"
```

Regras importantes:

- `Conexao.StringConexao` e `Clientes:JT_Estoque:StringConexao` devem apontar para o **mesmo** banco.
- `JwtSecretKey` é obrigatória no modo local (`TipoConexao=REDE`) e precisa de **no mínimo 32 caracteres**.
- `Migration:RemoverOrfaos` deve ficar `false` (evita dropar colunas/tabelas de outros módulos).
- Se o `FirebaseConfig` não existir no `appsettings.json` da VPS, inclua o bloco no `appsettings.Production.json` (o exemplo do repositório já traz os valores).

---

## 2. Instalar o script de seed

O `dotnet publish` **não** inclui a pasta `scripts/`, portanto o script não aparece em `APP_DIR`. Instale-o uma única vez em `/usr/local/bin`:

```bash
scp -P "$VPS_PORT" scripts/linux/seed-estoque.sh "$VPS_USER@$VPS_HOST:/tmp/seed-estoque.sh"
ssh -p "$VPS_PORT" "$VPS_USER@$VPS_HOST" "sudo install -m 755 /tmp/seed-estoque.sh /usr/local/bin/seed-estoque.sh && rm -f /tmp/seed-estoque.sh"
```

O script usa por padrão `APP_DIR=/var/www/jt_sistemas_estoque` e `SERVICE=jt_sistemas_estoque`. Se os caminhos forem diferentes, edite as variáveis no topo ou passe via ambiente:

```bash
sudo APP_DIR=/outro/caminho SERVICE_NAME=outro_servico /usr/local/bin/seed-estoque.sh
```

---

## 3. Backup do banco (obrigatório)

```bash
# O caminho do .bak é no servidor SQL, não na VPS Linux
/opt/mssql-tools18/bin/sqlcmd -S 179.198.119.165 -U "$SQL_USER" -P "$SQL_PASS" -C \
  -Q "BACKUP DATABASE [JT_Estoque] TO DISK='C:\Backups\JT_Estoque_$(date +%Y%m%d_%H%M).bak' WITH INIT, COMPRESSION;"
```

Se `sqlcmd` não estiver instalado na VPS: `sudo apt install mssql-tools18 unixodbc-dev`.

---

## 4. Validar o DDL sem alterar dados

```bash
sudo /usr/local/bin/seed-estoque.sh --dry-run
```

O script para o serviço, limpa o cache SQLite, imprime o DDL da migração e reinicia. Confira se não há erro de conexão.

---

## 5. Aplicar migração, reset e seed

```bash
sudo /usr/local/bin/seed-estoque.sh
```

Etapas executadas: para o serviço → remove `jt_local_cache.db3*` → `--migrate-estoque --reset-estoque --seed-estoque` como `www-data` → reinicia o serviço.

O log deve terminar com contagens próximas de:

```text
[MOCK] Contagens atuais: Produtos=12, Locais=4, Lotes=6, Movimentações=6, Inventários=1, Fornecedores=3
```

---

## 6. Validar dados e aplicação

```bash
/opt/mssql-tools18/bin/sqlcmd -S 179.198.119.165 -U "$SQL_USER" -P "$SQL_PASS" -C -d JT_Estoque -Q "
SELECT 'Produtos' AS Tabela, COUNT(*) AS Total FROM Produtos
UNION ALL SELECT 'Locais', COUNT(*) FROM LocaisEstoque
UNION ALL SELECT 'Lotes', COUNT(*) FROM EstoqueLotes
UNION ALL SELECT 'Movimentacoes', COUNT(*) FROM EstoqueMovimentacoes
UNION ALL SELECT 'Inventarios', COUNT(*) FROM EstoqueInventarios
UNION ALL SELECT 'Fornecedores', COUNT(*) FROM Fornecedores;"
```

- Abra `https://estoque.jtecsistemas.com.br` e confira as listas (produtos, locais, lotes, movimentações, inventários, fornecedores).
- Faça um teste de escrita: edite um produto e salve; recarregue e confirme que a alteração persistiu.
- Logs: `sudo journalctl -u "$SERVICE" -n 200 --no-pager | grep -i MOCK`

---

## 7. Problemas comuns

| Sintoma | Causa provável | Correção |
|---|---|---|
| `Config:JwtSecretKey deve estar configurada com pelo menos 32 caracteres` | Chave ausente/curta | Ajuste `Config:JwtSecretKey` e reinicie o serviço |
| `A network-related or instance-specific error` | SQL inacessível, firewall ou credenciais | Teste `sqlcmd`; libere a porta 1433 para a VPS |
| Login não autentica | `FirebaseConfig` ausente/incorreto | Inclua o bloco no `appsettings.Production.json` |
| Dados antigos após o seed | Cache SQLite local | Remova `APP_DIR/Logs/jt_local_cache.db3*` e reinicie |
| `Invalid object name 'Documentos'` | Schema incompleto | Rode `sudo /usr/local/bin/seed-estoque.sh` (migra antes) |
| HTTP 502 no Nginx | Serviço parado/porta errada | `systemctl status jt_sistemas_estoque` e `ss -lntp \| grep 5004` |

---

## 8. Observações para próximos deploys

- `rsync --delete` preserva `appsettings.json`, `appsettings.Production.json`, `*.db*`, `uploads/`, `logs/` e `keys/`.
- O script em `/usr/local/bin/seed-estoque.sh` não é sobrescrito pelo deploy.
- O reset remove os registros mock e também movimentações/lotes desses SKUs, inclusive lançamentos manuais. Rode-o apenas quando quiser restaurar a demo.
