using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json.Nodes;

namespace TheatreManagement.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient httpClient;
        public CustomAuthStateProvider(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity());

            try
            {
                var response = await httpClient.GetAsync("manage/info");
                
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

            catch (Exception ex)
            {

            }


            return new AuthenticationState(user);
        }

        public async Task<FormResult> LoginAsync(string email, string password)
        {
            try
            {
                var response = await httpClient.PostAsJsonAsync("login", new {email, password});
                
                if (response.IsSuccessStatusCode)
                {
                    var strResponse = await response.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);
                    var accessToken = jsonResponse?["accessToken"]?.ToString();
                    var refreshToken = jsonResponse?["refreshToken"]?.ToString();

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

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
    }

    public class FormResult
    {
        public bool Succeeded { get; set; }
        public string[] Errors { get; set; } = [];
    }

}

