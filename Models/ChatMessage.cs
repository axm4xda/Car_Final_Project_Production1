namespace Car_Project.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }

        // Sender
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;

        // Receiver (null = broadcast / group)
        public string? ReceiverId { get; set; }

        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        /// <summary>Mesajın oxunduğu vaxt (null = hələ oxunmayıb)</summary>
        public DateTime? ReadAt { get; set; }

        // Navigation
        public AppUser? Sender { get; set; }
        public AppUser? Receiver { get; set; }
    }
}
