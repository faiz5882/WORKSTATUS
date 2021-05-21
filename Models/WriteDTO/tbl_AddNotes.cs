using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_AddNotes
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        [JsonIgnore]
        public int Id { get; set; }
        [DbColumn]
        [JsonProperty("time")]
        public string time { get; set; }
        [JsonProperty("note")]
        [DbColumn]
        public string note { get; set; }
        [JsonProperty("projectId")]
        [DbColumn]
        public string projectId { get; set; }
        [JsonProperty("organization_id")]
        [DbColumn]
        public string organization_id { get; set; }
        [JsonProperty("tracker")]
        [DbColumn]
        public string tracker { get; set; }
        [JsonIgnore]
        [DbColumn]
        public int IsOffline { get; set; }
    }
}
