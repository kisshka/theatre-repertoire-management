using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class EventGetModel
    {
            public int EventId { get; set; }
            public DateTime? StartTime { get; set; }
            public DateTime? EndTime { get; set; }



        // Для отображения правильного времени для многодневных мероприятий
        public string GetTimePeriodForDate(DateTime date)
        {
            if (StartTime == null || EndTime == null)
                return "";

            var startDate = StartTime.Value.Date;
            var endDate = EndTime.Value.Date;
            var currentDate = date.Date;

            if (startDate == endDate)
            {
                return $"{StartTime:HH:mm} - {EndTime:HH:mm}";
            }

            if (currentDate == startDate)
            {
                return $"{StartTime:HH:mm} - 23:59";
            }

            if (currentDate == endDate)
            {
                return $"00:00 - {EndTime:HH:mm}";
            }

            return "00:00 - 23:59";
        }

        //для отображения более презентабельных дат в деталях мероприятия
        public string GetDetailsDate()
        {
            if (StartTime == null || EndTime == null)
                return "";

            if (StartTime.Value.Date == EndTime.Value.Date)
            {
                return $"{StartTime: dd MMMM yyyy}, {StartTime:HH:mm} - {EndTime:HH:mm}";
            }

            else
            {
                return $"{StartTime: dd MMMM yyyy}, {StartTime:HH:mm}  -  {EndTime: dd MMMM yyyy}, {EndTime:HH:mm}";
            }

        }

        public string? Type { get; set; }
            public DateTime LastEditTime { get; set; }
            public bool IsCanceled { get; set; }

            //Конфликты
            public List<string> Warnings { get; set; } = new();
            public bool HasConflict { get; set; }

            // Навигационные свойства
            public StationarDto? Stationar { get; set; }
            public TourDto? Tour { get; set; }
            public InstitutionDto? Institution { get; set; }

            public string UserFullName { get; set; } = "";

            // Связи
            public List<PlayDto>? Plays { get; set; }

            public List<PlayWithRolesDto>? PlaysWithRoles { get; set; } = new();
            public List<int> EmployeeRoles { get; set; } = new();

    }

    public class PlayWithRolesDto
    {
        public int PlayId { get; set; }
        public string PlayName { get; set; }
        public List<RoleGroupDto> RoleGroups { get; set; } = new();
    }
}
