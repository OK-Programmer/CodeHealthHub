using Blazored.LocalStorage;
using CodeHealthHub.Components;
using CodeHealthHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazoredLocalStorage();

// builder.Services.AddDbContextFactory<AppDbContext>(options =>
//     options.UseSqlite(builder.Configuration.GetConnectionString("CodeHealthHubDB")));

// Configure DbContext based on connection string or provider setting
var connectionString = builder.Configuration.GetConnectionString("CodeHealthHubDB");
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "sqlite";

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    switch (databaseProvider.ToLower())
    {
        case "sqlserver":
            options.UseSqlServer(connectionString);
            break;
        default:
            options.UseSqlite(connectionString);
            break;
    }
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();  // Add this to enable API controllers

var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5030";
apiBaseUrl = apiBaseUrl.TrimEnd('/');

builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseRouting();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();  // This maps API controller routes
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.Run();
