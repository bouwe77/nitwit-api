using Newtonsoft.Json;
using SQLite;

namespace Data.Entities
{
    [Table("Mentions")]
    public class Mention : ISqliteEntity
    {
        public Mention()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        public string PostId { get; set; }

        public string MentionedUserId { get; set; }
    }
}
