using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
    public class tbl_URLTracking
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [DbColumn]
        public string Start { get; set; }
        [DbColumn]
        public string URLStartDateTime { get; set; }
        [DbColumn]
        public string URLEndDateTime { get; set; }
        [DbColumn]
        public string URLConsumedTime { get; set; }
        [DbColumn]
        public string UrlName { get; set; }
        [DbColumn]
        public string urlPath { get; set; }
        [DbColumn]
        public string TotalTimeSpent { get; set; }
        [DbColumn]
        public int IsOffline { get; set; }
    }

    public class tbl_Apptracking
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [DbColumn]
        public string Start { get; set; }
        [DbColumn]
        public string AppStartDateTime { get; set; }
        [DbColumn]
        public string AppEndDateTime { get; set; }
        [DbColumn]
        public string AppConsumedTime { get; set; }
        [DbColumn]
        public string Activity_Name { get; set; }
        [DbColumn]
        public string Activity_TotalRun { get; set; }
        [DbColumn]
        public int IsOffline { get; set; }
    }

    public class tbl_AppAndUrl
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int Id { get; set; }
        [DbColumn]
        public string Start { get; set; }
        [DbColumn]
        public string IsApp { get; set; }
        [DbColumn]
        public string SpendTime { get; set; }
        [DbColumn]
        public string Name { get; set; }
    }
}
