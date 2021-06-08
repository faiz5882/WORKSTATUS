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
            long a = 0;
            try
            {
           
            BaseService<tbl_UserDetails> gg = new BaseService<tbl_UserDetails>();           
            a=  gg.Add(tbl_User);
                return a;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return a;
        }
    }
}
