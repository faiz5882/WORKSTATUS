using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
   public class ActivityLogRequestEntity
    {
        public string time_type { get; set; }
        public string projectId { get; set; }
        public string start { get; set; }
        public string stop { get; set; }        
        public string selfiVerification { get; set; }
        public string interval_time { get; set; }
        public List<Intervals> intervals { get; set; }
        public string source_type { get; set; }
        public string org_id { get; set; }
        public string todo_id { get; set; }

    }

    public class Intervals
    {
        public List<AppAndUrl> appAndUrls { get; set; }
        public string to { get; set; }
        public string interval_time_db { get; set; }
        public ActivityLevel activityLevel { get; set; }
        public Location location { get; set; }
        public string screenUrl { get; set; }
        public string from { get; set; }

    }
    public class AppAndUrl
    {
        public string isApp { get; set; }
        public string spendTime { get; set; }
        public string name { get; set; }
    }
    public class ActivityLevel
    {
        public string mouse { get; set; }
        public string average { get; set; }
        public string keyboard { get; set; }
    }
    public class Location
    {
        public string @long { get; set; }
        public string lat { get; set; }
    }
}
