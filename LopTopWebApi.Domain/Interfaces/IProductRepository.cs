using LopTopWebApi.Domain.Entities;

namespace LopTopWebApi.Domain.Interfaces
{
    public interface IProductRepository
    {
        Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    }
}