using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class EventPostModel
    {
        public int EventId { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public DateTime StartTime { get; set; }
        [Required(ErrorMessage = "Поле обязательно для заполнения")]
        public DateTime EndTime { get; set; }
        public bool IsCanceled { get; set; }

        public string Type { get; set; }

        public StationarDto? Stationar { get; set; } = new();

        public TourDto? Tour { get; set; } = new();

        public InstitutionDto? Institution { get; set; } = new();

        public List<PlayEventDto> PlayEvents { get; set; } = new();

    }
}
