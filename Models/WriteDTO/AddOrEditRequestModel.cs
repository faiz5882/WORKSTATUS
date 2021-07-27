using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
   public class AddOrEditRequestModel
    {
        public string name { get; set; }
        public string organization_id { get; set; }
        public string project_id { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string description { get; set; }
        public string privacy { get; set; }
        public string site { get; set; }
        public List<string> memberIds { get; set; }
        public List<HttpContent> attachments { get; set; }
        public int toggle_status { get; set; }
        public int repeat { get; set; }
        public int once_every { get; set; }
        public int end_after { get; set; }
        public int recurrence_status { get; set; }
    }
}
