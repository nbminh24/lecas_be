using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

namespace be_lecas.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Avatar { get; set; }
        
        // OAuth fields
        public string? GoogleId { get; set; }
        public string? GoogleAccessToken { get; set; }
        public string? GoogleRefreshToken { get; set; }
        public DateTime? GoogleTokenExpiry { get; set; }
        
        // Authentication fields
        public string? PasswordHash { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiryTime { get; set; }
        
        // Email verification
        public bool IsEmailVerified { get; set; } = false;
        public string? EmailVerificationToken { get; set; }
        public DateTime? EmailVerificationExpiry { get; set; }
        
        // Account status
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public List<string> Roles { get; set; } = new List<string> { "user" };

        public List<Address> Addresses { get; set; } = new List<Address>();

        [BsonIgnore]
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        [BsonIgnore]
        public bool IsOAuthUser => !string.IsNullOrEmpty(GoogleId);
        
        [BsonIgnore]
        public bool CanLoginWithPassword => !string.IsNullOrEmpty(PasswordHash);
    }

    public class Address
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string AddressLine { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string? Note { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}

