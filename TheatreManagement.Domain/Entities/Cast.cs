using Domain.Entities;
using System.Reflection.Metadata;

namespace TheatreManagement.Domain.Entities
{
    public class Cast
    {
        public int CastId { get; set; }
        public string? Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }

        // ссылки
        public int PlayId { get; set; }
        public Play? Play { get; set; }
        public string? UserId { get; set; }
        public User? User { get; set; }
        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
    }
}