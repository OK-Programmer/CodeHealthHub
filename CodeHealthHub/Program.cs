using Blazored.LocalStorage;
using CodeHealthHub.Components;
using CodeHealthHub.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazoredLocalStorage();

// Configure DbContext based on connection string or provider setting
var connectionString = builder.Configuration.GetConnectionString("CodeHealthHubDB");
var databaseProvider = builder.Configuration["DatabaseProvider"] ?? "sqlite";

// For SQLite ephemeral mode, ensure directory exists
if (databaseProvider.ToLower() == "sqlite" && connectionString?.Contains("Data Source=") == true)
{
    // Extract the path and ensure directory exists
    var dataSource = connectionString.Replace("Data Source=", "");
    var directory = Path.GetDirectoryName(dataSource);
    
    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
    {
        Directory.CreateDirectory(directory);
    }
}

builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    switch (databaseProvider.ToLower())
    {
        case "sqlserver":
            if (string.IsNullOrEmpty(connectionString))
                throw new InvalidOperationException("Connection string required for SQL Server. Set DATABASE_CONNECTION_STRING environment variable.");
            options.UseSqlServer(connectionString);
            break;
        default:
            options.UseSqlite(connectionString ?? "Data Source=/app/Data/CodeHealthHub.db");
            break;
    }
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();  // Add this to enable API controllers

// Add HttpContextAccessor to get current request URL
builder.Services.AddHttpContextAccessor();

// Configure HttpClient with dynamic base address
builder.Services.AddHttpClient("LocalApi", (sp, client) =>
{
    if (Environment.GetEnvironmentVariable("IS_CONTAINER") == "true")
    {
        client.BaseAddress = new Uri("http://localhost:80");
    }
    else
    {
        client.BaseAddress = new Uri("http://localhost:5000");
    }
});

// Configure data protection to persist keys
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"))
    .SetApplicationName("CodeHealthHub");
    

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
