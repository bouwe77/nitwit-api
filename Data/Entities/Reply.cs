using Newtonsoft.Json;
using SQLite;

namespace Data.Entities
{
    [Table("Replies")]
    public class Reply : ISqliteEntity
    {
        public Reply()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        public string PostId { get; set; }

        public string RepliedUserId { get; set; }
    }
}
