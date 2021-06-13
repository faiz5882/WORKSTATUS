using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Threading;
using AvaloniaProgressRing;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using WorkStatus.APIServices;
using WorkStatus.Common;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;
using WorkStatus.Views;
using static WorkStatus.Models.ReadDTO.ScreenshotIntervalResponse;
using MessageBoxAvaloniaEnums = MessageBox.Avalonia.Enums;

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
        private RenewAppResponseModel renewAppResponseModel;
        private RefreshTokenResponseModel tokenResponseModel;
        private AddNotesResponseModel addNotesResponse;
        private ScreenShotResponseModel ScreenshotResponse;
        private ChangeOrganizationResponseModel changeOrganizationResponse;
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
        private KeyBoardMouseActivityTracker activityTracker;

        public string projectIdSelected;
        private string orgdSelectedID;

        private int ToDoSelectedID;

        public ListBox listtodo;
        public ListBox listproject;
        public ListBox listOrg;
        //public ProgressBar Pbar;
        public ProgressRing pgrToDO;

        public ProgressRing pgrProject;

       


        ToggleButton changeorgbtn;

        System.Timers.Timer timerproject;
        System.Timers.Timer timerToDo;
        ThemeManager themeManager;
        string currentTime = string.Empty;
        int h1, m1, s1, h2, m2, s2, h3, m3, s3;
        int TotalSecound, TotalSMinute, Totalhour;

        #endregion

        #region activityLog properties

        public DispatcherTimer ActivityTimerObject;
        private List<tbl_Timer> tbl_TimersList;
        private CommonResponseModel responseModel;
        private ActivityLogRequestEntity activityLogRequestEntity;
        private List<ActivityLogRequestEntity> activityLogRequests;
        public List<tbl_KeyMouseTrack_Slot> track_Slots;
        BackgroundWorker workerProjectLoader;
        Task[] activityTasks;

        #endregion

        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandPlay { get; }
        public ReactiveCommand<Unit, Unit> CommandStop { get; }
        public ReactiveCommand<string, Unit> CommandProjectPlay { get; set; }
        public ReactiveCommand<string, Unit> CommandProjectStop { get; set; }
        public ReactiveCommand<int, Unit> CommandToDoPlay { get; set; }
        public ReactiveCommand<int, Unit> CommandToDoStop { get; set; }
        public ReactiveCommand<Unit, Unit> CommandSync { get; set; }

        #endregion

        #region List collection Properties

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

        private List<Organisation_Projects> _getProjectsList2;
        public List<Organisation_Projects> GetProjectsList2
        {
            get { return _getProjectsList2; }
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
                    WeeklylimitText = SelectedOrganisationItems.WeeklyLimit != null ? SelectedOrganisationItems.WeeklyLimit : "No Weekly Limit";
                    // string a = SelectedOrganisationItems.OrganizationId;
                    //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    //{
                    //    Pbar.IsVisible = true; //Make Progressbar visible
                    //}), DispatcherPriority.Background);
                    Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    {
                        BindUserProjectlistByOrganizationID(SelectedOrganisationItems.OrganizationId);
                    }), DispatcherPriority.Background);

                    orgdSelectedID = SelectedOrganisationItems.OrganizationId;
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

                    if (!CheckFromToDoList(Selectedproject.ProjectId))//&& Selectedproject.checkTodoApiCallOrNot == false
                    {
                        int ProjectId = Convert.ToInt32(Selectedproject.ProjectId);
                        int OrganisationId = Convert.ToInt32(Selectedproject.OrganisationId);
                        int UserId = Convert.ToInt32(Selectedproject.UserId);

                        Dispatcher.UIThread.InvokeAsync(new Action(() =>
                        {
                            BindUseToDoListFromLocalDB(ProjectId);
                        }), DispatcherPriority.Background);

                        //if (Common.Storage.IsProjectRuning == true)
                        //{
                        //    BindUseToDoListFromLocalDB(ProjectId);
                        //}
                        //else
                        //{
                        //BindUserToDoListFromApi(ProjectId, OrganisationId, UserId);
                        //}

                    }

                }
                //RaisePropertyChanged("Selectedproject");
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

        #endregion

        #region Local properties

        private bool _isForceUpgrade;
        public bool IsForceUpgrade
        {
            get => _isForceUpgrade;
            set
            {
                _isForceUpgrade = value;
                RaisePropertyChanged("IsForceUpgrade");
            }
        }

        private bool _isForceUpgradeApp;
        public bool IsForceUpgradeApp
        {
            get => _isForceUpgradeApp;
            set
            {
                _isForceUpgradeApp = value;
                RaisePropertyChanged("IsForceUpgradeApp");
            }
        }
        private string _appTitle;
        public string AppTitle
        {
            get => _appTitle;
            set
            {
                _appTitle = value;
                RaisePropertyChanged("AppTitle");
            }
        }

        private string _appDescription;
        public string AppDescription
        {
            get => _appDescription;
            set
            {
                _appDescription = value;
                RaisePropertyChanged("AppDescription");
            }
        }

        private bool _isAppBlock;
        public bool IsAppBlock
        {
            get => _isAppBlock;
            set
            {
                _isAppBlock = value;
                RaisePropertyChanged("IsAppBlock");
            }
        }

        private bool _isStayOpen;
        public bool IsStayOpen
        {
            get => _isStayOpen;
            set
            {
                _isStayOpen = value;
                RaisePropertyChanged("IsStayOpen");
            }
        }
        private string _weeklylimitText;
        public string WeeklylimitText
        {
            get => _weeklylimitText;
            set
            {
                _weeklylimitText = value;
                RaisePropertyChanged("WeeklylimitText");
            }
        }
        private bool _isNoToDoList;
        public bool IsNoToDoList
        {
            get => _isNoToDoList;
            set
            {
                _isNoToDoList = value;
                RaisePropertyChanged("IsNoToDoList");
            }
        }
        private string _noToDoAlert;
        public string NoToDoAlert
        {
            get => _noToDoAlert;
            set
            {
                _noToDoAlert = value;
                RaisePropertyChanged("NoToDoAlert");
            }
        }
        private bool _isUserOffline;
        public bool IsUserOffline
        {
            get => _isUserOffline;
            set
            {
                _isUserOffline = value;
                RaisePropertyChanged("IsUserOffline");
            }
        }

        private string _userOfflineAlert;
        public string UserOfflineAlert
        {
            get => _userOfflineAlert;
            set
            {
                _userOfflineAlert = value;
                RaisePropertyChanged("UserOfflineAlert");
            }
        }

        private bool isVisibleProjectProgressBar;
        public bool IsVisibleProjectProgressBar
        {
            get => isVisibleProjectProgressBar;
            set
            {
                isVisibleProjectProgressBar = value;
                RaisePropertyChanged("IsVisibleProjectProgressBar");
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
        private string _notes;
        public string Notes
        {
            get => _notes;
            set
            {
                _notes = value;
                RaisePropertyChanged("Notes");
            }
        }

        #endregion

        #region constructor  
        public DashboardViewModel(Window window)
        {
            _window = window;
            themeManager = new ThemeManager();
            _services = new DashboardService();
            ActivityTimerObject = new DispatcherTimer();
            objHeaderModel = new HeaderModel();
            responseModel = new CommonResponseModel();
            activityTracker = new KeyBoardMouseActivityTracker();
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
            changeorgbtn = _window.FindControl<ToggleButton>("changeOrgbutton");
            pgrToDO = _window.FindControl<ProgressRing>("toDoProgressBar");
            pgrProject = _window.FindControl<ProgressRing>("projectpgr");
            //pgrProject.IsVisible = false;
            //pgrToDO.IsVisible = false;
            IsAppBlock = true;
            CurrentVersion = Common.Storage.GetAppVersion();
            listproject = _window.FindControl<ListBox>("LayoutRoot");
            listtodo = _window.FindControl<ListBox>("todolist");
            listOrg = _window.FindControl<ListBox>("orglist");
            CommandPlay = ReactiveCommand.Create(PlayTimer);
            CommandStop = ReactiveCommand.Create(playStop);
            CommandProjectPlay = ReactiveCommand.Create<string>(ProjectPlay);
            CommandProjectStop = ReactiveCommand.Create<string>(ProjectStop);
            CommandToDoPlay = ReactiveCommand.Create<int>(ToDoPlay);
            CommandToDoStop = ReactiveCommand.Create<int>(ToDoStop);
            CommandSync = ReactiveCommand.Create(Manualsync); //SyncDataToServer
            HeaderTime = "00:00:00";
            TotalWorkTime = "Total worked today : 00:00:00";
            SlotTimerObject = new DispatcherTimer();
            Common.Storage.IsActivityCall = false;
            IsPlaying = true;
            IsStop = false;
            objHeaderModel.SessionID = "";
            string currentDate = DateTime.Now.ToString();
            LastUpdateText = "Last updated at: " + currentDate;
            //bool isNoProgress = ForceUpgardeAppAPICall();
            //if (!isNoProgress)
            //{
                
                ActivityTimerObject.Tick += new EventHandler(ActivityTimerObject_Tick);
                NoToDoAlert = "You have no todo's assigned";
                UserOfflineAlert = " Your Internet connection seems to be lost so all features of application will not work. You can still" + "\n" + " continue with the timer only on loaded Project & ToDo’s." +
                   "\n" + " Don’t worry -your data is safe with us and it will be synced back to server automatically once your" + "\n" + " internet connection is restored.";
                IsUserOffline = Common.CommonServices.IsConnectedToInternet() ? false : true;               
                GetScreeshotIntervelFromServer();
                Dispatcher.UIThread.InvokeAsync(new Action(() => BindUserOrganisationListFromApi()));
                timerproject = new System.Timers.Timer();               
                timerToDo = new System.Timers.Timer();
                timerproject.Interval = 1000;
                timerToDo.Interval = 1000;                
                timerproject.Elapsed += Timerproject_Elapsed;
                timerToDo.Elapsed += TimerToDo_Elapsed;             
                SlotTimerObject.Tick += new EventHandler(SlotTimerObject_Elapsed);
                SyncDataToServer();
            //}
        }
        //public DashboardViewModel(Window window)
        //{
        //    _window = window;
        //    ActivityTimerObject = new DispatcherTimer();
        //    activityTracker = new KeyBoardMouseActivityTracker();
        //    // Pbar = _window.FindControl<ProgressBar>("projectProgressBar");

        //    pgrToDO = _window.FindControl<ProgressBar>("toDoProgressBar");
        //    pgrProject = _window.FindControl<ProgressBar>("projectpgr");

        //    ActivityTimerObject.Tick += new EventHandler(ActivityTimerObject_Tick);
        //    responseModel = new CommonResponseModel();
        //    Common.Storage.IsActivityCall = false;
        //    themeManager = new ThemeManager();
        //    _services = new DashboardService();
        //    objHeaderModel = new HeaderModel();
        //    CommandPlay = ReactiveCommand.Create(PlayTimer);
        //    CommandStop = ReactiveCommand.Create(playStop);
        //    CommandProjectPlay = ReactiveCommand.Create<string>(ProjectPlay);
        //    CommandProjectStop = ReactiveCommand.Create<string>(ProjectStop);
        //    CommandToDoPlay = ReactiveCommand.Create<int>(ToDoPlay);
        //    CommandToDoStop = ReactiveCommand.Create<int>(ToDoStop);
        //    CommandSync = ReactiveCommand.Create(Manualsync); //SyncDataToServer
        //    HeaderTime = "00:00:00";
        //    NoToDoAlert = "You have no todo's assigned";
        //    UserOfflineAlert = " Your Internet connection seems to be lost so all features of application will not work. You can still" + "\n" + " continue with the timer only on loaded Project & ToDo’s." +
        //       "\n" + " Don’t worry -your data is safe with us and it will be synced back to server automatically once your" + "\n" + " internet connection is restored.";

        //    IsUserOffline = Common.CommonServices.IsConnectedToInternet() ? false : true;

        //    TotalWorkTime = "Total worked today : 00:00:00";

        //    string currentDate = DateTime.Now.ToString();
        //    LastUpdateText = "Last updated at: " + currentDate;



        //    IsPlaying = true;
        //    IsStop = false;
        //    objHeaderModel.SessionID = "";
        //    FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
        //    GetScreeshotIntervelFromServer();
        //    BindUserOrganisationListFromApi();
        //    timerproject = new System.Timers.Timer();
        //    timerToDo = new System.Timers.Timer();
        //    timerproject.Interval = 1000;
        //    timerToDo.Interval = 1000;
        //    timerproject.Elapsed += Timerproject_Elapsed;
        //    timerToDo.Elapsed += TimerToDo_Elapsed;
        //    SlotTimerObject = new DispatcherTimer();
        //    SlotTimerObject.Tick += new EventHandler(SlotTimerObject_Elapsed);
        //    CurrentVersion = Common.Storage.GetAppVersion();
        //    listproject = _window.FindControl<ListBox>("LayoutRoot");
        //    listtodo = _window.FindControl<ListBox>("todolist");
        //    listOrg = _window.FindControl<ListBox>("orglist");

        //    SyncDataToServer();
        //}

        ////private void WorkerProjectLoader_RunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
        ////{
        ////    Dispatcher.UIThread.InvokeAsync(new Action(() =>
        ////    {
        ////        Pbar.IsVisible = true; //Make Progressbar visible
        ////    }), DispatcherPriority.Background);

        ////}

        //private void WorkerProjectLoader_DoWork(object? sender, DoWorkEventArgs e)
        //{


        //    Task.Factory.StartNew(() => BindUserOrganisationListFromApi());           
        //}

        #endregion

        #region Methods  

        #region SlotTimer 
        private void SlotTimerObject_Elapsed(object sender, EventArgs e)
        {
            try
            {


                //SlotTimerObject.Stop();
                SlotTimerObject.Interval = new TimeSpan(0, SlotInterval, 0);
                BackgroundWorker backgroundWorkerObject = new BackgroundWorker();
                backgroundWorkerObject.DoWork += new DoWorkEventHandler(StartThreads);
                backgroundWorkerObject.RunWorkerAsync();
                SlotTimerObject.Start();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw;
            }
        }
        private void StartThreads(object sender, DoWorkEventArgs e)
        {
            try
            {

                tasks = new Task[1];
                tasks[0] = Task.Factory.StartNew(() => StartTimeIntervalAddToDB());
                // Give the tasks a second to start.
                // Thread.Sleep(1000);

            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void StartTimeIntervalAddToDB()
        {
            try
            {
                if (Common.Storage.SlotRunning)
                {
                    DateTime a = Convert.ToDateTime(Common.Storage.SlotTimerStartTime.ToString());
                    int b = Common.Storage.timeIntervel;
                    Common.Storage.SlotTimerPreviousEndTime = a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");

                    var keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal();
                    var MouseActivity = Common.Storage.MouseEventCount.ToStrVal();
                    var AverageActivity = Common.Storage.AverageEventCount.ToStrVal();

                    //LogFile.WriteMessageLog("1. Interval Start Time -" + a.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    " Interval End Time -" + a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    "\n" + "keyboardActivity : " + keyboardActivity + ", MouseActivity : " + MouseActivity
                    //+ ", AverageActivity : " + AverageActivity + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                    Common.Storage.AverageEventCount = 0;
                    Common.Storage.KeyBoradEventCount = 0;
                    Common.Storage.MouseEventCount = 0;

                    //LogFile.WriteMessageLog("2. Interval Start Time -" + a.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00")
                    //    + " Interval End Time -" + a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    "\n" + "K_C : " + Storage.KeyBoradEventCount + ", M_C : " + Storage.MouseEventCount
                    //   + ", A_C : " + Storage.AverageEventCount + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                    new DashboardSqliteService().AddTimeIntervalToDB(a.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                        a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), keyboardActivity,MouseActivity,AverageActivity);

                }
                else
                {
                    DateTime c = Convert.ToDateTime(Common.Storage.SlotTimerPreviousEndTime.ToString());
                    
                    var keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal();
                    var MouseActivity = Common.Storage.MouseEventCount.ToStrVal();
                    var AverageActivity = Common.Storage.AverageEventCount.ToStrVal();

                    //LogFile.WriteMessageLog("1. Interval Start Time -" + c.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    " Interval End Time -" + c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    "\n" + "keyboardActivity : " + keyboardActivity + ", MouseActivity : " + MouseActivity
                    //+ ", AverageActivity : " + AverageActivity + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                    Common.Storage.AverageEventCount = 0;
                    Common.Storage.KeyBoradEventCount = 0;
                    Common.Storage.MouseEventCount = 0;

                    //LogFile.WriteMessageLog("3. Interval Start Time -" + c.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00")
                    //    + " Interval End Time -" + c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    //    "\n" + "K_C : " + Storage.KeyBoradEventCount + ", M_C : " + Storage.MouseEventCount
                    //   + ", A_C : " + Storage.AverageEventCount + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                    new DashboardSqliteService().AddTimeIntervalToDB(c.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), 
                        c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                        keyboardActivity, MouseActivity, AverageActivity);
                    Common.Storage.SlotTimerPreviousEndTime = c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");
                }

                //Common.Storage.SlotTimerStartTime = null;
                Common.Storage.SlotRunning = false;
                Common.Storage.timeIntervel = SlotInterval;
                // SlotTimerObject.Interval = new TimeSpan(0, 1, 0);

                SlotTimerObject.Start();

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }

        }
        public void StopTimeIntervalUpdateToDB()
        {
            try
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
                string EndTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

                //string IntervalStratTime = Common.Storage.SlotTimerStartTime;
                //update tbl_KeyMouseTrack_Slot set  IntervalEndTime='04:06' where IntervalEndTime='04:10' and CreatedDate= '30/03/2021'                      
                new DashboardSqliteService().UpdateTimeIntervalToDB(IntervalEndTime, EndTime, oCurrentDate.ToString("dd/MM/yyyy"));

            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        private void TimeIntervalAddToDB()
        {
            try
            {
                DateTime oCurrentDate = DateTime.Now;
                string strMinute = Convert.ToString(oCurrentDate.Minute);
                char[] charArr = strMinute.ToCharArray();
                int a = 0;
                //35
                if (oCurrentDate.Minute > 10)
                {
                    a = Convert.ToInt32(charArr[1].ToString());
                }
                else
                {
                    a = Convert.ToInt32(charArr[0].ToString());
                }

                //int result = 10 - a;
                int result = Common.Storage.timeIntervel - a;

                if (result == 0)
                {
                    result = Common.Storage.timeIntervel;
                }
                else if (result.ToString().Contains('-'))
                {
                    result = 10 - a;
                }
                Common.Storage.timeIntervel = result;
                // Common.Storage.SlotTimerStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
                // new DashboardSqliteService().AddTimeIntervalToDB(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), oCurrentDate.AddMinutes(result).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"));
                SlotTimerObject.Interval = new TimeSpan(0, result, 0);
                SlotTimerObject.Start();
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        private bool CheckSlotExistNot()
        {
            bool t = false;
            try
            {


                DateTime oCurrentDate = DateTime.Now;

                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                tbl_KeyMouseTrack_Slot slot = new tbl_KeyMouseTrack_Slot();

                slot = gg.CheckSlotExistNotFromDb(Common.Storage.SlotTimerStartTime, oCurrentDate.ToString("dd/MM/yyyy"));
                if (slot != null)
                {
                    if (!string.IsNullOrEmpty(slot.End))
                    {
                        t = true;
                    }
                    else
                    {
                        t = false;
                    }
                }
                else
                {
                    t = false;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //throw;
            }
            return t;
        }
        private void AddSlot()
        {

            try
            {
                tbl_KeyMouseTrack_Slot keyMouseTrack_Slot;
                DateTime oCurrentDate = DateTime.Now;
                if (string.IsNullOrEmpty(Common.Storage.SlotTimerPreviousEndTime))
                {
                    Common.Storage.SlotTimerPreviousEndTime = Common.Storage.SlotTimerStartTime;
                }
                DateTime _IntervalStratTime = Convert.ToDateTime(Common.Storage.SlotTimerPreviousEndTime.ToString());

                LogFile.WriteMessageLog("1.Slot Added from AddSlot():- \n Start Time -" + _IntervalStratTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    " End Time -" + oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                    "\n" + "KeyBoradEventCount : " + Storage.KeyBoradEventCount + " MouseEventCount : " + Storage.MouseEventCount
                   + " AverageEventCount : " + Storage.AverageEventCount + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                keyMouseTrack_Slot = new tbl_KeyMouseTrack_Slot()
                {
                    IntervalStratTime = _IntervalStratTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                    IntervalEndTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                    Start = Common.Storage.SlotTimerStartTime,
                    End = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                    Hour = 0,
                    keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal(),
                    MouseActivity = Common.Storage.MouseEventCount.ToStrVal(),
                    AverageActivity = Common.Storage.AverageEventCount.ToStrVal(),
                    Id = 0,
                    Latitude = null,
                    Longitude = null,
                    ScreenActivity = Common.Storage.ScreenURl == null ? "" : Common.Storage.ScreenURl,
                    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                };

                Common.Storage.AverageEventCount = 0;
                Common.Storage.KeyBoradEventCount = 0;
                Common.Storage.MouseEventCount = 0;

                //LogFile.WriteMessageLog("2. Slot Added from AddSlot():- \n Start Time -" + _IntervalStratTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                //     " End Time -" + oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                //     "\n" + "KeyBoradEventCount : " + Storage.KeyBoradEventCount + " MouseEventCount : " + Storage.MouseEventCount
                //    + " AverageEventCount : " + Storage.AverageEventCount + "\n" + DateTime.Now.ToString("hh:mm:ss"));

                BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                Common.Storage.SlotID = gg.Add(keyMouseTrack_Slot);
                Common.Storage.SlotTimerPreviousEndTime = null;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }

        #endregion

        #region activityLog Methods
        private void ActivityTimerObject_Tick(object? sender, EventArgs e)
        {
            try
            {


                // ActivityTimerObject.Stop();
                //ActivityTimerObject.Interval = new TimeSpan(0, 30, 0);
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += new DoWorkEventHandler(CallActivity);
                worker.RunWorkerAsync();
                // ActivityTimerObject.Start();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //throw new Exception(ex.Message);
            }
        }
        public void CallActivity(object sender, DoWorkEventArgs e)
        {
            try
            {
                LastUpdateText = "refreshing..";
                activityTasks = new Task[1];
                //activityTasks[0] = Task.Factory.StartNew(() => GetActivityLogFromDB());
                Task.Factory.StartNew(() => GetActivityLogFromDB());
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //  throw new Exception(ex.Message);

            }

        }
        public async void CallActivityLog()
        {
            try
            {


                timerproject.Stop();
                if (GetProjectsList != null)
                {
                    foreach (var itemT in GetProjectsList)
                    {
                        if (Common.Storage.IsProjectRuning == true)
                        {
                            if (itemT.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                            {
                                itemT.ProjectPlayIcon = false;
                                itemT.ProjectStopIcon = true;
                                itemT.checkTodoApiCallOrNot = true;
                            }
                            else
                            {
                                itemT.ProjectPlayIcon = true;
                                itemT.ProjectStopIcon = false;
                                itemT.checkTodoApiCallOrNot = false;
                            }
                        }
                    }
                }
                if (Selectedproject != null)
                {
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                }
                ActivityTimerObject.Stop();
                string currentDate = DateTime.Now.ToString();
                LastUpdateText = "Last updated at: " + currentDate;
                var rtnResult = await Task.Run(() => SendIntervalToServer()).ConfigureAwait(true);
                if (rtnResult)
                {
                    Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    {
                        ActivitySyncTimerFromApi();
                        //BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                        //service2.Delete(new tbl_Temp_SyncTimer());
                        //BaseService<tbl_TempSyncTimerTodoDetails> service3 = new BaseService<tbl_TempSyncTimerTodoDetails>();
                        //service3.Delete(new tbl_TempSyncTimerTodoDetails());
                    }), DispatcherPriority.Background);
                }
                else
                {
                    if (Common.Storage.IsProjectRuning)
                    {
                        timerproject.Start();
                        int index = GetProjectsList.FindIndex(x => x.ProjectId == projectIdSelected);
                        listproject.SelectedIndex = index;
                    }
                    else
                    {
                        listproject.SelectedIndex = 0;
                    }

                    if (Common.Storage.IsToDoRuning)
                    {
                        timerToDo.Start();
                    }
                }
                GetNotesFromLocalDB();


                int a = Common.Storage.timeIntervel;
                ActivityTimerObject.Interval = new TimeSpan(0, 5, 0);
                ActivityTimerObject.Start();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }

        }
        public async void Manualsync()
        {
            try
            {

                if (GetProjectsList != null)
                {
                    foreach (var itemT in GetProjectsList)
                    {
                        if (Common.Storage.IsProjectRuning == true)
                        {
                            if (itemT.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                            {
                                itemT.ProjectPlayIcon = false;
                                itemT.ProjectStopIcon = true;
                                itemT.checkTodoApiCallOrNot = true;
                            }
                            else
                            {
                                itemT.ProjectPlayIcon = true;
                                itemT.ProjectStopIcon = false;
                                itemT.checkTodoApiCallOrNot = false;
                            }
                        }
                    }
                }
                if (Selectedproject != null)
                {
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                }

                string currentDate = DateTime.Now.ToString();
                LastUpdateText = "Last updated at: " + currentDate;
                // var rtnResult = await Task.Run(() => SendIntervalToServer()).ConfigureAwait(true);
                // if (rtnResult)
                // {

                // }

                Dispatcher.UIThread.InvokeAsync(new Action(async () =>
                {
                    bool rtnResult = await Task.Run(() => SendIntervalToServer()).ConfigureAwait(true);
                    if (rtnResult)
                    {
                        ActivitySyncTimerFromApi();
                    }
                }), DispatcherPriority.Background);

                GetNotesFromLocalDB();

                if (Common.Storage.IsProjectRuning)
                {
                    int index = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                    listproject.SelectedIndex = index;
                }
                else
                {
                    listproject.SelectedIndex = 0;
                }

                //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                //{
                //    ActivitySyncTimerFromApi();
                //    //BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                //    //service2.Delete(new tbl_Temp_SyncTimer());

                //    //BaseService<tbl_TempSyncTimerTodoDetails> service3 = new BaseService<tbl_TempSyncTimerTodoDetails>();
                //    //service3.Delete(new tbl_TempSyncTimerTodoDetails());
                //}), DispatcherPriority.Background);

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public string ActivtyPercentage(int count, int timeinterval)
        {
            Double totalSeconds = timeinterval * 60;
            Double percentage = ((count * 100) / (totalSeconds));
            return percentage.ToStrVal();
        }
        public List<Intervals> GetIntervalsList(string startTime)
        {
            List<Intervals> finalIntervals = new List<Intervals>();
            try
            {
                List<AppAndUrl> _appAndUrls = new List<AppAndUrl>();
                List<AppAndUrl> finalAppAndUrl = new List<AppAndUrl>();
                AppAndUrl appAnd;
                Models.WriteDTO.Location _location;
                ActivityLevel activityLevel;

                Intervals intervals;
                List<Intervals> listofIntervals = new List<Intervals>();

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
                            //activityLevel = new ActivityLevel()
                            //{
                            //    average = slot.AverageActivity.ToStrVal(),
                            //    keyboard = slot.keyboardActivity.ToStrVal(),
                            //    mouse = slot.MouseActivity.ToStrVal(),
                            //};
                            activityLevel = new ActivityLevel()
                            {
                                //percentage method
                                average = ActivtyPercentage(Convert.ToInt32(slot.AverageActivity), Common.Storage.ActivityIntervel),
                                keyboard = ActivtyPercentage(Convert.ToInt32(slot.keyboardActivity), Common.Storage.ActivityIntervel),
                                mouse = ActivtyPercentage(Convert.ToInt32(slot.MouseActivity), Common.Storage.ActivityIntervel),
                            };
                            intervals = new Intervals()
                            {
                                appAndUrls = _appAndUrls,
                                location = _location,
                                screenUrl = slot.ScreenActivity.ToStrVal(),
                                activityLevel = activityLevel,
                                from = slot.IntervalStratTime.ToStrVal(),
                                to = slot.IntervalEndTime.ToStrVal(),
                                interval_time_db = Common.Storage.timeIntervel.ToStrVal()

                            };

                            listofIntervals = new List<Intervals>();
                            listofIntervals.Add(intervals);
                            finalIntervals.AddRange(listofIntervals);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return finalIntervals;
        }
        public async Task<bool> SendIntervalToServer()
        {
            bool result = false;
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
                            interval_time = Common.Storage.timeIntervel.ToStrVal(),
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


                objHeaderModel = new HeaderModel();
                _baseURL = Configurations.UrlConstant + Configurations.ActivityLogApiConstant;
                objHeaderModel.SessionID = Common.Storage.TokenId;
                if (finallist != null && finallist.Count > 0)
                {
                    foreach (var item in finallist)
                    {
                        if (item.intervals != null && item.intervals.Count > 0)
                        {
                            List<ActivityLogRequestEntity> finallist2 = new List<ActivityLogRequestEntity>();
                            ActivityLogRequestEntity aa = new ActivityLogRequestEntity();
                            aa = item;
                            finallist2.Add(aa);
                            //if (activityLogRequestEntity.intervals.Count > 0)
                            //{
                            string strJson = JsonConvert.SerializeObject(finallist2);
                            LogFile.WriteLog(strJson);
                            //call api
                            responseModel = await _services.ActivityLogAsync(new Get_API_Url().ActivityLogApi(_baseURL), true, objHeaderModel, finallist2);
                            if (responseModel.Response != null)
                            {
                                if (responseModel.Response != null)
                                {
                                    if (responseModel.Response.Code == "1001")
                                    {
                                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                                        if (renewtoken)
                                        {
                                            objHeaderModel.SessionID = Common.Storage.TokenId;
                                            responseModel = await _services.ActivityLogAsync(new Get_API_Url().ActivityLogApi(_baseURL), true, objHeaderModel, finallist);
                                        }

                                    }
                                    if (responseModel.Response.Code == "200")
                                    {

                                        Common.Storage.ScreenURl = "";

                                        Common.Storage.IsScreenShotCapture = false;
                                        // delete data from localDB tbl_Timer
                                        if (tbl_TimersList.Count > 0)
                                        {
                                            BaseService<tbl_Timer> dbService2 = new BaseService<tbl_Timer>();
                                            foreach (var item2 in tbl_TimersList)
                                            {
                                                if (!string.IsNullOrEmpty(item2.Stop))
                                                {
                                                    dbService2.DeleteSlotFromtbl_Timer(item2.Sno);
                                                }
                                                // delete data from localDB tbl_KeyMouseTrack_Slot
                                                BaseService<tbl_KeyMouseTrack_Slot> dbService3 = new BaseService<tbl_KeyMouseTrack_Slot>();
                                                track_Slots = new List<tbl_KeyMouseTrack_Slot>(dbService3.GetAllById(item2.Start, "Start"));
                                                foreach (var item3 in track_Slots)
                                                {
                                                    dbService2.DeleteSlot(item3.Id);
                                                }

                                            }
                                        }
                                        result = true;
                                    }
                                    else
                                    {
                                        Common.Storage.IsScreenShotCapture = false;
                                        result = false;
                                    }
                                }
                                else
                                {
                                    result = false;
                                }
                            }
                            else
                            {
                                result = false;
                            }
                            //}
                        }
                    }
                }
                else
                {
                    result = false;
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                result = false;
                // await MyMessageBox.Show(new Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
                //throw new Exception(ex.Message);
            }
            return result;
        }
        public void GetActivityLogFromDB()
        {
            try
            {


                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    CallActivityLog();
                }), DispatcherPriority.Background);
                string currentDate = DateTime.Now.ToString();
                LastUpdateText = "Last updated at: " + currentDate;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
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
                LogFile.ErrorLog(ex);
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
                LogFile.ErrorLog(ex);
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
                LogFile.ErrorLog(ex);
            }


        }
        #endregion

        #region HeaderPlayTimer    

        public void PlayTimer()
        {

            try
            {


                if (Common.Storage.IsProjectRuning == false)
                {
                    activityTracker.KeyBoardActivity(true);
                    activityTracker.MouseActivity(true);
                    LogFile.WriteaActivityLog("HeaderPlay Success");
                }

                timerproject.Stop();
                timerToDo.Stop();
                Organisation_Projects SelectedProjectItem = new Organisation_Projects();
                SelectedProjectItem = (Organisation_Projects)listproject.SelectedItem;

                if (SelectedProjectItem != null)
                {
                    projectIdSelected = SelectedProjectItem.ProjectId;
                }

                // TimeIntervalAddToDB();
                if (!string.IsNullOrEmpty(projectIdSelected))
                {


                    string _time = GetTimeFromProject(projectIdSelected);
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
                    AddUpdateProjectTimeToDB(true);
                    TimeIntervalAddToDB();
                    timerproject.Start();

                    IsStop = true;
                    IsPlaying = false;

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
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void playStop()
        {

            try
            {


                //try
                //{

                //    if (activityTracker.globalKeyHook != null && activityTracker.globalMouseHook != null)
                //    {
                //        activityTracker.globalKeyHook.Dispose();
                //        activityTracker.globalMouseHook.Dispose();
                //        LogFile.WriteMessageLog("Dispose");
                //    }
                //}
                //catch (Exception ex)
                //{

                //    LogFile.ErrorLog(ex);
                //}

                SlotTimerObject.Stop();
                timerproject.Stop();
                timerToDo.Stop();
                // StopTimeIntervalUpdateToDB();
                AddUpdateProjectTimeToDB(false);
                bool checkslot = CheckSlotExistNot();
                if (!checkslot)
                {
                    AddSlot();
                }
                else
                {
                    StopTimeIntervalUpdateToDB();
                }
                IsStop = false;
                IsPlaying = true;
                UpdateProjectList(projectIdSelected, "AllStop");
                Common.Storage.IsProjectRuning = false;
                if (ToDoSelectedID > 0)
                {
                    TODoStopUpdate(ToDoSelectedID);
                }

                Common.Storage.IsToDoRuning = false;


                BaseService<tbl_Organisation_Projects> serviceP = new BaseService<tbl_Organisation_Projects>();
                serviceP.UpdateProjectDetails(Selectedproject.ProjectId, Selectedproject.OrganisationId);
                // BindTempProjectListFromLocalDB(Selectedproject.OrganisationId, true);
                Manualsync();
                string _time = GetTimeFromProject(projectIdSelected);
                HeaderTime = _time;
                BindTotalWorkTime();
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
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
            timerproject.Stop();
            try
            {


                if (Common.Storage.IsProjectRuning == false)
                {
                    try
                    {

                        if (activityTracker.globalKeyHook == null && activityTracker.globalMouseHook == null)
                        {
                            activityTracker.KeyBoardActivity(true);
                            activityTracker.MouseActivity(true);
                            LogFile.WriteaActivityLog("ProjectPlay success");
                        }
                    }
                    catch (Exception ex)
                    {

                        LogFile.ErrorLog(ex);
                    }
                }


                IsStop = false;
                IsPlaying = true;
                if (Common.Storage.IsToDoRuning == true)
                {

                    timerToDo.Stop();
                    // StopTimeIntervalUpdateToDB();

                    AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);
                    bool checkslot = CheckSlotExistNot();
                    if (!checkslot)
                    {
                        AddSlot();
                    }
                    else
                    {
                        StopTimeIntervalUpdateToDB();
                    }
                    TODoStopUpdate(ToDoSelectedID);
                    Common.Storage.IsToDoRuning = false;
                    ToDoSelectedID = 0;
                    Manualsync();
                }
                if (Common.Storage.IsProjectRuning == true)
                {
                    if (!string.IsNullOrEmpty(projectIdSelected))
                    {
                        // StopTimeIntervalUpdateToDB();

                        AddUpdateProjectTimeToDB(false);
                        bool checkslot = CheckSlotExistNot();
                        if (!checkslot)
                        {
                            //DateTime oCurrentDate = DateTime.Now;                       
                            // Common.Storage.SlotTimerStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
                            AddSlot();
                        }
                        else
                        {
                            StopTimeIntervalUpdateToDB();
                        }
                        Manualsync();
                    }

                }

                projectIdSelected = string.Empty;
                projectIdSelected = obj;
                AddUpdateProjectTimeToDB(true);
                TimeIntervalAddToDB();
                Common.Storage.IsProjectRuning = true;
                Common.Storage.CurrentProjectId = obj.ToInt32();
                RefreshSelectedItem(obj, true);
                string _time = GetTimeFromProject(obj);
                string[] arry = _time.Split(':');
                h2 = Convert.ToInt32(arry[0]);
                m2 = Convert.ToInt32(arry[1]);
                s2 = Convert.ToInt32(arry[2]);
                ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
                HeaderTime = ProjectTime;
                UpdateProjectList(projectIdSelected);
                timerproject.Start();
                IsStop = true;
                IsPlaying = false;
                listproject.ScrollIntoView(listproject.SelectedItem);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void ProjectStop(string obj)
        {
            try
            {


                //try
                //{

                //    if (activityTracker.globalKeyHook != null && activityTracker.globalMouseHook != null)
                //    {
                //        activityTracker.globalKeyHook.Dispose();
                //        activityTracker.globalMouseHook.Dispose();
                //        LogFile.WriteMessageLog("Dispose");
                //    }
                //}
                //catch (Exception ex)
                //{

                //    LogFile.ErrorLog(ex);
                //}

                // StopTimeIntervalUpdateToDB();
                SlotTimerObject.Stop();
                projectIdSelected = obj;

                AddUpdateProjectTimeToDB(false);
                timerproject.Stop();
                bool checkslot = CheckSlotExistNot();
                if (!checkslot)
                {
                    AddSlot();
                }
                else
                {
                    StopTimeIntervalUpdateToDB();
                }


                IsStop = false;
                IsPlaying = true;

                ProjectStopUpdate(projectIdSelected);
                if (Common.Storage.IsToDoRuning == true)
                {
                    ToDoStop(ToDoSelectedID);
                }
                Common.Storage.IsProjectRuning = false;

                // RaisePropertyChanged("HeaderTime");

                // BaseService<tbl_Organisation_Projects> serviceP = new BaseService<tbl_Organisation_Projects>();
                // serviceP.UpdateProjectDetails(Selectedproject.ProjectId, Selectedproject.OrganisationId);
                //  BindTempProjectListFromLocalDB(Selectedproject.OrganisationId, true);
                string _time = GetTimeFromProject(obj);
                HeaderTime = _time;

                Manualsync();
                BindTotalWorkTime();
                AddZebraPatternToProjectList();
                listproject.ScrollIntoView(listproject.SelectedItem);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        private void Timerproject_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {


                //s2 = 0;            
                //m2 += 1;
                //if (m2 == 60)
                //{
                //    m2 = 0;
                //    h2 += 1;
                //}

                //=====old
                s2 += 1;
                if (s2 == 60)
                {

                    m2 += 1;
                }
                if (m2 == 60)
                {
                    m2 = 0;
                    h2 += 1;
                }
                //=====old
                HeaderTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));

                if (s2 == 60)
                {

                    s2 = 0;
                    ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
                    //  HeaderTime = ProjectTime;               

                    UpdateProjectList(projectIdSelected);

                    //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    //{

                    //    listproject.SelectedItem = Selectedproject;

                    //}), DispatcherPriority.Background);
                }


                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    AddZebraPatternToProjectList();

                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void ProjectStopUpdate(string pid)
        {
            try
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
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
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
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    {
                        int index = GetProjectsList.FindIndex(x => x.ProjectId == projectIdSelected);
                        listproject.SelectedIndex = index;
                    }), DispatcherPriority.Background);
                }

                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;

                foreach (var itemT in GetProjectsList)
                {
                    string[] arryT = itemT.ProjectTime.Split(':');
                    int s, m, h;
                    s = arryT[2].ToInt32();
                    m = arryT[1].ToInt32();
                    h = arryT[0].ToInt32();
                    if (s == 60)
                    {
                        s = 0;
                        m += 1;
                    }
                    if (m == 60)
                    {
                        m = 0;
                        h += 1;
                    }

                    updateTotalwork(s, m, h);
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            // timerproject.Start();

        }
        public string GetTimeFromProject(string projectid)
        {
            string _time = "";
            try
            {


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
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
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
                    Common.Storage.SlotTimerStartTime = Common.Storage.ProjectStartTime;
                    Common.Storage.SlotRunning = true;
                }
                else
                {
                    t = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");


                }
                int todo;
                if (ToDoSelectedID == 0)
                {
                    todo = 0;
                }
                else
                {
                    todo = ToDoSelectedID.ToInt32();
                }
                if (Common.Storage.IsToDoRuning == false)
                {
                    todo = 0;
                }
                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
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

                ////tbl_Temp_SyncTimer temp_SyncTimer;
                ////string TotalHours = "00:00:00";
                ////if (boolVal)
                ////{
                ////    TotalHours = "00:00:00";
                ////}
                ////else
                ////{
                ////    TimeSpan diff = DateTime.Parse(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")) - DateTime.Parse(Common.Storage.ProjectStartTime);
                ////    var Seconds = diff.Seconds;
                ////    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                ////    diff.Hours,
                ////    diff.Minutes,
                ////    diff.Seconds);
                ////    TotalHours = logtime;
                ////}

                ////temp_SyncTimer = new tbl_Temp_SyncTimer()
                ////{
                ////    ProjectId = Convert.ToInt32(tblTimer.ProjectId),
                ////    OrganizationId = Convert.ToInt32(tblTimer.OrgId),
                ////    SNo = 0,
                ////    TodoId = todo,
                ////    TotalWorkedHours = TotalHours,
                ////    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                ////};
                ////new DashboardSqliteService().SaveTimeIntbl_ProjectDetailsDB(temp_SyncTimer, boolVal);

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }


        public void AddUpdateProjectTimeToDBFromTodoPlay(bool boolVal, string projectid = "")
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
                    t = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

                }
                int todo;
                if (ToDoSelectedID == 0)
                {
                    todo = 0;
                }
                else
                {
                    todo = ToDoSelectedID.ToInt32();
                }
                if (Common.Storage.IsToDoRuning == false)
                {
                    todo = 0;
                }
                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
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

                new DashboardSqliteService().SaveStartStopProjectTimeINLocalDB(tblTimer, boolVal);

                tbl_Temp_SyncTimer temp_SyncTimer;
                string TotalHours = "00:00:00";
                if (boolVal)
                {
                    TotalHours = "00:00:00";
                }
                else
                {
                    TimeSpan diff = DateTime.Parse(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")) - DateTime.Parse(Common.Storage.ProjectStartTime);
                    var Seconds = diff.Seconds;
                    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                    diff.Hours,
                    diff.Minutes,
                    diff.Seconds);
                    TotalHours = logtime;
                }

                temp_SyncTimer = new tbl_Temp_SyncTimer()
                {
                    ProjectId = Convert.ToInt32(tblTimer.ProjectId),
                    OrganizationId = Convert.ToInt32(tblTimer.OrgId),
                    SNo = 0,
                    TodoId = todo,
                    TotalWorkedHours = TotalHours,
                    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                };
                new DashboardSqliteService().SaveTimeIntbl_ProjectDetailsDB(temp_SyncTimer, boolVal);

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
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
                    Common.Storage.ProjectStartTime = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
                    Common.Storage.SlotTimerStartTime = Common.Storage.ProjectStartTime;
                    Common.Storage.SlotRunning = true;
                }
                else
                {
                    t = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

                }
                int todo;
                if (ToDoSelectedID == 0)
                {
                    todo = 0;
                }
                else
                {
                    todo = ToDoSelectedID.ToInt32();
                }

                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
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

                //////==================new projecttime============================

                ////tbl_Temp_SyncTimer temp_SyncTimer;
                ////string TotalHours = "00:00:00";
                ////if (boolVal)
                ////{
                ////    TotalHours = "00:00:00";
                ////}
                ////else
                ////{
                ////    TimeSpan diff = DateTime.Parse(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")) - DateTime.Parse(Common.Storage.ProjectStartTime);
                ////    var Seconds = diff.Seconds;
                ////    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                ////    diff.Hours,
                ////    diff.Minutes,
                ////    diff.Seconds);
                ////    TotalHours = logtime;
                ////}

                ////temp_SyncTimer = new tbl_Temp_SyncTimer()
                ////{
                ////    ProjectId = Convert.ToInt32(tblTimer.ProjectId),
                ////    OrganizationId = Convert.ToInt32(tblTimer.OrgId),
                ////    SNo = 0,
                ////    TodoId = todo,
                ////    TotalWorkedHours = TotalHours,
                ////    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                ////};
                ////new DashboardSqliteService().SaveTimeIntbl_ProjectDetailsDB(temp_SyncTimer, boolVal);


                //////========================todo==========================
                ////tbl_TempSyncTimerTodoDetails tempTimerTodoDetails;
                ////string TotalHours2 = "00:00:00";
                ////if (boolVal)
                ////{
                ////    TotalHours2 = "00:00:00";
                ////}
                ////else
                ////{
                ////    TimeSpan diff = DateTime.Parse(oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss")) - DateTime.Parse(Common.Storage.ProjectStartTime);
                ////    var Seconds = diff.Seconds;
                ////    string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                ////    diff.Hours,
                ////    diff.Minutes,
                ////    diff.Seconds);
                ////    TotalHours2 = logtime;
                ////}

                ////tempTimerTodoDetails = new tbl_TempSyncTimerTodoDetails()
                ////{
                ////    ProjectId = Convert.ToInt32(tblTimer.ProjectId),
                ////    OrganizationId = Convert.ToInt32(tblTimer.OrgId),
                ////    SNo = 0,
                ////    TodoId = todo,
                ////    TotalWorkedHours = TotalHours2,
                ////    CreatedDate = oCurrentDate.ToString("dd/MM/yyyy")
                ////};
                ////new DashboardSqliteService().Savetbl_TempSyncTimerTodoDetailsDB(tempTimerTodoDetails, boolVal);


            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //throw new Exception(ex.Message);
            }
        }
        private void TimerToDo_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {


                s3 += 1;
                if (s3 == 60)
                {
                    //s3 = 0;
                    m3 += 1;
                }
                if (m3 == 60)
                {
                    m3 = 0;
                    h3 += 1;
                }
                if (s3 == 60)
                {
                    s3 = 0;
                    TODOProjectTime = String.Format("{0}:{1}:{2}", h3.ToString().PadLeft(2, '0'), m3.ToString().PadLeft(2, '0'), s3.ToString().PadLeft(2, '0'));
                    UpdateToDoList(ToDoSelectedID);
                }

                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    AddZebraPatternToToDoList();

                }), DispatcherPriority.Background);
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void ToDoPlay(int TodoID)
        {
            try
            {
                if (Common.Storage.IsToDoRuning == false)
                {
                    try
                    {
                        if (activityTracker.globalKeyHook == null && activityTracker.globalMouseHook == null)
                        {
                            activityTracker.KeyBoardActivity(true);
                            activityTracker.MouseActivity(true);
                            LogFile.WriteaActivityLog("ToDoPlay success");

                        }

                    }
                    catch (Exception ex)
                    {

                        LogFile.ErrorLog(ex);
                    }
                }


                timerToDo.Stop();
                timerproject.Stop();
                IsStop = false;
                IsPlaying = true;
                string p;
                string _time = GetTimeFromToDoList(TodoID, out p);
                if (Common.Storage.IsProjectRuning == true)
                {
                    if (!string.IsNullOrEmpty(projectIdSelected))
                    {




                        AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);
                        if (Common.Storage.IsToDoRuning == false && Common.Storage.IsProjectRuning == true)
                        {
                            bool checkslot = CheckSlotExistNot();
                            if (!checkslot)
                            {
                                AddSlot();
                            }
                            Manualsync();
                        }
                    }

                }

                if (Common.Storage.IsToDoRuning == true)
                {
                    if (ToDoSelectedID > 0)
                    {


                        // StopTimeIntervalUpdateToDB();

                        //BaseService<tbl_ServerTodoDetails> serviceTODO = new BaseService<tbl_ServerTodoDetails>();
                        // serviceTODO.UpdateTODODetails(Selectedproject.ProjectId, Selectedproject.OrganisationId, ToDoSelectedID);

                        AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);

                        bool checkslot = CheckSlotExistNot();
                        if (!checkslot)
                        {
                            AddSlot();
                        }
                        else
                        {
                            StopTimeIntervalUpdateToDB();
                        }
                        Manualsync();
                        //BindTempToDoListFromLocalDB(projectIdSelected.ToInt32());
                    }
                }

                ToDoSelectedID = 0;
                ToDoSelectedID = TodoID;

                // RefreshSelectedItem(obj, true);

                string[] arry = _time.Split(':');
                s3 = 0;
                m3 = 0;
                h3 = 0;


                s3 = Convert.ToInt32(arry[2]);
                m3 = Convert.ToInt32(arry[1]);
                h3 = Convert.ToInt32(arry[0]);

                TODOProjectTime = String.Format("{0}:{1}:{2}", h3.ToString().PadLeft(2, '0'), m3.ToString().PadLeft(2, '0'), s3.ToString().PadLeft(2, '0'));
                UpdateToDoList(ToDoSelectedID);




                // AddUpdateProjectTimeToDBFromTodoPlay(true,p);
                AddUpdateProjectTimeToDBByToDoId(true, p);
                TimeIntervalAddToDB();
                timerToDo.Start();
                Common.Storage.IsToDoRuning = true;
                ProjectPlayFromToDO(p);
                IsStop = true;
                IsPlaying = false;
                listtodo.ScrollIntoView(listtodo.SelectedItem);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void ProjectPlayFromToDO(string projectId)
        {
            try
            {


                projectIdSelected = projectId;
                RefreshSelectedItem(projectId, true);
                string _time = GetTimeFromProject(projectId);
                //string[] arry = _time.Split(':');
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
                //ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));

                string[] arry = _time.Split(':');
                h2 = Convert.ToInt32(arry[0]);
                m2 = Convert.ToInt32(arry[1]);
                s2 = Convert.ToInt32(arry[2]);
                ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
                HeaderTime = ProjectTime;
                UpdateProjectList(projectIdSelected);
                Common.Storage.IsProjectRuning = true;
                Common.Storage.CurrentProjectId = projectIdSelected.ToInt32();
                timerproject.Start();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void ToDoStop(int TodoID)
        {
            try
            {
                //try
                //{

                //    if (activityTracker.globalKeyHook != null && activityTracker.globalMouseHook != null)
                //    {
                //        activityTracker.globalKeyHook.Dispose();
                //        activityTracker.globalMouseHook.Dispose();
                //        LogFile.WriteMessageLog("Dispose");
                //    }
                //}
                //catch (Exception ex)
                //{

                //    LogFile.ErrorLog(ex);
                //}

                SlotTimerObject.Stop();
                timerToDo.Stop();
                timerproject.Stop();
                IsStop = false;
                IsPlaying = true;
                ToDoSelectedID = TodoID;
                Common.Storage.IsProjectRuning = false;
                Common.Storage.IsToDoRuning = false;
                //StopTimeIntervalUpdateToDB();


                // AddUpdateProjectTimeToDBFromTodoPlay(false, projectIdSelected);
                AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);
                bool checkslot = CheckSlotExistNot();
                if (!checkslot)
                {
                    AddSlot();
                }
                //else
                //{
                //StopTimeIntervalUpdateToDB();
                //}
                // BaseService<tbl_Organisation_Projects> serviceP = new BaseService<tbl_Organisation_Projects>();
                // serviceP.UpdateProjectDetails(Selectedproject.ProjectId, Selectedproject.OrganisationId);
                // BindTempProjectListFromLocalDB(Selectedproject.OrganisationId, true);
                //string _time = GetTimeFromProject(projectIdSelected);
                //HeaderTime = _time;
                //BindTotalWorkTime();


                //BaseService<tbl_ServerTodoDetails> serviceTODO = new BaseService<tbl_ServerTodoDetails>();
                // serviceTODO.UpdateTODODetails(Selectedproject.ProjectId, Selectedproject.OrganisationId, ToDoSelectedID);
                //  BindTempToDoListFromLocalDB(projectIdSelected.ToInt32());
                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                Manualsync();

                string _time = GetTimeFromProject(projectIdSelected);
                HeaderTime = _time;
                BindTotalWorkTime();
                TODoStopUpdate(ToDoSelectedID);


                AddZebraPatternToToDoList();
                listtodo.ScrollIntoView(listtodo.SelectedItem);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public string GetTimeFromToDoList(int todoId, out string p)
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
        public void UpdateToDoList(int CurrentToDoId)
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
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void TODoStopUpdate(int CurrentToDoId)
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
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        #endregion

        #region Bind Data from Local DB
        public void ProgreshBarVisibleORNot()
        {
            pgrProject.IsVisible = true;
            pgrToDO.IsVisible = true;
        }  
        void BindUserOrganisationListFromLocalDB()
        {
            try
            {
                FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
                DashboardSqliteService s = new DashboardSqliteService();
                FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>(s.GetOrganisation());
                //  FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>(new DashboardSqliteService().GetOrganisation());
                // BaseService<tbl_OrganisationDetails> serviceOrg = new BaseService<tbl_OrganisationDetails>();
                // List<tbl_OrganisationDetails> lst = new List<tbl_OrganisationDetails>();


                //IList<tbl_OrganisationDetails> dd = serviceOrg.GetAll();
                //RaisePropertyChanged("FindOrganisationDetails");
                // ZebraPatternToOrganizationList();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //throw new Exception(ex.Message);
            }
        }
        void BindUserProjectListFromLocalDB(string OrganisationId)
        {
            try
            {
                // FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
                GetProjectsList = new ObservableCollection<Organisation_Projects>();
                GetProjectsList2 = new List<Organisation_Projects>();



                Organisation_Projects projects;
                ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));


                foreach (var item in FindUserProjectListFinal)
                {
                    bool IconProjectPlay = false;
                    bool IconProjectStop = false;
                    if (Common.Storage.IsProjectRuning == true)
                    {
                        if (item.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                        {
                            IconProjectPlay = false;
                            IconProjectStop = true;
                        }
                        else
                        {
                            IconProjectPlay = true;
                            IconProjectStop = false;
                        }
                    }
                    else
                    {
                        IconProjectPlay = true;
                        IconProjectStop = false;
                    }
                    string ProjectTimeConsumed = "";
                    if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                    {
                        ProjectTimeConsumed = "00:00:00";
                    }
                    else
                    {
                        ProjectTimeConsumed = item.ProjectTimeConsumed;
                    }
                    projects = new Organisation_Projects()
                    {
                        ProjectId = item.ProjectId,
                        ProjectName = item.ProjectName.Trim(),
                        OrganisationId = item.OrganisationId,
                        ProjectTime = ProjectTimeConsumed,
                        ProjectPlayIcon = IconProjectPlay,
                        ProjectStopIcon = IconProjectStop,
                        UserId = item.UserId
                    };
                    GetProjectsList.Add(projects);
                    GetProjectsList2.Add(projects);
                }

                //dynamic firstvalueOfProject = GetProjectsList2.FirstOrDefault();
                //if (firstvalueOfProject != null)
                //{
                //    Selectedproject = firstvalueOfProject;
                //    BindUserToDoListFromApi(Selectedproject.ProjectId.ToInt32(), Selectedproject.OrganisationId.ToInt32(), Selectedproject.UserId.ToInt32());
                //}
                 var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                if (data != null)
                {
                    // data.checkTodoApiCallOrNot = playStop;
                    Selectedproject = data;
                }

                RaisePropertyChanged("GetProjectsList");
                AddZebraPatternToProjectList();
                if (GetProjectsList.Count > 0)
                {
                    if (!Common.Storage.IsProjectRuning)
                    {
                        listproject.SelectedIndex = 0;
                    }
                    //   listproject.SelectedIndex = 0;
                    //dynamic firstvalueOfProject = GetProjectsList.FirstOrDefault();
                    //if (firstvalueOfProject != null)
                    //{
                    //    Selectedproject = firstvalueOfProject;
                    //}
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }
        void BindTempProjectListFromLocalDB(string OrganisationId, bool IsTempCallType)
        {
            //Dispatcher.UIThread.InvokeAsync(new Action(() =>
            //{
            //    Pbar.IsVisible = true;
            //    PBarToDO.IsVisible = true;

            //}), DispatcherPriority.Background);
            try
            {
                if (Common.Storage.IsProjectRuning == true && Common.Storage.IsActivityCall == true)
                {
                    timerproject.Stop();

                    if (Common.Storage.IsToDoRuning)
                    {
                        timerToDo.Stop();
                    }
                }
                // FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
                GetProjectsList = new ObservableCollection<Organisation_Projects>();
                GetProjectsList2 = new List<Organisation_Projects>();

                Organisation_Projects projects;
                ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
                foreach (var item in FindUserProjectListFinal)
                {
                    bool IconProjectPlay = false;
                    bool IconProjectStop = false;
                    if (Common.Storage.IsProjectRuning == true)
                    {
                        if (item.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                        {
                            IconProjectPlay = false;
                            IconProjectStop = true;
                        }
                        else
                        {
                            IconProjectPlay = true;
                            IconProjectStop = false;
                        }
                    }
                    else
                    {
                        IconProjectPlay = true;
                        IconProjectStop = false;
                    }
                    string ProjectTimeConsumed = "";
                    if (IsTempCallType)
                    {
                        if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                        {
                            ProjectTimeConsumed = "00:00:00";
                        }
                        else
                        {
                            ProjectTimeConsumed = item.ProjectTimeConsumed;
                        }
                        ////if (!Common.Storage.IsProjectRuning)
                        ////{
                        ////    if (item.IsOffline == 1)
                        ////    {

                        ////        if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                        ////        {
                        ////            ProjectTimeConsumed = "00:00:00";
                        ////        }
                        ////        else
                        ////        {
                        ////            ProjectTimeConsumed = item.ProjectTimeConsumed;
                        ////        }
                        ////    }
                        ////    else
                        ////    {
                        ////        ProjectTimeConsumed = item.ProjectTimeConsumed;
                        ////        //ProjectTimeConsumed = GetProjectTempTimeFromDBBeforeSync(item.ProjectId.ToInt32(), item.OrganisationId.ToInt32(), item.ProjectTimeConsumed);
                        ////    }
                        ////}
                        ////else
                        ////{
                        ////    if (item.IsOffline == 1)
                        ////    {
                        ////        if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                        ////        {
                        ////            ProjectTimeConsumed = "00:00:00";
                        ////        }
                        ////        else
                        ////        {
                        ////            ProjectTimeConsumed = item.ProjectTimeConsumed;
                        ////        }
                        ////    }
                        ////    else
                        ////    {
                        ////        ProjectTimeConsumed = item.ProjectTimeConsumed;
                        ////        //ProjectTimeConsumed = GetProjectTempTimeFromDBBeforeSync(item.ProjectId.ToInt32(), item.OrganisationId.ToInt32(), item.ProjectTimeConsumed);
                        ////    }
                        ////}

                    }
                    else
                    {
                        if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                        {
                            ProjectTimeConsumed = "00:00:00";
                        }
                        else
                        {
                            ProjectTimeConsumed = item.ProjectTimeConsumed;
                        }

                    }
                    //if (string.IsNullOrEmpty(item.ProjectTimeConsumed))
                    //{
                    //    //ProjectTimeConsumed = GetProjectTempTimeFromDB(item.ProjectId.ToInt32(), item.OrganisationId.ToInt32(), item.ProjectTimeConsumed);


                    //     ProjectTimeConsumed = "00:00:00";
                    //}
                    //else
                    //{
                    //    ProjectTimeConsumed = item.ProjectTimeConsumed;
                    //    //ProjectTimeConsumed = GetProjectTempTimeFromDB(item.ProjectId.ToInt32(),item.OrganisationId.ToInt32(), item.ProjectTimeConsumed);
                    //}
                    projects = new Organisation_Projects()
                    {
                        ProjectId = item.ProjectId,
                        ProjectName = item.ProjectName.Trim(),
                        OrganisationId = item.OrganisationId,
                        ProjectTime = ProjectTimeConsumed,
                        ProjectPlayIcon = IconProjectPlay,
                        ProjectStopIcon = IconProjectStop,
                        UserId = item.UserId
                    };

                    // GetProjectsList2.Remove(projects);
                    GetProjectsList2.Add(projects);
                }
                //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                //{
                //    Pbar.IsVisible = true;

                //}), DispatcherPriority.Background);
                GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                //if (Selectedproject != null)
                //{
                //    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                //    GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                //    GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                //}
                RaisePropertyChanged("GetProjectsList");

                if (GetProjectsList.Count > 0)
                {

                    if (Common.Storage.IsProjectRuning)
                    {
                        int index = GetProjectsList.FindIndex(x => x.ProjectId == projectIdSelected);
                        Common.Storage.CurrentProjectId = projectIdSelected.ToInt32();
                        listproject.SelectedIndex = index;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(projectIdSelected))
                        {
                            int index = GetProjectsList.FindIndex(x => x.ProjectId == projectIdSelected);
                            Common.Storage.CurrentProjectId = projectIdSelected.ToInt32();
                            listproject.SelectedIndex = index;
                        }
                        else
                        {
                            if (Selectedproject != null && Selectedproject.OrganisationId == OrganisationId)
                            {
                                int index = GetProjectsList.FindIndex(x => x.ProjectId == Selectedproject.ProjectId);
                                Common.Storage.CurrentProjectId = Selectedproject.ProjectId.ToInt32();
                                listproject.SelectedIndex = index;
                            }
                            else
                            {
                                listproject.SelectedIndex = 0;
                            }

                        }
                    }
                    //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    //{
                    //    Pbar.IsVisible = false;
                    //    PBarToDO.IsVisible = false;

                    //}), DispatcherPriority.Background);
                    //dynamic firstvalueOfProject = GetProjectsList.FirstOrDefault();
                    //if (firstvalueOfProject != null)
                    //{
                    //    Selectedproject = firstvalueOfProject;
                    //}
                    BindTempToDoListFromLocalDB(Common.Storage.CurrentProjectId);
                }
                if (Common.Storage.IsProjectRuning == true && Common.Storage.IsActivityCall == true)
                {
                    IsPlaying = false;
                    IsStop = true;
                    string _time = GetTimeFromProject(Common.Storage.CurrentProjectId.ToStrVal());
                    string[] arry = _time.Split(':');
                    h2 = Convert.ToInt32(arry[0]);
                    m2 = Convert.ToInt32(arry[1]);
                    s2 = Convert.ToInt32(arry[2]);
                    HeaderTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));

                    timerproject.Start();
                    if (Common.Storage.IsToDoRuning)
                    {
                        timerToDo.Start();
                    }
                }
                else
                {
                    IsPlaying = true;
                    IsStop = false;
                    //timerproject.Stop();
                }
                AddZebraPatternToProjectList();

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // MyMessageBox.Show(new Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
                //throw new Exception(ex.Message);
            }
        }
        public void BindUseToDoListFromLocalDB(int CurrentProjectId)
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
                    if (Common.Storage.IsToDoRuning)
                    {
                        if (item.Id == ToDoSelectedID)
                        {
                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = true;
                        }
                        else
                        {
                            item.ToDoPlayIcon = true;
                            item.ToDoStopIcon = false;
                        }
                    }
                    else
                    {
                        item.ToDoPlayIcon = true;
                        item.ToDoStopIcon = false;
                    }
                    //if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    //{
                    //    item.ToDoTimeConsumed = "00:00:00";
                    //}
                    item.ToDoName = ExtensionMethod.GetShortDescription(item.ToDoName, 25);
                    string itemTime = item.ToDoTimeConsumed;
                    item.ToDoTimeConsumed = itemTime;
                    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    {
                        item.ToDoTimeConsumed = "00:00:00";
                    }
                    ////if (item.IsOffline == true)
                    ////{
                    ////    string itemTime = item.ToDoTimeConsumed;
                    ////    item.ToDoTimeConsumed = itemTime;
                    ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    ////    {
                    ////        item.ToDoTimeConsumed = "00:00:00";
                    ////    }
                    ////}
                    ////else
                    ////{
                    ////    //item.ToDoTimeConsumed = GetToDoTempTimeFromDBBeforeSync(item.CurrentProjectId.ToInt32(), item.CurrentOrganisationId.ToInt32(), item.Id, item.ToDoTimeConsumed);
                    ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    ////    {
                    ////        item.ToDoTimeConsumed = "00:00:00";
                    ////    }

                    ////}
                    if (string.IsNullOrEmpty(item.StartDate))
                    {
                        item.StartDate = string.Empty.PadRight(20, (char)32);
                    }
                    if (string.IsNullOrEmpty(item.EndDate))
                    {
                        item.EndDate = string.Empty.PadRight(20, (char)32);
                    }
                    //item.ToDoPlayIcon = true;
                    //item.ToDoStopIcon = false;
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

                if (Common.Storage.IsActivityCall == false)
                {
                    ActivitySyncTimerFromApi();
                }

                if (ToDoListData.Count > 0)
                {
                    if (Common.Storage.IsToDoRuning)
                    {
                        int index = ToDoListData.FindIndex(x => x.Id == ToDoSelectedID);
                        listtodo.SelectedIndex = index;
                    }
                    else
                    {
                        if (ToDoSelectedID > 0)
                        {
                            int index = ToDoListData.FindIndex(x => x.Id == ToDoSelectedID);
                            listtodo.SelectedIndex = index;
                        }
                        else
                        {
                            listtodo.SelectedIndex = 0;
                        }
                    }
                    IsNoToDoList = false;
                }
                else
                {
                    IsNoToDoList = true;
                }
                pgrProject.IsVisible = false;
                pgrToDO.IsVisible = false;

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        void BindTempToDoListFromLocalDB(int CurrentProjectId)
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

                    //if (item.IsOffline == true)
                    //{
                    //    item.ToDoTimeConsumed = GetToDoTempTimeFromDBBeforeSync(item.CurrentProjectId.ToInt32(), item.CurrentOrganisationId.ToInt32(), item.Id, item.ToDoTimeConsumed);
                    //} 
                    //else
                    //{
                    //    if(string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    //    {
                    //        item.ToDoTimeConsumed = "00:00:00";
                    //    }                                                
                    //}

                    item.ToDoName = ExtensionMethod.GetShortDescription(item.ToDoName, 25);
                    string itemTime = item.ToDoTimeConsumed;
                    item.ToDoTimeConsumed = itemTime;
                    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    {
                        item.ToDoTimeConsumed = "00:00:00";
                    }
                    ////if (item.IsOffline == true)
                    ////{
                    ////    string itemTime = item.ToDoTimeConsumed;
                    ////    item.ToDoTimeConsumed = itemTime;
                    ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    ////    {
                    ////        item.ToDoTimeConsumed = "00:00:00";
                    ////    }
                    ////}
                    ////else
                    ////{
                    ////    // item.ToDoTimeConsumed = GetToDoTempTimeFromDBBeforeSync(item.CurrentProjectId.ToInt32(), item.CurrentOrganisationId.ToInt32(), item.Id, item.ToDoTimeConsumed);
                    ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    ////    {
                    ////        item.ToDoTimeConsumed = "00:00:00";
                    ////    }

                    ////}
                    if (string.IsNullOrEmpty(item.StartDate))
                    {
                        item.StartDate = string.Empty.PadRight(20, (char)32);
                    }
                    if (string.IsNullOrEmpty(item.EndDate))
                    {
                        item.EndDate = string.Empty.PadRight(20, (char)32);
                    }
                    if (Common.Storage.IsToDoRuning)
                    {
                        if (item.Id == ToDoSelectedID)
                        {
                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = true;
                        }
                        else
                        {
                            item.ToDoPlayIcon = true;
                            item.ToDoStopIcon = false;
                        }
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
                    if (Common.Storage.IsToDoRuning)
                    {
                        int index = ToDoListData.FindIndex(x => x.Id == ToDoSelectedID);
                        listtodo.SelectedIndex = index;
                    }
                    else
                    {
                        if (ToDoSelectedID > 0)
                        {
                            int index = ToDoListData.FindIndex(x => x.Id == ToDoSelectedID);
                            listtodo.SelectedIndex = index;
                        }
                        else
                        {
                            if (SelectedprojectToDo != null)
                            {
                                int index = ToDoListData.FindIndex(x => x.Id == SelectedprojectToDo.Id);
                                listtodo.SelectedIndex = index;
                            }
                            else
                            {
                                listtodo.SelectedIndex = 0;
                            }

                        }
                    }
                    // int indexSelected = ToDoListData.FindIndex(x => x.Id == SelectedprojectToDo.Id);
                    //listtodo.SelectedIndex = 0;
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
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
            bool t = false;
            try
            {

                tbl_Organisation_Projects t2 = new tbl_Organisation_Projects();
                BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                t2 = dbService.GetById(OrgID, "OrganisationId");
                if (t2 != null)
                {
                    if (!string.IsNullOrEmpty(t2.OrganisationId))
                    {
                        t = true;
                    }
                    else
                    {
                        t = false;
                    }
                }
                else
                {
                    t = false;
                }
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
                return t;
            }
            return t;
        }
        public bool CheckTodoExistOrNotInLocalDB(int pid)
        {
            bool a = false;
            try
            {


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
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return a;
        }
        public async void GetActivitysynTimerDataFromLocalDB()
        {
            try
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
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
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
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }  
        public void ClosedAllTimer()
        {
            try
            {

                timerproject.Close();
                timerToDo.Close();
                ActivityTimerObject.Stop();
                if (Common.Storage.IsProjectRuning)
                {
                    StopTimeIntervalUpdateToDB();
                    AddUpdateProjectTimeToDB(false);
                }

                projectIdSelected = string.Empty;
                ToDoSelectedID = 0;
                Common.Storage.CurrentProjectId = 0;
                Common.Storage.IsProjectRuning = false;
                Common.Storage.IsLogin = false;
                Common.Storage.IsToDoRuning = false;
                Common.Storage.IsActivityCall = false;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        } 
        public void SyncDataToServer()
        {
            try
            {

                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                CallActivityLog();
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }   
        public void updateTotalwork(int secound, int minute, int hour)
        {
            try
            {

                TotalSecound += secound;
                TotalSMinute += minute;
                Totalhour += hour;
                if (TotalSecound >= 60)
                {
                    int a = TotalSecound - 60;
                    TotalSecound = a;
                    TotalSMinute += 1;
                }
                if (TotalSMinute >= 60)
                {
                    int a = TotalSMinute - 60;
                    TotalSMinute = a;
                    Totalhour += 1;
                }

                string TotalTime = String.Format("{0}:{1}:{2}", Totalhour.ToString().PadLeft(2, '0'), TotalSMinute.ToString().PadLeft(2, '0'), TotalSecound.ToString().PadLeft(2, '0'));
                TotalWorkTime = "Total worked today : " + TotalTime;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public string GetProjectTempTimeFromDB(int projectID, int OrganizationId, string logTime)
        {
            string strLogTime = "";
            try
            {
                BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                tbl_Temp_SyncTimer p = new tbl_Temp_SyncTimer();
                DateTime oCurrentDate = DateTime.Now;
                p = service2.Gettbl_ProjectDetailsByIDs(projectID, OrganizationId, oCurrentDate.ToString("dd/MM/yyyy"));
                if (p != null)
                {
                    if (!string.IsNullOrEmpty(p.TotalWorkedHours))
                    {
                        string[] arryT = p.TotalWorkedHours.Split(':');
                        if (!string.IsNullOrEmpty(logTime))
                        {
                            string[] arryTlogTime = logTime.Split(':');
                            int x, y, z;
                            x = arryTlogTime[0].ToInt32();
                            y = arryTlogTime[1].ToInt32();
                            z = arryTlogTime[2].ToInt32();
                            int c = arryT[2].ToInt32();
                            z = c;
                            if (z >= 60)
                            {
                                z = 0;
                                y += 1;

                            }
                            if (y == 60)
                            {
                                y = 0;
                                x += 1;
                            }
                            strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
                        }
                        else
                        {
                            strLogTime = String.Format("{0}:{1}:{2}", arryT[0].ToString().PadLeft(2, '0'), arryT[1].ToString().PadLeft(2, '0'), arryT[2].ToString().PadLeft(2, '0'));
                        }
                    }
                    else
                    {
                        strLogTime = logTime;
                    }
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
                else
                {
                    strLogTime = logTime;
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return strLogTime;
        }
        public void BindTotalWorkTime()
        {
            try
            {


                foreach (var itemT in GetProjectsList)
                {
                    string[] arryT = itemT.ProjectTime.Split(':');
                    int s, m, h;
                    s = arryT[2].ToInt32();
                    m = arryT[1].ToInt32();
                    h = arryT[0].ToInt32();
                    if (s == 60)
                    {
                        s = 0;
                        m += 1;
                    }
                    if (m == 60)
                    {
                        m = 0;
                        h += 1;
                    }

                    updateTotalwork(s, m, h);
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public string GetProjectTempTimeFromDBBeforeSync(int projectID, int OrganizationId, string logTime)
        {
            string strLogTime = "";
            try
            {
                BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                tbl_Temp_SyncTimer p = new tbl_Temp_SyncTimer();
                DateTime oCurrentDate = DateTime.Now;
                p = service2.Gettbl_ProjectDetailsByIDs(projectID, OrganizationId, oCurrentDate.ToString("dd/MM/yyyy"));
                if (p != null)
                {
                    if (!string.IsNullOrEmpty(p.TotalWorkedHours))
                    {
                        string[] arryT = p.TotalWorkedHours.Split(':');
                        if (!string.IsNullOrEmpty(logTime))
                        {
                            string[] arryTlogTime = logTime.Split(':');
                            int x, y, z;
                            x = arryTlogTime[0].ToInt32();
                            y = arryTlogTime[1].ToInt32();
                            z = arryTlogTime[2].ToInt32();


                            int a = arryT[0].ToInt32();
                            int b = arryT[1].ToInt32();
                            int c = arryT[2].ToInt32();
                            x += a;
                            y += b;
                            z += c;

                            if (z >= 60)
                            {
                                int total = z - 60;

                                z = total;
                                // y += 1;

                            }
                            if (y >= 60)
                            {
                                y = 0;
                                x += 1;
                            }
                            strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
                        }
                        else
                        {
                            strLogTime = String.Format("{0}:{1}:{2}", arryT[0].ToString().PadLeft(2, '0'), arryT[1].ToString().PadLeft(2, '0'), arryT[2].ToString().PadLeft(2, '0'));
                        }
                    }
                    else
                    {
                        strLogTime = logTime;
                    }
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
                else
                {
                    strLogTime = logTime;
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return strLogTime;
        }
        public string GetToDoTempTimeFromDBBeforeSync(int projectID, int OrganizationId, int todoId, string logTime)
        {
            string strLogTime = "";
            try
            {

                BaseService<tbl_TempSyncTimerTodoDetails> service2 = new BaseService<tbl_TempSyncTimerTodoDetails>();
                tbl_TempSyncTimerTodoDetails p = new tbl_TempSyncTimerTodoDetails();
                DateTime oCurrentDate = DateTime.Now;
                p = service2.tbl_TempSyncTimerTodoDetails(projectID, OrganizationId, oCurrentDate.ToString("dd/MM/yyyy"), todoId);
                if (p != null)
                {
                    if (!string.IsNullOrEmpty(p.TotalWorkedHours))
                    {
                        string[] arryT = p.TotalWorkedHours.Split(':');
                        if (!string.IsNullOrEmpty(logTime))
                        {
                            string[] arryTlogTime = logTime.Split(':');
                            int x, y, z;
                            x = arryTlogTime[0].ToInt32();
                            y = arryTlogTime[1].ToInt32();
                            z = arryTlogTime[2].ToInt32();


                            int a = arryT[0].ToInt32();
                            int b = arryT[1].ToInt32();
                            int c = arryT[2].ToInt32();
                            x += a;
                            y += b;
                            z += c;

                            if (z >= 60)
                            {
                                int total = z - 60;

                                z = total;
                                // y += 1;

                            }
                            if (y >= 60)
                            {
                                y = 0;
                                x += 1;
                            }
                            strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
                        }
                        else
                        {
                            strLogTime = String.Format("{0}:{1}:{2}", arryT[0].ToString().PadLeft(2, '0'), arryT[1].ToString().PadLeft(2, '0'), arryT[2].ToString().PadLeft(2, '0'));
                        }
                    }
                    else
                    {
                        strLogTime = logTime;
                    }
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
                else
                {
                    strLogTime = logTime;
                    if (strLogTime == null)
                    {
                        strLogTime = "00:00:00";
                    }
                    return strLogTime;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
            return strLogTime;
        }
        #endregion

        #region Serach list
        public void SerachToDoDataList(string searchtext, int projectid, int organization_id)
        {
            try
            {

                if (!string.IsNullOrEmpty(searchtext))
                {
                    BaseService<tbl_ServerTodoDetails> dbService = new BaseService<tbl_ServerTodoDetails>();
                    ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>();
                    ObservableCollection<tbl_ServerTodoDetails> FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>();
                    FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>(dbService.SearchToDoByString(searchtext, projectid, organization_id));
                    //foreach (var item in FindUserToDoListFinal)
                    //{
                    //    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    //    {
                    //        item.ToDoTimeConsumed = "00:00:00";
                    //    }


                    //    item.ToDoPlayIcon = true;
                    //    item.ToDoStopIcon = false;
                    //    if (item.Site == "OnSite")
                    //    {
                    //        item.SiteColor = themeManager.OnSiteColor;
                    //    }
                    //    else
                    //    {
                    //        item.SiteColor = themeManager.OffSiteColor;
                    //    }
                    //    ToDoListData.Add(item);
                    //}
                    foreach (var item in FindUserToDoListFinal)
                    {

                        //if (item.IsOffline == true)
                        //{
                        //    item.ToDoTimeConsumed = GetToDoTempTimeFromDBBeforeSync(item.CurrentProjectId.ToInt32(), item.CurrentOrganisationId.ToInt32(), item.Id, item.ToDoTimeConsumed);
                        //} 
                        //else
                        //{
                        //    if(string.IsNullOrEmpty(item.ToDoTimeConsumed))
                        //    {
                        //        item.ToDoTimeConsumed = "00:00:00";
                        //    }                                                
                        //}

                        item.ToDoName = ExtensionMethod.GetShortDescription(item.ToDoName, 25);
                        string itemTime = item.ToDoTimeConsumed;
                        item.ToDoTimeConsumed = itemTime;
                        if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                        {
                            item.ToDoTimeConsumed = "00:00:00";
                        }
                        ////if (item.IsOffline == true)
                        ////{
                        ////    string itemTime = item.ToDoTimeConsumed;
                        ////    item.ToDoTimeConsumed = itemTime;
                        ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                        ////    {
                        ////        item.ToDoTimeConsumed = "00:00:00";
                        ////    }
                        ////}
                        ////else
                        ////{
                        ////    //item.ToDoTimeConsumed = GetToDoTempTimeFromDBBeforeSync(item.CurrentProjectId.ToInt32(), item.CurrentOrganisationId.ToInt32(), item.Id, item.ToDoTimeConsumed);
                        ////    if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                        ////    {
                        ////        item.ToDoTimeConsumed = "00:00:00";
                        ////    }

                        ////}
                        if (string.IsNullOrEmpty(item.StartDate))
                        {
                            item.StartDate = string.Empty.PadRight(20, (char)32);
                        }
                        if (string.IsNullOrEmpty(item.EndDate))
                        {
                            item.EndDate = string.Empty.PadRight(20, (char)32);
                        }
                        if (Common.Storage.IsToDoRuning)
                        {
                            if (item.Id == ToDoSelectedID)
                            {
                                item.ToDoPlayIcon = false;
                                item.ToDoStopIcon = true;
                            }
                            else
                            {
                                item.ToDoPlayIcon = true;
                                item.ToDoStopIcon = false;
                            }
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
                        ToDoListData.Add(item);
                        GetToDoListTemp.Add(item);
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
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void SerachProjectDataList(string searchtext, int projectid, int organization_id)
        {
            try
            {

                if (!string.IsNullOrEmpty(searchtext))
                {
                    Organisation_Projects projects;

                    BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                    GetProjectsList = new ObservableCollection<Organisation_Projects>();
                    ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                    FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(dbService.SearchProjectByString(searchtext, projectid, organization_id));
                    foreach (var item in FindUserProjectListFinal)
                    {
                        bool IconProjectPlay = false;
                        bool IconProjectStop = false;
                        if (Common.Storage.IsProjectRuning == true)
                        {
                            if (item.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                            {
                                IconProjectPlay = false;
                                IconProjectStop = true;
                            }
                            else
                            {
                                IconProjectPlay = true;
                                IconProjectStop = false;
                            }
                        }
                        else
                        {
                            IconProjectPlay = true;
                            IconProjectStop = false;
                        }
                        projects = new Organisation_Projects()
                        {
                            ProjectId = item.ProjectId,
                            ProjectName = item.ProjectName.Trim(),
                            OrganisationId = item.OrganisationId,
                            ProjectTime = "00:00:00",
                            ProjectPlayIcon = IconProjectPlay,
                            ProjectStopIcon = IconProjectStop,
                            UserId = item.UserId
                        };
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
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        #endregion

        #region API Call

        #region Organisation List API
        public async void BindUserOrganisationListFromApi()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.UserOrganisationListApiConstant;
                organisationListResponse = new UserOrganisationListResponse();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                List<tbl_OrganisationDetails> _OrganisationDetails = new List<tbl_OrganisationDetails>();
                organisationListResponse = await _services.GetAsyncData_GetApi(new Get_API_Url().UserOrganizationlist(_baseURL, Common.Storage.LoginId), true, objHeaderModel, organisationListResponse);
                if (organisationListResponse.response != null)
                {
                    if (organisationListResponse.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            organisationListResponse = await _services.GetAsyncData_GetApi(new Get_API_Url().UserOrganizationlist(_baseURL, Common.Storage.LoginId), true, objHeaderModel, organisationListResponse);
                        }
                    }
                    if (organisationListResponse.response.code == "200")
                    {
                        BaseService<tbl_OrganisationDetails> dbService = new BaseService<tbl_OrganisationDetails>();
                        dbService.Delete(new tbl_OrganisationDetails());
                        foreach (var item in organisationListResponse.response.data)
                        {
                            tbl_OrganisationDetails = new tbl_OrganisationDetails()
                            {
                                OrganizationId = Convert.ToString(item.id),
                                OrganizationName = item.name.ToStrVal().Replace("'", "''"),
                                WeeklyLimit = (string)item.weekly_limit

                            };
                            _OrganisationDetails.Add(tbl_OrganisationDetails);
                            //new DashboardSqliteService().InsertUserOrganisation(tbl_OrganisationDetails);
                        }
                        new DashboardSqliteService().InsertUserOrganisationList(_OrganisationDetails);

                    }
                }
                BindUserOrganisationListFromLocalDB();
                if (FindOrganisationDetails.Count > 0)
                {
                    //dynamic firstOrDefault = FindOrganisationDetails.FirstOrDefault();
                    //SelectedOrganisationItems = firstOrDefault;
                    //RaisePropertyChanged("SelectedOrganisationItems");

                    dynamic firstOrDefault = FindOrganisationDetails.FirstOrDefault(x => x.OrganizationId == Common.Storage.ServerOrg_Id);
                    SelectedOrganisationItems = firstOrDefault;
                    RaisePropertyChanged("SelectedOrganisationItems");
                }


            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        #endregion

        #region Project list By Organization ID API
        public void BindUserProjectlistByOrganizationID(string OrganizationID)
        {
            try
            {
                Selectedproject = null;

                userProjectlistResponse = new UserProjectlistByOrganizationIDResponse();
                objHeaderModel = new HeaderModel();
                OrganizationDTOEntity entity = new OrganizationDTOEntity() { organization_id = OrganizationID, unarchived = "0" };
                if (OrganizationID != Common.Storage.ServerOrg_Id)
                {
                    //CallActivityLog();
                    ChangeOrganizationAPICall(OrganizationID);
                }
                _baseURL = Configurations.UrlConstant + Configurations.UserProjectlistByOrganizationIDApiConstant;
                objHeaderModel.SessionID = Common.Storage.TokenId;
                userProjectlistResponse = _services.GetUserProjectlistByOrganizationIDAsync(new Get_API_Url().UserProjectlistByOrganizationID(_baseURL), true, objHeaderModel, entity);
                if (userProjectlistResponse.response != null)
                {
                    if (userProjectlistResponse.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            userProjectlistResponse = _services.GetUserProjectlistByOrganizationIDAsync(new Get_API_Url().UserProjectlistByOrganizationID(_baseURL), true, objHeaderModel, entity);
                        }
                    }
                    if (userProjectlistResponse.response.code == "200")
                    {
                        if (userProjectlistResponse.response.data.Count > 0)
                        {
                            List<tbl_Organisation_Projects> _OrganisationProjects = new List<tbl_Organisation_Projects>();
                            BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                            dbService.Delete(new tbl_Organisation_Projects());

                            BaseService<tbl_ServerTodoDetails> dbService2 = new BaseService<tbl_ServerTodoDetails>();
                            dbService2.Delete(new tbl_ServerTodoDetails());


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
                                //Dispatcher.UIThread.InvokeAsync(new Action(() => BindUserToDoListFromApi(tbl_organisation_Projects.ProjectId.ToInt32(), tbl_organisation_Projects.OrganisationId.ToInt32(), tbl_organisation_Projects.UserId.ToInt32())));
                                BindUserToDoListFromApi(tbl_organisation_Projects.ProjectId.ToInt32(), tbl_organisation_Projects.OrganisationId.ToInt32(), tbl_organisation_Projects.UserId.ToInt32());
                            }
                            //new DashboardSqliteService().InsertUserProjectsByOrganisationID(_OrganisationProjects);
                        }

                    }
                }
                // BindUserProjectListFromLocalDB(OrganizationID);

                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    ActivitySyncTimerFromApi();

                }), DispatcherPriority.Background);
                // ActivitySyncTimerFromApi();

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }
        #endregion

        #region TODO List API
        public void BindUserToDoListFromApi(int projectId, int organizationId, int userId)
        {
            try
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
                    memberIds = new List<int>() { Common.Storage.LoginUserID.ToInt32()}
                };
                toDoListResponseModel = _services.GetUserToDoListAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _toDoList);
                if (toDoListResponseModel.Response != null)
                {

                    if (toDoListResponseModel.Response.Code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            toDoListResponseModel = _services.GetUserToDoListAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _toDoList);
                        }

                    }
                    if (toDoListResponseModel.Response.Code == "200")
                    {

                        //BaseService<tbl_ServerTodoDetails> dbService = new BaseService<tbl_ServerTodoDetails>();
                        //dbService.Delete(new tbl_ServerTodoDetails());
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
                        // BindUseToDoListFromLocalDB(projectId);
                        // new DashboardSqliteService().InsertUserOrganisation(_OrganisationDetails);

                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        #endregion

        #region Activity Sync API
        public async void BindUserActivitySyncTimerFromApi()
        {
            try
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
                if (activitySyncTimerResponseModel.response != null)
                {


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
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public async void ActivitySyncTimerFromApi()
        {
            try
            {
                tbl_ServerTodoDetails tbl_ServerTodo;
                _baseURL = Configurations.UrlConstant + Configurations.UserActivitySyncTimertApiConstant;
                activitySyncTimerResponseModel = new ActivitySyncTimerResponseModel();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;

                string currentDate = DateTime.Now.ToString("yyyy'-'MM'-'dd''");
                string tracker = "0";
                if (Common.Storage.IsProjectRuning)
                {
                    tracker = "1";
                }
                else
                {
                    tracker = "0";
                }
                ActivitySyncTimerRequestModel _activitySyncTime = new ActivitySyncTimerRequestModel()
                {
                    date = currentDate,
                    tracker = tracker

                };
                activitySyncTimerResponseModel = await _services.GetActivitysynTimerDataAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _activitySyncTime);
                if (activitySyncTimerResponseModel.response != null)
                {

                    if (activitySyncTimerResponseModel.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            activitySyncTimerResponseModel = await _services.GetActivitysynTimerDataAsync(new Get_API_Url().UserToDoList(_baseURL), true, objHeaderModel, _activitySyncTime);
                        }
                    }

                    if (activitySyncTimerResponseModel.response.code == "200")
                    {
                        //add syncTimer tolocaldb
                        tbl_SyncTimer tbl_SyncTimer;
                        BaseService<tbl_SyncTimer> dbService = new BaseService<tbl_SyncTimer>();
                        List<tbl_SyncTimer> listSynTimer = new List<tbl_SyncTimer>();
                        dbService.Delete(new tbl_SyncTimer());
                        ////=================new
                        List<Plist> plst = new List<Plist>();
                        List<Tlist> tlst = new List<Tlist>();

                        Plist p;
                        Tlist t;

                        if (activitySyncTimerResponseModel.response.data == null)
                        {
                            return;
                        }
                        foreach (var a in activitySyncTimerResponseModel.response.data)
                        {
                            //tbl_SyncTimer = new tbl_SyncTimer()
                            //{
                            //    Id = 0,
                            //    ProjectId = Convert.ToString(a.projectId),
                            //    TimeLog = Convert.ToString(a.timeLog),
                            //    TodoId = a.todoId.ToStrVal()
                            //};
                            //listSynTimer.Add(tbl_SyncTimer);

                            if (a.projectId > 0)
                            {
                                if (Convert.ToInt32(a.todoId) > 0)
                                {
                                    t = new Tlist()
                                    {
                                        ProjectID = a.projectId,
                                        timeLog = a.timeLog,
                                        todoId = a.todoId
                                    };
                                    tlst.Add(t);
                                }
                                else
                                {
                                    p = new Plist()
                                    {
                                        ProjectID = a.projectId,
                                        timeLog = a.timeLog
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
                        foreach (var groupToDo in groupToDoList)
                        {
                            string localProjectID = "";
                            foreach (var b in groupToDo)
                            {
                                localProjectID = b.ProjectID.ToStrVal();
                                continue;
                            }
                            int totalTodto = groupToDo.Sum(a => Convert.ToInt32(a.timeLog));
                            TimeSpan _time2 = TimeSpan.FromSeconds(totalTodto);
                            string logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            _time2.Hours,
                            _time2.Minutes,
                            _time2.Seconds);
                            BaseService<tbl_ServerTodoDetails> serviceTodo = new BaseService<tbl_ServerTodoDetails>();
                            serviceTodo.UpdateToDoSyncTimeToLocalDB(logtime, groupToDo.Key.ToInt32(), localProjectID);
                        }

                        var groupedProjectList = plst.GroupBy(u => u.ProjectID).ToList();
                        foreach (var groupedProject in groupedProjectList)
                        {
                            int total = groupedProject.Sum(x => Convert.ToInt32(x.timeLog));
                            TimeSpan _time = TimeSpan.FromSeconds(total);
                            string pTime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                            _time.Hours,
                            _time.Minutes,
                            _time.Seconds
                            );
                            BaseService<tbl_Organisation_Projects> servicePro = new BaseService<tbl_Organisation_Projects>();
                            servicePro.UpdateProjectSyncTimeToLocalDB(pTime, groupedProject.Key);
                        }
                        //=====================================================
                        ///dbService.AddRange(listSynTimer);
                        // GetActivitysynTimerDataFromLocalDB();
                    }
                }
                BindTempProjectListFromLocalDB(Common.Storage.CurrentOrganisationId.ToStrVal(), false);


                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                if (GetProjectsList != null)
                {
                    foreach (var itemT in GetProjectsList)
                    {
                        //if (Common.Storage.IsProjectRuning == true)
                        //{
                        //    if (itemT.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                        //    {
                        //        itemT.ProjectPlayIcon = false;
                        //        itemT.ProjectStopIcon = true;
                        //        itemT.checkTodoApiCallOrNot = true;
                        //    }
                        //    else
                        //    {
                        //        itemT.ProjectPlayIcon = true;
                        //        itemT.ProjectStopIcon = false;
                        //        itemT.checkTodoApiCallOrNot = false;
                        //    }
                        //}
                        string[] arryT = itemT.ProjectTime.Split(':');
                        updateTotalwork(arryT[2].ToInt32(), arryT[1].ToInt32(), arryT[0].ToInt32());
                    }
                }
                Common.Storage.IsActivityCall = true;
                // timerproject.Start();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        #endregion

        #region Add Notes API       
        public void AddNotesAPICall()
        {
            try
            {
                bool result = false;
                long data = 0;
                _baseURL = Configurations.UrlConstant + Configurations.AddNotesApiConstant;
                addNotesResponse = new AddNotesResponseModel();
                List<tbl_AddNotes> listNotes = new List<tbl_AddNotes>();

                objHeaderModel = new HeaderModel();
                tbl_AddNotes entity = new tbl_AddNotes()
                {
                    note = Notes,
                    time = DateTime.Now.ToStrVal(),
                    organization_id = HeaderOrgId,
                    projectId = HeaderProjectId,
                    tracker = "1",
                    IsOffline = 0
                };
                listNotes.Add(entity);
                data = new DashboardSqliteService().InsertNotetoDB(entity);
                Notes = "";
                objHeaderModel.SessionID = Common.Storage.TokenId;
                addNotesResponse = _services.AddNotesAPI(_baseURL, true, objHeaderModel, listNotes).Result;
                if (addNotesResponse.response != null)
                {
                    if (addNotesResponse.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            addNotesResponse = _services.AddNotesAPI(_baseURL, true, objHeaderModel, listNotes).Result;
                        }
                    }
                    if (addNotesResponse.response.code == "200")
                    {
                        new DashboardSqliteService().UpdateNotetoDB(data);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }

        }

        public void GetNotesFromLocalDB()
        {
            try
            {
                List<tbl_AddNotes> listdata = new List<tbl_AddNotes>();
                listdata = new DashboardSqliteService().GetNotesList();
                if (listdata.Count > 0 && listdata != null)
                {
                    _baseURL = Configurations.UrlConstant + Configurations.AddNotesApiConstant;
                    objHeaderModel.SessionID = Common.Storage.TokenId;
                    //listdata = _services.AddNotesAPI(_baseURL, true, objHeaderModel, listdata).Result;
                    addNotesResponse = _services.AddNotesAPI(_baseURL, true, objHeaderModel, listdata).Result;
                    if (addNotesResponse.response != null)
                    {
                        if (addNotesResponse.response.code == "1001")
                        {
                            RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                            bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                            if (renewtoken)
                            {
                                objHeaderModel.SessionID = Common.Storage.TokenId;
                                addNotesResponse = _services.AddNotesAPI(_baseURL, true, objHeaderModel, listdata).Result;
                            }
                        }
                        if (addNotesResponse.response.code == "200")
                        {
                            foreach (var d in listdata)
                            {
                                new DashboardSqliteService().UpdateNotetoDB(d.Id);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        #endregion

        #region Send ScreenShots To Server API 
        public async void SendScreenShotsToServer(string screenshotFileName, byte[] ImageData)
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.SendScreenshotsApiConstant;
                ScreenshotResponse = new ScreenShotResponseModel();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                // ScreenshotResponse = _services.SendScreenshotToServerAPI(_baseURL, true, objHeaderModel, screenshotFileName,ImageData).Result;
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(screenshotFileName));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "screenshot", Path.GetFileName(screenshotFileName));
                HttpClient _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", objHeaderModel.SessionID);
                _httpClient.DefaultRequestHeaders.Add("OrgID", Common.Storage.ServerOrg_Id);
                _httpClient.DefaultRequestHeaders.Add("SDToken", Common.Storage.ServerSd_Token);
                try
                {
                    Uri myUri = new Uri(_baseURL, UriKind.Absolute);
                    var response = await _httpClient.PostAsync(myUri, form);
                    response.EnsureSuccessStatusCode();
                    var responseContent = await response.Content.ReadAsStringAsync();
                    ScreenshotResponse = JsonConvert.DeserializeObject<ScreenShotResponseModel>(responseContent);
                    if (ScreenshotResponse.response != null)
                    {
                        if (ScreenshotResponse.response.code == "200")
                        {
                            if (ScreenshotResponse.response.data.imageName != null)
                            {
                                Common.Storage.ScreenURl = ScreenshotResponse.response.data.imageName;
                                Common.Storage.IsScreenShotCapture = true;
                            }
                            else
                            {
                                Common.Storage.ScreenURl = "";
                                Common.Storage.IsScreenShotCapture = false;

                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogFile.ErrorLog(ex);
                    //throw new Exception(ex.Message);
                }

                //ScreenshotResponse = _services.SendScreenshotToServerAPI(_baseURL, true, objHeaderModel, screenshotFileName).Result;

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Change Organization API
        void ChangeOrganizationAPICall(string OrganizationID)
        {
            try
            {
                Selectedproject = null;
                _baseURL = Configurations.UrlConstant + Configurations.ChangeOrganizationApiConstant;
                changeOrganizationResponse = new ChangeOrganizationResponseModel();
                objHeaderModel = new HeaderModel();
                ChangeOrganizationRequestModel entity = new ChangeOrganizationRequestModel() { new_org_id = OrganizationID };
                objHeaderModel.SessionID = Common.Storage.TokenId;
                changeOrganizationResponse = _services.ChangeOrganizationAPI(_baseURL, true, objHeaderModel, entity);
                if (changeOrganizationResponse.response != null)
                {
                    if (changeOrganizationResponse.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            changeOrganizationResponse = _services.ChangeOrganizationAPI(_baseURL, true, objHeaderModel, entity);
                        }
                    }
                    if (changeOrganizationResponse.response.code == "200")
                    {
                        if (changeOrganizationResponse.response.data != null)
                        {
                            Common.Storage.TokenId = changeOrganizationResponse.response.data.token;
                            Common.Storage.ServerOrg_Id = OrganizationID;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                //throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Get Screeshot Intervel From Server API

        public static string DigitsOnly(string strRawData)
        {
            return Regex.Replace(strRawData, "[^0-9]", "");
        }
        public void GetScreeshotIntervelFromServer()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.GetScreenshotIntervalApiConstant;
                ScreenshotInterval screenshot = new ScreenshotInterval();

                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                ScreeshotIntervelFromServer entity = new ScreeshotIntervelFromServer() { org_id = Common.Storage.ServerOrg_Id, user_id = Common.Storage.LoginUserID };
                List<tbl_OrganisationDetails> _OrganisationDetails = new List<tbl_OrganisationDetails>();
                screenshot = _services.GetScreeshotIntervelFromServerAPI(new Get_API_Url().ScreenshotApi(_baseURL), true, objHeaderModel, entity);
                if (screenshot.response != null)
                {
                    if (screenshot.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            screenshot = _services.GetScreeshotIntervelFromServerAPI(new Get_API_Url().ScreenshotApi(_baseURL), true, objHeaderModel, entity);
                        }
                    }
                    if (screenshot.response.code == "200")
                    {
                        string intrval = DigitsOnly(screenshot.response.data.timeInterval);
                        SlotInterval = intrval.ToInt32();
                        Common.Storage.timeIntervel = SlotInterval;
                        Common.Storage.ActivityIntervel = SlotInterval;
                    }
                    else
                    {
                        SlotInterval = 10;
                        Common.Storage.timeIntervel = SlotInterval;
                        Common.Storage.ActivityIntervel = SlotInterval;
                    }
                }
                else
                {
                    SlotInterval = 10;
                    Common.Storage.timeIntervel = SlotInterval;
                    Common.Storage.ActivityIntervel = SlotInterval;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }
        #endregion

        #region Forcefully Upgarde App API      
        public bool ForceUpgardeAppAPICall()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.ForceUpgardeAppApiConstant;
                renewAppResponseModel = new RenewAppResponseModel();
                RenewAppRequestModel entity = new RenewAppRequestModel()
                {
                    app_version = 1.1,
                    os = "Mac",//"window",
                    api_version = "1.0"
                };
                renewAppResponseModel = _services.ForceUpgardeAppAPI(_baseURL, entity).Result;
                if (renewAppResponseModel.response != null)
                {
                    if (renewAppResponseModel.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            renewAppResponseModel = _services.ForceUpgardeAppAPI(_baseURL, entity).Result;
                        }
                    }
                    if (renewAppResponseModel.response.code == "200")
                    {
                        if (renewAppResponseModel.response.data != null)
                        {
                            IsForceUpgrade = renewAppResponseModel.response.data.force_upgrade;
                            if (!IsForceUpgrade)
                            {
                                IsForceUpgradeApp = true;
                                AppTitle = renewAppResponseModel.response.data.app_title;
                                AppDescription = renewAppResponseModel.response.data.app_description;
                                Common.Storage.AppDownload_Link = renewAppResponseModel.response.data.download_link;
                                IsAppBlock = false;
                                IsStayOpen = true;
                                return true;
                            }
                            else
                            {
                                IsForceUpgradeApp = false;
                                AppTitle = renewAppResponseModel.response.data.app_title;
                                AppDescription = renewAppResponseModel.response.data.app_description;
                                Storage.AppDownload_Link = renewAppResponseModel.response.data.download_link;
                                IsAppBlock = true;
                                IsStayOpen = false;
                                return false;
                            }
                        }

                    }
                    else
                    {
                        IsForceUpgradeApp = false;
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                return false;
            }
            return true;
        }

        #endregion

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
