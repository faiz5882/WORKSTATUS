using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
  public class tbl_IdleTimeDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
       
        public int Sno { get; set; }
        [DbColumn]        
        public int ProjectId { get; set; }
        [DbColumn]
        public string ProjectIdleStartTime { get; set; }
        [DbColumn]
        public string ProjectIdleEndTime { get; set; }
        [DbColumn]
        public int UserId { get; set; }
        [DbColumn]
        public string CreatedOn { get; set; }

        [DbColumn]
        public int IsActive { get; set; }
    }
}
