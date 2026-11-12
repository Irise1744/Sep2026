using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Builder;

var appBuilder = WebApplication.CreateBuilder(args);

appBuilder.Services.AddControllers();
appBuilder.Services.AddEndpointsApiExplorer();
appBuilder.Services.AddSwaggerGen();

var sqlBuilder = new SqlConnectionStringBuilder
{
    DataSource = "localhost,1433",
    InitialCatalog = "Sep2026.SupermarketPos",
    UserID = "sa",
    Password = "<your SQL Server password>",
    IntegratedSecurity = false,
    Encrypt = true,
    TrustServerCertificate = true
};

var connectionString = sqlBuilder.ConnectionString;

appBuilder.Services.AddScoped<IDbConnection>(
    _ => new SqlConnection(connectionString));

var app = appBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();