using Microsoft.EntityFrameworkCore;
using LopTopWebApi.Domain.Entities;
using LopTopWebApi.Domain.Interfaces;
using LaptopsApi.Infrastructure.Data;

namespace LaptopsApi.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;

        public ProductRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Products
            .AsNoTracking()
            .ToListAsync(ct);
        }
    }
}