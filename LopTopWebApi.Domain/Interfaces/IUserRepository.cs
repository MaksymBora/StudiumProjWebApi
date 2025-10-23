using LopTopWebApi.Domain.Entities;

namespace LopTopWebApi.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email, CancellationToken ct);
        Task<bool> UsernameExistsAsync(string username, CancellationToken ct);
        Task AddAsync(User user, CancellationToken ct);
        Task<int> SaveChangesAsync(CancellationToken ct);
        Task<User?> FindByUsernameOrEmailAsync(string login, CancellationToken ct);
    }
}
