using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class ChangeOrganizationResponseModel
    {
        public ChangeOrganizationResponse response { get; set; }
    }
    public class Data
    {
        public string token { get; set; }
    }

    public class ChangeOrganizationResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public string description { get; set; }
        public Data data { get; set; }
    }
}
