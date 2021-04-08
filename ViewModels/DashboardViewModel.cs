using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WorkStatus.APIServices;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;

namespace WorkStatus.ViewModels
{
    public class DashboardViewModel : ReactiveObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region global properties
        Window _window;
        DispatcherTimer SlotTimerObject;
        bool SlotTimerRuning = false;
        int SlotInterval;
        Task[] tasks;
        private string _baseURL = string.Empty;
        public object DashboardSqliteService;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        private ActivitySyncTimerResponseModel activitySyncTimerResponseModel;
        private UserOrganisationListResponse organisationListResponse;
        private UserProjectlistByOrganizationIDResponse userProjectlistResponse;
        private tbl_OrganisationDetails tbl_OrganisationDetails;
        private tbl_Organisation_Projects tbl_organisation_Projects;
        private ToDoListResponseModel toDoListResponseModel;
        private tbl_AddTodoDetails tbl_AddTodoDetails;

        #region activityLog properties
        public DispatcherTimer ActivityTimerObject;
        private List<tbl_Timer> tbl_TimersList;
        private CommonResponseModel responseModel;
        private ActivityLogRequestEntity activityLogRequestEntity;
        private List<ActivityLogRequestEntity> activityLogRequests;
        public List<tbl_KeyMouseTrack_Slot> track_Slots;

        Task[] activityTasks;

        #endregion

        private string projectIdSelected;
        private string orgdSelectedID;

        private long ToDoSelectedID;

        public ListBox listtodo;
        public ListBox listproject;
        public ListBox listOrg;

        System.Timers.Timer timerHeader;
        System.Timers.Timer timerproject;
        System.Timers.Timer timerToDo;
        ThemeManager themeManager;
        string currentTime = string.Empty;
        int h1, m1, s1, h2, m2, s2, h3, m3, s3;
        int TotalSecound, TotalSMinute, Totalhour;
        #endregion
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandPlay { get; }
        public ReactiveCommand<Unit, Unit> CommandStop { get; }
        public ReactiveCommand<string, Unit> CommandProjectPlay { get; set; }
        public ReactiveCommand<string, Unit> CommandProjectStop { get; set; }
        public ReactiveCommand<long, Unit> CommandToDoPlay { get; set; }
        public ReactiveCommand<long, Unit> CommandToDoStop { get; set; }
        public ReactiveCommand<Unit, Unit> CommandSync { get; set; }



        #endregion

        #region Local properties

        private ObservableCollection<tbl_OrganisationDetails> _findOrganisationDetails;
        public ObservableCollection<tbl_OrganisationDetails> FindOrganisationDetails
        {
            get => _findOrganisationDetails;
            set
            {
                _findOrganisationDetails = value;
                RaisePropertyChanged("FindOrganisationDetails");
            }
        }

        private ObservableCollection<tbl_Organisation_Projects> _findUserProjectList;
        public ObservableCollection<tbl_Organisation_Projects> FindUserProjectList
        {
            get => _findUserProjectList;
            set
            {
                _findUserProjectList = value;
                RaisePropertyChanged("FindUserProjectList");
            }
        }
        private ObservableCollection<Organisation_Projects> _getProjectsList;
        public ObservableCollection<Organisation_Projects> GetProjectsList
        {
            get { return _getProjectsList; }
            set
            {
                _getProjectsList = value;
                RaisePropertyChanged("GetProjectsList");
                // OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, value));
            }
        }

        private List<Organisation_Projects> _getProjectsList2 = new List<Organisation_Projects>();
        public List<Organisation_Projects> GetProjectsList2
        {
            get => _getProjectsList2;
            set
            {
                _getProjectsList2 = value;
                RaisePropertyChanged("GetProjectsList2");
                // OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, value));
            }
        }
        private tbl_OrganisationDetails _selectedOrganisationItems;
        public tbl_OrganisationDetails SelectedOrganisationItems
        {
            get => _selectedOrganisationItems;
            set
            {
                _selectedOrganisationItems = value;
                if (_selectedOrganisationItems != null)
                {
                    HeaderOrgId = SelectedOrganisationItems.OrganizationId;
                    HeaderOrgName = SelectedOrganisationItems.OrganizationName;
                    // string a = SelectedOrganisationItems.OrganizationId;
                    BindUserProjectlistByOrganizationID(SelectedOrganisationItems.OrganizationId);
                    orgdSelectedID = SelectedOrganisationItems.OrganizationId;
                    // BindUserToDoListFromApi(0,0,0);
                }

            }
        }

