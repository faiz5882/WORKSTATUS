using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
    public class AddOrEditToDoAttachments
    {
        public string ImageName { get; set; }
        public string ImageIcon { get; set; }
        public String CloseIcon { get; set; }
        public string Url {get;set;}
    }
}
