using LaptopsApi.Infrastructure.Data;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace LaptopsApi.Infrastructure.Repositories
{
    public sealed class UserRepository : IUserRepository
    {
        private readonly AppDbContext _ctx;
        public UserRepository(AppDbContext ctx) => _ctx = ctx;

        public async Task AddAsync(User user, CancellationToken ct)
        {
            await _ctx.Users.AddAsync(user, ct);
        }

        public Task<bool> EmailExistsAsync(string email, CancellationToken ct) =>
           _ctx.Users.AsNoTracking().AnyAsync(u => u.Email == email, ct);

        public Task<bool> UsernameExistsAsync(string username, CancellationToken ct) =>
             _ctx.Users.AsNoTracking().AnyAsync(u => u.Username == username, ct);

        public Task<int> SaveChangesAsync(CancellationToken ct) => _ctx.SaveChangesAsync(ct);

        public Task<User?> FindByUsernameOrEmailAsync(string login, CancellationToken ct) => 
            _ctx.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Username == login || u.Email == login, ct);

    }
}
