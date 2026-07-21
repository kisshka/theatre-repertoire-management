using System.Net.Http.Json;
using TheatreManagement.Shared.Helpers;

namespace TheatreManagement.Client.Services
{
    public class SceneTypeService
    {
        private readonly HttpClient _httpClient;

        public SceneTypeService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Guide>> GetSceneTypesAsync()
        {
            return await _httpClient.GetFromJsonAsync<List<Guide>>("api/SceneTypes");
        }

        public async Task AddSceneTypeAsync(string name)
        {
            await _httpClient.PostAsJsonAsync("api/SceneTypes", new {name});
            //response.EnsureSuccessStatusCode();
        }

        public async Task UpdateSceneTypeAsync(int id, string name)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/SceneTypes/{id}", name);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteSceneTypeAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/SceneTypes/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}
