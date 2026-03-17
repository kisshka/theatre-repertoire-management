using Domain.Entities;

namespace TheatreManagement.Domain.Entities
{
    public class Event
    {
        public int EventId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; } 
        public string? Type { get; set; }
        public DateTime LastEditTime { get; set; }
        public bool IsCanceled { get; set; }

        // ссылки
        public int InstitutionId { get; set; }        
        public Institution? Institution { get; set; }
        public int PlayId { get; set; }
        public Play? Play { get; set; }
        public int TourId { get; set; }
        public Tour? Tour { get; set; }
        public int StationarId { get; set; }
        public Stationar? Stationar { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }

        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
        public List<PlayEvent> PlayEvents { get; set; } = new();
    }
}