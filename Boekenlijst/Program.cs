using Boekenlijst.Components;
using Boekenlijst.Models;
using Boekenlijst.Services;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add DbContext for MySQL
builder.Services.AddDbContext<BoekenLijstContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// Add BoekenLijstFileProcessor service
builder.Services.AddScoped<BoekenLijstFileProcessor>();

var app = builder.Build();

// Seed the database from file
using (var scope = app.Services.CreateScope())
{
    var processor = scope.ServiceProvider.GetRequiredService<BoekenLijstFileProcessor>();
    var env = scope.ServiceProvider.GetRequiredService<IHostEnvironment>();
    var boekenlijstPath = Path.Combine(env.ContentRootPath, "Data", "BOEKENLIJST.txt");
    // Ensure database exists. If MySQL reports unknown database, create it and continue.
    var connString = builder.Configuration.GetConnectionString("DefaultConnection");
    try
    {
        processor.ProcessBoekenLijstFile(boekenlijstPath).Wait();
    }
    catch (AggregateException ae) when (ae.InnerException is MySqlException mysqlEx && mysqlEx.Message.Contains("Unknown database", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Database not found. Attempting to create database 'BoekenLijst' on server from connection string.");
        // Remove any database=...; part so we can connect to server
        var noDbConn = Regex.Replace(connString ?? string.Empty, "(?i)database=[^;]*;?", "");
        if (string.IsNullOrWhiteSpace(noDbConn))
        {
            Console.WriteLine("Unable to derive server connection string to create the database.");
            throw;
        }

        using (var conn = new MySqlConnection(noDbConn))
        {
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "CREATE DATABASE IF NOT EXISTS BoekenLijst CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";
            cmd.ExecuteNonQuery();
            Console.WriteLine("Database 'BoekenLijst' created or already exists.");
        }

        // Create tables according to the model (if not present)
        var options = new DbContextOptionsBuilder<BoekenLijstContext>()
            .UseMySql(connString, ServerVersion.AutoDetect(connString))
            .Options;
        using (var createCtx = new BoekenLijstContext(options))
        {
            createCtx.Database.EnsureCreated();
            Console.WriteLine("Ensured database schema is created.");
        }

        // Retry processing after creating the database and tables using a fresh DbContext
        var freshProcessor = scope.ServiceProvider.GetRequiredService<BoekenLijstFileProcessor>();
        freshProcessor.ProcessBoekenLijstFile(boekenlijstPath).Wait();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
