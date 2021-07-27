using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models.WriteDTO
{
   public class tbl_ToDoAttachments
    {
		[DbColumn(IsIdentity = true, IsPrimary = true)]
		public int SNo { get; set; }
		[DbColumn]
		public int Id { get; set; }
		[DbColumn]
		public int OrgId { get; set; }
		[DbColumn]
		public int ProjectId { get; set; }
		[DbColumn]
		public int ToDoId { get; set; }
		[DbColumn]
		public string Image { get; set; }
		[DbColumn]
		public string ImageURL { get; set; }
		[DbColumn]
		public string AttachmentImage { get; set; }
	}
}
