using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
   public class UserProjectlistByOrganizationIDResponse
    {       
        public UserProjectlistResponse response { get; set; }
    }
    public class UserProjectlistResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<DatumProject> data { get; set; }
    }

    public class DatumProject
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public bool billable { get; set; }
        public int organization_id { get; set; }
        public int added_by { get; set; }
        public string added_time { get; set; }
        public int archieved { get; set; }
        public int counttodo { get; set; }
        public List<object> userproject { get; set; }
        public List<object> client { get; set; }
    }
}
