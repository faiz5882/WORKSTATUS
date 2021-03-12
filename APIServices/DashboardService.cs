using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;
using WorkStatus.Models;

namespace WorkStatus.APIServices
{
    public class DashboardService : GetRequestHandler,IDashboard
    {
        private HttpClient _client;
        public DashboardService()
        {
            _client = new HttpClient();
        }
        
    }
}
