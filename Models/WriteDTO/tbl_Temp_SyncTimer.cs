using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_Temp_SyncTimer
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int SNo { get; set; }
        [DbColumn]
        public int OrganizationId { get; set; }
        [DbColumn]
        public int ProjectId { get; set; }
        [DbColumn]
        public int TodoId { get; set; }
        [DbColumn]        
        public string TotalWorkedHours { get; set; }       
        [DbColumn]
        public string CreatedDate { get; set; }
    }

    public class tbl_TempSyncTimerTodoDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int SNo { get; set; }
        [DbColumn]
        public int OrganizationId { get; set; }
        [DbColumn]
        public int ProjectId { get; set; }
        [DbColumn]
        public int TodoId { get; set; }
        [DbColumn]
        public string TotalWorkedHours { get; set; }
        [DbColumn]
        public string CreatedDate { get; set; }
    }
}
