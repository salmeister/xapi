namespace XAPI.Models
{
    public class AppSettings
    {
        public string Environment { get; set; }
        public string XAPIBaseURL { get; set; }
        public required string XAPIKey { get; set; }
    }
}
