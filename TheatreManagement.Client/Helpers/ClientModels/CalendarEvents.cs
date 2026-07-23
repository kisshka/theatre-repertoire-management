namespace TheatreManagement.Client.Helpers.ClientModels
{
    class DayData
    {
        public bool HasConflict = false;
        public int EventsCount;
        public List<EventData> Events = new();
    }

    class EventData
    {
        public string TimePeriod;
        public string EventType;
        public string Content;
        public string CancellationReason;
    }
}
