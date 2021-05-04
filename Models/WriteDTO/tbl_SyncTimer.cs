using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_SyncTimer
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [DbColumn]
        public string ProjectId { get; set; }
        [DbColumn]
        public string TodoId { get; set; }
        [DbColumn]
        public string? TimeLog { get; set; }
    }
}
