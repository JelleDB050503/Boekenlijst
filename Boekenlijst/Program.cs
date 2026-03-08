using Boekenlijst.Models;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

var railwayPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(railwayPort))
{
    builder.WebHost.UseUrls($"http://*:{railwayPort}");
}

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = ResolveConnectionString(builder.Configuration);

// Add DbContext for MySQL
builder.Services.AddDbContext<BoekenLijstContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();

app.Run();

static string ResolveConnectionString(IConfiguration configuration)
{
    var direct = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(direct))
    {
        return direct;
    }

    var mysqlUrl = Environment.GetEnvironmentVariable("MYSQL_URL")
        ?? Environment.GetEnvironmentVariable("DATABASE_URL")
        ?? Environment.GetEnvironmentVariable("MYSQLPUBLIC_URL");

    if (!string.IsNullOrWhiteSpace(mysqlUrl))
    {
        return ConvertMySqlUrlToConnectionString(mysqlUrl);
    }

    throw new InvalidOperationException(
        "No database connection string found. Configure ConnectionStrings__DefaultConnection or MYSQL_URL in Railway environment variables.");
}

static string ConvertMySqlUrlToConnectionString(string mysqlUrl)
{
    var uri = new Uri(mysqlUrl);
    var userInfo = uri.UserInfo.Split(':', 2);
    var user = Uri.UnescapeDataString(userInfo[0]);
    var password = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
    var database = uri.AbsolutePath.TrimStart('/');

    var builder = new MySqlConnectionStringBuilder
    {
        Server = uri.Host,
        Port = (uint)(uri.Port > 0 ? uri.Port : 3306),
        UserID = user,
        Password = password,
        Database = database,
        SslMode = MySqlSslMode.Preferred
    };

    return builder.ConnectionString;
}
