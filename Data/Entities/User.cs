using Newtonsoft.Json;
using SQLite;
using System;

namespace Data.Entities
{
    [Table("Users")]
    public class User : ISqliteEntity
    {
        public User()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        [Indexed(Name = "UQ_UserName", Unique = true)]
        [JsonProperty(PropertyName = "user")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "created")]
        public DateTime CreatedAt { get; set; }
    }
}
