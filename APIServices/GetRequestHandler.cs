using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Utility;

namespace WorkStatus.APIServices
{
    public class GetRequestHandler
    {
        private HttpClient _client;
        public GetRequestHandler()
        {
            _client = new HttpClient();
        }
        #region generic Get Api
        public async Task<T> GetAsyncData_GetApi<T>(string uri, Boolean IsHeaderRequired, HeaderModel objHeaderModel, T Tobject) where T : new()
        {
            try
            {
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri(uri),
                    Method = HttpMethod.Get,
                };
                if (IsHeaderRequired)
                {
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
                    // _client.DefaultRequestHeaders.Add("Authorization",objHeaderModel.SessionID);
                    // _client.DefaultRequestHeaders.Add("OrgID", Common.Storage.ServerOrg_Id);
                    // _client.DefaultRequestHeaders.Add("SDToken", Common.Storage.ServerSd_Token);
                    // _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                }
                HttpResponseMessage response = await _client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = response.Content;
                    var SucessResponse = await response.Content.ReadAsStringAsync();
                    Tobject = JsonConvert.DeserializeObject<T>(SucessResponse);
                    return Tobject;
                }
                else
                {
                    var responseContent = response.Content;
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    Tobject = JsonConvert.DeserializeObject<T>(ErrorResponse);
                    return Tobject;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                return Tobject;
            }
           
        }
        #endregion
    }
}
