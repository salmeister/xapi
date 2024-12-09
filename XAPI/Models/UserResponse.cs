namespace XAPI.Models
{
    public class UserResponse
    {
        public UserData data { get; set; }
        public UserIncludes includes { get; set; }
    }

    public class UserData
    {
        public string url { get; set; }
        public string name { get; set; }
        public string most_recent_tweet_id { get; set; }
        public string username { get; set; }
        public string pinned_tweet_id { get; set; }
        public string id { get; set; }
    }

    public class UserIncludes
    {
        public List<TweetData> tweets { get; set; }
    }
}

