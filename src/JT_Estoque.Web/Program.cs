using JT_Aplicacao.Config;
using JT_Classes.Acessos;
using JT_Classes.CRM;
using JT_Estoque.Core.Data;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.IATools;
using JT_Estoque.Core.Services;
using JT_Estoque.Web.Components;
using JT_Estoque.Web.Providers;
using JT_Estoque.Web.Services;
using JT_Servicos.CRM;
using JT_Servicos.IA.Tools;
using JT_UserInterface.Shared;
using JT_UserInterface.Shared.Layout;
using JT_UserInterface.Shared.Services.Interfaces.Comum;
using System.Globalization;
using Microsoft.AspNetCore.HttpOverrides;

// Configura Cultura padrão pt-BR (moeda R$, datas, etc.)
var culturaPtBr = new CultureInfo("pt-BR");
CultureInfo.DefaultThreadCurrentCulture = culturaPtBr;
CultureInfo.DefaultThreadCurrentUICulture = culturaPtBr;

var builder = WebApplication.CreateBuilder(args);

// 1. Garante que o assembly JT_Estoque.Core está carregado no AppDomain para o DbSchemaMigrator e RegrasDiscoveryService
_ = typeof(ProdutoEstoque).Assembly;

// 2. Inicializa a configuração Multi-Tenant do JT_Aplicacao
var configSection = builder.Configuration.GetSection("Config").Get<Config>();
if (configSection != null)
{
    Config.SetInstancia(configSection);
}
Config.Instancia.ClientId = "JT_Estoque";

// 2.1 CLI administrativa para migração/seed/reset do módulo Estoque
var executarMigracao = args.Any(a => a.Equals("--migrate-estoque", StringComparison.OrdinalIgnoreCase));
var executarSeed = args.Any(a => a.Equals("--seed-estoque", StringComparison.OrdinalIgnoreCase));
var executarReset = args.Any(a => a.Equals("--reset-estoque", StringComparison.OrdinalIgnoreCase));
var executarDryRun = args.Any(a => a.Equals("--dry-run", StringComparison.OrdinalIgnoreCase));

if (executarMigracao || executarSeed || executarReset)
{
    var connStringCli = builder.Configuration["Config:Clientes:JT_Estoque:StringConexao"]
                     ?? builder.Configuration["Config:Conexao:StringConexao"];

    if (string.IsNullOrWhiteSpace(connStringCli))
    {
        Console.Error.WriteLine("[MOCK][ERRO] String de conexão não configurada em Config:Clientes:JT_Estoque:StringConexao ou Config:Conexao:StringConexao.");
        Environment.ExitCode = 1;
        return;
    }

    try
    {
        Console.WriteLine("[MOCK] Execução administrativa do Estoque iniciada.");
        Console.WriteLine($"[MOCK] Ações: migrar={executarMigracao}, seed={executarSeed}, reset={executarReset}, dry-run={executarDryRun}");

        await EstoqueMockSeeder.ExecutarAsync(
            connStringCli,
            garantirSchema: executarMigracao || executarSeed,
            reset: executarReset,
            dryRun: executarDryRun,
            removerOrfaos: false,
            aplicarSeed: executarSeed);

        Console.WriteLine("[MOCK] Execução administrativa concluída.");
        return;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"[MOCK][ERRO] {ex.Message}");
        Environment.ExitCode = 1;
        return;
    }
}

// 2.2 Validação mínima para execução local (TipoConexao=REDE)
var modoApi = !string.Equals(Config.Instancia.Executor, "API", StringComparison.OrdinalIgnoreCase)
           && string.Equals(Config.Instancia.TipoConexao, "API", StringComparison.OrdinalIgnoreCase);

if (!modoApi && (string.IsNullOrWhiteSpace(Config.Instancia.JwtSecretKey) || Config.Instancia.JwtSecretKey.Length < 32))
{
    throw new InvalidOperationException(
        "Config:JwtSecretKey deve estar configurada com pelo menos 32 caracteres para execução local (TipoConexao=REDE).");
}

