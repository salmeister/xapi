namespace XAPI.Models
{
    public class Tweet
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public DateTime CreatedDate { get; set; }
        public TwitterUser Author { get; set; }
        public string TweetType { get; set; } = "Original";
        public List<Tweet> ReferencedTweet { get; set; }
        public List<TweetURL> URLs { get; set; }
    }

    public class TweetURL
    {
        public string Url { get; set; }
        public string ExpandedUrl { get; set; }
    }

    public class TwitterUser
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
    }
}
