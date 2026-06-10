using Application.Interfaces;
using Application.Services;
using Application.Services.AI;
using Application.Services.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Options;

public static class DependencyInjection
{
    public static IServiceCollection ServiceRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        //var apiKey = configuration["OpenAI:ApiKey"];
        services.AddHttpContextAccessor();

        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IDocumentParserService, DocumentParserService>();
        services.AddScoped<IDocumentProcessingService, DocumentProcessingService>();
        services.AddScoped<ITextChunkingService, TextChunkingService>();
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddHttpClient("Ollama", (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<OllamaOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
        });
        services.AddScoped<IEmbeddingService, OllamaEmbeddingService>();
        services.AddScoped<IOllamaChatService, ChatService>();
        services.AddScoped<IRagService, RagService>();
        services.AddScoped<ISummaryService, SummaryService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IRerankingService, RerankingService>();
        services.AddScoped<IChatSessionService, ChatSessionService>();
        services.AddScoped<IDocumentAuthorizationService, DocumentAuthorizationService>();
        return services;
    }
}