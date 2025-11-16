using CodeHealthHub.Components;
using CodeHealthHub.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContextFactory<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("AppDBContext") ?? throw new InvalidOperationException("Connection string 'AppDBContext' not found.")));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddControllers();  // Add this to enable API controllers
builder.Services.AddHttpClient("LocalApi", client =>
{
   client.BaseAddress = new Uri(builder.Configuration["AppSettings:BaseUrl"] ?? "http://localhost:5030/");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

// Add these lines to map controllers
app.UseRouting();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapControllers();  // This maps API controller routes
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
