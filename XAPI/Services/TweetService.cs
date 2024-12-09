using System.Net.Http.Headers;
using System.Text.Json;
using XAPI.Models;

namespace XAPI.Services
{
    public interface ITweetService
    {
        Task<UserResponse> GetUserByName(string userID);
        Task<TweetResponse> GetTweets(List<string> ids);
        Task<List<Tweet>> GetTweetsByUserIDInTimeSpan(string userID, DayRange dayRange);
        Task<UserResponse> GetFollowingByUserID(string userID);
        Task<UserResponse> GetListsByUserID(string userID);
        Task<List<Tweet>> GetTweetsByListIDInTimeSpan(string listID, DayRange dayRange);
    }

    public class TweetService(AppSettings settings, HttpClient httpClient) : ITweetService
    {
        private readonly AppSettings appSettings = settings;

        /// <summary>
        /// Get user by user handle
        /// </summary>
        /// <param name="userHandle"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<UserResponse> GetUserByName(string userHandle)
        {
            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/users/by/username/{userHandle}?expansions=most_recent_tweet_id,pinned_tweet_id&tweet.fields=text,created_at");
            var responseJson = await ProcessHttpResponse(response);
            var responseObject = JsonSerializer.Deserialize<UserResponse>(responseJson);
            return responseObject;
        }

        /// <summary>
        /// Get following by user ID - This is forbidden for the Free API Access Level
        /// </summary>
        /// <param name="userID"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<UserResponse> GetFollowingByUserID(string userID)
        {
            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/users/{userID}/following?expansions=most_recent_tweet_id,pinned_tweet_id&tweet.fields=created_at,text");
            var responseJson = await ProcessHttpResponse(response);
            var responseObject = JsonSerializer.Deserialize<UserResponse>(responseJson);
            return responseObject;
        }

        /// <summary>
        /// Get tweets by tweet IDs
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<TweetResponse> GetTweets(List<string> ids)
        {
            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/tweets?ids={string.Join(",", ids)}&tweet.fields=text,created_at");
            var responseJson = await ProcessHttpResponse(response);
            var responseObject = JsonSerializer.Deserialize<TweetResponse>(responseJson);
            return responseObject;
        }

        /// <summary>
        /// Get tweets by user ID in a time span
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="dayRange"></param>
        /// <returns></returns>
        /// <exception cref="HttpRequestException"></exception>
        public async Task<List<Tweet>> GetTweetsByUserIDInTimeSpan(string userID, DayRange dayRange)
        {
            int maxResults = 100;
            var (startDate, endDate) = GetTodayMidnightUtc(dayRange);

            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/users/{userID}/tweets?max_results={maxResults}&start_time={startDate:yyyy-MM-ddTHH:mm:ssZ}&end_time={endDate:yyyy-MM-ddTHH:mm:ssZ}&tweet.fields=created_at,entities,in_reply_to_user_id,text&expansions=referenced_tweets.id,referenced_tweets.id.author_id&user.fields=id,name,url,username");
            var responseJson = await ProcessHttpResponse(response);
            var responseObject = JsonSerializer.Deserialize<TweetResponse>(responseJson);
            List<Tweet> tweets = ProcessTweetResponse(responseObject);
            return tweets;
        }

        public async Task<UserResponse> GetListsByUserID(string userID)
        {
            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/users/{userID}/followed_lists?list.fields=id,name,owner_id,private");
            var responseJson = await ProcessHttpResponse(response);
            var responseObject = JsonSerializer.Deserialize<UserResponse>(responseJson);
            return responseObject;
        }

        public async Task<List<Tweet>> GetTweetsByListIDInTimeSpan(string listID, DayRange dayRange)
        {
            int maxResults = 100;
            var (startDate, endDate) = GetTodayMidnightUtc(dayRange);

            AddHeader();
            var response = await httpClient.GetAsync($"{appSettings.XAPIBaseURL.TrimEnd('/').Trim()}/lists/{listID}/tweets?max_results={maxResults}&tweet.fields=created_at,entities,in_reply_to_user_id,text&expansions=referenced_tweets.id,referenced_tweets.id.author_id&user.fields=id,name,url,username");
            var responseJson = await ProcessHttpResponse(response);

            var responseObject = JsonSerializer.Deserialize<TweetResponse>(responseJson);
            List<Tweet> tweets = ProcessTweetResponse(responseObject);
            return tweets;
        }

