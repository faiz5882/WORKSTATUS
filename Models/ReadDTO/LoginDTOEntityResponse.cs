using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Models.ReadDTO
{
   public class LoginDTOEntityResponse
    {

        public LoginDTOEntityResponse()
        {
            Data = new LoginDTOEntity();
        }
        public string Code { get; set; }
        public string Message { get; set; }
        public string Description { get; set; }
        public LoginDTOEntity Data { get; set; }
    }
    public class LoginResponse
    {
        public LoginResponse()
        {
            Response = new LoginDTOEntityResponse();
        }
        public LoginDTOEntityResponse Response { get; set; }
    }
}
