using Application.Interfaces.IRepositories;
using Domain.Entities;

namespace Infrastructure.Configurations;

public interface IChatMessageRepository : IGenericRepository<ChatMessage>
{
    Task<List<ChatMessage>>
       GetRecentMessagesAsync(
           Guid sessionId,
           int count,
           CancellationToken cancellationToken = default);

    Task<List<ChatMessage>>
        GetBySessionIdAsync(
            Guid sessionId,
            CancellationToken cancellationToken = default);

}
