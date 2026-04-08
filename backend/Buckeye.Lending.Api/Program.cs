using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Buckeye.Lending.Api.Data;
using Buckeye.Lending.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// CORS — allow the React dev server to call our API
// Without this, the browser blocks cross-origin requests from localhost:5173 → localhost:5000
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")   // Vite dev server
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

var app = builder.Build();

// Initialize on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<LendingContext>();
    context.Database.EnsureCreated();
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

// Enable CORS — must be called before MapControllers
app.UseCors();

// Only redirect to HTTPS in production.
// In development, HTTPS redirect causes preflight (OPTIONS) requests to receive a 307,
// and browsers cannot follow redirects for CORS preflights — resulting in a 403 block.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

app.Run();
