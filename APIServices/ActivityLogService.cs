using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.APIServices
{
   public class ActivityLogService:IActivityLog
    {
        private HttpClient _client;
        public ActivityLogService()
        {
            _client = new HttpClient();
        }
     
        public async Task<CommonResponseModel> ActivityLogAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel,List<ActivityLogRequestEntity> _objRequest)
        {
            CommonResponseModel objFPResponse;
            string strJson = JsonConvert.SerializeObject(_objRequest);
            HttpResponseMessage response = null;
            using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
            {
                if (IsHeaderRequired)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", objHeaderModel.SessionID);
                }
                response = await _client.PostAsync(uri, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var SucessResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(SucessResponse);
                    return objFPResponse;
                }
                else
                {
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(ErrorResponse);
                    return objFPResponse;
                }
            }
        }

       
    }
}
