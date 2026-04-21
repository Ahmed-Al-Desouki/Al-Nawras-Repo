using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class User
    {
        public int Id { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }   // nullable — Google users have no password
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string? GoogleId { get; private set; }      // null for normal users
        public string? ProfilePictureUrl { get; private set; }
        public int RoleId { get; private set; }
        public Guid? ClientId { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Role Role { get; private set; }
        public Client Client { get; private set; }
        public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

        private User() { }

        // Normal registration
        public User(string email, string passwordHash, string firstName,
            string lastName, int roleId, Guid? clientId = null)
        {
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            RoleId = roleId;
            ClientId = clientId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        // Google registration
        public User(string email, string firstName, string lastName,
            string googleId, string? profilePictureUrl, int roleId, Guid? clientId = null)
        {
            Email = email;
            PasswordHash = string.Empty;   // no password for Google users
            FirstName = firstName;
            LastName = lastName;
            GoogleId = googleId;
            ProfilePictureUrl = profilePictureUrl;
            RoleId = roleId;
            ClientId = clientId;
            IsActive = true;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void RecordLogin()
        {
            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateGoogleProfile(string googleId, string? profilePictureUrl)
        {
            GoogleId = googleId;
            ProfilePictureUrl = profilePictureUrl;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
