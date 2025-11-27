using LopTopWebApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LaptopsApi.Infrastructure.Helpers
{
    public static class ReviewQueries
    {
        public static IQueryable<Review> RootRatings(this IQueryable<Review> query)
        {
            return query.Where(r =>
                r.ParentReviewId == null &&
                !r.IsDeleted &&
                r.Rating != null);
        }

        public static IQueryable<double> RootRatingValuesForProduct(this IQueryable<Review> query, Guid productId)
        {
            return query
                .RootRatings()
                .Where(r => r.ProductId == productId)
                .Select(r => (double)r.Rating!);
        }
    }
}
