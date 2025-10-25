namespace Domain.Entities
{
    public class Tour
    {
        public int TourId { get; set; }
        public string? Country { get; set; }
        public string? Area { get; set; }

        //ссылки
        public int EventId { get; set; }
        public Event? Event { get; set; }
    }
}