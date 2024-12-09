using Xunit;
using System.Net;
using System.Net.Http.Headers;
using Moq;
using Moq.Protected;
using XAPI.Models;
using XAPI.Services;


namespace XAPI.Tests.Services
{
    public class TweetServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;
        private readonly TweetService _tweetService;

        public TweetServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _appSettings = new AppSettings
            {
                XAPIBaseURL = "https://api.example.com",
                XAPIKey = "test_api_key"
            };
            _tweetService = new TweetService(_appSettings, _httpClient);
        }

        /// <summary>
        /// Test to get user by user name when unsuccessful
        /// </summary>
        [Fact]
        public async Task GetUserByNameTest_WhenUnsuccessful()
        {
            // Arrange
            var userHandle = "testuser";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/by/username/{userHandle}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Bad Request"
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _tweetService.GetUserByName(userHandle));
            Assert.Contains("Error: 400", exception.Message);
        }

        /// <summary>
        /// Test to get user by user name
        /// </summary>
        [Fact()]
        public void GetUserByNameTest_WhenSuccessful()
        {
            // Arrange
            var userHandle = "testuser";
            var responseContent = new StringContent("{\"data\": {\"id\": \"12345\", \"name\": \"Test User\", \"username\": \"testuser\"}}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/by/username/{userHandle}")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = _tweetService.GetUserByName(userHandle).Result;

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345", result.data.id);
            Assert.Equal("Test User", result.data.name);
            Assert.Equal("testuser", result.data.username);
        }

        /// <summary>
        /// Test to get following user by user id
        /// </summary>
        [Fact]
        public async Task GetFollowingByUserIDTest_WhenSuccessful()
        {
            // Arrange
            var userID = "12345";
            var responseContent = new StringContent("{\"data\": {\"id\": \"12345\", \"name\": \"Test User\", \"username\": \"testuser\"}}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/{userID}/following")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _tweetService.GetFollowingByUserID(userID);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("12345", result.data.id);
            Assert.Equal("Test User", result.data.name);
            Assert.Equal("testuser", result.data.username);
        }

        /// <summary>
        /// Test to get following user by user id
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetFollowingByUserIDTest_WhenUnsuccessful()
        {
            // Arrange
            var userID = "12345";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/{userID}/following")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Bad Request"
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _tweetService.GetFollowingByUserID(userID));
            Assert.Contains("Error: 400", exception.Message);
        }

        /// <summary>
        /// Test to get tweets by tweet ids
        /// </summary>
        /// <returns></returns>
        [Fact()]
        public async Task GetTweetsTest_WhenSuccessful()
        {
            // Arrange
            var tweetIds = new List<string> { "1", "2", "3" };
            var responseContent = new StringContent("{\"data\": [{\"id\": \"1\", \"text\": \"Test tweet 1\"}, {\"id\": \"2\", \"text\": \"Test tweet 2\"}, {\"id\": \"3\", \"text\": \"Test tweet 3\"}]}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/tweets?ids=1,2,3&tweet.fields=text")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _tweetService.GetTweets(tweetIds);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.data.Count);
            Assert.Equal("1", result.data[0].id);
            Assert.Equal("Test tweet 1", result.data[0].text);
            Assert.Equal("2", result.data[1].id);
            Assert.Equal("Test tweet 2", result.data[1].text);
            Assert.Equal("3", result.data[2].id);
            Assert.Equal("Test tweet 3", result.data[2].text);
        }

        /// <summary>
        /// Test to get tweets by tweet ids
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetTweetsTest_WhenUnsuccessful()
        {
            // Arrange
            var tweetIds = new List<string> { "1", "2", "3" };

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/tweets?ids=1,2,3&tweet.fields=text")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Bad Request"
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _tweetService.GetTweets(tweetIds));
            Assert.Contains("Error: 400", exception.Message);
        }

        [Fact]
        public async Task GetTodaysTweetsByUserID_ReturnsTweetResponse_WhenSuccessful()
        {
            // Arrange
            var userID = "12345";
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);
            var responseContent = new StringContent("{\"data\": [{\"id\": \"1\", \"text\": \"Test tweet\", \"created_at\": \"2023-10-01T00:00:00Z\"}]}");
            responseContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/{userID}/tweets?max_results=100&start_time={today:yyyy-MM-ddTHH:mm:ssZ}&end_time={tomorrow:yyyy-MM-ddTHH:mm:ssZ}&tweet.fields=text,created_at")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = responseContent
                });

            // Act
            var result = await _tweetService.GetTweetsByUserIDInTimeSpan(userID, DayRange.Today);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.data);
            Assert.Equal("1", result.data[0].id);
            Assert.Equal("Test tweet", result.data[0].text);
        }

        [Fact]
        public async Task GetTodaysTweetsByUserID_ThrowsHttpRequestException_WhenUnsuccessful()
        {
            // Arrange
            var userID = "12345";
            var today = DateTime.UtcNow.Date;
            var tomorrow = today.AddDays(1);

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri == new Uri($"{_appSettings.XAPIBaseURL}/users/{userID}/tweets?max_results=100&start_time={today:yyyy-MM-ddTHH:mm:ssZ}&end_time={tomorrow:yyyy-MM-ddTHH:mm:ssZ}&tweet.fields=text,created_at")),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = "Bad Request"
                });

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => _tweetService.GetTweetsByUserIDInTimeSpan(userID, DayRange.Today));
            Assert.Contains("Error: 400", exception.Message);
        }
    }
}
