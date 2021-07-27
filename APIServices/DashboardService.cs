using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;
using WorkStatus.Views;
using static WorkStatus.Models.ReadDTO.ScreenshotIntervalResponse;

namespace WorkStatus.APIServices
{
    public class DashboardService : GetRequestHandler, IDashboard
    {
        private HttpClient _client;

        public DashboardService()
        {
            _client = new HttpClient();
        }

        public UserProjectlistByOrganizationIDResponse GetUserProjectlistByOrganizationIDAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, OrganizationDTOEntity _objRequest)
        {
            UserProjectlistByOrganizationIDResponse objFPResponse=new UserProjectlistByOrganizationIDResponse();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);

            }
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
        public ToDoListResponseModel GetUserToDoListAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ToDoListRequestModel _objRequest)
        {
            ToDoListResponseModel objFPResponse = new ToDoListResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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
                LogFile.ErrorLog(ex);
                //  MyMessageBox.Show(new Avalonia.Controls.Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
            }
            return objFPResponse;
        }
        public async Task<ActivitySyncTimerResponseModel> GetActivitysynTimerDataAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ActivitySyncTimerRequestModel _objRequest)
        {
            ActivitySyncTimerResponseModel objFPResponse = new ActivitySyncTimerResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
                }
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(strJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response1 = request.GetResponse();
                var streamReader = new StreamReader(response1.GetResponseStream());
                var result = streamReader.ReadToEnd();
                objFPResponse = JsonConvert.DeserializeObject<ActivitySyncTimerResponseModel>(result);
               
                return objFPResponse;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //MyMessageBox.Show(new Avalonia.Controls.Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
            }
            return objFPResponse;
        }
        public CommonResponseModel ActivityLogAsyncForIdle(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest)
        {
            // System.Threading.Thread.Sleep(10000);
            CommonResponseModel objFPResponse = new CommonResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
                }
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(strJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response1 = request.GetResponse();
                var streamReader = new StreamReader(response1.GetResponseStream());
                var result = streamReader.ReadToEnd();
                objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(result);
                return objFPResponse;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //MyMessageBox.Show(new Avalonia.Controls.Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
            }
            return objFPResponse;
           
        }
        public async Task<CommonResponseModel> ActivityLogAsync(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, List<ActivityLogRequestEntity> _objRequest)
        {
            // System.Threading.Thread.Sleep(10000);
            CommonResponseModel objFPResponse = new CommonResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
                }
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(strJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response1 = request.GetResponse();
                var streamReader = new StreamReader(response1.GetResponseStream());
                var result = streamReader.ReadToEnd();
                objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(result);                
                return objFPResponse;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //MyMessageBox.Show(new Avalonia.Controls.Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
            }
            return objFPResponse;
            //string strJson = JsonConvert.SerializeObject(_objRequest);
            //HttpResponseMessage response = null;
            //using (var stringContent = new StringContent(strJson, System.Text.Encoding.UTF8, "application/json"))
            //{
            //    if (IsHeaderRequired)
            //    {
            //        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("token", objHeaderModel.SessionID);
            //    }
            //    response = await _client.PostAsync(uri, stringContent);
            //    if (response.IsSuccessStatusCode)
            //    {
            //        var SucessResponse = await response.Content.ReadAsStringAsync();
            //        objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(SucessResponse);
            //        return objFPResponse;
            //    }
            //    else
            //    {
            //        var ErrorResponse = await response.Content.ReadAsStringAsync();
            //        objFPResponse = JsonConvert.DeserializeObject<CommonResponseModel>(ErrorResponse);
            //        return objFPResponse;
            //    }
            //}
        }
        public async Task<AddNotesResponseModel> AddNotesAPI(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, List<tbl_AddNotes> _objRequest)
        {
            AddNotesResponseModel objFPResponse = new AddNotesResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                request.UseDefaultCredentials = true;
                request.Proxy.Credentials = System.Net.CredentialCache.DefaultCredentials;
                if (Common.CommonServices.IsConnectedToInternet())
                {
                    if (IsHeaderRequired)
                    {
                        request.PreAuthenticate = true;
                        request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                        request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                        request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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
                    objFPResponse = JsonConvert.DeserializeObject<AddNotesResponseModel>(result);
                    if (objFPResponse.response.message == "Note added successfully")
                    {
                        return objFPResponse;
                    }
                    else
                    {
                        return objFPResponse;
                    }
                }
                else
                {
                    return objFPResponse;
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                return objFPResponse;
            }
        }
        public ChangeOrganizationResponseModel ChangeOrganizationAPI(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ChangeOrganizationRequestModel _objRequest)
        {

            ChangeOrganizationResponseModel objFPResponse = new ChangeOrganizationResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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
                objFPResponse = JsonConvert.DeserializeObject<ChangeOrganizationResponseModel>(result);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);

            }
            return objFPResponse;
        }
        public ScreenshotInterval GetScreeshotIntervelFromServerAPI(string uri, bool IsHeaderRequired, HeaderModel objHeaderModel, ScreeshotIntervelFromServer _objRequest)
        {
            ScreenshotInterval objFPResponse = new ScreenshotInterval();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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

                objFPResponse = JsonConvert.DeserializeObject<ScreenshotInterval>(result);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);

            }
            return objFPResponse;
        }
        public async Task<bool> RenewTokenAPI(bool IsHeaderRequired, HeaderModel objHeaderModel, RefreshTokenRequest _objRequest)
        {
            RefreshTokenResponseModel objFPResponse;
            try
            {
                string strJson = JsonConvert.SerializeObject(_objRequest);
                string uri = Configurations.UrlConstant + Configurations.RenewTokenApiConstant;
                Uri myUri = new Uri(uri, UriKind.Absolute);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                if (IsHeaderRequired)
                {
                    request.PreAuthenticate = true;
                    request.Headers.Add("Authorization", "Bearer " + objHeaderModel.SessionID);
                    request.Headers.Add("OrgID", Common.Storage.ServerOrg_Id);
                    request.Headers.Add("SDToken", Common.Storage.ServerSd_Token);
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
                objFPResponse = JsonConvert.DeserializeObject<RefreshTokenResponseModel>(result);
                if (objFPResponse.response.code == "200")
                {
                    Common.Storage.TokenId = objFPResponse.response.token;
                    return true;
                }
            }
            catch (Exception ex)
            {

                //LogFile.ErrorLog(ex);

            }
            return true;
        }
        public async Task<RenewAppResponseModel> ForceUpgardeAppAPI(string uri, RenewAppRequestModel _objRequest)
        {
            RenewAppResponseModel objFPResponse = new RenewAppResponseModel();
            try
            {
                Uri myUri = new Uri(uri, UriKind.Absolute);
                string strJson = JsonConvert.SerializeObject(_objRequest);
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(myUri);
                request.ContentType = "application/json; charset=utf-8";
                request.Method = "POST";
                using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                {
                    streamWriter.Write(strJson);
                    streamWriter.Flush();
                    streamWriter.Close();
                }
                WebResponse response = request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream());
                var result = streamReader.ReadToEnd();
                objFPResponse = JsonConvert.DeserializeObject<RenewAppResponseModel>(result);
                if (objFPResponse.response.code == "200")
                {
                    return objFPResponse;
                }
                else
                {
                    return objFPResponse = null;
                }

            }
            catch (Exception ex)
            {
                return objFPResponse = null;
            }
            return objFPResponse;
        }
        public async Task<AddorEditToDoResponseModel> AddorEditToDoApiCall(string uri, HeaderModel objHeaderModel, AddOrEditRequestModel ar)
        {
            AddorEditToDoResponseModel addToDoResponse = new AddorEditToDoResponseModel();
            try
            {
                if (ar != null)
                {
                    if (Common.CommonServices.IsConnectedToInternet())
                    {
                        string responseContent = "";
                        using var form = new MultipartFormDataContent();
                        HttpClient _httpClient = new HttpClient();
                        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                        _httpClient.DefaultRequestHeaders.Add("OrgID", Common.Storage.ServerOrg_Id);
                        _httpClient.DefaultRequestHeaders.Add("SDToken", Common.Storage.ServerSd_Token);

                        if (ar.attachments.Count > 0)
                        {
                            foreach (HttpContent ht in ar.attachments)
                            {
                                form.Add(ht);
                            }
                        }
                        for (int i = 0; i < ar.memberIds.Count; i++)
                        {
                            form.Add(new StringContent(ar.memberIds[i]), "memberIds");
                        }
                        form.Add(new StringContent(ar.name), "name");
                        form.Add(new StringContent(ar.description), "description");
                        DateTime dtStart;
                        bool isconverted = DateTime.TryParse(ar.startDate, out dtStart);
                        if (isconverted)
                        {
                            form.Add(new StringContent(dtStart.ToString("yyyy-MM-dd")), "startDate");
                        }
                        DateTime dtEnd;
                        bool isconverted1 = DateTime.TryParse(ar.endDate, out dtEnd);
                        if (isconverted)
                        {
                            form.Add(new StringContent(dtEnd.ToString("yyyy-MM-dd")), "endDate");
                        }
                        form.Add(new StringContent(ar.privacy), "privacy");
                        form.Add(new StringContent(ar.site), "site");
                        form.Add(new StringContent(ar.organization_id), "organization_id");
                        form.Add(new StringContent(ar.project_id), "project_id");
                        var response = await _httpClient.PostAsync(uri, form);
                        response.EnsureSuccessStatusCode();
                        responseContent = response.Content.ReadAsStringAsync().Result;
                        addToDoResponse = JsonConvert.DeserializeObject<AddorEditToDoResponseModel>(responseContent);
                        if (addToDoResponse.response.code == "200")
                        {
                            return addToDoResponse;
                        }
                    }
                    return addToDoResponse;
                }
                return addToDoResponse;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                return addToDoResponse;
            }
        }
    }
}
