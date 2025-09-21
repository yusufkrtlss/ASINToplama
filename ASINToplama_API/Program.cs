using ASINToplama_BusinessLayer.Abstract;
using ASINToplama_BusinessLayer.Concrete;
using ASINToplama_BusinessLayer.Mapping;
using ASINToplama_DataAccessLayer.Abstract;
using ASINToplama_DataAccessLayer.EntityFramework.Concrete;
using ASINToplama_DataAccessLayer.EntityFramework.Context;
using ASINToplama_DataAccessLayer.Repository;
using ASINToplama_EntityLayer.Concrete;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default"),
        sql => sql.EnableRetryOnFailure()));

builder.Services.AddHttpClient<IAmazonSearchService, AmazonSearchService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
    if (!client.DefaultRequestHeaders.Contains("User-Agent"))
    {
        client.DefaultRequestHeaders.TryAddWithoutValidation(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Safari/537.36");
    }
})
.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
{
    AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
});

// Generic repo + UoW (gerekiyorsa spesifik repo’larý da ayrýca eklersin)
builder.Services.AddScoped<IGenericRepository<User>, GenericRepository<User>>();
builder.Services.AddScoped<IGenericRepository<Subscription>, GenericRepository<Subscription>>();
builder.Services.AddScoped<IGenericRepository<Payment>, GenericRepository<Payment>>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();

builder.Services.AddAutoMapper(cfg =>
{
    cfg.AddProfile<UserMappingProfile>();
});



builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<ILicenseGuard, LicenseGuard>();
builder.Services.AddScoped<IAuthService, AuthService>();
// JWT
var jwt = builder.Configuration.GetSection("Jwt");
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SigningKey"]!));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = signingKey,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("RequireAdmin", p => p.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
