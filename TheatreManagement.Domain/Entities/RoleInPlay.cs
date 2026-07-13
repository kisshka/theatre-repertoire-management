using Domain.Entities;
using System.Reflection.Metadata;

namespace TheatreManagement.Domain.Entities
{
    public class RoleInPlay
    {
        public int RoleInPlayId { get; set; }
        public string? Name { get; set; }
        public string? Type { get; set; }

    // Ссылки
        public int PlayId { get; set; }
        public Play? Play { get; set; }
        public List<EmployeeRole> EmployeeRoles { get; set; } = new();

    }
}