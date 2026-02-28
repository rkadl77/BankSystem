using Microsoft.EntityFrameworkCore;
using BankSystem.Credit.Data;
using BankSystem.Credit.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<CreditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICreditTariffService, CreditTariffService>();
builder.Services.AddScoped<ICreditService, CreditService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();
app.Run();