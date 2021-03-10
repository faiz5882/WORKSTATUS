using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Interfaces
{
   public interface IService
    {
        #region LoginService
        Task<tbl_UserDetails> LoginAsync(string uri, LoginDTOEntity _objRequest);
        #endregion       
    }
}
