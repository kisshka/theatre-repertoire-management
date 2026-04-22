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

            public string StationarTimePeriod
            {
                get
                {
                if (StartTime != null & EndTime != null)
                {
                    return $"{StartTime.Value.ToString("HH:mm")}-{EndTime.Value.ToString("HH:mm")}";
                }
                else
                    return "";
                                                              
                }
            }

            public string TourTimePeriod
            {
                get
                {
                    if (StartTime != null & EndTime != null)
                    {
                        return $"{StartTime.Value.ToString("dd/MM")}-{EndTime.Value.ToString("dd/MM")}";
                    }
                    else
                        return "";

                }
            }

            public string VisitTimePeriod
            {
                get
                {
                    if (StartTime != null & EndTime != null)
                    {
                        return $"{StartTime.Value.ToString("HH:mm")}-{EndTime.Value.ToString("HH:mm")}";
                    }
                    else
                        return "";

                }
            }

        public string? Type { get; set; }
            public DateTime LastEditTime { get; set; }
            public bool IsCanceled { get; set; }
            
            // Навигационные свойства
            public StationarDto? Stationar { get; set; }
            public TourDto? Tour { get; set; }
            public InstitutionDto? Institution { get; set; }

            public string UserFullName { get; set; } = "";

            // Связи
            public List<PlayDto>? Plays { get; set; }

            public List<PlayWithRolesDto>? PlaysWithRoles { get; set; } = new();

    }

    public class PlayWithRolesDto
    {
        public int PlayId { get; set; }
        public string PlayName { get; set; }
        public List<RoleGroupDto> RoleGroups { get; set; } = new();
    }
}
