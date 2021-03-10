using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Common;
using WorkStatus.Models;

namespace WorkStatus.Utility
{
   public class UserLoginSqliteService:BaseService<tbl_UserDetails>        
    {
        public UserLoginSqliteService()
            : base(Storage.ConnectionString) { }

        public long InsertUser(tbl_UserDetails tbl_User)
        {
            return new UserLoginSqliteService().Add(tbl_User);
        }
    }
}
