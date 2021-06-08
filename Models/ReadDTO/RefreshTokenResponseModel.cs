using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
   public class RefreshTokenResponseModel
    {
        public RefreshTokenResponseModel()
        {
            response = new ResponseModel();
        }
        public ResponseModel response { get; set; }
    }
    public class ResponseModel
    {
        public string code { get; set; }
        public string message { get; set; }
        public string token { get; set; }
    }
}
