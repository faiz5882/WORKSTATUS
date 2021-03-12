using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;

namespace WorkStatus.Interfaces
{
   public interface IGetRequest
    {
        Task<T> GetAsyncData_GetApi<T>(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, T Tobject) where T : new();
    }
}
