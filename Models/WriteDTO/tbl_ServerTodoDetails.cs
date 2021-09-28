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
        public int Sno { get; set; }
        [DbColumn]
        public int Id { get; set; }
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
        public string Member_Id { get; set; }
        public SolidColorBrush SiteColor { get; set; }

        public bool ToDoPlayIcon { get; set; }
        public bool ToDoStopIcon { get; set; }
        public bool ToDoCompleteIcon { get; set; }
        public bool ToDoStopIsEnabled { get; set; }
        public bool ToDoPlayIsEnabled { get; set; }
		public List<string> AttachmentImage { get; set; }
        public bool IsMarkComplete { get; set; }
        public bool IsOnlyDeleteVisible { get; set; }
        public bool IsPublicCheck { get => Privacy == "Public" ? true : false ; }
        public bool IsPrivateCheck { get => Privacy == "Private" ? true : false; }

    }
}
