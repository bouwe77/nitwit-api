using Newtonsoft.Json;
using SQLite;
using System;

namespace Data.Entities
{
    [Table("Posts")]
    public class Post : ISqliteEntity
    {
        public Post()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "post")]
        public string Content { get; set; }

        [JsonIgnore]
        public string UserId { get; set; }

        [JsonProperty(PropertyName = "user")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedAt { get; set; }
    }
}
