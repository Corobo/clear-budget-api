
namespace Messaging.Events
{
    public class CategoryEvent
    {
        public string EventType { get; set; } = default!; // e.g. "created", "updated", "deleted"
        public Guid CategoryId { get; set; }
        public Guid? UserId { get; set; } // null if it's a global (admin) category
    }
}

