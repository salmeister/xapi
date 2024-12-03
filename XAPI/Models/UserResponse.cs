namespace XAPI.Models
{
    public class UserResponse
    {
        public UserData data { get; set; }

        public class UserData
        {
            public string id { get; set; }
            public string name { get; set; }
            public string username { get; set; }
        }
    }
}
