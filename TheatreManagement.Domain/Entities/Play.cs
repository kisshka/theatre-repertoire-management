using Domain.Entities;

namespace TheatreManagement.Domain.Entities
{
    public class Play
    {
        public int PlayId { get; set; }
        public string? Name { get; set; }
        public string? Duration { get; set; }
        public bool IsActive { get; set; }
        public int AgeCategory { get; set; }
        public DateTime LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }

    //ссылки
        public string? UserId { get; set; }
        public User? User { get; set; }

        public List<Event> Events { get; set; } = new();
        public List<RoleInPlay> RoleInPlays { get; set; }  = new();
        public List<PlayEvent> PlayEvents { get; set; } = new();
    }
}