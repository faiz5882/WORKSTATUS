using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
  public class Plist
    {
        public int ProjectID { get; set; }
        public string timeLog { get; set; }
    }

    public class Tlist
    {
        public int ProjectID { get; set; }

        public int? todoId { get; set; }
        public string timeLog { get; set; }
    }
}
