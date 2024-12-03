using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using XAPI.Models;

namespace XAPI.Services
{
    internal interface ITweetService
    {
        Task<UserResponse> GetUserByName(string userID);
    }

    internal class TweetService(AppSettings settings) : ITweetService
    {
        private readonly AppSettings appSettings = settings;
        private readonly HttpClient _httpClient = new();

        public async Task<UserResponse> GetUserByName(string userHandle)
        {
            AddHeader();
            var response = await _httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/users/by/username/{userHandle}");
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var responseObject = JsonSerializer.Deserialize<UserResponse>(responseJson);
                return responseObject;
            }
            else
            {
                throw new HttpRequestException($"Error: {response.StatusCode}, Details: {response.ReasonPhrase}");
            }
        }

        private void AddHeader()
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appSettings.XAPIKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private string GetTweetsEndpoint(string userId)
        {
            var today = GetTodayMidnightUtc();
            var tomorrow = today.AddDays(1);
            return $"/2/users/{userId}/tweets?max_results=100&start_time={today.ToString("yyyy-MM-ddTHH:mm:ssZ")}&end_time={tomorrow.ToString("yyyy-MM-ddTHH:mm:ssZ")}";
        }

        private DateTime GetTodayMidnightUtc()
        {
            var now = DateTime.UtcNow;
            return new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        }
    }
}
