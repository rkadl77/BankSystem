using Microsoft.EntityFrameworkCore;
using BankSystem.Auth.Data;
using BankSystem.Auth.Services;
using BankSystem.Auth.Config;
using Duende.IdentityServer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddHttpClient<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IAuthService, AuthService>();

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
builder.Services.AddScoped<IAuthStorageService, AuthStorageService>();

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

app.UseCors("AllowAll");
app.UseIdentityServer();
app.MapControllers();
app.Run();