        private Organisation_Projects _selectedproject;
        public Organisation_Projects Selectedproject
        {
            get
            {
                return _selectedproject;
            }
            set
            {
                _selectedproject = value;
                if (Selectedproject != null)
                {

                    HeaderProjectName = Selectedproject.ProjectName;
                    HeaderProjectId = Selectedproject.ProjectId;
                    HeaderTime = Selectedproject.ProjectTime;
                    //string pid = "";
                    //if(!string.IsNullOrEmpty(projectIdSelected))
                    //{
                    //    pid=projectIdSelected;
                    //}
                    //else
                    //{
                    //    pid = Selectedproject.ProjectId;
                    //}
                    if (!CheckFromToDoList(Selectedproject.ProjectId) && Selectedproject.checkTodoApiCallOrNot == false)
                    {
                        int ProjectId = Convert.ToInt32(Selectedproject.ProjectId);
                        int OrganisationId = Convert.ToInt32(Selectedproject.OrganisationId);
                        int UserId = Convert.ToInt32(Selectedproject.UserId);
                        BindUserToDoListFromApi(ProjectId, OrganisationId, UserId);
                    }
                }
                //RaisePropertyChanged("Selectedproject");
            }
        }



        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            set
            {
                isPlaying = value;
                RaisePropertyChanged("IsPlaying");
            }
        }
        private string _headerTime;
        public string HeaderTime
        {
            get => _headerTime;
            set
            {
                _headerTime = value;
                RaisePropertyChanged("HeaderTime");
            }
        }

        private string _totalWorkTime;
        public string TotalWorkTime
        {
            get => _totalWorkTime;
            set
            {
                _totalWorkTime = value;
                RaisePropertyChanged("TotalWorkTime");
            }
        }
        private string _projectTime;
        public string ProjectTime
        {
            get => _projectTime;
            set
            {
                _projectTime = value;
                RaisePropertyChanged("ProjectTime");
            }
        }
        private string _todoprojectTime;
        public string TODOProjectTime
        {
            get => _todoprojectTime;
            set
            {
                _todoprojectTime = value;
                RaisePropertyChanged("TODOProjectTime");
            }
        }
        private string _headerProjectName;
        public string HeaderProjectName
        {
            get => _headerProjectName;
            set
            {
                _headerProjectName = value;
                RaisePropertyChanged("HeaderProjectName");
            }
        }
        private bool isStop;
        public bool IsStop
        {
            get => isStop;
            set
            {
                isStop = value;
                RaisePropertyChanged("IsStop");
            }
        }

        private int _selectedIndex;
        public int SelectedIndex
        {
            get
            {
                return _selectedIndex;
            }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
            }
        }
        private tbl_ServerTodoDetails tbl_ServerAddTodoDetails;
        private ObservableCollection<tbl_ServerTodoDetails> _toDoListData;
        public ObservableCollection<tbl_ServerTodoDetails> ToDoListData
        {
            get => _toDoListData;
            set
            {
                _toDoListData = value;
                RaisePropertyChanged("ToDoListData");
            }
        }

        private List<tbl_ServerTodoDetails> _getToDoListTemp = new List<tbl_ServerTodoDetails>();
        public List<tbl_ServerTodoDetails> GetToDoListTemp
        {
            get => _getToDoListTemp;
            set
            {
                _getToDoListTemp = value;
                RaisePropertyChanged("GetToDoListTemp");
            }
        }
        private tbl_ServerTodoDetails _selectedprojectToDo;
        public tbl_ServerTodoDetails SelectedprojectToDo
        {
            get
            {
                return _selectedprojectToDo;
            }
            set
            {
                _selectedprojectToDo = value;
                RaisePropertyChanged("SelectedprojectToDo");
            }
        }
        private string _currentVersion;
        public string CurrentVersion
        {
            get => _currentVersion;
            set
            {
                _currentVersion = value;
                RaisePropertyChanged("CurrentVersion");
            }
        }
        private string _headerProjectid;
        public string HeaderProjectId
        {
            get => _headerProjectid;
            set
            {
                _headerProjectid = value;
                RaisePropertyChanged("HeaderProjectId");
            }
        }

        private string _headerOrgId;
        public string HeaderOrgId
        {
            get => _headerOrgId;
            set
            {
                _headerOrgId = value;
                RaisePropertyChanged("HeaderOrgId");
            }
        }
        private string _headerOrgName;
        public string HeaderOrgName
        {
            get => _headerOrgName;
            set
            {
                _headerOrgName = value;
                RaisePropertyChanged("HeaderOrgName");
            }
        }

        private string _totalTodoCount;
        public string TotalTodoCount
        {
            get => _totalTodoCount;
            set
            {
                _totalTodoCount = value;
                RaisePropertyChanged("TotalTodoCount");
            }
        }

        private string _lastUpdateText;
        public string LastUpdateText
        {
            get => _lastUpdateText;
            set
            {
                _lastUpdateText = value;
                RaisePropertyChanged("LastUpdateText");
            }
        }
        #endregion
        #region constructor  
        public DashboardViewModel(Window window)
        {
            _window = window;
            ActivityTimerObject = new DispatcherTimer();
            ActivityTimerObject.Tick += new EventHandler(ActivityTimerObject_Tick);
            responseModel = new CommonResponseModel();

            themeManager = new ThemeManager();
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            CommandPlay = ReactiveCommand.Create(PlayTimer);
            CommandStop = ReactiveCommand.Create(playStop);
            CommandProjectPlay = ReactiveCommand.Create<string>(ProjectPlay);
            CommandProjectStop = ReactiveCommand.Create<string>(ProjectStop);
            CommandToDoPlay = ReactiveCommand.Create<long>(ToDoPlay);
            CommandToDoStop = ReactiveCommand.Create<long>(ToDoStop);
            CommandSync = ReactiveCommand.Create(SyncDataToServer);

            HeaderTime = "00:00:00";
            TotalWorkTime = "Total worked today : 00:00:00";

            string currentDate = DateTime.Now.ToString();
            LastUpdateText = "Last updated at: " + currentDate;
            //LastUpdateText = "Last updated at: " + currentDate.Substring(0, 8);

            SlotInterval = System.Configuration.ConfigurationSettings.AppSettings["SlotInterval"].ToInt32();
            IsPlaying = true;
            IsStop = false;
            objHeaderModel.SessionID = "";
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();

            BindUserOrganisationListFromApi();


            //if(ToDoListData!=null && ToDoListData.Count>0)
            // {
            //     GetActivitysynTimerDataFromLocalDB
            // }
            timerHeader = new System.Timers.Timer();
            timerproject = new System.Timers.Timer();
            timerToDo = new System.Timers.Timer();

            timerHeader.Interval = 1000;
            timerproject.Interval = 1000;
            timerToDo.Interval = 1000;

            timerproject.Elapsed += Timerproject_Elapsed;
            timerHeader.Elapsed += TimerHeader_Elapsed;
            timerToDo.Elapsed += TimerToDo_Elapsed;


            SlotTimerObject = new DispatcherTimer();
            SlotTimerObject.Tick += new EventHandler(SlotTimerObject_Elapsed);

            CurrentVersion = Common.Storage.GetAppVersion();
            listproject = _window.FindControl<ListBox>("LayoutRoot");
            listtodo = _window.FindControl<ListBox>("todolist");
            listOrg = _window.FindControl<ListBox>("orglist");

        }



        private void SlotTimerObject_Elapsed(object sender, EventArgs e)
        {
            SlotTimerObject.Stop();
            SlotTimerObject.Interval = new TimeSpan(0, SlotInterval, 0);
            BackgroundWorker backgroundWorkerObject = new BackgroundWorker();
            backgroundWorkerObject.DoWork += new DoWorkEventHandler(StartThreads);
            backgroundWorkerObject.RunWorkerAsync();
            SlotTimerObject.Start();
        }

        private void StartThreads(object sender, DoWorkEventArgs e)
        {
            tasks = new Task[1];
            tasks[0] = Task.Factory.StartNew(() => StartTimeIntervalAddToDB());
            // Give the tasks a second to start.
            // Thread.Sleep(1000);
        }

        public void StartTimeIntervalAddToDB()
        {
            DateTime oCurrentDate = DateTime.Now;
            new DashboardSqliteService().AddTimeIntervalToDB(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), oCurrentDate.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"));

        }
        public void StopTimeIntervalUpdateToDB()
        {
            SlotTimerObject.Stop();
            DateTime oCurrentDate = DateTime.Now;
            string strMinute = Convert.ToString(oCurrentDate.Minute);
            char[] charArr = strMinute.ToCharArray();
            int a = 0;
            if (oCurrentDate.Minute > 10)
            {
                a = Convert.ToInt32(charArr[1].ToString());
            }
            else
            {
                a = Convert.ToInt32(charArr[0].ToString());
            }

            int result = 10 - a;
            string IntervalEndTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");
            //string IntervalStratTime = Common.Storage.SlotTimerStartTime;
            //update tbl_KeyMouseTrack_Slot set  IntervalEndTime='04:06' where IntervalEndTime='04:10' and CreatedDate= '30/03/2021'                      
            new DashboardSqliteService().UpdateTimeIntervalToDB(IntervalEndTime, oCurrentDate.ToString("dd/MM/yyyy"));

        }
        private void TimeIntervalAddToDB()
        {
            DateTime oCurrentDate = DateTime.Now;
            string strMinute = Convert.ToString(oCurrentDate.Minute);
            char[] charArr = strMinute.ToCharArray();
            int a = 0;
            if (oCurrentDate.Minute > 10)
            {
                a = Convert.ToInt32(charArr[1].ToString());
            }
            else
            {
                a = Convert.ToInt32(charArr[0].ToString());
            }

            int result = 10 - a;
            //string LocalInterval = oCurrentDate.ToString("hh:mm") + "- " + oCurrentDate.AddMinutes(result).ToString("hh:mm");
            // Console.WriteLine(LocalInterval);
            // int Firsttimerduration = result * 60;
            Common.Storage.SlotTimerStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");
            new DashboardSqliteService().AddTimeIntervalToDB(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), oCurrentDate.AddMinutes(result).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"));
            SlotTimerObject.Interval = new TimeSpan(0, result, 0);
            SlotTimerObject.Start();

            //ActivityLogManager activity = new ActivityLogManager();
            // activity.CallActivityLog();

        }


        #endregion

        #region Methods  
        #region activityLog Methods
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
            LastUpdateText = "refreshing..";
            activityTasks = new Task[1];
            activityTasks[0] = Task.Factory.StartNew(() => GetActivityLogFromDB());
        }
        public void CallActivityLog()
        {
            ActivityTimerObject.Stop();
            string currentDate = DateTime.Now.ToString();
            LastUpdateText = "Last updated at: " + currentDate;
            //SendIntervalToServer();
            ActivitySyncTimerFromApi();
            // ActivityTimerObject.Interval = new TimeSpan(0, 5, 0);
            //ActivityTimerObject.Start();
        }
        public List<Intervals> GetIntervalsList(string startTime)
        {
            List<AppAndUrl> _appAndUrls = new List<AppAndUrl>();
            List<AppAndUrl> finalAppAndUrl = new List<AppAndUrl>();
            AppAndUrl appAnd;
            Models.WriteDTO.Location _location;
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
                        _location = new Models.WriteDTO.Location()
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
                            todo_id = Convert.ToString(item.ToDoId)
                        };
                        activityLogRequests = new List<ActivityLogRequestEntity>();
                        activityLogRequests.Add(activityLogRequestEntity);
                        finallist.AddRange(activityLogRequests);
                    }
                }
                string strJson = JsonConvert.SerializeObject(finallist);
                objHeaderModel = new HeaderModel();
                _baseURL = Configurations.UrlConstant + Configurations.ActivityLogApiConstant;
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
                //throw new Exception(ex.Message);
            }
        }
        public void GetActivityLogFromDB()
        {
            SendIntervalToServer();
            string currentDate = DateTime.Now.ToString();
            LastUpdateText = "Last updated at: " + currentDate;
            //  BindUserActivitySyncTimerFromApi();

        }
        #endregion
        #region Zebra Pattern
        public void AddZebraPatternToToDoList()
        {
            var colorBrushWhite = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#FFFFFF"));

            var colorBrushMetalic = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#f2f3f4"));

            var colorBrushBlue = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#D3E5FF"));//#3cdfff

            var colorBrushCompleted = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#E5E5E5"));
            try
            {
                if (listtodo != null)
                {
                    var data = (ObservableCollection<tbl_ServerTodoDetails>)listtodo.Items;
                    for (int i = 0; i <= listtodo.ItemContainerGenerator.Containers.Count() - 1; i++)
                    {
                        if (i % 2 == 0)
                        {
                            var listBoxItem = listtodo.ItemContainerGenerator.ContainerFromIndex(listtodo.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushMetalic;
                            if (IsPlaying)
                            {
                                if (data[i].Id == ToDoSelectedID)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }
                            if (data[i].IsCompleted == 1)
                                (listBoxItem as ListBoxItem).Background = colorBrushCompleted;

                        }
                        else
                        {
                            var listBoxItem = listtodo.ItemContainerGenerator.ContainerFromIndex(listtodo.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushWhite;
                            if (IsPlaying)
                            {
                                if (data[i].Id == ToDoSelectedID)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }
                            if (data[i].IsCompleted == 1)
                                (listBoxItem as ListBoxItem).Background = colorBrushCompleted;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }

        }
        public void AddZebraPatternToProjectList()
        {
            var colorBrushWhite = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#FFFFFF"));

            var colorBrushMetalic = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#f2f3f4"));

            var colorBrushBlue = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#D3E5FF"));
            try
            {
                if (listproject != null)
                {
                    for (int i = 0; i <= listproject.ItemContainerGenerator.Containers.Count() - 1; i++)
                    {
                        var data = (ObservableCollection<Organisation_Projects>)listproject.Items;
                        if (i % 2 == 0)
                        {
                            var listBoxItem = listproject.ItemContainerGenerator.ContainerFromIndex(listproject.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushMetalic;
                            if (IsPlaying)
                            {
                                if (data[i].ProjectId == HeaderProjectId)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }

                        }
                        else
                        {
                            var listBoxItem = listproject.ItemContainerGenerator.ContainerFromIndex(listproject.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushWhite;

                            if (IsPlaying)
                            {
                                if (data[i].ProjectId == HeaderProjectId)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }


        }
        public void ZebraPatternToOrganizationList()
        {
            var colorBrushWhite = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#FFFFFF"));

            var colorBrushMetalic = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#f2f3f4"));

            var colorBrushBlue = (Avalonia.Media.SolidColorBrush)(new Avalonia.Media.BrushConverter().ConvertFromString("#D3E5FF"));
            try
            {
                if (listOrg != null)
                {
                    for (int i = 0; i <= listOrg.ItemContainerGenerator.Containers.Count() - 1; i++)
                    {
                        var data = (ObservableCollection<tbl_OrganisationDetails>)listOrg.Items;
                        if (i % 2 == 0)
                        {
                            var listBoxItem = listOrg.ItemContainerGenerator.ContainerFromIndex(listOrg.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushMetalic;
                            if (IsPlaying)
                            {
                                if (data[i].OrganizationId == HeaderOrgId)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }

                        }
                        else
                        {
                            var listBoxItem = listOrg.ItemContainerGenerator.ContainerFromIndex(listOrg.ItemContainerGenerator.Containers.ToList()[i].Index);
                            (listBoxItem as ListBoxItem).Background = colorBrushWhite;

                            if (IsPlaying)
                            {
                                if (data[i].OrganizationId == HeaderOrgId)
                                    (listBoxItem as ListBoxItem).Background = colorBrushBlue;
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }


        }
        #endregion
        #region HeaderPlayTimer       
        public void PlayTimer()
        {
            timerHeader.Stop();
            timerproject.Stop();
            timerToDo.Stop();


            TimeIntervalAddToDB();

            timerHeader.Start();
            IsStop = true;
            IsPlaying = false;
            if (!string.IsNullOrEmpty(projectIdSelected))
            {
                string _time = GetTimeFromProject(projectIdSelected);
                string[] arry = _time.Split(':');
                if (arry[1] == "00")
                {
                    m2 = 0;
                }
                else
                {
                    m2 = Convert.ToInt32(arry[1]);
                }
                s2 = 0;
                if (m2 == 60)
                {
                    m2 = 0;
                    h2 += 1;
                }
                ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
                UpdateProjectList(projectIdSelected);
                AddUpdateProjectTimeToDB(true);
                timerproject.Start();

            }
            //if (ToDoSelectedID > 0)
            //{
            //    string p;
            //    string _time1 = GetTimeFromToDoList(ToDoSelectedID, out p);
            //    string[] arry1 = _time1.Split(':');
            //    if (arry1[2] == "00")
            //    {
            //        s3 = 0;
            //    }
            //    else
            //    {
            //        s3 = Convert.ToInt32(arry1[2]);
            //    }
            //    timerToDo.Start();

            //}
        }
        public void playStop()
        {

            timerHeader.Stop();
            timerproject.Stop();
            timerToDo.Stop();
            StopTimeIntervalUpdateToDB();
            AddUpdateProjectTimeToDB(false);
            IsStop = false;
            IsPlaying = true;
            UpdateProjectList(projectIdSelected, "AllStop");
            TODoStopUpdate(ToDoSelectedID);

        }
        private void TimerHeader_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            s1 += 1;
            if (s1 == 60)
            {
                s1 = 0;
                m1 += 1;
            }
            if (m1 == 60)
            {
                m1 = 0;
                h1 += 1;
            }
            HeaderTime = String.Format("{0}:{1}:{2}", h1.ToString().PadLeft(2, '0'), m1.ToString().PadLeft(2, '0'), s1.ToString().PadLeft(2, '0'));
            TotalWorkTime = "Total worked today : " + HeaderTime;

        }
        #endregion

        #region ProjectPlayTimer
        public void RefreshSelectedItem(string p, bool playStop)
        {
            var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == p);
            if (data != null)
            {
                // data.checkTodoApiCallOrNot = playStop;
                Selectedproject = data;
            }

        }
        public void ProjectPlay(string obj)
        {

            timerHeader.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;


            if (Common.Storage.IsToDoRuning == true)
            {
                timerToDo.Stop();
                StopTimeIntervalUpdateToDB();
                AddUpdateProjectTimeToDBByToDoId(false);

                TODoStopUpdate(ToDoSelectedID);
                Common.Storage.IsToDoRuning = false;
                ToDoSelectedID = 0;
            }
            if (Common.Storage.IsProjectRuning == true)
            {
                if (!string.IsNullOrEmpty(projectIdSelected))
                {
                    StopTimeIntervalUpdateToDB();
                    AddUpdateProjectTimeToDB(false);
                }
            }
            TimeIntervalAddToDB();

            projectIdSelected = string.Empty;
            projectIdSelected = obj;
            AddUpdateProjectTimeToDB(true);
            Common.Storage.IsProjectRuning = true;
            RefreshSelectedItem(obj, true);


            string _time = GetTimeFromProject(obj);
            string[] arry = _time.Split(':');
            //if (arry[1] == "00")
            //{
            //    m2 = 0;               
            //}
            //else
            //{
            //    m2 = Convert.ToInt32(arry[1]);               
            //}
            //s2 = 0;
            //if (m2 == 60)
            //{
            //    m2 = 0;
            //    h2 += 1;
            //}


            h2 = Convert.ToInt32(arry[0]);
            m2 = Convert.ToInt32(arry[1]);
            s2 = Convert.ToInt32(arry[2]);





            ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
            HeaderTime = ProjectTime;

            UpdateProjectList(projectIdSelected);
            //  timerHeader.Start();
            timerproject.Start();
            IsStop = true;
            IsPlaying = false;

        }
        public void ProjectStop(string obj)
        {
            StopTimeIntervalUpdateToDB();
            projectIdSelected = obj;
            AddUpdateProjectTimeToDB(false);
            // RefreshSelectedItem(obj, false);
            timerHeader.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            ProjectStopUpdate(projectIdSelected);
            if (Common.Storage.IsToDoRuning == true)
            {
                ToDoStop(ToDoSelectedID);
            }
            Common.Storage.IsProjectRuning = false;
            // UpdateProjectList(projectIdSelected, "ProjectStop");


        }
        private void Timerproject_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //s2 = 0;            
            //m2 += 1;            
            //if (m2 == 60)
            //{
            //    m2 = 0;
            //    h2 += 1;
            //}
            s2 += 1;
            TotalSecound += 1;
            if (s2 == 60)
            {
                s2 = 0;
                m2 += 1;
                TotalSecound = 0;
                TotalSMinute += 1;
            }
            if (m2 == 60)
            {
                m2 = 0;
                h2 += 1;
                TotalSMinute = 0;
                Totalhour += 1;
            }
            ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
            HeaderTime = ProjectTime;
            string tt = String.Format("{0}:{1}:{2}", Totalhour.ToString().PadLeft(2, '0'), TotalSMinute.ToString().PadLeft(2, '0'), TotalSecound.ToString().PadLeft(2, '0'));
            TotalWorkTime = "Total worked today : " + tt;
            UpdateProjectList(projectIdSelected);
        }
        public void ProjectStopUpdate(string pid)
        {
            ObservableCollection<Organisation_Projects> ProjectListFinal = new ObservableCollection<Organisation_Projects>();
            foreach (var item in GetProjectsList)
            {
                if (item.ProjectId == pid)
                {
                    item.ProjectPlayIcon = true;
                    item.ProjectStopIcon = false;
                    item.ProjectTime = ProjectTime;
                    item.checkTodoApiCallOrNot = false;
                }
                //ProjectListFinal.Remove(item);
                //ProjectListFinal.Add(item);
            }

            if (Selectedproject != null)
            {
                GetProjectsList = null;
                GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
            }
        }
        public void UpdateProjectList(string selectedItem, string commandType = "")
        {
            try
            {
                if (GetProjectsList != null)
                {
                    foreach (var item in GetProjectsList)
                    {
                        if (commandType == "AllStop")
                        {
                            item.ProjectPlayIcon = true;
                            item.ProjectStopIcon = false;
                            continue;
                        }
                        else
                        {
                            if (item.ProjectId == selectedItem)
                            {
                                item.ProjectPlayIcon = false;
                                item.ProjectStopIcon = true;
                                item.ProjectTime = ProjectTime;
                                item.checkTodoApiCallOrNot = true;
                            }
                            else
                            {
                                item.ProjectPlayIcon = true;
                                item.ProjectStopIcon = false;
                                item.checkTodoApiCallOrNot = false;
                            }
                        }
                    }
                }
                if (Selectedproject != null)
                {
                    GetProjectsList = null;
                    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                }

            }
            catch (Exception)
            {

            }
            // timerproject.Start();

        }
        public string GetTimeFromProject(string projectid)
        {
            string _time = "";
            var p = GetProjectsList.FirstOrDefault(x => x.ProjectId == projectid);
            if (p != null)
            {
                if (string.IsNullOrEmpty(p.ProjectTime))
                {
                    _time = "00:00:00";
                }
                else
                {
                    _time = p.ProjectTime;
                }
            }
            return _time;
        }
        public bool CheckFromToDoList(string projectId)
        {
            try
            {

                var p = ToDoListData.FirstOrDefault(x => x.CurrentProjectId == projectId);
                if (p != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {

                return false;
            }
        }

        public void AddUpdateProjectTimeToDB(bool boolVal)
        {
            try
            {
                string t;
                DateTime oCurrentDate = DateTime.Now;
                if (boolVal)
                {
                    t = "";
                    Common.Storage.ProjectStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
                }
                else
                {
                    t = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");

                }
                long? todo;
                if (ToDoSelectedID == 0)
                {
                    todo = null;
                }
                else
                {
                    todo = ToDoSelectedID;
                }
                if (Common.Storage.IsToDoRuning == false)
                {
                    todo = null;
                }
                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                    ProjectId = projectIdSelected,
                    OrgId = orgdSelectedID,
                    SelfieVerification = "true",
                    SourceType = "1",
                    Stop = t,
                    TimeType = "1",
                    ToDoId = todo,
                    IntervalTime = 0,
                    Sno = 0
                };

                new DashboardSqliteService().SaveStartStopProjectTimeINLocalDB(tblTimer, boolVal);
                ////tbl_ProjectDetails tbl_Project;
                ////string TotalHours;
                ////if (boolVal)
                ////{
                ////    TotalHours = "00:00:00";
                ////}
                ////else
                ////{
                ////    TimeSpan diff = DateTime.Parse(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")) - DateTime.Parse(Common.Storage.ProjectStartTime);
                ////    var Seconds = diff.Seconds;
                ////    TimeSpan _time2 = TimeSpan.FromSeconds(Seconds);
                ////    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                ////    _time2.Hours,
                ////    _time2.Minutes,
                ////    _time2.Seconds);
                ////    TotalHours = logtime;
                ////}

                ////string _time = GetTimeFromProject(obj);
                ////string[] arry = _time.Split(':');

                ////tbl_Project = new tbl_ProjectDetails()
                ////{
                ////    ProjectId =Convert.ToInt32(tblTimer.ProjectId),
                ////    OrganizationId = Convert.ToInt32(tblTimer.OrgId),
                ////    SNo = 0,
                ////    IsOffline = 0,
                ////    TotalWorkedHours = TotalHours,
                ////    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                ////};
                ///new DashboardSqliteService().SaveTimeIntbl_ProjectDetailsDB(tbl_Project);

            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }

        #endregion

        #region ToDoPlayTimer
        public void AddUpdateProjectTimeToDBByToDoId(bool boolVal, string projectid = "")
        {
            try
            {
                string t;
                DateTime oCurrentDate = DateTime.Now;
                if (boolVal)
                {
                    t = "";
                    Common.Storage.ProjectStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");
                }
                else
                {
                    t = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");

                }
                long? todo;
                if (ToDoSelectedID == 0)
                {
                    todo = null;
                }
                else
                {
                    todo = ToDoSelectedID;
                }

                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                    ProjectId = projectid,
                    OrgId = orgdSelectedID,
                    SelfieVerification = "true",
                    SourceType = "1",
                    Stop = t,
                    TimeType = "1",
                    ToDoId = todo,
                    IntervalTime = 0,
                    Sno = 0
                };

                new DashboardSqliteService().AddUpdateProjectTimeINLocalDBByToDoID(tblTimer, boolVal);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        private void TimerToDo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            s3 += 1;
            if (s3 == 60)
            {
                s3 = 0;
                m3 += 1;
            }
            if (m3 == 60)
            {
                m3 = 0;
                h3 += 1;
            }
            TODOProjectTime = String.Format("{0}:{1}:{2}", h3.ToString().PadLeft(2, '0'), m3.ToString().PadLeft(2, '0'), s3.ToString().PadLeft(2, '0'));
            UpdateToDoList(ToDoSelectedID);
        }
        public void ToDoPlay(long TodoID)
        {
            timerToDo.Stop();
            timerproject.Stop();
            timerHeader.Stop();
            IsStop = false;
            IsPlaying = true;

            if (Common.Storage.IsProjectRuning == true)
            {
                if (!string.IsNullOrEmpty(projectIdSelected))
                {
                    StopTimeIntervalUpdateToDB();
                    AddUpdateProjectTimeToDB(false);
                }
            }

            if (Common.Storage.IsToDoRuning == true)
            {
                if (ToDoSelectedID > 0)
                {
                    StopTimeIntervalUpdateToDB();
                    AddUpdateProjectTimeToDBByToDoId(false);
                }
            }

            ToDoSelectedID = 0;
            ToDoSelectedID = TodoID;

            // RefreshSelectedItem(obj, true);
            string p;
            string _time = GetTimeFromToDoList(TodoID, out p);
            string[] arry = _time.Split(':');
            s3 = 0;
            m3 = 0;
            h3 = 0;
            if (arry[2] == "00")
            {
                s3 = 0;
            }
            else
            {
                s3 = Convert.ToInt32(arry[2]);
            }

            if (s3 == 60)
            {
                s3 = 0;
                m3 += 1;
            }
            if (m3 == 60)
            {
                m3 = 0;
                h3 += 1;
            }
            TODOProjectTime = String.Format("{0}:{1}:{2}", h3.ToString().PadLeft(2, '0'), m3.ToString().PadLeft(2, '0'), s3.ToString().PadLeft(2, '0'));
            UpdateToDoList(ToDoSelectedID);



            TimeIntervalAddToDB();
            AddUpdateProjectTimeToDBByToDoId(true, p);
            timerToDo.Start();
            timerHeader.Start();
            //timerproject.Start(); 
            Common.Storage.IsToDoRuning = true;
            ProjectPlayFromToDO(p);
            IsStop = true;
            IsPlaying = false;

        }
        public void ProjectPlayFromToDO(string projectId)
        {
            projectIdSelected = projectId;
            RefreshSelectedItem(projectId, true);
            string _time = GetTimeFromProject(projectId);
            string[] arry = _time.Split(':');
            if (arry[1] == "00")
            {
                m2 = 0;
            }
            else
            {
                m2 = Convert.ToInt32(arry[1]);
            }
            s2 = 0;
            if (m2 == 60)
            {
                m2 = 0;
                h2 += 1;
            }
            ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
            UpdateProjectList(projectIdSelected);
            timerproject.Start();
        }
        public void ToDoStop(long TodoID)
        {
            timerToDo.Stop();
            timerHeader.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            ToDoSelectedID = TodoID;
            StopTimeIntervalUpdateToDB();
            AddUpdateProjectTimeToDBByToDoId(false);



            TODoStopUpdate(ToDoSelectedID);
            ProjectStopUpdate(projectIdSelected);
            Common.Storage.IsToDoRuning = false;
        }
        public string GetTimeFromToDoList(long todoId, out string p)
        {
            string _time = "";
            p = "";
            var t = ToDoListData.FirstOrDefault(x => x.Id == todoId);
            if (t != null)
            {
                if (string.IsNullOrEmpty(t.ToDoTimeConsumed))
                {
                    _time = "00:00:00";
                }
                else
                {
                    _time = t.ToDoTimeConsumed;
                }
                p = t.CurrentProjectId;
            }
            return _time;
        }
        public void UpdateToDoList(long CurrentToDoId)
        {
            try
            {

                if (ToDoListData != null && ToDoListData.Count > 0)
                {


                    foreach (var item in ToDoListData)
                    {
                        if (item.Id == CurrentToDoId)
                        {
                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = true;
                            item.ToDoTimeConsumed = TODOProjectTime;
                            SelectedprojectToDo = item;
                        }
                        else
                        {
                            item.ToDoPlayIcon = true;
                            item.ToDoStopIcon = false;
                        }

                        if (item.Site == "OnSite")
                        {
                            item.SiteColor = themeManager.OnSiteColor;
                        }
                        else
                        {
                            item.SiteColor = themeManager.OffSiteColor;
                        }
                        //ToDoListData.Add(item);
                    }
                }
                if (SelectedprojectToDo != null)
                {
                    ToDoListData = null;
                    GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
                    ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);

                }

            }
            catch (Exception)
            {


            }
        }
        public void TODoStopUpdate(long CurrentToDoId)
        {
            try
            {
                if (ToDoListData != null && ToDoListData.Count > 0)
                {
                    foreach (var item in ToDoListData)
                    {
                        if (item.Id == CurrentToDoId)
                        {
                            item.ToDoPlayIcon = true;
                            item.ToDoStopIcon = false;
                            //item.ToDoTimeConsumed = TODOProjectTime;
                        }

                        if (item.Site == "OnSite")
                        {
                            item.SiteColor = themeManager.OnSiteColor;
                        }
                        else
                        {
                            item.SiteColor = themeManager.OffSiteColor;
                        }
                        //ToDoListData.Add(item);
                    }
                }
                if (SelectedprojectToDo != null)
                {
                    ToDoListData = null;
                    GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
                    ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);
                }
            }
            catch (Exception)
            {

            }
        }
        #endregion
        #region OrganisationDetails
        public async void BindUserToDoListFromApi(int projectId, int organizationId, int userId)
        {
            Common.Storage.CurrentOrganisationId = organizationId;
            Common.Storage.CurrentProjectId = projectId;
            Common.Storage.CurrentUserId = userId;

            _baseURL = Configurations.UrlConstant + Configurations.UserToDoListApiConstant;
            toDoListResponseModel = new ToDoListResponseModel();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = Common.Storage.TokenId;
            ToDoListRequestModel _toDoList = new ToDoListRequestModel()
            {
                project_id = projectId,
                organization_id = organizationId,
                userid = userId
            };
            toDoListResponseModel = await _services.GetUserToDoListAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _toDoList);
            if (toDoListResponseModel.Response.Code == "200")
            {
                BaseService<tbl_ServerTodoDetails> dbService = new BaseService<tbl_ServerTodoDetails>();
                dbService.Delete(new tbl_ServerTodoDetails());
                foreach (var item in toDoListResponseModel.Response.Data)
                {
                    tbl_ServerAddTodoDetails = new tbl_ServerTodoDetails()
                    {
                        ToDoName = item.name.ToStrVal().Replace("'", "''"),
                        CurrentUserId = Convert.ToString(item.user_id),
                        CurrentProjectId = Convert.ToString(item.project_id),
                        CurrentOrganisationId = Convert.ToString(item.organization_id),
                        StartDate = Convert.ToString(item.startDate),
                        EndDate = Convert.ToString(item.endDate),
                        EstimatedHours = Convert.ToString(item.estiamtedHours),
                        Description = item.description.ToStrVal().Replace("'", "''"),
                        IsCompleted = item.complete,
                        Privacy = item.privacy,
                        Site = item.site,
                        IsOffline = false,
                        ToDoTimeConsumed = "",
                        Id = item.id
                    };
                    // _OrganisationDetails.Add(tbl_OrganisationDetails);
                    new DashboardSqliteService().InsertUserToDoList(tbl_ServerAddTodoDetails);
                }
                BindUseToDoListFromLocalDB(projectId);
                // new DashboardSqliteService().InsertUserOrganisation(_OrganisationDetails);

            }
        }

        void BindUseToDoListFromLocalDB(int CurrentProjectId)
        {
            try
            {
                ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>();
                tbl_ServerTodoDetails tbl_ServerTodo;
                ObservableCollection<tbl_ServerTodoDetails> FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>();
                FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>(new DashboardSqliteService().GetToDoListData(CurrentProjectId));
                ToDoListData.Clear();
                GetToDoListTemp.Clear();
                foreach (var item in FindUserToDoListFinal)
                {
                    item.ToDoTimeConsumed = "00:00:00";
                    item.ToDoPlayIcon = true;
                    item.ToDoStopIcon = false;
                    if (item.Site == "OnSite")
                    {
                        item.SiteColor = themeManager.OnSiteColor;
                    }
                    else
                    {
                        item.SiteColor = themeManager.OffSiteColor;
                    }
                    ToDoListData.Add(item);
                    GetToDoListTemp.Add(item);
                }
                // GetToDoListTemp = new ObservableCollection<tbl_ServerTodoDetails>(ToDoListData);
                RaisePropertyChanged("ToDoListData");
                int countdata = ToDoListData.Count;
                TotalTodoCount = " Showing " + countdata + " of " + countdata + " tasks";
                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                // GetActivitysynTimerDataFromLocalDB();
                AddZebraPatternToToDoList();
                if (ToDoListData.Count > 0)
                {
                    listtodo.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        async void BindUserOrganisationListFromApi()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.UserOrganisationListApiConstant;
                organisationListResponse = new UserOrganisationListResponse();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                List<tbl_OrganisationDetails> _OrganisationDetails = new List<tbl_OrganisationDetails>();
                organisationListResponse = await _services.GetAsyncData_GetApi(new Get_API_Url().UserOrganizationlist(_baseURL), true, objHeaderModel, organisationListResponse);
                if (organisationListResponse.response.code == "200")
                {
                    BaseService<tbl_OrganisationDetails> dbService = new BaseService<tbl_OrganisationDetails>();
                    dbService.Delete(new tbl_OrganisationDetails());
                    foreach (var item in organisationListResponse.response.data)
                    {
                        tbl_OrganisationDetails = new tbl_OrganisationDetails()
                        {
                            OrganizationId = Convert.ToString(item.id),
                            OrganizationName = item.name.ToStrVal().Replace("'", "''")
                        };
                        // _OrganisationDetails.Add(tbl_OrganisationDetails);
                        new DashboardSqliteService().InsertUserOrganisation(tbl_OrganisationDetails);
                    }
                    //  new DashboardSqliteService().InsertUserOrganisation(_OrganisationDetails);

                }
                BindUserOrganisationListFromLocalDB();
                //Get First OrganisationDetails here
                if (FindOrganisationDetails.Count > 0)
                {
                    dynamic firstOrDefault = FindOrganisationDetails.FirstOrDefault();
                    SelectedOrganisationItems = firstOrDefault;
                    RaisePropertyChanged("SelectedOrganisationItems");
                }


            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        async void BindUserProjectlistByOrganizationID(string OrganizationID)
        {
            try
            {


                _baseURL = Configurations.UrlConstant + Configurations.UserProjectlistByOrganizationIDApiConstant;
                userProjectlistResponse = new UserProjectlistByOrganizationIDResponse();
                objHeaderModel = new HeaderModel();
                OrganizationDTOEntity entity = new OrganizationDTOEntity() { organization_id = OrganizationID };
                objHeaderModel.SessionID = Common.Storage.TokenId;
                userProjectlistResponse = await _services.GetUserProjectlistByOrganizationIDAsync(new Get_API_Url().UserOrganizationlist(_baseURL), true, objHeaderModel, entity);
                if (userProjectlistResponse.response.code == "200")
                {
                    if (userProjectlistResponse.response.data.Count > 0)
                    {
                        List<tbl_Organisation_Projects> _OrganisationProjects = new List<tbl_Organisation_Projects>();
                        BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                        dbService.Delete(new tbl_Organisation_Projects());
                        foreach (var item in userProjectlistResponse.response.data)
                        {
                            tbl_organisation_Projects = new tbl_Organisation_Projects()
                            {
                                ProjectId = Convert.ToString(item.id),
                                ProjectName = item.name.ToStrVal().Replace("'", "''"),
                                OrganisationId = Convert.ToString(item.organization_id),
                                UserId = Convert.ToString(item.user_id)
                            };
                            // _OrganisationProjects.Add(tbl_organisation_Projects);
                            new DashboardSqliteService().InsertUserProjectsByOrganisationID(tbl_organisation_Projects);

                        }
                        //new DashboardSqliteService().InsertUserProjectsByOrganisationID(_OrganisationProjects);
                    }

                }
                BindUserProjectListFromLocalDB(OrganizationID);
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        void BindUserOrganisationListFromLocalDB()
        {
            try
            {
                FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
                FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>(new DashboardSqliteService().GetOrganisation());
                RaisePropertyChanged("FindOrganisationDetails");
                ZebraPatternToOrganizationList();
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        void BindUserProjectListFromLocalDB(string OrganisationId)
        {
            try
            {
                // FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
                GetProjectsList = new ObservableCollection<Organisation_Projects>();


                Organisation_Projects projects;
                ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
                foreach (var item in FindUserProjectListFinal)
                {
                    projects = new Organisation_Projects()
                    {
                        ProjectId = item.ProjectId,
                        ProjectName = item.ProjectName.Trim(),
                        OrganisationId = item.OrganisationId,
                        ProjectTime = "00:00:00",
                        ProjectPlayIcon = true,
                        ProjectStopIcon = false,
                        UserId = item.UserId
                    };
                    GetProjectsList.Add(projects);
                    GetProjectsList2.Add(projects);
                }
                RaisePropertyChanged("GetProjectsList");
                //  BindUserActivitySyncTimerFromApi();

                AddZebraPatternToProjectList();
                if (GetProjectsList.Count > 0)
                {

                    listproject.SelectedIndex = 0;
                    //dynamic firstvalueOfProject = GetProjectsList.FirstOrDefault();
                    //if (firstvalueOfProject != null)
                    //{
                    //    Selectedproject = firstvalueOfProject;
                    //}
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public void SerachToDoDataList(string searchtext, int projectid, int organization_id)
        {
            if (searchtext != null)
            {
                BaseService<tbl_ServerTodoDetails> dbService = new BaseService<tbl_ServerTodoDetails>();
                ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>();
                ObservableCollection<tbl_ServerTodoDetails> FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>();
                FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>(dbService.SearchToDoByString(searchtext, projectid, organization_id));
                foreach (var item in FindUserToDoListFinal)
                {
                    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    {
                        item.ToDoTimeConsumed = "00:00:00";
                    }


                    item.ToDoPlayIcon = true;
                    item.ToDoStopIcon = false;
                    if (item.Site == "OnSite")
                    {
                        item.SiteColor = themeManager.OnSiteColor;
                    }
                    else
                    {
                        item.SiteColor = themeManager.OffSiteColor;
                    }
                    ToDoListData.Add(item);
                }
            }
            else
            {
                BindUseToDoListFromLocalDB(projectid);
            }
            RaisePropertyChanged("ToDoListData");
            int countdata = ToDoListData.Count;
            TotalTodoCount = " Showing " + countdata + " of " + countdata + " tasks";
        }
        public void SerachProjectDataList(string searchtext, int projectid = 272, int organization_id = 245)
        {
            if (searchtext != null)
            {
                Organisation_Projects projects;

                BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                GetProjectsList = new ObservableCollection<Organisation_Projects>();
                ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(dbService.SearchProjectByString(searchtext, projectid, organization_id));
                foreach (var item in FindUserProjectListFinal)
                {
                    projects = new Organisation_Projects()
                    {
                        ProjectId = item.ProjectId,
                        ProjectName = item.ProjectName.Trim(),
                        OrganisationId = item.OrganisationId,
                        ProjectTime = "00:00:00",
                        ProjectPlayIcon = true,
                        ProjectStopIcon = false,
                        UserId = item.UserId
                    };
                    // item.ProjectId = Convert.ToString("00:00:00");
                    GetProjectsList.Add(projects);
                }
                RaisePropertyChanged("GetProjectsList");
            }
            else
            {
                BindUserProjectListFromLocalDB(Convert.ToString(organization_id));
            }
            RaisePropertyChanged("ToDoListData");
        }
        public tbl_Timer GetProjectTimerFromDB(string projectID)
        {
            tbl_Timer t = new tbl_Timer();

            BaseService<tbl_Timer> dbService = new BaseService<tbl_Timer>();
            dbService.GetById(projectID, "ProjectId");
            return t;
        }

        public bool CheckProjectExistOrNotInLocalDB(string OrgID)
        {
            tbl_Organisation_Projects t = new tbl_Organisation_Projects();
            BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
            t = dbService.GetById(OrgID, "OrganisationId");
            if (t != null)
            {
                if (!string.IsNullOrEmpty(t.OrganisationId))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        public bool CheckTodoExistOrNotInLocalDB(int pid)
        {
            bool a = false;
            tbl_ServerTodoDetails tbl_ServerTodo = new tbl_ServerTodoDetails();
            BaseService<tbl_ServerTodoDetails> dbService = new BaseService<tbl_ServerTodoDetails>();
            tbl_ServerTodo = dbService.GetById(pid, "CurrentProjectId");
            if (tbl_ServerTodo != null)
            {
                if (!string.IsNullOrEmpty(tbl_ServerTodo.CurrentProjectId))
                {
                    a = true;
                }
                else
                {
                    a = false;
                }
            }
            else
            {
                a = false;
            }
            return a;
        }
        public void ClosedAllTimer()
        {
            projectIdSelected = string.Empty;
            ToDoSelectedID = 0;
            timerHeader.Close();
            timerproject.Close();
            timerToDo.Close();
        }

        public async void BindUserActivitySyncTimerFromApi()
        {
            tbl_ServerTodoDetails tbl_ServerTodo;
            _baseURL = Configurations.UrlConstant + Configurations.UserActivitySyncTimertApiConstant;
            activitySyncTimerResponseModel = new ActivitySyncTimerResponseModel();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = Common.Storage.TokenId;

            string currentDate = DateTime.Now.ToString("yyyy'-'MM'-'dd''");
            ActivitySyncTimerRequestModel _activitySyncTime = new ActivitySyncTimerRequestModel()
            {
                date = "2021-04-03"
            };


            activitySyncTimerResponseModel = await _services.GetActivitysynTimerDataAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _activitySyncTime);
            if (activitySyncTimerResponseModel.response.code == "200")
            {
                if (GetProjectsList != null)
                {
                    GetProjectsList.Clear();
                }
                //List<Plist> plst = new List<Plist>();
                //List<Tlist> tlst = new List<Tlist>();

                //Plist p;
                //Tlist t;

                foreach (var a in activitySyncTimerResponseModel.response.data)
                {
                    //neww code
                    //if (a.projectId > 0)
                    //{
                    //    if (a.todoId != null && a.todoId > 0)
                    //    {
                    //        t = new Tlist()
                    //        {
                    //            ProjectID = a.projectId,
                    //            timeLog = a.timeLog,
                    //            todoId = a.todoId
                    //        };
                    //        tlst.Add(t);
                    //    }
                    //    else
                    //    {
                    //        p = new Plist()
                    //        {
                    //            ProjectID = a.projectId,
                    //            timeLog = a.timeLog
                    //        };
                    //        plst.Add(p);
                    //    }

                    //}


                    var data = GetProjectsList2.FirstOrDefault(x => x.ProjectId == Convert.ToString(a.projectId));
                    if (data != null)
                    {
                        if (a.projectId == Convert.ToInt32(data.ProjectId))
                        {
                            foreach (var item in GetProjectsList2)
                            {
                                if (a.projectId == Convert.ToInt32(item.ProjectId))
                                {
                                    item.ProjectId = item.ProjectId;
                                    item.ProjectName = item.ProjectName.Trim();
                                    item.OrganisationId = item.OrganisationId;
                                    TimeSpan t = TimeSpan.FromMilliseconds(Convert.ToInt32(a.timeLog));
                                    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds,
                                    t.Milliseconds);
                                    item.ProjectTime = logtime;
                                    item.ProjectPlayIcon = true;
                                    item.ProjectStopIcon = false;
                                    GetProjectsList.Add(item);
                                }
                            }

                        }
                    }
                    var tododata = GetToDoListTemp.FirstOrDefault(x => x.Id == Convert.ToInt32(a.todoId));
                    if (a.todoId != null)
                    {
                        foreach (var item in GetToDoListTemp)
                        {
                            if (Convert.ToInt32(a.todoId) == Convert.ToInt32(item.Id))
                            {
                                item.ToDoPlayIcon = true;
                                item.ToDoStopIcon = false;
                                if (item.Site == "OnSite")
                                {
                                    item.SiteColor = themeManager.OnSiteColor;
                                }
                                else
                                {
                                    item.SiteColor = themeManager.OffSiteColor;
                                }
                                TimeSpan t = TimeSpan.FromMilliseconds(Convert.ToInt32(a.timeLog));
                                string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                                t.Hours,
                                t.Minutes,
                                t.Seconds,
                                t.Milliseconds);
                                item.ToDoTimeConsumed = logtime;
                            }
                            tbl_ServerTodo = new tbl_ServerTodoDetails();
                            tbl_ServerTodo = item;
                            ToDoListData.Add(tbl_ServerTodo);

                        }
                    }
                }

                //foreach (var x in tlst)
                //{
                //    p = new Plist()
                //    {
                //        ProjectID = x.ProjectID,
                //        timeLog = x.timeLog
                //    };
                //    plst.Add(p);
                //}

                //var groupedCustomerList = plst.GroupBy(u => u.ProjectID).ToList();
                //int se=0;
                //foreach (var group in groupedCustomerList)
                //{
                //    int total = group.Sum(x => Convert.ToInt32(x.timeLog));                    
                //}

                RaisePropertyChanged("GetProjectsList");
                //  RaisePropertyChanged("ToDoListData");
                // AddZebraPatternToProjectList();
                AddZebraPatternToToDoList();

            }
        }

        public void SyncDataToServer()
        {
            TotalSecound = 0;
            TotalSMinute = 0;
            Totalhour = 0;
            CallActivityLog();
        }

        public async void ActivitySyncTimerFromApi()
        {
            tbl_ServerTodoDetails tbl_ServerTodo;
            _baseURL = Configurations.UrlConstant + Configurations.UserActivitySyncTimertApiConstant;
            activitySyncTimerResponseModel = new ActivitySyncTimerResponseModel();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = Common.Storage.TokenId;

            string currentDate = DateTime.Now.ToString("yyyy'-'MM'-'dd''");
            ActivitySyncTimerRequestModel _activitySyncTime = new ActivitySyncTimerRequestModel()
            {
                date = "2021-04-07"
            };


            activitySyncTimerResponseModel = await _services.GetActivitysynTimerDataAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _activitySyncTime);
            if (activitySyncTimerResponseModel.response.code == "200")
            {
                //add syncTimer tolocaldb
                tbl_SyncTimer tbl_SyncTimer;
                BaseService<tbl_SyncTimer> dbService = new BaseService<tbl_SyncTimer>();
                List<tbl_SyncTimer> listSynTimer = new List<tbl_SyncTimer>();
                dbService.Delete(new tbl_SyncTimer());
                foreach (var a in activitySyncTimerResponseModel.response.data)
                {
                    tbl_SyncTimer = new tbl_SyncTimer()
                    {
                        Id = 0,
                        ProjectId = Convert.ToString(a.projectId),
                        TimeLog = Convert.ToString(a.timeLog),
                        TodoId = a.todoId.ToStrVal()
                    };
                    listSynTimer.Add(tbl_SyncTimer);
                }

                dbService.AddRange(listSynTimer);
                GetActivitysynTimerDataFromLocalDB();
            }
        }

        public async void GetActivitysynTimerDataFromLocalDB()
        {
            List<ActivitySyncTimerResponse> activitySyncsList = new List<ActivitySyncTimerResponse>();
            List<tbl_SyncTimer> syncTimerList = new List<tbl_SyncTimer>();
            BaseService<tbl_SyncTimer> dbservice = new BaseService<tbl_SyncTimer>();
            syncTimerList = new List<tbl_SyncTimer>(dbservice.GetAll());
            if (syncTimerList == null)
            {
                return;
            }
            if (GetProjectsList != null)
            {
                GetProjectsList.Clear();
            }
            List<Plist> plst = new List<Plist>();
            List<Tlist> tlst = new List<Tlist>();

            Plist p;
            Tlist t;

            foreach (var a in syncTimerList)
            {
                //neww code
                if (!string.IsNullOrEmpty(a.ProjectId) && Convert.ToInt32(a.ProjectId) > 0)
                {
                    if (!string.IsNullOrEmpty(a.TodoId) && Convert.ToInt32(a.TodoId) > 0)
                    {
                        t = new Tlist()
                        {
                            ProjectID = a.ProjectId.ToInt32(),
                            timeLog = a.TimeLog,
                            todoId = a.TodoId.ToInt32()
                        };
                        tlst.Add(t);
                    }
                    else
                    {
                        p = new Plist()
                        {
                            ProjectID = a.ProjectId.ToInt32(),
                            timeLog = a.TimeLog
                        };
                        plst.Add(p);
                    }
                }
            }

            foreach (var x in tlst)
            {
                p = new Plist()
                {
                    ProjectID = x.ProjectID,
                    timeLog = x.timeLog
                };
                plst.Add(p);
            }


            var groupToDoList = tlst.GroupBy(z => z.todoId).ToList();
            foreach (var group2 in groupToDoList)
            {
                int totalTodto = group2.Sum(a => Convert.ToInt32(a.timeLog));
                if (GetToDoListTemp.Count > 0)
                {
                    foreach (var item in GetToDoListTemp)
                    {
                        if (group2.Key == Convert.ToInt32(item.Id))
                        {
                            item.ToDoPlayIcon = true;
                            item.ToDoStopIcon = false;
                            if (item.Site == "OnSite")
                            {
                                item.SiteColor = themeManager.OnSiteColor;
                            }
                            else
                            {
                                item.SiteColor = themeManager.OffSiteColor;
                            }
                            TimeSpan _time2 = TimeSpan.FromSeconds(totalTodto);
                            string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            _time2.Hours,
                            _time2.Minutes,
                            _time2.Seconds);
                            item.ToDoTimeConsumed = logtime;
                        }
                    }
                }
            }

            var groupedProjectList = plst.GroupBy(u => u.ProjectID).ToList();
            foreach (var group in groupedProjectList)
            {
                int total = group.Sum(x => Convert.ToInt32(x.timeLog));
                foreach (var item in GetProjectsList2)
                {
                    if (group.Key == Convert.ToInt32(item.ProjectId))
                    {
                        TimeSpan _time = TimeSpan.FromSeconds(total);
                        string answer = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        _time.Hours,
                        _time.Minutes,
                        _time.Seconds
                        );
                        updateTotalwork(_time.Seconds, _time.Minutes, _time.Hours);
                        item.ProjectTime = answer;
                        item.ProjectPlayIcon = true;
                        item.ProjectStopIcon = false;
                        GetProjectsList.Add(item);
                    }
                }
            }
            if (Selectedproject != null)
            {
                GetProjectsList = null;
                GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                // listproject.SelectedIndex = 0;
            }
            if (ToDoListData != null)
            {
                if (ToDoListData.Count > 0)
                {
                    ToDoListData.Clear();
                }
            }
            ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);
            //RaisePropertyChanged("GetProjectsList");
            //RaisePropertyChanged("ToDoListData");
            AddZebraPatternToProjectList();
            AddZebraPatternToToDoList();
        }
        public void updateTotalwork(int secound, int minute, int hour)
        {
            TotalSecound += secound;
            TotalSMinute += minute;
            Totalhour += hour;
            string TotalTime = String.Format("{0}:{1}:{2}", Totalhour.ToString().PadLeft(2, '0'), TotalSMinute.ToString().PadLeft(2, '0'), TotalSecound.ToString().PadLeft(2, '0'));
            TotalWorkTime = "Total worked today : " + TotalTime;
        }
        #endregion
        #endregion
        #region MVVM INotifyPropertyChanged     
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName))
            {
                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            var handler = CollectionChanged;
            if (handler != null)
                handler(this, args);
        }
        #endregion
    }
}
