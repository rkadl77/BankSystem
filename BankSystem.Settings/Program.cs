using BankSystem.Settings.Data;
using BankSystem.Settings.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<SettingsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("SettingsConnection")));

builder.Services.AddScoped<ISettingsService, SettingsService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<SettingsContext>();
    dbContext.Database.Migrate();
}

app.Run();
