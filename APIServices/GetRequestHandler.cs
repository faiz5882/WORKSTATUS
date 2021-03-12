using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models;

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

                if (IsHeaderRequired)
                {
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                }
                HttpResponseMessage response = await _client.GetAsync(uri);
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
                var msg = ex.Message;
                throw;
            }
        }
        #endregion
    }
}
