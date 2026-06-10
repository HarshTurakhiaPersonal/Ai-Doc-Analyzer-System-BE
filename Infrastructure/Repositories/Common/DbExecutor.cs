using Infrastructure.Interfaces.ICommon;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Repositories.Common;

public sealed class DbExecutor(ILogger<DbExecutor> logger) : IDbExecutor
{
    public async Task ExecuteAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            await operation();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database operation failed");
            throw;
        }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        try
        {
            return await operation();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Database operation failed");
            throw;
        }
    }
}