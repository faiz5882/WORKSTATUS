using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Common;
using WorkStatus.Models;

namespace WorkStatus.Utility
{
   public class UserLoginSqliteService
    {
        
        public long InsertUser(tbl_UserDetails tbl_User)
        {
            BaseService<tbl_UserDetails> gg = new BaseService<tbl_UserDetails>();           
            return  gg.Add(tbl_User);
        }
    }
}
