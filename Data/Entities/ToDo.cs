using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Entities
{
    public class ToDo : ISqliteEntity
    {
        public ToDo()
        {
            Id = IdGenerator.GetId();
        }

        [PrimaryKey]
        [JsonIgnore]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
