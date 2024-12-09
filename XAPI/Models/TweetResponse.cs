namespace XAPI.Models
{
    public class TweetResponse
    {
        public List<TweetData> data { get; set; }
        public TweetIncludes includes { get; set; }
    }

    public class TweetIncludes
    {
        public List<UserData> users { get; set; }
        public List<TweetData> tweets { get; set; }
    }

    public class TweetData
    {
        public DateTime created_at { get; set; }
        public string id { get; set; }
        public Entities entities { get; set; }
        public string author_id { get; set; }
        public string text { get; set; }
        public List<string> edit_history_tweet_ids { get; set; }
        public List<ReferencedTweet> referenced_tweets { get; set; }
        public string in_reply_to_user_id { get; set; }
    }

    public class Entities
    {
        public List<Url> urls { get; set; }
        public List<Annotation> annotations { get; set; }
        public List<Mention> mentions { get; set; }
    }

    public class Url
    {
        public int start { get; set; }
        public int end { get; set; }
        public string url { get; set; }
        public string expanded_url { get; set; }
        public string display_url { get; set; }
        public string media_key { get; set; }
    }

    public class Annotation
    {
        public int start { get; set; }
        public int end { get; set; }
        public double probability { get; set; }
        public string type { get; set; }
        public string normalized_text { get; set; }
    }

    public class Mention
    {
        public int start { get; set; }
        public int end { get; set; }
        public string username { get; set; }
        public string id { get; set; }
    }

    public class ReferencedTweet
    {
        public string type { get; set; }
        public string id { get; set; }
    }
}