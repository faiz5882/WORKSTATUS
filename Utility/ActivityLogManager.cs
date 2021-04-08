using Avalonia.Threading;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.APIServices;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Utility
{

    public class ActivityLogManager
    {
        #region global properties
        public DispatcherTimer ActivityTimerObject;
        private List<tbl_Timer> tbl_TimersList;
        private CommonResponseModel responseModel;
        private ActivityLogRequestEntity activityLogRequestEntity;
        private List<ActivityLogRequestEntity> activityLogRequests;
        public List<tbl_KeyMouseTrack_Slot> track_Slots;
        private readonly IActivityLog _services;
        private string _baseURL = string.Empty;
        private HeaderModel objHeaderModel;
        Task[] tasks;
        #endregion
        public ActivityLogManager()
        {
            ActivityTimerObject = new DispatcherTimer();
            ActivityTimerObject.Tick += new EventHandler(ActivityTimerObject_Tick);
            responseModel = new CommonResponseModel();
            _services = new ActivityLogService();
            _baseURL = Configurations.UrlConstant + Configurations.ActivityLogApiConstant;
        }

        private void ActivityTimerObject_Tick(object? sender, EventArgs e)
        {
            ActivityTimerObject.Stop();
            ActivityTimerObject.Interval = new TimeSpan(0, 5, 0);
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(CallActivity);
            worker.RunWorkerAsync();
            ActivityTimerObject.Start();
        }
        public void CallActivity(object sender, DoWorkEventArgs e)
        {
            tasks = new Task[1];
            tasks[0] = Task.Factory.StartNew(() => GetActivityLogFromDB());
        }

        public void CallActivityLog()
        {
            SendIntervalToServer();
            ActivityTimerObject.Interval = new TimeSpan(0, 5, 0);
            ActivityTimerObject.Start();
        }

        public List<Intervals> GetIntervalsList(string startTime)
        {
            List<AppAndUrl> _appAndUrls = new List<AppAndUrl>();
            List<AppAndUrl> finalAppAndUrl = new List<AppAndUrl>();
            AppAndUrl appAnd;
            Location _location;
            ActivityLevel activityLevel;

            Intervals intervals;
            List<Intervals> listofIntervals = new List<Intervals>();
            List<Intervals> finalIntervals = new List<Intervals>();           
            BaseService<tbl_KeyMouseTrack_Slot> dbService2 = new BaseService<tbl_KeyMouseTrack_Slot>();
            track_Slots = new List<tbl_KeyMouseTrack_Slot>(dbService2.GetAllById(startTime, "Start"));
            if (track_Slots != null)
            {
                if (track_Slots.Count > 0)
                {
                    foreach (var slot in track_Slots)
                    {
                        _location = new Location()
                        {
                            @long = slot.Longitude.ToStrVal(),
                            lat = slot.Latitude.ToStrVal()
                        };
                        activityLevel = new ActivityLevel()
                        {
                            average = slot.AverageActivity.ToStrVal(),
                            keyboard = slot.keyboardActivity.ToStrVal(),
                            mouse = slot.MouseActivity.ToStrVal(),
                        };
                        intervals = new Intervals()
                        {
                            appAndUrls = _appAndUrls,
                            location = _location,
                            screenUrl = "",
                            activityLevel = activityLevel,
                            from = slot.IntervalStratTime.ToStrVal(),
                            to = slot.IntervalEndTime.ToStrVal(),
                            interval_time_db = "5"

                        };

                        listofIntervals = new List<Intervals>();
                        listofIntervals.Add(intervals);
                        finalIntervals.AddRange(listofIntervals);
                    }
                }
            }
            return finalIntervals;
        }
        public async void SendIntervalToServer()
        {
            try
            {
                                
                List<ActivityLogRequestEntity> finallist = new List<ActivityLogRequestEntity>();                               
                BaseService<tbl_Timer> dbService = new BaseService<tbl_Timer>();
                tbl_TimersList = new List<tbl_Timer>(dbService.GetAll());
                if (tbl_TimersList != null && tbl_TimersList.Count > 0)
                {
                    foreach (var item in tbl_TimersList)
                    {                                                                                                                                               
                                activityLogRequestEntity = new ActivityLogRequestEntity()
                                    {
                                        projectId = item.ProjectId,
                                        org_id = item.OrgId,
                                        interval_time = "5",
                                        start = item.Start,
                                        stop = item.Stop,
                                        time_type = item.SourceType,
                                        selfiVerification = item.SelfieVerification,
                                        source_type = item.SourceType,
                                        intervals = GetIntervalsList(item.Start),
                                        todo_id=Convert.ToString(item.ToDoId)
                                };
                                 activityLogRequests = new List<ActivityLogRequestEntity>();
                                 activityLogRequests.Add(activityLogRequestEntity);                                
                                finallist.AddRange(activityLogRequests);                                                   
                    }
                }
                string strJson = JsonConvert.SerializeObject(finallist);
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                if (finallist != null && finallist.Count > 0)
                {
                    //call api
                     responseModel = await _services.ActivityLogAsync(new Get_API_Url().ActivityLogApi(_baseURL), true, objHeaderModel, finallist);
                    if (responseModel.Response.Code == "200")
                    {
                        // delete data from localDB tbl_KeyMouseTrack_Slot
                        if (track_Slots.Count > 0)
                        {
                            BaseService<tbl_KeyMouseTrack_Slot> dbService2 = new BaseService<tbl_KeyMouseTrack_Slot>();
                            foreach (var item in track_Slots)
                            {
                                 dbService2.DeleteSlot(item.Id);
                            }
                        }

                        // delete data from localDB tbl_Timer
                        if (tbl_TimersList.Count > 0)
                        {
                            BaseService<tbl_Timer> dbService2 = new BaseService<tbl_Timer>();

                            foreach (var item in tbl_TimersList)
                            {
                                if (!string.IsNullOrEmpty(item.Stop))
                                    dbService2.DeleteSlotFromtbl_Timer(item.Sno);
                            }
                        }
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        public void GetActivityLogFromDB()
        {
            SendIntervalToServer();
        }

    }
}
