using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Domain.Entities
{
    public class Review
    {
        public Guid ReviewId { get; private set; } = Guid.NewGuid();
        public Guid? ProductId { get; private set; }  // NULL for replies
        public Guid UserId { get; private set; }

        // NULL for root reviews
        public Guid? ParentReviewId { get; private set; }

        public int? Rating { get; private set; }  // NULL for replies
        public string? Comment { get; private set; }
        public DateTime ReviewDate { get; private set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual Product? Product { get; private set; }
        public virtual User User { get; private set; } = null!;
        public virtual Review? ParentReview { get; private set; }
        public virtual ICollection<Review> ChildReviews { get; private set; } = new List<Review>();

        // Factory methods
        public static Review CreateRootReview(Guid productId, Guid userId, int rating, string? comment = null)
        {
            if (productId == Guid.Empty)
                throw new ArgumentException("ProductId is required for root review");
            if (rating < 1 || rating > 5)
                throw new ArgumentException("Rating must be between 1 and 5");

            return new Review
            {
                ProductId = productId,
                UserId = userId,
                ParentReviewId = null,  // Root review
                Rating = rating,
                Comment = comment?.Trim()
            };
        }

        public static Review CreateReply(Guid parentReviewId, Guid userId, string? comment = null)
        {
            if (parentReviewId == Guid.Empty)
                throw new ArgumentException("Cannot reply directly to root level");

            return new Review
            {
                UserId = userId,
                ParentReviewId = parentReviewId,
                Comment = comment?.Trim()
                // ProductId and Rating remain null for replies
            };
        }

        // Checks
        public bool IsRootReview => ParentReviewId == null;
        public bool IsReply => ParentReviewId != null;
    }
}