using System.Diagnostics;
using Blazored.LocalStorage;
using CodeHealthHub.Components;
using CodeHealthHub.Data;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddBlazoredLocalStorage();

// Configure DbContext based on connection string or provider setting
string? connectionString;
if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("CodeHealthHubDB");
}
else if (Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") == null)
{
    Debug.WriteLine("No connection string given.");
    throw new InvalidOperationException("Connection string required for Database connection. Set DATABASE_CONNECTION_STRING environment variable.");
}
else
{
    connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");
}

// Choose context builder based on connection string, default is for SQLite
builder.Services.AddDbContextFactory<AppDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseSqlite(connectionString);
    }
    else
    {
        options.UseSqlServer(connectionString);
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
        client.BaseAddress = new Uri("http://localhost:5030");
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
    // Cannot use HSTS in development because it can cause issues with self-signed certificates and local testing
    app.UseHsts();
    app.UseMigrationsEndPoint();

    // Apply migrations at startup
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseRouting();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();  // This maps API controller routes
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
