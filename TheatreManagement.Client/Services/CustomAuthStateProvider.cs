using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;
using TheatreManagement.Shared.DTOs;


namespace TheatreManagement.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ISyncLocalStorageService _localStorage;
        public CustomAuthStateProvider(HttpClient httpClient, ISyncLocalStorageService localStorage)
        {
            this._httpClient = httpClient;
            this._localStorage = localStorage;

            var accessToken = localStorage.GetItem<string>("accessToken");
            if (accessToken != null)
            {
                this._httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var response = await _httpClient.GetAsync("manage/info");
                
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var email = jsonResponse!["email"]!.ToString();

                    var claims = new List<Claim>
                    {
                        new(ClaimTypes.Name, email),
                        new(ClaimTypes.Email, email),
                    };

                    var identity = new ClaimsIdentity(claims, "Token");
                    user = new ClaimsPrincipal(identity);
                    return new AuthenticationState(user);
                }
            }

            catch (Exception)
            {
            }

            return new AuthenticationState(user);
        }

        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("login", new {email, password});
                
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();

                    _localStorage.SetItem("accessToken", accessToken);
                    _localStorage.SetItem("refreshToken", refreshToken);

                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

                    return new FormResult { Succeeded = true };
                }
                else
                {
                    return new FormResult { Succeeded = false, Errors = ["Неверный логин или пароль"] };
                }
            }
            catch { }

            return new FormResult {Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public void Logout()
        {
            _localStorage.RemoveItem("accessToken");
            _localStorage.RemoveItem("refreshToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public async Task<FormResult> RegisterAsync (RegisterModel registerModel)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Account/register", registerModel);
                return new FormResult {Succeeded = true};
            }
            catch { }

            return new FormResult {Succeeded = false, Errors = ["Ошибка подключения"] };
        }

        public async Task<UserViewModel> GetCurrentUserAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Account/current");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserViewModel>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<List<UserViewModel>> GetAllUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/Account/all");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync< List<UserViewModel>>();
                }
                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }



    public class FormResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; } = [];
    }

}

