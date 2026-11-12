using System.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Data.SqlClient;

var connectionStringBuilder = new SqlConnectionStringBuilder
{
    DataSource = "localhost",
    InitialCatalog = "Sep2026.SupermarketPos", // Must match the database you created
    UserID = "sa",
    Password = "sasa@123",
    IntegratedSecurity = false, // Use SQL Server login
    Encrypt = true,
    TrustServerCertificate = true // For local development
};

var webBuilder = WebApplication.CreateBuilder(args);

webBuilder.Services.AddControllers();
webBuilder.Services.AddEndpointsApiExplorer();
webBuilder.Services.AddSwaggerGen();
webBuilder.Services.AddScoped<IDbConnection>(
    _ => new SqlConnection(connectionStringBuilder.ConnectionString));

var app = webBuilder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();
