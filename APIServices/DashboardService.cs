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
    public class DashboardService : GetRequestHandler,IDashboard
    {
        private HttpClient _client;
        public DashboardService()
        {
            _client = new HttpClient();
        }

        public async Task<UserProjectlistByOrganizationIDResponse> GetUserProjectlistByOrganizationIDAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, OrganizationDTOEntity _objRequest)
        {
            UserProjectlistByOrganizationIDResponse objFPResponse;
            string strJson = JsonConvert.SerializeObject(_objRequest);
            HttpResponseMessage response = null;
            using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
            {
                if (IsHeaderRequired)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                }
                response = await _client.PostAsync(uri, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var SucessResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<UserProjectlistByOrganizationIDResponse>(SucessResponse);
                    return objFPResponse;
                }
                else
                {
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<UserProjectlistByOrganizationIDResponse>(ErrorResponse);
                    return objFPResponse;
                }
            }
        }
    }
}
