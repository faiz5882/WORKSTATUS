using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Common
{
  public class Storage
    {
        public static string ConnectionString { get; set; }
        public static bool IsLogin { get; set; }
        public static string TokenId { get; set; }
        public static int timeIntervel { get; set; }
        public static bool checkTodoApiCallOrNot { get; set; }

    }
}
