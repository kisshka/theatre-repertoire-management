using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Domain.Entities
{
    public class HallType
    {
        public int HallTypeId { get; set; }
        public string Name { get; set; }
        // ссылки
        public List<Stationar> Stationars { get; set; } = new();
    }
}