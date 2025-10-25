using System.Reflection.Metadata;

namespace Domain.Entities
{
    public class Cast
    {
        public int CastId { get; set; }
        public string? Name { get; set; }
        public DateTime LastEditTime { get; set; }
        public bool IsDeleted { get; set; }

        // ссылки
        public string? UserId { get; set; }
        public User? User { get; set; }
        public List<EmployeeRole> EmployeeRoles { get; set; } = new();
    }
}