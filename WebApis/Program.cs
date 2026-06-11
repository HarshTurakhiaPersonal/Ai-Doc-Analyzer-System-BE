using Infrastructure;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using WebApi.Endpoints;
using WebApi.Extensions;
using Microsoft.OpenApi.Models;
using WebApis.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:4200")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AI Document Analyzer API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();

builder.Services.ServiceRegistration(builder.Configuration);
builder.Services.RepositoryRegistration(builder.Configuration);

builder.Services.AddAuth(builder.Configuration);

builder.Services.AddHttpClient(
    "Ollama",
    client =>
    {
        client.BaseAddress = new Uri("http://localhost:11434");
    });

builder.Services
    .AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Postgres")!, name: "postgres")
    .AddCheck<OllamaHealthCheck>("ollama")
    .AddCheck<StorageHealthCheck>("storage");

var app = builder.Build();

app.UseGlobalExceptionHandling();
app.UseHttpsRedirection();

app.UseRouting();
app.UseCors("AngularPolicy");

app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
        "/swagger/v1/swagger.json",
        "AI Document Analyzer API");
    });
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    await db.Database.MigrateAsync();
    await db.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS vector;");

    await RoleSeeder.SeedAsync(scope.ServiceProvider);
}
app.MapAuthEndpoints();
app.MapDocumentEndpoints();
app.MapChatEndpoints();

app.Use(async (context, next) =>
{
    string correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers["X-Correlation-Id"] = correlationId;
    await next();
});

app.Run();