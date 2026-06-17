using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheatreManagement.Domain.Entities
{
    public class SceneType
    {
        public int SceneTypeId { get; set; }
        public string Name { get; set; }
        // ссылки
        public List<Play> Plays { get; set; } = new();
    }
}
