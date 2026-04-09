using BankSystem.Credit.Clients;
using BankSystem.Credit.Data;
using BankSystem.Credit.Services;
using BankSystem.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = "http://localhost:5109";
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5109",
            ValidateAudience = true,
            ValidAudience = "bank.api",
            ValidateLifetime = true,
        };
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx => {
                Console.WriteLine($"JWT failed: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = ctx => {
                Console.WriteLine("JWT valid. Claims:");
                foreach (var c in ctx.Principal.Claims)
                    Console.WriteLine($"  {c.Type}: {c.Value}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddDbContext<CreditDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICreditTariffService, CreditTariffService>();
builder.Services.AddScoped<ICreditService, CreditService>();

builder.Services.AddHttpClient<CoreServiceClient>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CreditService API", Version = "v1", Description = "Credit Management API" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.SchemaFilter<BankSystem.Credit.SwaggerExamples>();

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' [space] and then your token"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add monitoring and tracing
builder.Services.AddBankSystemMonitoring();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CreditDbContext>();
    dbContext.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tracing middleware MUST be first in pipeline
app.UseBankSystemTracing();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Chaos engineering - simulates random failures
app.UseChaosEngineering();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();
app.Run();