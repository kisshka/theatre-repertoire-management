namespace TheatreManagement.Domain.Entities
{
    public class Institution
    {
        public int InstitutionId { get; set; }
        public int InstitutionTypeId { get; set; }
        public string? Name { get; set; }
        public string? Town { get; set; }
        public string? Street { get; set; }
        public string? House { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Comment { get; set; }

        // ссылки
        public InstitutionType Type { get; set; }
        public List<Event> Events { get; set; } = new();
    }
}