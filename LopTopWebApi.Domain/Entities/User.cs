using System.ComponentModel.DataAnnotations;

namespace LopTopWebApi.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; private set; } = Guid.NewGuid();
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string Username { get; private set; } = string.Empty;
        public string Email { get; private set; } = string.Empty;
        public string PasswordHash { get; private set; } = string.Empty;
        public DateTime RegistrationDate { get; private set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Product> AddedProducts { get; private set; } = new List<Product>();
        public virtual ICollection<Review> Reviews { get; private set; } = new List<Review>();

        // Factory method
        public static User Create(
            string firstName,
            string lastName,
            string username,
            string email,
            string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email is required");

            return new User
            {
                FirstName = firstName?.Trim(),
                LastName = lastName?.Trim(),
                Username = username.Trim(),
                Email = email.Trim().ToLowerInvariant(),
                PasswordHash = passwordHash
            };
        }

        // Computed full name
        public string FullName => $"{FirstName} {LastName}".Trim();

        // For display (full name or username)
        public string DisplayName => string.IsNullOrWhiteSpace(FullName) ? Username : FullName;
    }
}