using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
    public class RenewAppRequestModel
    {
        public double app_version { get; set; }
        public string os { get; set; }
        public string api_version { get; set; }
    }
}
