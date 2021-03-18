using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
    public class ToDoListRequestModel
    {
        public int project_id { get; set; }
        public int organization_id { get; set; }
        public int userid { get; set; }
    }
}
