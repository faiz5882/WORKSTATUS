using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
    public class tbl_AddTodoDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public int id { get; set; }
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
        public bool IsOffline { get; set; }
        [DbColumn]
        public string ToDoTimeConsumed { get; set; }

    }
}
