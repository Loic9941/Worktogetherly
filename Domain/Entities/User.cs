using Microsoft.AspNetCore.Identity;

namespace WorkTogetherly.Domain.Entities
{
    public class User : IdentityUser<Guid>
    {
        public string FirstName { get; private set; } = string.Empty;
        public string LastName { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public string? PhotoPath { get; private set; }
        public ICollection<Workspace> Workspaces { get; private set; } = [];
        public ICollection<Booking> Bookings { get; private set; } = [];
        public ICollection<Review> Reviews { get; private set; } = [];
        public ICollection<Message> SentMessages { get; private set; } = [];
        public ICollection<Message> ReceivedMessages { get; private set; } = [];

        private User() { }

        public User(string firstName, string lastName, string email)
        {
            Id = Guid.NewGuid();
            FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
            LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
            UserName = email ?? throw new ArgumentNullException(nameof(email));
            Email = email ?? throw new ArgumentNullException(nameof(email));
            CreatedAt = DateTime.UtcNow;
        }

        public static User Create(string firstName, string lastName, string email)
        {
            return new User(firstName, lastName, email);
        }

        public void UpdateProfile(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }

        public string? RemovePhoto()
        {
            var previous = PhotoPath;
            PhotoPath = null;
            return previous;
        }

        public string? ReplacePhoto(string newPath)
        {
            var previous = PhotoPath;
            PhotoPath = newPath;
            return previous;
        }
    }
}