// 3. Inicializa e registra o FirebaseConfig
builder.Services.Configure<FirebaseConfig>(builder.Configuration.GetSection("Config:FirebaseConfig"));
var secaoFConfig = builder.Configuration.GetSection("Config:FirebaseConfig").Get<FirebaseConfig>();
if (secaoFConfig != null)
{
    FirebaseConfig.SetInstancia(secaoFConfig);
    builder.Services.AddSingleton(FirebaseConfig.Instancia);
}

// 4. Configuração de cabeçalhos de proxy reverso (Nginx / Linux)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// 5. Configura os componentes Blazor no modo Servidor Interativo
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 6. Registra armazenamento seguro na Web e interface compartilhada
builder.Services.AddScoped<ISecureStorageService, WebSecureStorageService>();
builder.Services.AddScoped<IRequestConfigProvider, EstoqueRequestConfigProvider>();
builder.Services.AddScoped<JT_Servicos.Comum.SchemaService>();
builder.Services.AddJTUserInterface();

// 7. Configuração do HttpClient padrão com cabeçalho X-Client-Config (Multi-Tenant)
builder.Services.AddHttpClient();
builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = factory.CreateClient(string.Empty);

    var baseUrl = builder.Configuration["Config:Conexao:BaseApiUrl"]
               ?? builder.Configuration["Config:FirebaseConfig:BaseApiUrl"]
               ?? "https://api.jtecsistemas.com.br";
    httpClient.BaseAddress = new Uri(baseUrl);

    var configPayload = new ClientConfigPayload { ClientId = "JT_Estoque" };
    if (Config.Instancia != null)
    {
        Config.Instancia.ClientId = configPayload.ClientId;
    }

    var json = System.Text.Json.JsonSerializer.Serialize(configPayload);
    var encoded = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));
    if (!httpClient.DefaultRequestHeaders.Contains("X-Client-Config"))
    {
        httpClient.DefaultRequestHeaders.Add("X-Client-Config", encoded);
    }

    var currentUser = sp.GetService<CurrentUserState>();
    if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.IdToken) && httpClient.DefaultRequestHeaders.Authorization == null)
    {
        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", currentUser.IdToken);
    }

    return httpClient;
});

// 8. Registra os Serviços de Domínio do Estoque (App Client)
builder.Services.AddScoped<LocalEstoqueService>();
builder.Services.AddScoped<ProdutoEstoqueService>();
builder.Services.AddScoped<LoteEstoqueService>();
builder.Services.AddScoped<MovimentacaoEstoqueService>();
builder.Services.AddScoped<InventarioEstoqueService>();
builder.Services.AddScoped<FornecedorService>();

// 9. Registra os Providers de Menu, Categorias e Tools de IA do Estoque
builder.Services.AddScoped<IMenuProvider, EstoqueMenuProvider>();
builder.Services.AddScoped<IModuloMenuProvider, EstoqueMenuProvider>();
builder.Services.AddScoped<IModuloCategoriaProvider, EstoqueCategoriaProvider>();
builder.Services.AddScoped<ITutorialProvider, EstoqueTutorialProvider>();

builder.Services.AddScoped<IAToolBase, ConsultarRupturaEstoqueTool>();
builder.Services.AddScoped<IAToolBase, ConsultarKardexProdutoTool>();

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(MainLayout).Assembly
    );

// 10. Execução de Migração de Schema e Mock Data em Desenvolvimento
if (app.Environment.IsDevelopment())
{
    var connString = builder.Configuration["Config:Clientes:JT_Estoque:StringConexao"]
                  ?? builder.Configuration["Config:Conexao:StringConexao"];
    if (!string.IsNullOrWhiteSpace(connString))
    {
        try
        {
            await EstoqueMockSeeder.ExecutarAsync(connString, garantirSchema: true);
            Console.WriteLine("[MOCK] Dados de teste do Estoque carregados com sucesso.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[MOCK][AVISO] Falha ao executar mock data na inicialização: {ex.Message}");
        }
    }
}

app.Run();
