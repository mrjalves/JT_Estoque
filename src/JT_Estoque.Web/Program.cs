using JT_Aplicacao.Config;
using JT_Classes.CRM;
using JT_Estoque.Core.Entities;
using JT_Estoque.Core.IATools;
using JT_Estoque.Core.Services;
using JT_Estoque.Web.Components;
using JT_Estoque.Web.Providers;
using JT_Servicos.CRM;
using JT_Servicos.IA.Tools;
using JT_UserInterface.Shared;
using JT_UserInterface.Shared.Layout;
using JT_UserInterface.Shared.Services.Interfaces.Comum;

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

// 3. Configura os componentes Blazor no modo Servidor Interativo
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// 4. Registra todos os serviços e componentes compartilhados da DLL JT_UserInterface.Shared
builder.Services.AddJTUserInterface();

// 5. Configuração do HttpClient padrão
builder.Services.AddHttpClient(string.Empty, client =>
{
    var baseUrl = builder.Configuration["Config:Conexao:BaseApiUrl"] ?? "https://localhost:7166";
    client.BaseAddress = new Uri(baseUrl);
});

// 6. Registra os Serviços de Domínio do Estoque (App Client)
builder.Services.AddScoped<LocalEstoqueService>();
builder.Services.AddScoped<ProdutoEstoqueService>();
builder.Services.AddScoped<LoteEstoqueService>();
builder.Services.AddScoped<MovimentacaoEstoqueService>();
builder.Services.AddScoped<InventarioEstoqueService>();
builder.Services.AddScoped<FornecedorService>();

// 7. Registra os Providers de Menu, Categorias e Tools de IA do Estoque
builder.Services.AddScoped<IMenuProvider, EstoqueMenuProvider>();
builder.Services.AddScoped<IModuloMenuProvider, EstoqueMenuProvider>();
builder.Services.AddScoped<IModuloCategoriaProvider, EstoqueCategoriaProvider>();

builder.Services.AddScoped<IAToolBase, ConsultarRupturaEstoqueTool>();
builder.Services.AddScoped<IAToolBase, ConsultarKardexProdutoTool>();

var app = builder.Build();

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
        typeof(MainLayout).Assembly,
        typeof(ProdutoEstoque).Assembly
    );

app.Run();
