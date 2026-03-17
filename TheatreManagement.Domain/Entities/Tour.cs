namespace TheatreManagement.Domain.Entities
{
    public class Tour
    {
        public int TourId { get; set; }
        public string? Country { get; set; }
        public string? Area { get; set; }

        //ссылки
        public Event? Event { get; set; }
    }
}