using Newtonsoft.Json;
using SQLite;
using System;

namespace Data.Entities
{
    [Table("Following")]
    public class Following : ISqliteEntity
    {
        public Following()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        [Indexed(Name = "UQ_FK_Following", Order = 1, Unique = true)]
        [JsonIgnore]
        public string UserId { get; set; }

        [Indexed(Name = "UQ_FK_Following", Order = 2, Unique = true)]
        [JsonIgnore]
        public string FollowingUserId { get; set; }

        [Ignore]
        [JsonProperty(PropertyName = "user")]
        public string FollowingUsername { get; set; }

        [JsonProperty(PropertyName = "created")]
        [JsonIgnore]
        public DateTime CreatedAt { get; set; }
    }
}
