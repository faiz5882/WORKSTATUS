using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;

namespace WorkStatus.APIServices
{
   public class AccountService: IAccounts
    {
        private HttpClient _client;
        public AccountService()
        {
            _client = new HttpClient();
        }
        public async Task<LoginResponse> LoginAsync(string uri, LoginRequestDTOEntity _objRequest)
        {
            LoginResponse objLoginResponse;
            try
            {

            
            string strJson = JsonConvert.SerializeObject(_objRequest);
            HttpResponseMessage response = null;
            using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
            {
                //if (IsHeaderRequired)
                //{
                //    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                //}
                response = await _client.PostAsync(uri, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var SucessResponse = await response.Content.ReadAsStringAsync();
                    objLoginResponse = JsonConvert.DeserializeObject<LoginResponse>(SucessResponse);
                    return objLoginResponse;
                }
                else
                {
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    objLoginResponse = JsonConvert.DeserializeObject<LoginResponse>(ErrorResponse);
                    return objLoginResponse;
                }
            }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                objLoginResponse = new LoginResponse();
              //  throw;
            }
            return objLoginResponse;
        }
        public async Task<ForgetPasswordResponseModel> ForgotPasswordAsync(string uri, ForgetPasswordReqeuestModel _objRequest)
        {
            ForgetPasswordResponseModel objFPResponse=new ForgetPasswordResponseModel();
            try
            {

            
            string s = JsonConvert.SerializeObject(_objRequest);
            HttpResponseMessage response = null;
            using (var stringContent = new StringContent(s, System.Text.Encoding.UTF8, "application/json"))
            {
                //if (IsHeaderRequired)
                //{
                //    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                //}
                response = await _client.PostAsync(uri, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var SucessResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<ForgetPasswordResponseModel>(SucessResponse);
                    return objFPResponse;
                }
                else
                {
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<ForgetPasswordResponseModel>(ErrorResponse);
                    return objFPResponse;
                }
            }
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
                return objFPResponse;
            }
        }


    }
}
