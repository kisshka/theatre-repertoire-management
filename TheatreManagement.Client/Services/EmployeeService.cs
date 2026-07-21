using AntDesign;
using System.Net.Http.Json;
using TheatreManagement.Shared.DTOs.Employees;
using TheatreManagement.Shared.DTOs.Events;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class EmployeeService
    {
        private readonly HttpClient _httpClient;

        public EmployeeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<HttpResponseMessage> CreateEmployeeAsync(EmployeeDto employee)
        {
            return await _httpClient.PostAsJsonAsync("api/employees", employee);
        }
        public async Task<HttpResponseMessage> UpdateEmployeeAsync(EmployeeDto employee)
        {
            return await _httpClient.PutAsJsonAsync("api/employees", employee);
        }
        public async Task<HttpResponseMessage> SoftDeleteEmployeeAsync(int employeeId)
        {
            return await _httpClient.PutAsJsonAsync($"api/employees/{employeeId}/soft-delete", new { });
        }

        public async Task<HttpResponseMessage> RestoreEmployeeAsync(int employeeId)
        {
            return await _httpClient.PutAsJsonAsync($"api/employees/{employeeId}/restore", new { });
        }

        public async Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(int page = 1, int pageSize = 10, string? searchText = null, bool isArchive = false)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<EmployeeDto>>(
                $"api/employees?page={page}&pageSize={pageSize}&searchText={searchText}&isArchive={isArchive}");
        }

        public async Task<EmployeeDto> GetEmployeeAsync(int id)
        {
            return await _httpClient.GetFromJsonAsync<EmployeeDto>(
                $"api/employees/{id}");
        }


        public async Task<List<EmployeeDto>> GetActorsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<EmployeeDto>>(
                $"api/employees/actors");
        }

        public async Task<List<EmployeeDto>> GetTechnicsAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<EmployeeDto>>(
                $"api/employees/technics");
        }

        // Участие сотрудников
        public async Task<PagedResult<EventGetModel>> GetEmployeeEventsPreviewAsync(int employeeId, int page = 1, int pageSize = 3)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<EventGetModel>>(
                $"api/employees/{employeeId}/events/preview?page={page}&pageSize={pageSize}");
        }

        public async Task<List<EventGetModel>> GetEmployeeEventsByDateRangeAsync(DateTime startDate, DateTime endDate, int employeeId)
        {
            var start = startDate.ToString("yyyy-MM-dd");
            var end = endDate.ToString("yyyy-MM-dd");
            return await _httpClient.GetFromJsonAsync<List<EventGetModel>>($"api/employees/{employeeId}/events?start={start}&end={end}");
        }
        public async Task<PagedResult<EmployeePlays>> GetEmployeeCastsAsync(int employeeId, int page = 1, int pageSize = 10, string? searchText = null)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<EmployeePlays>>($"api/employees/{employeeId}/casts?page={page}&pageSize={pageSize}&searchText={searchText}");
        }
    }
}
