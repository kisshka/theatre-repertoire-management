using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Helpers.ClientModels
{
    public class EventTableRow
    {
        public int LocationId { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public DateTime Event { get; set; }
        public Dictionary<DateTime, string> EventsByDate { get; set; } = new();
    }

}
