using Boekenlijst.Components;
using Boekenlijst.Models;
using Boekenlijst.Services;
using Microsoft.EntityFrameworkCore;

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
    processor.ProcessBoekenLijstFile(boekenlijstPath).Wait();
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