        private static async Task<string> ProcessHttpResponse(HttpResponseMessage response)
        {

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                if (responseJson.StartsWith("{\"errors\":"))
                {
                    var errorObject = JsonSerializer.Deserialize<ErrorResponse>(responseJson);
                    throw new HttpRequestException($"Error: {errorObject.errors?.FirstOrDefault()?.title}, Details: {errorObject.errors?.FirstOrDefault()?.detail}");
                }
                else
                {
                    return responseJson;
                }

            }
            else
            {
                string limitMsg = ReadResponseHeader(response);
                throw new HttpRequestException($"Error: {(int)response.StatusCode}, Details: {response.ReasonPhrase}, Limits: {limitMsg}");
            }
        }

        private static List<Tweet> ProcessTweetResponse(TweetResponse? responseObject)
        {
            List<Tweet> tweets = [];
            foreach (var obj in responseObject?.data.Where(t => t.in_reply_to_user_id == null).ToList())
            {
                // Tweet Info
                var tweet = new Tweet
                {
                    Id = obj.id,
                    CreatedDate = obj.created_at,
                    Text = obj.text
                };
                if (tweet.Text.ToUpper().StartsWith("RT "))
                {
                    tweet.TweetType = "Repost";
                }

                // Author Info
                var author = responseObject?.includes?.users?.FirstOrDefault(u => u.id == obj.author_id);
                tweet.Author = new TwitterUser
                {
                    Id = author?.id,
                    Name = author?.name,
                    Username = author?.username,
                };

                // URL Info
                if (obj.entities?.urls is not null)
                {
                    var urls = new List<TweetURL>();
                    foreach (var entity in obj.entities.urls)
                    {
                        urls.Add(new TweetURL
                        {
                            Url = entity.url,
                            ExpandedUrl = entity?.expanded_url
                        });
                    }
                    tweet.URLs = urls;
                }

                // Referenced Tweets
                if (obj.referenced_tweets is not null)
                {
                    var refTweets = new List<Tweet>();
                    foreach (var refTweetObj in obj.referenced_tweets)
                    {
                        var refTweet = new Tweet
                        {
                            Id = refTweetObj.id,
                            TweetType = refTweetObj.type
                        };

                        var refTweetData = responseObject?.includes?.tweets?.FirstOrDefault(t => t.id == refTweetObj.id);
                        refTweet.Text = refTweetData.text;
                        refTweet.CreatedDate = refTweetData.created_at;

                        // Referenced Tweet Author Info
                        var refAuthor = responseObject?.includes?.users?.FirstOrDefault(u => u.id == refTweetData.author_id);
                        refTweet.Author = new TwitterUser
                        {
                            Id = refAuthor?.id,
                            Name = refAuthor?.name,
                            Username = refAuthor?.username,
                        };
                        refTweets.Add(refTweet);
                    }
                    tweet.ReferencedTweet = refTweets;
                }
                tweets.Add(tweet);
            }
            return tweets;
        }

        private void AddHeader()
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", appSettings.XAPIKey);
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static string ReadResponseHeader(HttpResponseMessage response)
        {
            var header = response.Headers.Where(h => h.Key.Equals("x-rate-limit-reset")).FirstOrDefault();
            string value = header.Value.FirstOrDefault() ?? "";
            if (int.TryParse(value, out int epochSecsToReset) && epochSecsToReset > 0)
            {
                DateTime now = DateTime.UtcNow;
                DateTime waitTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(epochSecsToReset);
                TimeSpan difference = waitTime - now;
                value = difference.TotalMinutes.ToString("0.##") + " minutes";
            }

            string result = $"{header.Key}: {value}";
            Console.WriteLine(result);
            return result;
        }

        private static (DateTime startDate, DateTime endDate) GetTodayMidnightUtc(DayRange dayRange)
        {
            var now = DateTime.UtcNow;
            var startDate = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            if (dayRange == DayRange.Yesterday)
            {
                startDate = startDate.AddDays(-1);
                endDate = endDate.AddDays(-1);
            }
            if (dayRange == DayRange.Last7Days)
            {
                startDate = startDate.AddDays(-7);
                endDate = endDate.AddDays(-7);
            }
            return (startDate, endDate);
        }
    }
}
