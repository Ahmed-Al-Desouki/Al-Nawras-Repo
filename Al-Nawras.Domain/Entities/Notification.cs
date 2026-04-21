using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class Notification
    {
        public Guid Id { get; private set; }
        public int UserId { get; private set; }
        public NotificationType Type { get; private set; }
        public string Title { get; private set; }
        public string Body { get; private set; }
        public Guid? RelatedEntityId { get; private set; }
        public string RelatedEntityType { get; private set; }
        public bool IsRead { get; private set; }
        public DateTime? ReadAt { get; private set; }
        public DateTime CreatedAt { get; private set; }

        public User User { get; private set; }

        private Notification() { }

        public Notification(int userId, NotificationType type, string title,
            string body, Guid? relatedEntityId = null, string relatedEntityType = null)
        {
            Id = Guid.NewGuid();
            UserId = userId;
            Type = type;
            Title = title;
            Body = body;
            RelatedEntityId = relatedEntityId;
            RelatedEntityType = relatedEntityType;
            IsRead = false;
            CreatedAt = DateTime.UtcNow;
        }

        public void MarkAsRead()
        {
            IsRead = true;
            ReadAt = DateTime.UtcNow;
        }
    }
}
