using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_OrganisationDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int Sno { get; set; }
        [DbColumn]
        public string OrganizationId { get; set; }
        [DbColumn]
        public string OrganizationName { get; set; }
        [DbColumn]
        public int IsOffline { get; set; }
        [DbColumn]
        public string WeeklyLimit { get; set; }

    }
}
