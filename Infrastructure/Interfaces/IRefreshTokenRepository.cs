using Application.Interfaces.IRepositories;
using Domain.Entities;

namespace Infrastructure.Interfaces;

public interface IRefreshTokenRepository : IGenericRepository<RefreshToken>
{
    Task<RefreshToken?> GetByTokenHashAsync(string token, CancellationToken cancellationToken);

    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}