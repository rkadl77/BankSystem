using BankSystem.Auth.Config;
using BankSystem.Auth.Data;
using BankSystem.Auth.Services;
using BankSystem.Common;
using Duende.IdentityServer;
using Duende.IdentityServer.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IUserProfileService, UserProfileService>(client => {
    client.BaseAddress = new Uri("http://localhost:5276/");
})
.AddPolicyHandler(PollyPolicies.GetRetryPolicy("BankSystem.Users"))
.AddPolicyHandler(PollyPolicies.GetCircuitBreakerPolicy("BankSystem.Users"));
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IAuthStorageService, AuthStorageService>();

builder.Services.AddIdentityServer()
    .AddConfigurationStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName));
    })
    .AddOperationalStore(options =>
    {
        options.ConfigureDbContext = b =>
            b.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"),
                sql => sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName));
    })
    .AddDeveloperSigningCredential()
    .AddResourceOwnerValidator<ResourceOwnerPasswordValidator>()
    .AddProfileService<ProfileService>()
    .AddInMemoryClients(IdentityServerConfig.GetClients())
    .AddInMemoryApiResources(IdentityServerConfig.GetApiResources())
    .AddInMemoryApiScopes(IdentityServerConfig.GetApiScopes())
    .AddInMemoryIdentityResources(IdentityServerConfig.GetIdentityResources());

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
builder.Services.AddHttpClient();

// Add monitoring and tracing
builder.Services.AddBankSystemMonitoring();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
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
app.UseIdentityServer();
app.MapControllers();
app.Run();