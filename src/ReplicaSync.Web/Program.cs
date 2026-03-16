using ReplicaSync.Core.Enums;
using ReplicaSync.Infrastructure.Data;
using ReplicaSync.Infrastructure.Extensions;
using ReplicaSync.Web.Components;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add Blazor services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure the sync config database
var configDbConnection = builder.Configuration.GetConnectionString("ConfigDatabase")
    ?? "Data Source=ReplicaSync.db";
var configDbTypeStr = builder.Configuration.GetValue<string>("ConfigDatabaseType") ?? "SqlServer";
var configDbType = Enum.Parse<DbmsType>(configDbTypeStr, ignoreCase: true);

builder.Services.AddReplicaSyncInfrastructure(configDbConnection, configDbType);

var app = builder.Build();

// Auto-create database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
