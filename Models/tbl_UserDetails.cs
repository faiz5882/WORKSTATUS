using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Utility;

namespace WorkStatus.Models
{
  public class tbl_UserDetails
    {
        [DbColumn(IsIdentity = true, IsPrimary = true)]
        public long Sno { get; set; }
        [DbColumn]
        public int UserId { get; set; }
        [DbColumn]
        public string UserEmail { get; set; }
        [DbColumn]
        public string UserName { get; set; }
        [DbColumn]
        public string Token { get; set; }
        [DbColumn]
        public string CreatedOn { get; set; }
        [DbColumn]
        public string UpdatedOn { get; set; }
        [DbColumn]
        public string IsRemember { get; set; }
        [DbColumn]
        public int OrganisationId { get; set; }        
    }
}
