using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs
{
    public class PlayDTO
    {
        public int PlayId { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string? Name { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public string? Duration { get; set; }

        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public int AgeCategory { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastEditTime { get; set; }
        public DateTime? DeletionTime { get; set; }
    }
}
