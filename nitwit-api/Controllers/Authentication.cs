using Newtonsoft.Json;

namespace nitwitapi.Controllers
{
    public class Authentication
    {
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "password")]
        public string Password { get; set; }
    }
}
