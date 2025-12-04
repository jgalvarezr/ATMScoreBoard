using ATMScoreBoard.Shared;
using ATMScoreBoard.Web.Components;
using ATMScoreBoard.Web.Hubs;
using ATMScoreBoard.Web.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();

//builder.Services.AddHttpClient();
builder.Services.AddHttpClient("LocalApi", client =>
{
    // Aquí le decimos a HttpClient cuál es su dirección base.
    // Usamos la misma URL que la aplicación está usando para ejecutarse.
    // OJO: Asegúrate de que esta URL coincida con la de tu launchSettings.json
    client.BaseAddress = new Uri("https://localhost:7286");
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(connectionString));

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<JugadorService>();
builder.Services.AddScoped<PartidaService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddSingleton<ConfiguracionService>();

builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.MapHub<MarcadorHub>("/marcadorhub");

app.Run();
