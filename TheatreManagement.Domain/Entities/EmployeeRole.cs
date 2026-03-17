using Domain.Entities;

namespace TheatreManagement.Domain.Entities
{
    public class EmployeeRole
    {
        public int  EmployeeRoleId { get; set; }
        public DateTime LastEditTime { get; set; }

        // ссылки
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }
        public int RoleInPlayId { get; set; }
        public RoleInPlay? RoleInPlay { get; set; }
        public int EventId { get; set; }
        public Event? Event { get; set; }
        public int CastId { get; set; }
        public Cast? Cast { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
    }
}