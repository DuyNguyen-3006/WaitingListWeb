using Ardalis.GuardClauses;

namespace WaitingListWeb.Domain.Entities
{
    public class WaitingListEntry : BaseEntity
    {
        public string Email { get; private set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber{ get; set; }

        public string? WishMessage { get; set; }

        private WaitingListEntry() { }

        public WaitingListEntry(string email, string? fullName = null)
        {
            Email = Guard.Against.NullOrWhiteSpace(email, nameof(email))
                         .Trim().ToLowerInvariant();

            if (Email.Length > 100)
                throw new ArgumentException("Email too long.", nameof(email));

        }

    }
}