namespace TheatreManagement.Domain.Entities
{
    public class Stationar
    {
        public int StationarId { get; set; }
        public string? Hall { get; set; }
        public string? Type { get; set; }

        public Event? Event { get; set; }
    }
}