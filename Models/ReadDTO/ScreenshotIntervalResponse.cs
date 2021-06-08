using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class ScreenshotIntervalResponse
    {

        public class ScreenshotInterval
        {
            public ResponseInterval response { get; set; }
        }
        public class ResponseInterval
        {
            public string code { get; set; }
            public string message { get; set; }
            public Data data { get; set; }
        }
        public class Data
        {
            public int id { get; set; }
            public int org_id { get; set; }
            public int user_id { get; set; }
            public int screenshot_interval { get; set; }
            public object date { get; set; }
            public DateTime? created_at { get; set; }
            public DateTime? updated_at { get; set; }
            public string timeInterval { get; set; }
        }
    }
}
