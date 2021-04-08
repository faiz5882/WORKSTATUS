using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_KeyMouseTrack_Slot
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }
        [DbColumn]
        public string IntervalStratTime { get; set; }
        [DbColumn]
        public string IntervalEndTime { get; set; }
        [DbColumn]
        public string MouseActivity { get; set; }
        [DbColumn]
        public string ScreenActivity { get; set; }
        [DbColumn]
        public string keyboardActivity { get; set; }
        [DbColumn]
        public string AverageActivity { get; set; }
        [DbColumn]
        public string Longitude { get; set; }
        [DbColumn]
        public string Latitude { get; set; }
        [DbColumn]
        public int Hour { get; set; }
        [DbColumn]
        public string Start { get; set; }
        [DbColumn]
        public string End { get; set; }
        [DbColumn]
        public string CreatedDate { get; set; }
    }
}
