using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class PlayDto
    {
        public int PlayId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public int Duration { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public int AgeCategory { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }

        public List<RoleDto> RoleDtos { get; set; } = new();

        // Тип сцены
        public int SceneTypeId { get; set; }
        public string SceneTypeName { get; set; } = string.Empty;
        // Для отображения
        public bool IsUsed { get; set; }

        public string UserFullName { get; set; } = "";
    }
    public class SceneTypeDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
