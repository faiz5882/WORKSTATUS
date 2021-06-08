using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using static WorkStatus.Models.ReadDTO.ScreenshotIntervalResponse;

namespace WorkStatus.Interfaces
{
   public interface IDashboard:IGetRequest
    {
         UserProjectlistByOrganizationIDResponse GetUserProjectlistByOrganizationIDAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, OrganizationDTOEntity _objRequest);
        ToDoListResponseModel GetUserToDoListAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ToDoListRequestModel _objRequest);
        Task<ActivitySyncTimerResponseModel> GetActivitysynTimerDataAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ActivitySyncTimerRequestModel _objRequest);
        Task<CommonResponseModel> ActivityLogAsync(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest);

        Task<AddNotesResponseModel> AddNotesAPI(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, List<tbl_AddNotes> _objRequest);
        ChangeOrganizationResponseModel ChangeOrganizationAPI(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ChangeOrganizationRequestModel _objRequest);
        ScreenshotInterval GetScreeshotIntervelFromServerAPI(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, ScreeshotIntervelFromServer _objRequest);
        #region TokenExpire
        Task<bool> RenewTokenAPI(bool IsHeaderRequired, HeaderModel objHeaderModel, RefreshTokenRequest _objRequest);
        Task<RenewAppResponseModel> ForceUpgardeAppAPI(string uri, RenewAppRequestModel _objRequest);



        #endregion

    }
}
