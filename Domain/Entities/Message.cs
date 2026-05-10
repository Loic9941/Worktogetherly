namespace WorkTogetherly.Domain.Entities
{
    public class Message
    {
        public int Id { get; private set; }
        public Guid SenderId { get; private set; }
        public Guid RecipientId { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public bool IsRead { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User Sender { get; private set; } = null!;
        public User Recipient { get; private set; } = null!;

        private Message() { }

        public static Message Create(Guid senderId, Guid recipientId, string content)
        {
            return new Message
            {
                SenderId = senderId,
                RecipientId = recipientId,
                Content = content,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };
        }

        public void MarkAsRead()
        {
            IsRead = true;
        }
    }
}
