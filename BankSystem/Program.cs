using BankSystem.Clients;
using BankSystem.Data;
using BankSystem.Services;
using BankSystem.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Net.WebSockets;
using System.Security.Cryptography;

var builder = WebApplication.CreateBuilder(args);

var rsa = RSA.Create();
rsa.ImportFromPem(@"
-----BEGIN RSA PUBLIC KEY-----
MIIBCgKCAQEAyuFEkRtySztyGn8j9Av0WAJlFDO/AQb9oeuGnotwPLvkdIqzFKir
dg2fXAwiOMDycqjI71gdsu5qrkP4JCgzY+qCGqE7wBhKDxJZqGtUZMt+7pXOUhTb
9+X9QGM/YBFoST89HJY8nwDX2jyukUyp7Ptge4l7FNL5j2dgl6WNpDC59JMFeeAq
+r8irSKCCX9c0atsuLmD50XzmhzNqf5DsxXVVbp9hrynhVqGLXu07BsLxx4iBbII
zxppJf4WUAoc5RThDf2OFnglsHo9eLo2PJKva7TJBR3hhZ68x+bB4uyapBYw1Fmr
YX3Du2UI53PxLA5AIIMJhGEzP7+vUd8pUQIDAQAB
-----END RSA PUBLIC KEY-----
");

var signingKey = new RsaSecurityKey(rsa);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5004",
            ValidateAudience = true,
            ValidAudience = "bank.api",
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey
        };

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("=== TOKEN CLAIMS ===");
                foreach (var claim in context.Principal.Claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
                Console.WriteLine("JWT Token validated successfully");
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

builder.Services.AddHttpClient<UserServiceClient>();
builder.Services.AddHttpClient<UserRoleService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "BankSystem API", Version = "v1" });

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

app.UseWebSockets();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();