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
   public interface IActivityLog
    {
        Task<CommonResponseModel> ActivityLogAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest);
    }
}
