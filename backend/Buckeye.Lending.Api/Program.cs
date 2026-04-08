using System.Text;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Middleware;
using Buckeye.Lending.Api.Models;
using Buckeye.Lending.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS — allow the React dev server to call our API
// Without this, the browser blocks cross-origin requests from localhost:5173 → localhost:5000
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origin = builder.Configuration["Frontend:Origin"] ?? "http://localhost:5173";
        policy.WithOrigins(origin)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Prevent circular reference errors from navigation properties
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// FluentValidation — register all validators from this assembly
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// AutoMapper - register all mappers from this assembly
builder.Services.AddAutoMapper(typeof(Program));

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Problem Details support (RFC 7807)
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = context =>
    {
        // Add machine name for diagnostics (only in development)
        if (builder.Environment.IsDevelopment())
        {
            context.ProblemDetails.Extensions["machine"] = Environment.MachineName;
        }
    };
});

// Add global exception handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// EF Core — InMemory provider
builder.Services.AddDbContext<LendingContext>(options =>
    options.UseInMemoryDatabase("BuckeyeLending"));

// ASP.NET Core Identity
builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<LendingContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

// JWT Bearer Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]
                    ?? throw new InvalidOperationException("JWT signing key is not configured. Use dotnet user-secrets.")))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddScoped<TokenService>();

var app = builder.Build();

// Initialize on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LendingContext>();
    context.Database.EnsureCreated();

    // Seed roles and users
    await DbSeeder.SeedAsync(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Buckeye Lending API v1");
    });
}

// Use exception handler middleware
app.UseExceptionHandler();

// Security headers on every response
app.UseMiddleware<SecurityHeadersMiddleware>();

// Enable CORS — must be called before MapControllers
app.UseCors();

// Only redirect to HTTPS in production.
// In development, HTTPS redirect causes preflight (OPTIONS) requests to receive a 307,
// and browsers cannot follow redirects for CORS preflights — resulting in a 403 block.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

// Make the implicit Program class accessible to WebApplicationFactory<Program>
public partial class Program { }
