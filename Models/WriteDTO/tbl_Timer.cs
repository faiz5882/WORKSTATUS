using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_Timer
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long Sno { get; set; }
        [DbColumn]
        public string Start { get; set; }

        [DbColumn]
        public string Stop { get; set; }
        [DbColumn]
        public string SelfieVerification { get; set; }
        [DbColumn]
        public string ProjectId { get; set; }
        [DbColumn]
        public string OrgId { get; set; }
        [DbColumn]
        public string TimeType { get; set; }
        [DbColumn]
        public long? ToDoId { get; set; }
        [DbColumn]
        public int IntervalTime { get; set; }
        [DbColumn]
        public string SourceType { get; set; }
    }
}
