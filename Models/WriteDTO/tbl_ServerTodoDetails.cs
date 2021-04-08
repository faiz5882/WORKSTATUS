using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
    public class tbl_ServerTodoDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long Sno { get; set; }
        [DbColumn]
        public long Id { get; set; }
        [DbColumn]
        public string ToDoName { get; set; }
        [DbColumn]
        public string CurrentUserId { get; set; }
        [DbColumn]
        public string CurrentProjectId { get; set; }
        [DbColumn]
        public string CurrentOrganisationId { get; set; }
        [DbColumn]
        public string StartDate { get; set; }
        [DbColumn]
        public string EndDate { get; set; }
        [DbColumn]
        public string EstimatedHours { get; set; }
        [DbColumn]
        public string Description { get; set; }
        [DbColumn]
        public int IsCompleted { get; set; }
        [DbColumn]
        public bool IsOffline { get; set; }
        [DbColumn]
        public string ToDoTimeConsumed { get; set; }
        [DbColumn]
        public string Privacy { get; set; }
        [DbColumn]
        public string Site { get; set; }
        public SolidColorBrush SiteColor { get; set; }

        public bool ToDoPlayIcon { get; set; }
        public bool ToDoStopIcon { get; set; }
        public bool ToDoStopIsEnabled { get; set; }
        public bool ToDoPlayIsEnabled { get; set; }



    }
}
