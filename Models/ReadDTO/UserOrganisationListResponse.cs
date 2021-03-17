using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
   public class UserOrganisationListResponse
    {
        public UserOrganisationResponse response { get; set; }
    }
    public class UserOrganisationResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<Datum> data { get; set; }
    }
    public class Permissions
    {
        public bool selfiRequired { get; set; }
    }
    public class Datum
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public string timezone { get; set; }
        public string team_size { get; set; }
        public int industry_id { get; set; }
        public int user_status { get; set; }
        public object weekly_limit { get; set; }
        public string industry_name { get; set; }
        public Permissions permissions { get; set; }
        public string selfie_authentication { get; set; }
        public object organization_timedate { get; set; }
        public string start_week_on { get; set; }
        public string dateformat { get; set; }
    }
}
