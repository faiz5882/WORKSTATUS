using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class ScreenShotResponseModel
    {
        public ScreenShotResponse response { get; set; }
    }
    public class ScreenShotResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public ScreenShotData data { get; set; }
    }
    public class ScreenShotData
    {
        public string imageName { get; set; }
    }
}
