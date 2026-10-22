# Scripts de Infraestrutura Linux (VPS) - JT_Estoque

Esta pasta contém os arquivos de configuração para hospedar o **JT_Estoque.Web** no servidor Linux:

1. **[nginx-jt_estoque.conf](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/scripts/linux/nginx-jt_estoque.conf)**:
   - Configuração do proxy reverso Nginx com suporte a WebSockets/SignalR e cabeçalhos de segurança para o Blazor InteractiveServer.
2. **[jt_sistemas_estoque.service](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/scripts/linux/jt_sistemas_estoque.service)**:
   - Definição do serviço Systemd (.NET 10) na porta interna `5004`.
3. **[deploy-estoque.sh](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/scripts/linux/deploy-estoque.sh)**:
   - Script para automatizar o deploy na VPS (parada do serviço, permissões e reload).
4. **[seed-estoque.sh](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/scripts/linux/seed-estoque.sh)**:
   - Migra o schema, limpa o cache SQLite, reseta e aplica os dados mock do Estoque.
   - Não é publicado pelo `dotnet publish`; instale em `/usr/local/bin` conforme o checklist.
5. **[appsettings.Production.example.json](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/scripts/linux/appsettings.Production.example.json)**:
   - Modelo para criar o `appsettings.Production.json` na VPS (modo `REDE`, JWT e migração).

Para o manual completo e resolução do certificado SSL (HTTPS / Certbot), consulte:
- **[docs/DEPLOY_VPS_NGINX_SSL.md](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/docs/DEPLOY_VPS_NGINX_SSL.md)**
- **[docs/CHECKLIST_IMPLANTACAO_VPS.md](file:///c:/Users/sirja/source/repos/mrjalves/JT_Estoque/docs/CHECKLIST_IMPLANTACAO_VPS.md)**
