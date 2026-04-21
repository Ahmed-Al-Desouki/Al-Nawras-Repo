using Al_Nawras.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Al_Nawras.Domain.Entities
{
    public class DealTask
    {
        public Guid Id { get; private set; }
        public Guid DealId { get; private set; }
        public int AssignedToUserId { get; private set; }
        public string Title { get; private set; }
        public string Description { get; private set; }
        public DealTaskStatus Status { get; private set; }
        public TaskPriority Priority { get; private set; }
        public DateTime? DueDate { get; private set; }
        public DateTime? CompletedAt { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public Deal Deal { get; private set; }
        public User AssignedToUser { get; private set; }

        private DealTask() { }

        public DealTask(Guid dealId, int assignedToUserId, string title,
            string description, TaskPriority priority, DateTime? dueDate = null)
        {
            Id = Guid.NewGuid();
            DealId = dealId;
            AssignedToUserId = assignedToUserId;
            Title = title;
            Description = description;
            Status = DealTaskStatus.Pending;
            Priority = priority;
            DueDate = dueDate;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Complete()
        {
            Status = DealTaskStatus.Completed;
            CompletedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
