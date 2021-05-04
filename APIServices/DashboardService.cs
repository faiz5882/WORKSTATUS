using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Views;

namespace WorkStatus.APIServices
{
    public class DashboardService : GetRequestHandler,IDashboard
    {
        private HttpClient _client;
        public DashboardService()
        {
            _client = new HttpClient();
        }

        public  UserProjectlistByOrganizationIDResponse GetUserProjectlistByOrganizationIDAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, OrganizationDTOEntity _objRequest)
        {
            UserProjectlistByOrganizationIDResponse objFPResponse;
            string strJson = JsonConvert.SerializeObject(_objRequest);           
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.ContentType = "application/json; charset=utf-8";
            request.Method = "POST";
            if (IsHeaderRequired)
            {
                request.PreAuthenticate = true;
                request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);               
            }
            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(strJson);
                streamWriter.Flush();
                streamWriter.Close();
            }
            WebResponse response = request.GetResponse();
            var streamReader = new StreamReader(response.GetResponseStream());
            var result = streamReader.ReadToEnd();
            objFPResponse = JsonConvert.DeserializeObject<UserProjectlistByOrganizationIDResponse>(result);
            return objFPResponse;
            //HttpResponseMessage response = null;
            //using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
            //{
            //    if (IsHeaderRequired)
            //    {
            //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
            //    }
            //    response = await _client.PostAsync(uri, stringContent);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var SucessResponse = await response.Content.ReadAsStringAsync();
            //        objFPResponse = JsonConvert.DeserializeObject<UserProjectlistByOrganizationIDResponse>(SucessResponse);
            //        return objFPResponse;
            //    }
            //    else
            //    {
            //        var ErrorResponse = await response.Content.ReadAsStringAsync();
            //        objFPResponse = JsonConvert.DeserializeObject<UserProjectlistByOrganizationIDResponse>(ErrorResponse);
            //        return objFPResponse;
            //    }
            //}
        }
        public  ToDoListResponseModel GetUserToDoListAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ToDoListRequestModel _objRequest)
        {
            ToDoListResponseModel objFPResponse=new ToDoListResponseModel();
            try
            {
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                }
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(strJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response = request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream());
                var result = streamReader.ReadToEnd();
                objFPResponse = JsonConvert.DeserializeObject<ToDoListResponseModel>(result);
                return objFPResponse;
                //using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
                //{
                //    if (IsHeaderRequired)
                //    {
                //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                //    }
                //    response = await _client.PostAsync(uri, stringContent);
                //    if (response.IsSuccessStatusCode)
                //    {
                //        var SucessResponse = await response.Content.ReadAsStringAsync();
                //        objFPResponse = JsonConvert.DeserializeObject<ToDoListResponseModel>(SucessResponse);
                //        return objFPResponse;
                //    }
                //    else
                //    {
                //        var ErrorResponse = await response.Content.ReadAsStringAsync();
                //        objFPResponse = JsonConvert.DeserializeObject<ToDoListResponseModel>(ErrorResponse);
                //        return objFPResponse;
                //    }
                //}

            }
            catch (Exception ex)
            {
                MyMessageBox.Show(new Avalonia.Controls.Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
            }
            return objFPResponse;
        }
        public async Task<ActivitySyncTimerResponseModel> GetActivitysynTimerDataAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ActivitySyncTimerRequestModel _objRequest)
        {
            ActivitySyncTimerResponseModel objFPResponse;
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
                    objFPResponse = JsonConvert.DeserializeObject<ActivitySyncTimerResponseModel>(SucessResponse);
                    return objFPResponse;
                }
                else
                {
                    var ErrorResponse = await response.Content.ReadAsStringAsync();
                    objFPResponse = JsonConvert.DeserializeObject<ActivitySyncTimerResponseModel>(ErrorResponse);
                    return objFPResponse;
                }
            }
        }
        public async Task<CommonResponseModel> ActivityLogAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest)
        {
           // System.Threading.Thread.Sleep(10000);
            CommonResponseModel objFPResponse =new CommonResponseModel();

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
