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
   public interface IDashboard:IGetRequest
    {
         UserProjectlistByOrganizationIDResponse GetUserProjectlistByOrganizationIDAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, OrganizationDTOEntity _objRequest);
        ToDoListResponseModel GetUserToDoListAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ToDoListRequestModel _objRequest);
        Task<ActivitySyncTimerResponseModel> GetActivitysynTimerDataAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ActivitySyncTimerRequestModel _objRequest);
        Task<CommonResponseModel> ActivityLogAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest);
    }
}
