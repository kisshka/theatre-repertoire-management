namespace TheatreManagement.Domain.Entities
{
    public class Institution
    {
        public int InstitutionId { get; set; }
        public string? Name { get; set; }
        public string? Town { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }

        // ссылки
        public List<Event> Events { get; set; } = new();
    }
}