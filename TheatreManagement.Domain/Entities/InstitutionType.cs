using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Domain.Entities
{
    public class InstitutionType
    {
        public int InstitutionTypeId { get; set; }
        public string Name { get; set; }
        // ссылки
        public List<Institution> Institutions { get; set; } = new();
    }
}
