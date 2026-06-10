using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Documents> Documents { get; set; } = [];
}