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
        public long Sno { get; set; }
        [DbColumn]
        public string OrganizationId { get; set; }
        [DbColumn]
        public string OrganizationName { get; set; }
        [DbColumn]
        public bool IsOffline { get; set; }

    }
}
