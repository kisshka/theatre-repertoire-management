using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Shared.DTOs.Events
{
    public class StationarDto
    {
        public int StationarId { get; set; }
        public string? Hall { get; set; }
        public string? Type { get; set; }
    }

    public class TourDto
    {
        public int TourId { get; set; }
        public string? Country { get; set; }
        public string? Area { get; set; }
    }

    public class InstitutionDto
    {
        public int InstitutionId { get; set; }
        public string? Name { get; set; }
        public string? Town { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
    }
}
