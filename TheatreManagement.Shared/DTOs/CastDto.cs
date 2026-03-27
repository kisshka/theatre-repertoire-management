using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class CastDto
    {
        public int CastId { get; set; }
        public string? Name { get; set; }
        public DateTime? LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }

        // ссылки
        public string? UserId { get; set; }
        public UserDto? User { get; set; }

        public List<EmployeeRoleCreateDto> EmployeeRolesCreate { get; set; } = new();
    }



}
