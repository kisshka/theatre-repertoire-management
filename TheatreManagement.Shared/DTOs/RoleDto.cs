using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class RoleDto
    {
        public RoleDto( string RoleType)
        {
            this.RoleType = RoleType;
        }
        public RoleDto()
        {
        }

        public int RoleInPlayId { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string? Name { get; set; }
        public string? RoleType { get; set; }
        public DateTime LastEditTime { get; set; }
    }
}
