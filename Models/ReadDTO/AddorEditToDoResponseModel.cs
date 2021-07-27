using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class AddorEditToDoResponseModel
    {
        public AddorEditToDoResponse response { get; set; }
    }
    public class AddorEditToDoResponse
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}
