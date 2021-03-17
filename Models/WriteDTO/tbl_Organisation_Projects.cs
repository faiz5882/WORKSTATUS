using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_Organisation_Projects
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long Sno { get; set; }
        [DbColumn]
        public string ProjectId { get; set; }

        [DbColumn]
        public string ProjectName { get; set; }
        [DbColumn]
        public string OrganisationId { get; set; }
        [DbColumn]
        public string UserId { get; set; }
        [DbColumn]
        public int IsOffline { get; set; }
    }
}
