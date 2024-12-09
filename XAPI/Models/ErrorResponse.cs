namespace XAPI.Models
{
    public class ErrorResponse
    {
        public List<Error> errors { get; set; }
    }

    public class Error
    {
        public string value { get; set; }
        public string detail { get; set; }
        public string title { get; set; }
        public string resource_type { get; set; }
        public string parameter { get; set; }
        public string resource_id { get; set; }
        public string type { get; set; }
    }

    
}
