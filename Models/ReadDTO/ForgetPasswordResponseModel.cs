using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
   public class ForgetPasswordResponseModel
    {
        public ForgetPassword Response { get; set; }
    }
    public class ForgetPassword
    {
        public string Code { get; set; }
        public string Message { get; set; }
    }
}
