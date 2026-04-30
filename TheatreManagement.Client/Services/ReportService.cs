namespace TheatreManagement.Client.Services
{
    public class ReportService
    {
        private readonly HttpClient _httpClient;

        public ReportService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task CreateReport()
        {

        }
    }
}
