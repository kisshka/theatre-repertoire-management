namespace TheatreManagement.Domain.Entities
{
    public class Stationar
    {
        public int StationarId { get; set; }
        public string Type { get; set; }
        // ссылки
        public int HallTypeId { get; set; }
        public HallType HallType { get; set; }
        public Event? Event { get; set; }
    }
}