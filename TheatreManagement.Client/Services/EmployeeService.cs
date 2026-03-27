using System.Net.Http.Json;
using TheatreManagement.Shared.DTOs;
using TheatreManagement.Shared;

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

        public async Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(int page = 1, int pageSize = 10, string? searchText = null)
        {
            return await _httpClient.GetFromJsonAsync<PagedResult<EmployeeDto>>(
                $"api/employees?page={page}&pageSize={pageSize}&searchText={searchText}");
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

    }
}
