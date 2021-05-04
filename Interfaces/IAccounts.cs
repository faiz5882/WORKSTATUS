using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Interfaces
{
   public interface IAccounts
    {        
        Task<LoginResponse> LoginAsync(string uri, LoginRequestDTOEntity _objRequest);
        Task<ForgetPasswordResponseModel> ForgotPasswordAsync(string uri, ForgetPasswordReqeuestModel _objRequest);
    }
}
