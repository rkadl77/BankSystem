using BankSystem.Clients;
using BankSystem.Data;
using BankSystem.Services;
using BankSystem.Models;
using BankSystem.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;

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

builder.Services.AddDbContext<BankSystemContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IMasterAccountService, MasterAccountService>();
builder.Services.AddSingleton<WebSocketHandler>();
builder.Services.AddScoped<ICurrencyExchangeService, CurrencyExchangeService>();

// Add Polly policies to all HttpClient registrations
builder.Services.AddHttpClient<IUserServiceClient, UserServiceClient>()
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy("BankSystem.Users"))
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("BankSystem.Users"));

builder.Services.AddHttpClient<CurrencyExchangeService>()
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy("CurrencyExchange"))
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("CurrencyExchange"));

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<UserRoleService>()
    .AddPolicyHandler(PollyPolicies.GetRetryPolicy("UserRole"))
    .AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("UserRole"));
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankSystem API", Version = "v1", Description = "Core Banking Operations API" });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.SchemaFilter<BankSystem.SwaggerExamples>();

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
    var dbContext = scope.ServiceProvider.GetRequiredService<BankSystemContext>();
    dbContext.Database.Migrate();

    var masterAccountId = Guid.Parse(builder.Configuration["MasterAccount:Id"]);
    var masterAccount = dbContext.Accounts.Find(masterAccountId);

    if (masterAccount == null)
    {
        masterAccount = new Account
        {
            Id = masterAccountId,
            AccountNumber = builder.Configuration["MasterAccount:AccountNumber"],
            ClientId = Guid.Empty,
            Balance = decimal.Parse(builder.Configuration["MasterAccount:InitialBalance"]),
            Currency = builder.Configuration["MasterAccount:Currency"],
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        dbContext.Accounts.Add(masterAccount);
        dbContext.SaveChanges();

        Console.WriteLine($"Master account created: {masterAccount.AccountNumber}, Balance: {masterAccount.Balance} {masterAccount.Currency}");
    }
    else
    {
        Console.WriteLine($"Master account already exists: {masterAccount.AccountNumber}, Balance: {masterAccount.Balance} {masterAccount.Currency}");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Tracing middleware MUST be first in pipeline
app.UseBankSystemTracing();
app.UseWebSockets();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

// Chaos engineering - simulates random failures
app.UseChaosEngineering();

// Health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

app.MapControllers();
app.Run();