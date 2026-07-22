using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Helpers.ClientModels
{
    public class EventTableRow
    {
        public string Location { get; set; }
        public Dictionary<DateTime, string> EventsByDate { get; set; } = new();
    }
}
