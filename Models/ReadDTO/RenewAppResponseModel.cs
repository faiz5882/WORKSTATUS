using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class RenewAppResponseModel
    {
        public RenewAppResponse response { get; set; }
    }
    public class RenewAppResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public RenewAppData data { get; set; }
    }

    public class RenewAppData
    {
        public int id { get; set; }
        public int api_version { get; set; }
        public int app_version { get; set; }
        public string os { get; set; }
        public string latest_supported_version { get; set; }
        public bool force_upgrade { get; set; }
        public string app_title { get; set; }
        public string app_description { get; set; }
        public string page_description { get; set; }
        public object created_at { get; set; }
        public object updated_at { get; set; }
        public string download_link { get; set; }
    }
}
