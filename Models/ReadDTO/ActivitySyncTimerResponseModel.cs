using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class ActivitySyncTimerResponseModel
    {
        public Response response { get; set; }
    }
    public class ActivitySyncTimerResponse
    {
        public string timeLog { get; set; }
        public int projectId { get; set; }
        public int? todoId { get; set; }
    }

    public class Response
    {
        public string code { get; set; }
        public string message { get; set; }
        public List<ActivitySyncTimerResponse> data { get; set; }
    }
}
