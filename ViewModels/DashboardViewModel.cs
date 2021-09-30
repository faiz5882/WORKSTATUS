using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using AvaloniaProgressRing;
using MessageBox.Avalonia.DTO;
using MessageBox.Avalonia.Enums;
using MessageBox.Avalonia.Models;
using Microsoft.Win32;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reactive;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
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

namespace WorkStatus.ViewModels
{
    public class DashboardViewModel : ReactiveObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region global properties

        Window _window;
        DispatcherTimer SlotTimerObject;
        public DispatcherTimer m_screen = new DispatcherTimer();
        bool SlotTimerRuning = false;
        bool IsSuspend = false;
        bool isWindows = false;
        int SlotInterval;
        public bool IsSlotTimer = false;
        Task[] tasks;
        DispatcherTimer keyTimer = new DispatcherTimer();
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
        private GetResponseModel getResponseModel;
        private GetToDoDetailsResponseModel getToDoDetailsResponseModel;
        public string projectIdSelected;
        private string orgdSelectedID;

        private int ToDoSelectedID;
        public MessageBoxCustomParamsWithImage customMsgBox;
        public ListBox listtodo;
        public ListBox listproject;
        public ListBox listOrg;
        //public ProgressBar Pbar;
        public ProgressRing pgrToDO;

        public ProgressRing pgrProject;
        ToggleButton changeorgbtn;
        int counter = 0;
        DispatcherTimer dispatcherTimerNotes = new DispatcherTimer();
        public System.Timers.Timer timerproject;
        public System.Timers.Timer timerAutoIdle;
        System.Timers.Timer timerToDo;
        ThemeManager themeManager = null;
        string currentTime = string.Empty;
        int h1, m1, s1, h2, m2, s2, h3, m3, s3, autoIdleHour, autoIdleMinute, autoIdleSecound;
        string sH2, sM2, sS2;
        int TotalSecound, TotalSMinute, Totalhour;

        public DispatcherTimer AppandUrlTracking = new DispatcherTimer();
        DateTime? timerStartTime = null;
        DateTime? timerStopTime = null;
        static Process currentProcess = null;
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern Int32 GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        string prevBrowserTitle = null;
        Stopwatch stopWatchAppUrl = new Stopwatch();
        Stopwatch stopWatchForUrl = new Stopwatch();
        DateTime? currentProsessStartTime = null;
        DateTime? currentURLProsessStartTime = null;
        string URLConsumedTime = string.Empty;
        string idleAppConsumedTime = string.Empty;
        private List<tbl_AppAndUrl> appAndUrl_Tracking;
        private List<tbl_Apptracking> app_Tracking;
        private List<tbl_URLTracking> url_Tracking;


        #endregion

        #region activityLog properties

        public DispatcherTimer ActivityTimerObject;
        private List<tbl_Timer> tbl_TimersList;
        private CommonResponseModel responseModel;
        private ActivityLogRequestEntity activityLogRequestEntity;
        private List<ActivityLogRequestEntity> activityLogRequests;
        public List<tbl_KeyMouseTrack_Slot> track_Slots;
        public List<tbl_KeyMouseTrack_Slot_Idle> track_Slots_Idle;

        BackgroundWorker workerProjectLoader;
        Task[] activityTasks;

        #endregion

        #region All ReactiveCommand
        public int SelectedToDoItem = 0;
        public ReactiveCommand<Unit, Unit> CommandPlay { get; }
        public ReactiveCommand<Unit, Unit> CommandStop { get; }
        public ReactiveCommand<string, Unit> CommandProjectPlay { get; set; }
        public ReactiveCommand<string, Unit> CommandProjectStop { get; set; }
        public ReactiveCommand<int, Unit> CommandToDoPlay { get; set; }
        public ReactiveCommand<int, Unit> CommandToDoStop { get; set; }
        public ReactiveCommand<Unit, Unit> CommandSync { get; set; }
        public ReactiveCommand<int, Unit> AddOrEditToDo { get; set; }
        public ReactiveCommand<Unit, Unit> AddOrEditToDoView { get; set; }
        public ReactiveCommand<Unit, Unit> MarkCompeletToDo { get; set; }
        public ReactiveCommand<Unit, Unit> DeletToDo { get; set; }
        public ReactiveCommand<int, Unit> CommandToDoDetail { get; set; }
        public ReactiveCommand<Unit, Unit> CommandReassignIdleTime { get; set; }
        public ReactiveCommand<Unit, Unit> CommandStopIdleTime { get; set; }
        public ReactiveCommand<Unit, Unit> CommandContinueIdleTime { get; set; }
        public ReactiveCommand<int, Unit> CommandAssign { get; set; }
        public ReactiveCommand<int, Unit> CommandAssignCancel { get; set; }

        #endregion

        #region List collection Properties  

        //attachment
        private ObservableCollection<tbl_ToDoAttachments> _toDoAttachmentListData;
        public ObservableCollection<tbl_ToDoAttachments> ToDoAttachmentListData
        {
            get => _toDoAttachmentListData;
            set
            {
                _toDoAttachmentListData = value;
                RaisePropertyChanged("ToDoAttachmentListData");
            }
        }
        //

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

        private List<tbl_Organisation_Projects> _findUserProjectList;
        public List<tbl_Organisation_Projects> FindUserProjectList
        {
            get => _findUserProjectList;
            set
            {
                _findUserProjectList = value;
                RaisePropertyChanged("FindUserProjectList");
            }
        }

        private List<Organisation_Projects> _getProjectsList;
        public List<Organisation_Projects> GetProjectsList
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

                    Common.Storage.ServerOrg_Id = orgdSelectedID = SelectedOrganisationItems.OrganizationId;

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
                    // add edit
                    Storage.AddOrEditToDoProjectName = HeaderProjectName;
                    Storage.AddOrEditToDoProjectId = HeaderProjectId.ToInt32();
                    //
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
        //idle
        private tbl_Organisation_Projects _idleSelectedproject;
        public tbl_Organisation_Projects IdleSelectedProject
        {
            get
            {
                return _idleSelectedproject;
            }
            set
            {
                _idleSelectedproject = value;
                RaisePropertyChanged("IdleSelectedProject");
            }
        }

        private tbl_ServerTodoDetails _idleSelectedprojectToDo;
        public tbl_ServerTodoDetails IdleSelectedProjectToDo
        {
            get
            {
                return _idleSelectedprojectToDo;
            }
            set
            {
                _idleSelectedprojectToDo = value;
                RaisePropertyChanged("IdleSelectedProjectToDo");
            }
        }

        private List<tbl_Organisation_Projects> _ProjectsList;
        public List<tbl_Organisation_Projects> ProjectsList
        {
            get { return _ProjectsList; }
            set
            {
                _ProjectsList = value;
                RaisePropertyChanged("ProjectsList");
                // OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset, value));
            }
        }

        private ObservableCollection<tbl_ServerTodoDetails> _toDoList;
        public ObservableCollection<tbl_ServerTodoDetails> ToDoList
        {
            get => _toDoList;
            set
            {
                _toDoList = value;
                RaisePropertyChanged("ToDoList");
            }
        }
        //
        private tbl_ServerTodoDetails tbl_ServerAddTodoDetails;

        //attachments
        private tbl_ToDoAttachments tbl_todoattachment;
        //
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
        private ObservableCollection<tbl_ServerTodoDetails> _toDoDetailsData;
        public ObservableCollection<tbl_ServerTodoDetails> ToDoDetailData
        {
            get => _toDoDetailsData;
            set
            {
                _toDoDetailsData = value;
                RaisePropertyChanged("ToDoDetailData");
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

        private List<string> _getProjectsNameList;
        public List<string> GetProjectsNameList
        {
            get { return _getProjectsNameList; }
            set
            {
                _getProjectsNameList = value;
                RaisePropertyChanged("GetProjectsNameList");
            }
        }

        private List<string> _getToDoNameList;
        public List<string> GetToDoNameList
        {
            get { return _getToDoNameList; }
            set
            {
                _getToDoNameList = value;
                RaisePropertyChanged("GetToDoNameList");
            }
        }
        private List<string> _getToDoAttachmentsList;
        public List<string> GetToDoAttachmentsList
        {
            get { return _getToDoAttachmentsList; }
            set
            {
                _getToDoAttachmentsList = value;
                RaisePropertyChanged("GetToDoAttachmentsList");
            }
        }

        private bool _isAddTaskMode;
        public bool IsAddTaskMode
        {
            get => _isAddTaskMode;
            set
            {
                _isAddTaskMode = value;
                RaisePropertyChanged("IsAddTaskMode");
            }
        }

        private bool _isAddTaskModeQuitAlert;
        public bool IsAddTaskModeQuitAlert
        {
            get => _isAddTaskModeQuitAlert;
            set
            {
                _isAddTaskModeQuitAlert = value;
                RaisePropertyChanged("IsAddTaskModeQuitAlert");
            }
        }
        #endregion

        #region Local properties

        private SolidColorBrush _newTextColor;
        public SolidColorBrush NewTextColor
        {
            get => _newTextColor;
            set
            {
                _newTextColor = value;
                RaisePropertyChanged("NewTextColor");
            }
        }

        private SolidColorBrush _newBlueTextColor;
        public SolidColorBrush NewBlueTextColor
        {
            get => _newBlueTextColor;
            set
            {
                _newBlueTextColor = value;
                RaisePropertyChanged("NewBlueTextColor");
            }
        }

        private SolidColorBrush _newStackPanelLogoColor;
        public SolidColorBrush NewStackPanelLogoColor
        {
            get => _newStackPanelLogoColor;
            set
            {
                _newStackPanelLogoColor = value;
                RaisePropertyChanged("NewStackPanelLogoColor");
            }
        }
        private SolidColorBrush _txtWelcomeColor;
        public SolidColorBrush TxtWelcomeColor
        {
            get => _txtWelcomeColor;
            set
            {
                _txtWelcomeColor = value;
                RaisePropertyChanged("TxtWelcomeColor");
            }
        }
        private string msg;
        public string Msg
        {
            get => msg;
            set
            {
                msg = value;
                RaisePropertyChanged("Msg");
            }
        }
        private bool _isMarkComplete;
        public bool IsMarkComplete
        {
            get => _isMarkComplete;
            set
            {
                _isMarkComplete = value;
                RaisePropertyChanged("IsMarkComplete");
            }
        }

        private bool _isOnlyDeleteVisible;
        public bool IsOnlyDeleteVisible
        {
            get => _isOnlyDeleteVisible;
            set
            {
                _isOnlyDeleteVisible = value;
                RaisePropertyChanged("IsOnlyDeleteVisible");
            }
        }
        private bool _isToDoDetailsPopUp;
        public bool IsToDoDetailsPopUp
        {
            get => _isToDoDetailsPopUp;
            set
            {
                _isToDoDetailsPopUp = value;
                RaisePropertyChanged("IsToDoDetailsPopUp");
            }
        }
        private bool _isStaysOpenToDoDetails;
        public bool IsStaysOpenToDoDetails
        {
            get => _isStaysOpenToDoDetails;
            set
            {
                _isStaysOpenToDoDetails = value;
                RaisePropertyChanged("IsStaysOpenToDoDetails");
            }
        }

        private string _hibernateMessage;
        public string HibernateMessage
        {
            get => _hibernateMessage;
            set
            {
                _hibernateMessage = value;
                RaisePropertyChanged("HibernateMessage");
            }
        }
        private string _idleSelectedProjectId;
        public string IdleSelectedProjectId
        {
            get => _idleSelectedProjectId;
            set
            {
                _idleSelectedProjectId = value;
                RaisePropertyChanged("IdleSelectedProjectId");
            }
        }

        private int _idleSelectedToDoId;
        public int IdleSelectedToDoId
        {
            get => _idleSelectedToDoId;
            set
            {
                _idleSelectedToDoId = value;
                RaisePropertyChanged("IdleSelectedToDoId");
            }
        }
        private string _selectedProjectsName;
        public string SelectedProjectsName
        {
            get => _selectedProjectsName;
            set
            {
                _selectedProjectsName = value;
                RaisePropertyChanged("SelectedProjectsName");
            }
        }

        private string _selectedToDoName;
        public string SelectedToDoName
        {
            get => _selectedToDoName;
            set
            {
                _selectedToDoName = value;
                RaisePropertyChanged("SelectedToDoName");
            }
        }

        private bool _isReassignBtnClick;
        public bool IsReassignBtnClick
        {
            get => _isReassignBtnClick;
            set
            {
                _isReassignBtnClick = value;
                RaisePropertyChanged("IsReassignBtnClick");
            }
        }
        private bool _isReassignPopUpOpen;
        public bool IsReassignPopUpOpen
        {
            get => _isReassignPopUpOpen;
            set
            {
                _isReassignPopUpOpen = value;
                RaisePropertyChanged("IsReassignPopUpOpen");
            }
        }
        private bool _isRemeberMeKeepIdle;
        public bool RemeberMeKeepIdle
        {
            get => _isRemeberMeKeepIdle;
            set
            {
                _isRemeberMeKeepIdle = value;
                RaisePropertyChanged("RemeberMeKeepIdle");
            }
        }
        private string _idleTimeMessage;
        public string IdleTimeMessage
        {
            get => _idleTimeMessage;
            set
            {
                _idleTimeMessage = value;
                RaisePropertyChanged("IdleTimeMessage");
            }
        }
        private bool _isIdleTimeClose;
        public bool IsIdleTimeClose
        {
            get => _isIdleTimeClose;
            set
            {
                _isIdleTimeClose = value;
                RaisePropertyChanged("IsIdleTimeClose");
            }
        }

        private bool _isIdleTimeQuitAlert;
        public bool IsIdleTimeQuitAlert
        {
            get => _isIdleTimeQuitAlert;
            set
            {
                _isIdleTimeQuitAlert = value;
                RaisePropertyChanged("IsIdleTimeQuitAlert");
            }
        }

        private bool _isSignOut;
        public bool IsSignOut
        {
            get => _isSignOut;
            set
            {
                _isSignOut = value;
                RaisePropertyChanged("IsSignOut");
            }
        }
        private bool _isMiniMizeAppBtn;
        public bool IsMiniMizeAppBtn
        {
            get => _isMiniMizeAppBtn;
            set
            {
                _isMiniMizeAppBtn = value;
                RaisePropertyChanged("IsMiniMizeAppBtn");
            }
        }
        private bool _isCancelAppBtn;
        public bool IsCancelAppBtn
        {
            get => _isCancelAppBtn;
            set
            {
                _isCancelAppBtn = value;
                RaisePropertyChanged("IsCancelAppBtn");
            }
        }
        private bool _isQuitAppBtn;
        public bool IsQuitAppBtn
        {
            get => _isQuitAppBtn;
            set
            {
                _isQuitAppBtn = value;
                RaisePropertyChanged("IsQuitAppBtn");
            }
        }


        private bool _isDashBoardClose;
        public bool IsDashBoardClose
        {
            get => _isDashBoardClose;
            set
            {
                _isDashBoardClose = value;
                RaisePropertyChanged("IsDashBoardClose");
            }
        }

        private bool _isDashBoardQuitAlert;
        public bool IsDashBoardQuitAlert
        {
            get => _isDashBoardQuitAlert;
            set
            {
                _isDashBoardQuitAlert = value;
                RaisePropertyChanged("IsDashBoardQuitAlert");
            }
        }


        private bool _isSleepMode;
        public bool IsSleepMode
        {
            get => _isSleepMode;
            set
            {
                _isSleepMode = value;
                RaisePropertyChanged("IsSleepMode");
            }
        }

        private bool _isSleepModeQuitAlert;
        public bool IsSleepModeQuitAlert
        {
            get => _isSleepModeQuitAlert;
            set
            {
                _isSleepModeQuitAlert = value;
                RaisePropertyChanged("IsSleepModeQuitAlert");
            }
        }
        private bool _remeberMe;
        public bool RemeberMe
        {
            get => _remeberMe;
            set
            {
                _remeberMe = value;
                RaisePropertyChanged("RemeberMe");
            }
        }

        private double _addNoteButtonOpacity;
        public double AddNoteButtonOpacity
        {
            get => _addNoteButtonOpacity;
            set
            {
                _addNoteButtonOpacity = value;
                RaisePropertyChanged("AddNoteButtonOpacity");
            }
        }

        private string _addnoteStatus { get; set; }
        public string AddnoteStatus
        {
            get => _addnoteStatus;
            set
            {
                _addnoteStatus = value;
                RaisePropertyChanged("AddnoteStatus");
            }
        }
        private string _infoColor { get; set; }
        public string InfoColor
        {
            get => _infoColor;
            set
            {
                _infoColor = value;
                RaisePropertyChanged("InfoColor");
            }
        }

        private bool _isAddnoteStatus;
        public bool IsAddnoteStatus
        {
            get => _isAddnoteStatus;
            set
            {
                _isAddnoteStatus = value;
                RaisePropertyChanged("IsAddnoteStatus");
            }
        }
        private bool _isAddNoteButtonEnabled;
        public bool IsAddNoteButtonEnabled
        {
            get => _isAddNoteButtonEnabled;
            set
            {
                _isAddNoteButtonEnabled = value;
                RaisePropertyChanged("IsAddNoteButtonEnabled");
            }
        }
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
            try
            {
                _window = window;
                isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
                customMsgBox = new MessageBoxCustomParamsWithImage();
                themeManager = new ThemeManager();
                NewTextColor = themeManager.NewTextColor;// themeManager.NewTextColor;
                NewBlueTextColor = themeManager.NewBlueTextColor;
                NewStackPanelLogoColor = themeManager.NewStackPanelLogoColor;
                TxtWelcomeColor = themeManager.TxtWelcomeColor;
                _services = new DashboardService();
                ActivityTimerObject = new DispatcherTimer();
                objHeaderModel = new HeaderModel();
                responseModel = new CommonResponseModel();
                if (isWindows)
                {
                    SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
                    activityTracker = new KeyBoardMouseActivityTracker();
                }
                else
                {
                    keyTimer.Tick += new EventHandler(keyTimer_Tick);
                    keyTimer.Interval = new TimeSpan(0, 1, 0);
                    keyTimer.Start();
                }
                FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
                changeorgbtn = _window.FindControl<ToggleButton>("changeOrgbutton");
                pgrToDO = _window.FindControl<ProgressRing>("toDoProgressBar");
                pgrProject = _window.FindControl<ProgressRing>("projectpgr");

                //pgrProject.IsVisible = false;
                //pgrToDO.IsVisible = false;
                IsAppBlock = true;
                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
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
                AddOrEditToDo = ReactiveCommand.Create<int>(AddOrEditPageOpen);
                AddOrEditToDoView = ReactiveCommand.Create(AddOrEditPageOpen_ToDoView);
                MarkCompeletToDo = ReactiveCommand.Create(MarkCompleteToDoAPICall);
                DeletToDo = ReactiveCommand.Create(DeleteToDoAPICall);
                CommandToDoDetail = ReactiveCommand.Create<int>(ToDoDetailCall);
                CommandReassignIdleTime = ReactiveCommand.Create(ReassignIdleTimeCall);
                CommandStopIdleTime = ReactiveCommand.Create(StopIdleTimeCall);
                CommandContinueIdleTime = ReactiveCommand.Create(ContinueIdleTimeCall);
                CommandAssign = ReactiveCommand.Create<int>(AssignIdleTimeCall);
                CommandAssignCancel = ReactiveCommand.Create<int>(CancelAssignIdleTimeCall);
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
                timerAutoIdle = new System.Timers.Timer();
                timerToDo = new System.Timers.Timer();
                timerproject.Interval = 1000;
                timerToDo.Interval = 1000;
                timerAutoIdle.Interval = 1000;
                timerproject.Elapsed += Timerproject_Elapsed;
                timerAutoIdle.Elapsed += TimerAutoIdle_Elapsed;
                timerToDo.Elapsed += TimerToDo_Elapsed;
                SlotTimerObject.Tick += new EventHandler(SlotTimerObject_Elapsed);
                //autoIdleHour = 0;
                //autoIdleMinute = 20;
                //autoIdleSecound = 50;
                SyncDataToServer();
                //CheckIdleTimeFromDb();
                //IsIdleTimeClose = true;
                //IsIdleTimeQuitAlert = true;
                //timerAutoIdle.Start();
                //}
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }



        private void TimerAutoIdle_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            autoIdleSecound += 1;
            if (autoIdleSecound == 60)
            {

                autoIdleMinute += 1;
            }
            if (autoIdleMinute == 60)
            {
                autoIdleMinute = 0;
                autoIdleHour += 1;
            }


            if (autoIdleSecound == 60)
                autoIdleSecound = 0;
            string totalTimeStr = String.Format("{0}:{1}:{2}", autoIdleHour.ToString().PadLeft(2, '0'), autoIdleMinute.ToString().PadLeft(2, '0'), autoIdleSecound.ToString().PadLeft(2, '0'));
            IdleTimeMessage = "You are idle from last " + totalTimeStr + "";

        }

        #endregion

        #region Methods  

        #region App and Url Tracking

        private void AppandUrlTracking_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (Common.Storage.IsProjectRuning || Common.Storage.IsToDoRuning)
                {
                    TrackAndSaveAppandURLActivity();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        private static Process GetActiveProcess()
        {
            try
            {
                IntPtr hwnd = GetForegroundWindow();
                return hwnd != null ? GetProcessByHandle(hwnd) : null;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
        private static Process GetProcessByHandle(IntPtr hwnd)
        {
            try
            {
                uint processID;
                GetWindowThreadProcessId(hwnd, out processID);
                return Process.GetProcessById((int)processID);
            }
            catch { return null; }
        }
        public void TrackAndSaveAppandURLActivity()
        {
            timerStartTime = timerStartTime == null ? DateTime.Now : timerStartTime;
            string url = string.Empty;
            var prevProcessName = currentProcess != null ? currentProcess.ProcessName : null;
            currentProcess = GetActiveProcess();
            if (currentProcess != null && (currentProcess.ProcessName == "chrome" || currentProcess.ProcessName == "firefox" ||
                                            currentProcess.ProcessName == "iexplore" || currentProcess.ProcessName == "msedge"))
            {
                if (prevBrowserTitle == null)
                {
                    stopWatchForUrl.Start();
                    prevBrowserTitle = currentProcess != null ? currentProcess.MainWindowTitle : null;
                    currentURLProsessStartTime = currentURLProsessStartTime == null ? DateTime.Now
                                                                    : currentURLProsessStartTime;
                }
                TimeSpan timeSpanStopWatchUrl = stopWatchForUrl.Elapsed;
                URLConsumedTime = String.Format("{0:00}:{1:00}:{2:00}",
                    timeSpanStopWatchUrl.Hours, timeSpanStopWatchUrl.Minutes, timeSpanStopWatchUrl.Seconds);
                if (prevBrowserTitle != null)
                {
                    if (currentProcess.MainWindowTitle != prevBrowserTitle)
                    {
                        stopWatchForUrl.Stop();
                        var currentURLProsessEndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        SaveURLTrackingData(currentURLProsessStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), currentURLProsessEndTime, URLConsumedTime, prevBrowserTitle, "", URLConsumedTime);
                        prevBrowserTitle = null;
                        currentURLProsessStartTime = Convert.ToDateTime(currentURLProsessEndTime);
                        stopWatchForUrl.Reset();
                    }
                    else
                        idleAppConsumedTime = URLConsumedTime;
                }
            }
            if (currentProcess != null)
            {
                currentProsessStartTime = currentProsessStartTime == null ? DateTime.Now
                                                                        : currentProsessStartTime;
                TimeSpan timeSpanStopWatchAppUrl = stopWatchAppUrl.Elapsed;
                var appConsumedTime = String.Format("{0:00}:{1:00}:{2:00}",
                    timeSpanStopWatchAppUrl.Hours, timeSpanStopWatchAppUrl.Minutes, timeSpanStopWatchAppUrl.Seconds);
                if (prevProcessName != null)
                {
                    if (currentProcess.ProcessName != prevProcessName.ToString())
                    {
                        var currentProsessEndTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                        stopWatchAppUrl.Stop();
                        stopWatchAppUrl.Reset();
                        SaveAppTrackingData(currentProsessStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss"), currentProsessEndTime, appConsumedTime, prevProcessName, appConsumedTime);  //Ritesh DB save	
                        stopWatchAppUrl.Start();
                        currentProsessStartTime = Convert.ToDateTime(currentProsessEndTime);
                        timeSpanStopWatchAppUrl = stopWatchAppUrl.Elapsed;
                        appConsumedTime = String.Format("{0:00}:{1:00}:{2:00}",
                            timeSpanStopWatchAppUrl.Hours, timeSpanStopWatchAppUrl.Minutes, timeSpanStopWatchAppUrl.Seconds);
                    }
                    else
                        idleAppConsumedTime = appConsumedTime;
                }
            }

        }
        private void SaveURLTrackingData(string startDate, string endDate, string timeFrame, string UrlName, string urlPath, string totalTime)
        {
            int TotalTimeinSecond = 0;
            string[] arryTlogTime = totalTime.Split(':');
            int h, m, s;
            h = arryTlogTime[0].ToInt32();
            m = arryTlogTime[1].ToInt32();
            s = arryTlogTime[2].ToInt32();
            if (h != 00)
            {
                h = h * 60 * 60;
            }
            if (m != 00)
            {
                m = m * 60;
            }
            TotalTimeinSecond = h + m + s;
            UrlName = string.IsNullOrEmpty(UrlName) ? "NA" : UrlName;
            urlPath = string.IsNullOrEmpty(urlPath) ? "NA" : urlPath;
            var urldata = new tbl_URLTracking()
            {
                Start = Common.Storage.ProjectStartTime,
                URLStartDateTime = startDate,
                URLEndDateTime = endDate,
                URLConsumedTime = timeFrame,
                UrlName = UrlName.Replace("'", "''"),
                urlPath = urlPath,
                TotalTimeSpent = TotalTimeinSecond.ToStrVal(),
                IsOffline = 1

            };

            //LogFile.WriteMessageLog("1. URL Start Time -" + startDate + "\n" + "2. End Time -" + endDate +
            //            "\n" + "3. UrlName : " + UrlName + "\n" + "4. TotalTimeSpent : " + TotalTimeinSecond.ToStrVal()
            //        + "\n" + "5." + DateTime.Now.ToString("hh:mm:ss"));

            new DashboardSqliteService().InsertURLTrackingData(urldata);
        }
        private void SaveAppTrackingData(string startDate, string endDate, string timeFrame, string activityName, string totalTime)
        {
            int TotalTimeinSecond = 0;
            string[] arryTlogTime = totalTime.Split(':');
            int h, m, s;
            h = arryTlogTime[0].ToInt32();
            m = arryTlogTime[1].ToInt32();
            s = arryTlogTime[2].ToInt32();
            //00:01:10
            if (h != 00)
            {
                h = h * 60 * 60;
            }
            if (m != 00)
            {
                m = m * 60;
            }
            TotalTimeinSecond = h + m + s;
            var appdata = new tbl_Apptracking()
            {
                Start = Common.Storage.ProjectStartTime,
                AppStartDateTime = startDate,
                AppEndDateTime = endDate,
                AppConsumedTime = timeFrame,
                Activity_Name = activityName,
                Activity_TotalRun = TotalTimeinSecond.ToStrVal(),
                IsOffline = 1

            };
            //LogFile.WriteMessageLog("1.Start Time -" + startDate + "\n" + "End Time -" + endDate +
            //            "\n" + "3. activityName : " + activityName + "\n" + "4. Activity_TotalRun : " + TotalTimeinSecond.ToStrVal()
            //        + "\n" + "5." + DateTime.Now.ToString("hh:mm:ss"));

            new DashboardSqliteService().InsertAppTrackingData(appdata);
        }
        #endregion
        #region hibernate
        public ReactiveCommand<object, Unit> ShowMsgBoxCommand { get; }
        private async void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            switch (e.Mode)
            {
                case PowerModes.Suspend:
                    LogFile.WriteaActivityLog("sleep mode and hibernate mode call: " + DateTime.Now);
                    //timerproject.Dispose();
                    timerproject.Stop();
                    SlotTimerObject.Stop();
                    AddUpdateProjectTimeToDB(false);
                    bool checkslot = CheckSlotExistNot();
                    if (!checkslot)
                    {
                        AddSlot();
                    }
                    IsStop = false;
                    IsPlaying = true;
                    IsSuspend = true;
                    Common.Storage.IsProjectRuning = false;
                    ProjectStopUpdate(projectIdSelected);
                    string _time = GetTimeFromProject(projectIdSelected);
                    HeaderTime = _time;
                    string strmess = "WorkStatus noticed that your system was on sleep\n mode. Please, Start the Timer again!";
                    HibernateMessage = strmess;

                    Manualsync();


                    // System.Windows.Forms.Application.Restart();
                    // System.Windows.Application.Current.Shutdown();
                    // IshibernateClose = true;
                    // IshibernateQuitAlert = true;
                    customMsgBox = new MessageBoxCustomParamsWithImage
                    {
                        ContentMessage = strmess,
                        Icon = LoadEmbeddedResources("/Assets/DotsIcon.png"),
                        ButtonDefinitions = new[]
                        {
                        new ButtonDefinition {Name = "Ok", Type = ButtonType.Colored},

                    },
                        WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner
                    };
                    break;

                case PowerModes.Resume:
                    LogFile.WriteaActivityLog("e.Mode : " + e.Mode + DateTime.Now);
                    Task.Delay(500);
                    IsSleepMode = true;
                    IsSleepModeQuitAlert = true;
                    DateTime AppTodayStartTM = DateTime.Now;
                    List<tbl_UserDetails> userList = new List<tbl_UserDetails>();
                    BaseService<tbl_UserDetails> dbService = new BaseService<tbl_UserDetails>();
                    userList = new List<tbl_UserDetails>(dbService.GetAll());
                    if (userList != null && userList.Count > 0)
                    {
                        AppTodayStartTM = Convert.ToDateTime(userList[0].CreatedOn);

                    }
                    if (AppTodayStartTM.Date < DateTime.Now.Date)
                    {
                        Storage.AppTodayStartTM = DateTime.Now;
                        Common.Storage.IsToDoRuning = false;
                        BindUserOrganisationListFromApi();
                    }

                    break;

                default:
                    LogFile.WriteaActivityLog("e.Mode : " + e.Mode + DateTime.Now);
                    break;
            }
        }
        public Avalonia.Media.Imaging.Bitmap LoadEmbeddedResources(string localFilePath)
        {
            try
            {
                string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var rawUri = localFilePath;
                var uri = new Uri($"avares://{assemblyName}{rawUri}");

                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets.Open(uri);

                return new Avalonia.Media.Imaging.Bitmap(asset);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
        #endregion
        #region IdleTime Manage
        public void CheckIdleTimeFromDb()
        {
            try
            {
                List<tbl_IdleTimeDetails> userIdleTimeList = new List<tbl_IdleTimeDetails>();
                BaseService<tbl_IdleTimeDetails> dbService = new BaseService<tbl_IdleTimeDetails>();
                userIdleTimeList = new List<tbl_IdleTimeDetails>(dbService.GetAll());
                if (userIdleTimeList != null)
                {
                    if (userIdleTimeList.Count > 0)
                    {
                        if (userIdleTimeList.Count >= 2)
                        {
                            autoIdleHour = 0;
                            autoIdleMinute = 0;
                            autoIdleSecound = 0;

                            IsIdleTimeClose = true;
                            IsIdleTimeQuitAlert = true;

                            DateTime dt = Convert.ToDateTime(userIdleTimeList[0].ProjectIdleStartTime);
                            DateTime dt2 = Convert.ToDateTime(userIdleTimeList[userIdleTimeList.Count - 1].ProjectIdleEndTime);
                            TimeSpan ts = (dt2 - dt);
                            if (ts.Hours == 0)
                            {
                                Storage.ContinueProjectEventCountTime = ts.Minutes;

                            }
                            else
                            {
                                Storage.ContinueProjectEventCountTime = ts.Hours + ts.Minutes;

                            }

                            autoIdleHour = ts.Hours;
                            autoIdleMinute = ts.Minutes;
                            autoIdleSecound = ts.Seconds;
                            timerAutoIdle.Start();
                            if (isWindows)
                            {
                                try
                                {
                                    if (activityTracker.globalKeyHook != null && activityTracker.globalMouseHook != null)
                                    {
                                        activityTracker.globalKeyHook.Dispose();
                                        activityTracker.globalMouseHook.Dispose();
                                        activityTracker.globalKeyHook = null;
                                        activityTracker.globalMouseHook = null;
                                        //LogFile.WriteMessageLog("Dispose");
                                    }

                                }
                                catch (Exception ex)
                                {

                                    LogFile.ErrorLog(ex);
                                }
                            }

                        }
                    }
                    else
                    {
                        IsIdleTimeClose = false;
                        IsIdleTimeQuitAlert = false;
                    }

                }
                else
                {
                    IsIdleTimeClose = false;
                    IsIdleTimeQuitAlert = false;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public void ValidateFormsAndError(int timeOut)
        {
            DateTime oCurrentDate2 = DateTime.Now;
            int s = oCurrentDate2.Second;
            int result = s + timeOut;
            if (result >= 60)
            {
                s = 0;
                result = s + timeOut;
            }
            counter = result;

            dispatcherTimerNotes.Tick += new EventHandler(dispatcherTimerNotes_Tick);
            dispatcherTimerNotes.Start();
        }
        private void dispatcherTimerNotes_Tick(object sender, EventArgs e)
        {
            DateTime oCurrentDate = DateTime.Now;
            int s = oCurrentDate.Second;

            if (counter == s)
            {

                IsAddnoteStatus = false;
                dispatcherTimerNotes.Stop();
            }
            //else
            //{
            //    oCurrentDate.AddSeconds(1);
            //}
        }
        public void ReassignIdleTimeCall()
        {
            timerAutoIdle.Stop();
            IsIdleTimeClose = false;
            IsIdleTimeQuitAlert = false;
            IsReassignBtnClick = true;
            IsReassignPopUpOpen = true;

            BindUserProjectListforComboBox(HeaderOrgId);
        }
        public void BashNoWait(string cmd)
        {
            Task t = new Task(() =>
            {
                var escapedArgs = cmd.Replace("\"", "\\\"");
                var process = new Process()
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/bash",
                        Arguments = $"-c \"{escapedArgs}\"",
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();
                // Msg = process.StartInfo.Arguments + "/" + process.StartInfo.FileName;
                // IsDashBoardClose = true;
                // IsDashBoardQuitAlert = true;
            });
            t.Start();
        }
        public void StopIdleTimeCall()
        {
            timerAutoIdle.Stop();
            if (RemeberMeKeepIdle)
            {
                //==============stop timer send to server intevel and stop timer
                StopIdleTime();
                IsIdleTimeClose = false;
                IsIdleTimeQuitAlert = false;
            }
            else
            {
                //============ Don’t keep idle time Stop and remove idle intervals
                if (Storage.IsProjectRuning && !Storage.IsToDoRuning)
                {
                    ProjectTime = Storage.LastProjectEventCountTime;
                    ProjectStopUpdate(projectIdSelected);
                    StopTimerAfterIdlePopUp();
                    BindTotalWorkTime();
                    Storage.IdleProjectTime = ProjectTime;
                }
                if (Storage.IsToDoRuning)
                {
                    TODOProjectTime = Storage.LastToDoEventCountTime;
                    ProjectTime = Storage.LastProjectEventCountTime;
                    ProjectStopUpdate(projectIdSelected);
                    StopTimerAfterIdlePopUp();
                    UpdateToDoListAfterIdlePopUp(ToDoSelectedID);
                    BindTotalWorkTime();
                }
                IsIdleTimeClose = false;
                IsIdleTimeQuitAlert = false;
                AddUpdateProjectTimeToDBinStopIdle(false);
                BaseService<tbl_KeyMouseTrack_Slot_Idle> service = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                service.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                BaseService<tbl_IdleTimeDetails> service1 = new BaseService<tbl_IdleTimeDetails>();
                service1.Delete(new tbl_IdleTimeDetails());
                BaseService<tbl_Timer> service2 = new BaseService<tbl_Timer>();
                service2.Delete(new tbl_Timer());
            }
        }
        public void ContinueIdleTimeCall()
        {
            timerAutoIdle.Stop();
            if (RemeberMeKeepIdle)
            {
                //==============stop timer send to server intevel and continue timer
                ContinueIdleTime();
                IsIdleTimeClose = false;
                IsIdleTimeQuitAlert = false;
            }
            else
            {
                IsIdleTimeClose = false;
                IsIdleTimeQuitAlert = false;
                //============ Don’t keep idle time and Continue and remove idle intervals
                if (Storage.IsProjectRuning && !Storage.IsToDoRuning)
                {
                    SlotTimerObject.Stop();
                    timerproject.Stop();

                    //Storage.ContinueProjectEventCountTime = GetTimeFromProject(projectIdSelected);
                    ProjectTime = Storage.LastProjectEventCountTime;

                    IsStop = false;
                    IsPlaying = true;
                    TotalSecound = 0;
                    TotalSMinute = 0;
                    Totalhour = 0;
                    ProjectStopUpdate(projectIdSelected);
                    BindTotalWorkTime();
                    // timerproject.Start();
                    // IsStop = true;
                    // IsPlaying = false;
                    ProjectPlayUpdate(projectIdSelected);
                    string _time = GetTimeFromProject(projectIdSelected);
                    Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    {
                        HeaderTime = _time;
                    }), DispatcherPriority.Background);

                   // AddUpdateProjectTimeToDBinContinueIdle(false);
                    BaseService<tbl_IdleTimeDetails> service = new BaseService<tbl_IdleTimeDetails>();
                    service.Delete(new tbl_IdleTimeDetails());
                    BaseService<tbl_KeyMouseTrack_Slot_Idle> service1 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                    service1.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                    BaseService<tbl_Timer> service2 = new BaseService<tbl_Timer>();
                    service2.Delete(new tbl_Timer());

                    ProjectPlay(projectIdSelected);
                }
                if (Storage.IsToDoRuning)
                {
                    //Storage.ContinueToDoEventCountTime = TODOProjectTime;
                    //Storage.ContinueProjectEventCountTime = GetTimeFromProject(projectIdSelected);
                    TODOProjectTime = Storage.LastToDoEventCountTime;
                    ProjectTime = Storage.LastProjectEventCountTime;
                    ProjectStopUpdate(projectIdSelected);
                    ContinueTimerAfterIdlePopUp();
                    // AddUpdateProjectTimeToDBinContinueIdle(false); //comment by inder on 14092021
                    BaseService<tbl_IdleTimeDetails> service = new BaseService<tbl_IdleTimeDetails>();
                    service.Delete(new tbl_IdleTimeDetails());
                    BaseService<tbl_KeyMouseTrack_Slot_Idle> service1 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                    service1.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                    BaseService<tbl_Timer> service2 = new BaseService<tbl_Timer>();
                    service2.Delete(new tbl_Timer());
                    ToDoPlay(ToDoSelectedID);
                }

            }
            if (isWindows)
            {

                if (activityTracker.globalKeyHook == null && activityTracker.globalMouseHook == null)
                {
                    activityTracker.KeyBoardActivity(true);
                    activityTracker.MouseActivity(true);
                    LogFile.WriteaActivityLog("Continue success");
                }
            }

        }
        public void AssignIdleTimeCall(int Id)
        {
            IsReassignBtnClick = false;
            IsReassignPopUpOpen = false;
            if (RemeberMeKeepIdle)
            {

                //if (Selectedproject.ProjectId != IdleSelectedProject.ProjectId || SelectedprojectToDo.Id != IdleSelectedProjectToDo.Id)
                //{
                //update project time of running project to last active time and old time set to project
                ProjectTime = Storage.LastProjectEventCountTime == null ? ProjectTime : Storage.LastProjectEventCountTime;
                ProjectPlayUpdate(Selectedproject.ProjectId);

                SlotTimerObject.Stop();
                timerproject.Stop();
                IsStop = false;
                IsPlaying = true;
                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                Common.Storage.IsProjectRuning = false;

                //update ToDo time of running project to last active time and old time set to todo 
                if (Common.Storage.IsToDoRuning)
                {
                    TODOProjectTime = Storage.LastToDoEventCountTime == null ? TODOProjectTime : Storage.LastToDoEventCountTime;
                    UpdateToDoListAfterIdlePopUp(ToDoSelectedID);
                    timerToDo.Stop();
                    Common.Storage.IsToDoRuning = false;
                }
                if (IdleSelectedProjectToDo != null)
                {
                    UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, IdleSelectedProjectToDo.Id);
                }
                else
                {
                    UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, 0);
                }
                List<tbl_KeyMouseTrack_Slot_Idle> Idle = new List<tbl_KeyMouseTrack_Slot_Idle>();
                BaseService<tbl_KeyMouseTrack_Slot_Idle> dbService = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                Idle = new List<tbl_KeyMouseTrack_Slot_Idle>(dbService.GetAll());
                if (Idle != null)
                {
                    if (Idle.Count > 0)
                    {
                        new DashboardSqliteService().UpdateSlot_IdleStartTimeInLocalDB(Storage.StopTimeForDB, Idle[0].Id);
                    }
                }
                if (IdleSelectedProjectToDo != null)
                {
                    // bind idle time to selected Todo from assign popup.
                    DateTime dt1 = DateTime.Now;
                    DateTime time1 = Convert.ToDateTime(IdleSelectedProjectToDo.ToDoTimeConsumed);
                    dt1 = time1;
                    dt1 = dt1.AddMinutes(Storage.ContinueProjectEventCountTime);
                    var v1 = dt1.ToString("00:mm:ss");
                    IdleSelectedProjectToDo.ToDoTimeConsumed = v1;
                    TODOProjectTime = IdleSelectedProjectToDo.ToDoTimeConsumed;

                    // when no todo selected then????
                    ToDoSelectedID = IdleSelectedProjectToDo.Id;
                    //
                }

                //new
                // bind idle time to selected project from assign popup.
                DateTime dt = DateTime.Now;
                DateTime time = Convert.ToDateTime(IdleSelectedProject.ProjectTimeConsumed);
                dt = time;
                dt = dt.AddMinutes(Storage.ContinueProjectEventCountTime);
                var v = dt.ToString("00:mm:ss");
                IdleSelectedProject.ProjectTimeConsumed = v;
                ProjectTime = IdleSelectedProject.ProjectTimeConsumed;

                // change Current Project Id with selected ID from assign popup.
                Common.Storage.CurrentProjectId = IdleSelectedProject.ProjectId.ToInt32();

                //retrieve assign popup selected project data from list and save it as selected project.
                // project list update
                ProjectPlayUpdate(IdleSelectedProject.ProjectId);
                var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                if (data != null)
                {
                    Selectedproject = data;
                }
                int index = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                listproject.SelectedIndex = index;

                // need to check for condition only if project selected.....
                // update todolist data  with project id.
                if (IdleSelectedProjectToDo != null)
                {
                    BindUseToDoListFromLocalDB(Common.Storage.CurrentProjectId);

                    // ToDo list update
                    UpdateToDoListAfterIdlePopUp(ToDoSelectedID);

                    var data1 = ToDoListData.FirstOrDefault(x => x.Id == IdleSelectedProjectToDo.Id);
                    if (data1 != null)
                    {
                        SelectedprojectToDo = data1;
                    }
                    TODOProjectTime = SelectedprojectToDo.ToDoTimeConsumed;
                    ToDoSelectedID = SelectedprojectToDo.Id;
                }
                HeaderTime = "00:00:00";
                ProjectTime = Selectedproject.ProjectTime;
                string _time = GetTimeFromProject(Selectedproject.ProjectId);
                projectIdSelected = Selectedproject.ProjectId;
                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    HeaderTime = _time;
                }), DispatcherPriority.Background);
                DateTime oCurrentDate = DateTime.Now;
                Storage.StopTimeForDB = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
                AddUpdateProjectTimeToDBinContinueIdle(false);
                ManualSyncIdleTimeAssign();
                BindTotalWorkTime();
                AddZebraPatternToProjectList();
                listproject.ScrollIntoView(listproject.SelectedItem);

                // check
                if (IdleSelectedProjectToDo != null)
                {
                    ToDoPlay(ToDoSelectedID);
                }
                else
                {
                    ProjectPlay(projectIdSelected);
                }
                //}

                if (isWindows && activityTracker.globalKeyHook == null && activityTracker.globalMouseHook == null)
                {
                    activityTracker.KeyBoardActivity(true);
                    activityTracker.MouseActivity(true);
                    LogFile.WriteaActivityLog("Continue success");
                }
            }
            else
            {
                // if (Selectedproject.ProjectId != IdleSelectedProject.ProjectId || SelectedprojectToDo.Id != IdleSelectedProjectToDo.Id)
                //{
                // 1. update timer table
                // 2. bind last active timer time to old project and todo
                // 3. set selected project and todo from assign pop up
                // 4. run timer on it.
                // 5. delete idle slot.
                //

                //update project time of running project to last active time and old time set to project
                ProjectTime = Storage.LastProjectEventCountTime == null ? ProjectTime : Storage.LastProjectEventCountTime;
                ProjectPlayUpdate(Selectedproject.ProjectId);

                SlotTimerObject.Stop();
                timerproject.Stop();
                IsStop = false;
                IsPlaying = true;
                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                Common.Storage.IsProjectRuning = false;

                //update ToDo time of running project to last active time and old time set to todo 
                if (Common.Storage.IsToDoRuning)
                {
                    TODOProjectTime = Storage.LastToDoEventCountTime == null ? TODOProjectTime : Storage.LastToDoEventCountTime;
                    UpdateToDoListAfterIdlePopUp(ToDoSelectedID);
                    timerToDo.Stop();
                    Common.Storage.IsToDoRuning = false;
                }
                AddUpdateProjectTimeToDBinContinueIdle(false);
                Manualsync();
                BaseService<tbl_IdleTimeDetails> service = new BaseService<tbl_IdleTimeDetails>();
                service.Delete(new tbl_IdleTimeDetails());
                BaseService<tbl_KeyMouseTrack_Slot_Idle> service1 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                service1.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                BaseService<tbl_Timer> service2 = new BaseService<tbl_Timer>();
                service2.Delete(new tbl_Timer());
                //
                //if (IdleSelectedProjectToDo != null)
                //{
                //    UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, IdleSelectedProjectToDo.Id);
                //}
                //else
                //{
                //    UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, 0);
                //}
                // change Current Project Id with selected ID from assign popup.
                Common.Storage.CurrentProjectId = IdleSelectedProject.ProjectId.ToInt32();

                //retrieve assign popup selected project data from list and save it as selected project.
                // project list update

                var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                if (data != null)
                {
                    Selectedproject = data;
                }
                int index = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                listproject.SelectedIndex = index;

                // need to check for condition only if project selected.....
                // update todolist data  with project id.
                if (IdleSelectedProjectToDo != null)
                {
                    ToDoSelectedID = IdleSelectedProjectToDo.Id;
                    BindUseToDoListFromLocalDB(Common.Storage.CurrentProjectId);

                    // ToDo list update
                    // UpdateToDoListAfterIdlePopUp(ToDoSelectedID);

                    var data1 = ToDoListData.FirstOrDefault(x => x.Id == IdleSelectedProjectToDo.Id);
                    if (data1 != null)
                    {
                        SelectedprojectToDo = data1;
                    }
                    TODOProjectTime = SelectedprojectToDo.ToDoTimeConsumed;
                    ToDoSelectedID = SelectedprojectToDo.Id;
                }
                HeaderTime = "00:00:00";
                ProjectTime = Selectedproject.ProjectTime;
                string _time = GetTimeFromProject(Selectedproject.ProjectId);
                projectIdSelected = Selectedproject.ProjectId;
                Dispatcher.UIThread.InvokeAsync(new Action(() =>
                {
                    HeaderTime = _time;
                }), DispatcherPriority.Background);

                BindTotalWorkTime();
                AddZebraPatternToProjectList();
                listproject.ScrollIntoView(listproject.SelectedItem);

                // check
                if (IdleSelectedProjectToDo != null)
                {
                    ToDoPlay(ToDoSelectedID);
                    //UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, IdleSelectedProjectToDo.Id);
                }
                else
                {
                    ProjectPlay(projectIdSelected);
                    //UpdateProjectIdandToDoIdinTimerTable(false, IdleSelectedProject.ProjectId, 0);
                }
                // }
            }

        }
        public void CancelAssignIdleTimeCall(int Id)
        {

            IsReassignBtnClick = false;
            IsReassignPopUpOpen = false;
            IsIdleTimeClose = true;
            IsIdleTimeQuitAlert = true;
        }

        #endregion

        #region IdleTime functionlity
        public void StopIdleTime()
        {
            try
            {

                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
                SlotTimerObject.Stop();

                timerproject.Stop();
                timerToDo.Stop();
                AddUpdateProjectTimeToDB(false);
                //AddUpdateProjectTimeToDBinIdle(false);
                //bool checkslot = CheckSlotExistNot();
                //if (!checkslot)
                //{
                //    AddSlot();
                //}
                //else
                //{
                //    StopTimeIntervalUpdateToDB();
                //}


                IsStop = false;
                IsPlaying = true;
                ProjectStopUpdate(projectIdSelected);
                //if (Common.Storage.IsToDoRuning == true)
                //{
                //    ToDoStop(ToDoSelectedID);
                //}
                Common.Storage.IsProjectRuning = false;
                Common.Storage.IsToDoRuning = false;
                string _time = GetTimeFromProject(projectIdSelected);
                HeaderTime = _time;
                ManualSyncIdleTime();
                BindTotalWorkTime();
                AddZebraPatternToProjectList();
                listproject.ScrollIntoView(listproject.SelectedItem);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void ContinueIdleTime()
        {
            try
            {

                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
                //SlotTimerObject.Stop();
                //AddUpdateProjectTimeToDB(false);
                // AddUpdateProjectTimeToDBinContinueIdle(false); //comment by inder on 14092021
                //bool checkslot = CheckSlotExistNot();
                //if (!checkslot)
                //{
                //    AddSlot();
                //}
                //else
                //{
                //    StopTimeIntervalUpdateToDB();
                //}


                //IsStop = false;
                //IsPlaying = true;
                //ProjectStopUpdate(projectIdSelected);
                //if (Common.Storage.IsToDoRuning == true)
                //{
                //    ToDoStop(ToDoSelectedID);
                //}
                //Common.Storage.IsProjectRuning = false;
                //string _time = GetTimeFromProject(projectIdSelected);
                //HeaderTime = _time;
                ManualSyncIdleTime();
                BindTotalWorkTime();
                AddZebraPatternToProjectList();
                listproject.ScrollIntoView(listproject.SelectedItem);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public async void ManualSyncIdleTime()
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
                    GetProjectsList = new List<Organisation_Projects>();
                    // GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
                }

                string currentDate = DateTime.Now.ToString();
                LastUpdateText = "Last updated at: " + currentDate;

                Dispatcher.UIThread.InvokeAsync(new Action(async () =>
                {
                    bool rtnResult = await Task.Run(() => SendIdleTimeSlotIntervalToServer()).ConfigureAwait(true);
                    if (rtnResult)
                    {
                        ActivitySyncTimerFromApi();
                    }
                }), DispatcherPriority.Background);

                // GetNotesFromLocalDB();

                if (Common.Storage.IsProjectRuning)
                {
                    int index = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                    listproject.SelectedIndex = index;
                }
                else
                {
                    listproject.SelectedIndex = 0;
                }


            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }

        private void keyTimer_Tick(object sender, EventArgs e)
        {
            if (!IsPlaying && IsStop)
            {
                DateTime mdatTime = DateTime.Now;
                mdatTime = mdatTime.AddMinutes(-1);
                string datetimestr = mdatTime.ToString("yyyy-MM-dd HH:mm");
                Console.WriteLine(datetimestr);
                string textFile = "keyboardlogger.txt";
                int Keycount = 0;
                if (File.Exists(textFile))
                {
                    string[] lines = File.ReadAllLines(textFile);
                    foreach (var ln in lines)
                    {
                        if (ln.StartsWith(datetimestr))
                        {
                            Keycount++;
                        }
                    }
                    Console.WriteLine("KeyPad Activity Updated");
                    File.WriteAllText("keyboardlogger.txt", String.Empty);
                }
                Storage.KeyBoradEventCount += Keycount;
                Storage.AverageEventCount += Keycount;
                Storage.LastProjectEventCountTime = Storage.IdleProjectTime;
                Storage.LastToDoEventCountTime = Storage.IdleToDoTime;
                DateTime oCurrentDate = DateTime.Now;
                Storage.StopTimeForDB = oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");

                textFile = "mouse_log0.txt";
                //string textFile = @"C:\Users\Anuriti sharma\Documents\mouse_log0.txt"; 
                int Mousecount = 1;
                if (File.Exists(textFile))
                {
                    // Read a text file line by line.  
                    string[] lines = File.ReadAllLines(textFile);
                    foreach (var ln in lines)
                    {
                        if (ln.StartsWith(datetimestr))
                        {
                            Mousecount++;
                        }
                    }
                    Console.WriteLine("Mouse Activity Updated");
                    File.WriteAllText(textFile, String.Empty);
                }
                Storage.MouseEventCount += Mousecount;
                Storage.AverageEventCount += Mousecount;
                Storage.LastProjectEventCountTime = Storage.IdleProjectTime;
                Storage.LastToDoEventCountTime = Storage.IdleToDoTime;
                DateTime oCurrentDate1 = DateTime.Now;
                Storage.StopTimeForDB = oCurrentDate1.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss");
            }
        }

        public async Task<bool> SendIdleTimeSlotIntervalToServer()
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
                            interval_time = Common.Storage.ActivityIntervel.ToStrVal(),
                            start = item.Start,
                            stop = item.Stop,
                            time_type = item.TimeType,
                            selfiVerification = item.SelfieVerification,
                            source_type = item.SourceType,
                            intervals = GetIdleIntervalsList(item.Start),
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
                                                BaseService<tbl_KeyMouseTrack_Slot_Idle> dbService3 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                                                track_Slots_Idle = new List<tbl_KeyMouseTrack_Slot_Idle>(dbService3.GetAllById(item2.Start, "Start"));
                                                foreach (var item3 in track_Slots_Idle)
                                                {
                                                    dbService2.DeleteSlot(item3.Id);
                                                }
                                                BaseService<tbl_KeyMouseTrack_Slot_Idle> service = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                                                service.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                                                BaseService<tbl_IdleTimeDetails> service1 = new BaseService<tbl_IdleTimeDetails>();
                                                service1.Delete(new tbl_IdleTimeDetails());

                                                // App & Url
                                                BaseService<tbl_AppAndUrl> dbService4 = new BaseService<tbl_AppAndUrl>();
                                                appAndUrl_Tracking = new List<tbl_AppAndUrl>(dbService4.GetAllById(item2.Start, "Start"));
                                                foreach (var item3 in appAndUrl_Tracking)
                                                {
                                                    dbService2.DeleteAppAndURL(item3.Id);
                                                }
                                                //
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
                                    Common.Storage.IsScreenShotCapture = false;
                                    result = false;
                                }
                            }
                            else
                            {
                                Common.Storage.IsScreenShotCapture = false;
                                result = false;
                            }
                            //}
                        }
                    }
                }
                else
                {
                    Common.Storage.IsScreenShotCapture = false;
                    result = false;
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                Common.Storage.IsScreenShotCapture = false;
                result = false;
                // await MyMessageBox.Show(new Window(), ex.Message, "Error", MyMessageBox.MessageBoxButtons.Ok);
                //throw new Exception(ex.Message);
            }
            return result;
        }
        public void ManualSyncIdleTimeAssign()
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
                    GetProjectsList = new List<Organisation_Projects>();
                    //GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
                }

                string currentDate = DateTime.Now.ToString();
                LastUpdateText = "Last updated at: " + currentDate;
                bool rtnResult = SendIdleTimeSlotIntervalToServerAssign();
                if (rtnResult)
                {
                    ActivitySyncTimerFromApi();
                }

                // GetNotesFromLocalDB();

                if (Common.Storage.IsProjectRuning)
                {
                    int index = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.CurrentProjectId.ToStrVal());
                    listproject.SelectedIndex = index;
                }
                else
                {
                    listproject.SelectedIndex = 0;
                }


            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public bool SendIdleTimeSlotIntervalToServerAssign()
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
                            interval_time = Common.Storage.ActivityIntervel.ToStrVal(),
                            start = item.Start,
                            stop = item.Stop,
                            time_type = item.TimeType,
                            selfiVerification = item.SelfieVerification,
                            source_type = item.SourceType,
                            intervals = GetIdleIntervalsList(item.Start),
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
                            responseModel = _services.ActivityLogAsyncForIdle(new Get_API_Url().ActivityLogApi(_baseURL), true, objHeaderModel, finallist2);
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
                                            responseModel = _services.ActivityLogAsyncForIdle(new Get_API_Url().ActivityLogApi(_baseURL), true, objHeaderModel, finallist);
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
                                                BaseService<tbl_KeyMouseTrack_Slot_Idle> dbService3 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                                                track_Slots_Idle = new List<tbl_KeyMouseTrack_Slot_Idle>(dbService3.GetAllById(item2.Start, "Start"));
                                                foreach (var item3 in track_Slots_Idle)
                                                {
                                                    dbService2.DeleteSlot(item3.Id);
                                                }
                                                BaseService<tbl_KeyMouseTrack_Slot_Idle> service = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                                                service.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                                                BaseService<tbl_IdleTimeDetails> service1 = new BaseService<tbl_IdleTimeDetails>();
                                                service1.Delete(new tbl_IdleTimeDetails());
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
        public List<Intervals> GetIdleIntervalsList(string startTime)
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
                //App & Url
                AppAndUrl appAndUrls;
                BaseService<tbl_AppAndUrl> dbService = new BaseService<tbl_AppAndUrl>();
                appAndUrl_Tracking = new List<tbl_AppAndUrl>(dbService.GetAllById(startTime, "Start"));
                if (appAndUrl_Tracking != null)
                {
                    if (appAndUrl_Tracking.Count > 0)
                    {
                        foreach (var appAndUrlData in appAndUrl_Tracking)
                        {
                            appAndUrls = new AppAndUrl()
                            {
                                isApp = appAndUrlData.IsApp,
                                spendTime = appAndUrlData.SpendTime,
                                name = appAndUrlData.Name
                            };
                            _appAndUrls.Add(appAndUrls);
                        }
                    }
                }
                //
                BaseService<tbl_KeyMouseTrack_Slot_Idle> dbService2 = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                track_Slots_Idle = new List<tbl_KeyMouseTrack_Slot_Idle>(dbService2.GetAllById(startTime, "Start"));
                if (track_Slots_Idle != null)
                {
                    if (track_Slots_Idle.Count > 0)
                    {
                        foreach (var slot in track_Slots_Idle)
                        {
                            _location = new Models.WriteDTO.Location()
                            {
                                @long = slot.Longitude.ToStrVal(),
                                lat = slot.Latitude.ToStrVal()
                            };
                            activityLevel = new ActivityLevel()
                            {
                                //percentage method
                                average = ActivtyPercentage(Convert.ToInt32(slot.AverageActivity), Common.Storage.ActivityIntervel),
                                keyboard = ActivtyPercentage(Convert.ToInt32(slot.keyboardActivity), Common.Storage.ActivityIntervel),
                                mouse = ActivtyPercentage(Convert.ToInt32(slot.MouseActivity), Common.Storage.ActivityIntervel),
                            };
                            intervals = new Intervals()
                            {
                                Entry = slot.Entry,
                                appAndUrls = _appAndUrls,
                                location = _location,
                                screenUrl = slot.ScreenActivity.ToStrVal(),
                                activityLevel = activityLevel,
                                from = slot.IntervalStratTime.ToStrVal(),
                                to = slot.IntervalEndTime.ToStrVal(),
                                interval_time_db = Common.Storage.ActivityIntervel.ToStrVal()

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
        public void StopTimerAfterIdlePopUp()
        {
            SlotTimerObject.Stop();
            timerToDo.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            TotalSecound = 0;
            TotalSMinute = 0;
            Totalhour = 0;
            HeaderTime = "00:00:00";
            string _time = GetTimeFromProject(projectIdSelected);
            Dispatcher.UIThread.InvokeAsync(new Action(() =>
            {
                HeaderTime = _time;
            }), DispatcherPriority.Background);

            Common.Storage.IsProjectRuning = false;
            Common.Storage.IsToDoRuning = false;
        }
        public void UpdateToDoListAfterIdlePopUp(int CurrentToDoId)
        {
            try
            {

                if (ToDoListData != null && ToDoListData.Count > 0)
                {


                    foreach (var item in ToDoListData)
                    {
                        if (item.Id == CurrentToDoId)
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoStopIcon = false;
                            item.ToDoTimeConsumed = TODOProjectTime;
                            SelectedprojectToDo = item;
                        }
                        else
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                    // GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
                    ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);

                }

            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        public void ContinueTimerAfterIdlePopUp()
        {
            SlotTimerObject.Stop();
            timerToDo.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            TotalSecound = 0;
            TotalSMinute = 0;
            Totalhour = 0;
            Common.Storage.IsProjectRuning = false;
            Common.Storage.IsToDoRuning = false;
            string _time = GetTimeFromProject(projectIdSelected);
            Dispatcher.UIThread.InvokeAsync(new Action(() =>
            {
                HeaderTime = _time;
            }), DispatcherPriority.Background);

            UpdateToDoListAfterIdlePopUp(ToDoSelectedID);
            BindTotalWorkTime();
            timerToDo.Start();
            timerproject.Start();
            //TODOProjectTime = Storage.ContinueToDoEventCountTime;
            //ProjectTime = Storage.ContinueProjectEventCountTime;
            IsStop = true;
            IsPlaying = false;
        }
        public void ProjectPlayUpdate(string pid)
        {
            try
            {
                ObservableCollection<Organisation_Projects> ProjectListFinal = new ObservableCollection<Organisation_Projects>();
                foreach (var item in GetProjectsList)
                {
                    if (item.ProjectId == pid)
                    {
                        item.ProjectPlayIcon = false;
                        item.ProjectStopIcon = true;
                        item.ProjectTime = ProjectTime;
                        item.checkTodoApiCallOrNot = false;
                    }
                }

                if (Selectedproject != null)
                {
                    GetProjectsList = new List<Organisation_Projects>();
                    // GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void AddUpdateProjectTimeToDBinContinueIdle(bool boolVal)
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
                    t = Storage.StopTimeForDB;
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
                    SourceType = Common.Storage.OpreatingSystem,
                    Stop = t,
                    TimeType = "1",
                    ToDoId = todo,
                    IntervalTime = 0,
                    Sno = 0
                };

                new DashboardSqliteService().SaveStartStopProjectTimeINLocalDB(tblTimer, boolVal);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }
        public void AddUpdateProjectTimeToDBinStopIdle(bool boolVal)
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
                    t = Storage.StopTimeForDB;
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
                    SourceType = Common.Storage.OpreatingSystem,
                    Stop = t,
                    TimeType = "1",
                    ToDoId = todo,
                    IntervalTime = 0,
                    Sno = 0
                };

                new DashboardSqliteService().SaveStartStopProjectTimeINLocalDB(tblTimer, boolVal);
            }

            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public void UpdateProjectIdandToDoIdinTimerTable(bool boolVal, string ProjectId, int TodoId)
        {
            try
            {
                string t;
                string IdleProjectId = "";
                int IdleToDoId = 0;
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
                    t = Storage.StopTimeForDB;
                    IdleProjectId = ProjectId;
                    IdleToDoId = TodoId;
                }
                if (ToDoSelectedID == 0)
                {
                    IdleToDoId = 0;
                }
                else
                {
                    IdleToDoId = TodoId;
                }
                if (Common.Storage.IsToDoRuning == false)
                {
                    IdleToDoId = 0;
                }
                tbl_Timer tblTimer;
                tblTimer = new tbl_Timer()
                {
                    Start = t, // oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss"),
                    ProjectId = IdleProjectId,//projectIdSelected,
                    OrgId = orgdSelectedID,
                    SelfieVerification = "true",
                    SourceType = Common.Storage.OpreatingSystem,
                    Stop = "",
                    TimeType = "1",
                    ToDoId = IdleToDoId,//todo,
                    IntervalTime = 0,
                    Sno = 0
                };

                new DashboardSqliteService().SaveProjectIdandToDoIdInLocalDB(tblTimer, boolVal);


            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                // throw new Exception(ex.Message);
            }
        }

        #endregion

        #region SlotTimer 
        private void SlotTimerObject_Elapsed(object sender, EventArgs e)
        {
            try
            {

                IsSlotTimer = true;
                //SlotTimerObject.Stop();
                //
                //SlotInterval = 2;
                //
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


                var keyboardActivity = Common.Storage.KeyBoradEventCount.ToStrVal();
                var MouseActivity = Common.Storage.MouseEventCount.ToStrVal();
                var AverageActivity = Common.Storage.AverageEventCount.ToStrVal();

                Common.Storage.AverageEventCount = 0;
                Common.Storage.KeyBoradEventCount = 0;
                Common.Storage.MouseEventCount = 0;


                // App & Url
                List<tbl_AppAndUrl> _appAndUrls = new List<tbl_AppAndUrl>();
                tbl_AppAndUrl appAndUrls;
                //app data
                BaseService<tbl_Apptracking> dbService = new BaseService<tbl_Apptracking>();
                app_Tracking = new List<tbl_Apptracking>(dbService.GetAll());
                if (app_Tracking != null && app_Tracking.Count > 0)
                {
                    var groupApp_TrackingList = app_Tracking.GroupBy(z => z.Activity_Name).ToList();
                    string logtime;
                    string AppOrUrl_Name = "";
                    string startTime = "";
                    foreach (var groupApp_Tracking in groupApp_TrackingList)
                    {
                        foreach (var b in groupApp_Tracking)
                        {
                            AppOrUrl_Name = b.Activity_Name;
                            startTime = b.Start;
                            continue;
                        }
                        //00:00:00
                        int sum_AppUrl = groupApp_Tracking.Sum(a => Convert.ToInt32(a.Activity_TotalRun));
                        TimeSpan _time2 = TimeSpan.FromSeconds(sum_AppUrl);
                        logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        _time2.Hours,
                        _time2.Minutes,
                        _time2.Seconds);
                        appAndUrls = new tbl_AppAndUrl()
                        {
                            Start = startTime,
                            IsApp = "true",
                            SpendTime = sum_AppUrl.ToStrVal(),
                            Name = AppOrUrl_Name
                        };
                        _appAndUrls.Add(appAndUrls);
                    }
                }
                //url data
                BaseService<tbl_URLTracking> dbService1 = new BaseService<tbl_URLTracking>();
                url_Tracking = new List<tbl_URLTracking>(dbService1.GetAll());
                if (url_Tracking != null && url_Tracking.Count > 0)
                {
                    var groupURL_TrackingList = url_Tracking.GroupBy(z => z.UrlName).ToList();
                    string urlLogtime;
                    string Url_Name = "";
                    string startTime = "";
                    foreach (var groupUrl_Tracking in groupURL_TrackingList)
                    {
                        foreach (var b in groupUrl_Tracking)
                        {
                            Url_Name = b.UrlName;
                            startTime = b.Start;
                            continue;
                        }
                        int sum_Url = groupUrl_Tracking.Sum(a => Convert.ToInt32(a.TotalTimeSpent));
                        TimeSpan _time2 = TimeSpan.FromSeconds(sum_Url);
                        urlLogtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        _time2.Hours,
                        _time2.Minutes,
                        _time2.Seconds);
                        appAndUrls = new tbl_AppAndUrl()
                        {
                            Start = startTime,
                            IsApp = "false",
                            SpendTime = sum_Url.ToStrVal(),
                            Name = Url_Name
                        };
                        _appAndUrls.Add(appAndUrls);
                    }
                }

                if (_appAndUrls.Count > 0)
                {
                    BaseService<tbl_AppAndUrl> addAppAndUrl = new BaseService<tbl_AppAndUrl>();
                    addAppAndUrl.AddRange(_appAndUrls);
                }
                BaseService<tbl_Apptracking> service2 = new BaseService<tbl_Apptracking>();
                service2.Delete(new tbl_Apptracking());
                BaseService<tbl_URLTracking> service3 = new BaseService<tbl_URLTracking>();
                service3.Delete(new tbl_URLTracking());

               // LogFile.WriteMessageLog("New app and url" + DateTime.Now);
                //

                if (Common.Storage.SlotRunning)
                {
                    DateTime a = Convert.ToDateTime(Common.Storage.SlotTimerStartTime.ToString());
                    int b = Common.Storage.timeIntervel;
                    Common.Storage.SlotTimerPreviousEndTime = a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");
                    new DashboardSqliteService().AddTimeIntervalToDB(a.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                            a.AddMinutes(b).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"), keyboardActivity, MouseActivity, AverageActivity);

                }
                else
                {
                    DateTime c = Convert.ToDateTime(Common.Storage.SlotTimerPreviousEndTime.ToString());
                    new DashboardSqliteService().AddTimeIntervalToDB(c.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                            c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00"),
                            keyboardActivity, MouseActivity, AverageActivity);
                    Common.Storage.SlotTimerPreviousEndTime = c.AddMinutes(SlotInterval).ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00");

                }

                //Common.Storage.SlotTimerStartTime = null;
                Common.Storage.SlotRunning = false;
                Common.Storage.timeIntervel = SlotInterval;// 2;
                // SlotTimerObject.Interval = new TimeSpan(0, 1, 0);
                IsSlotTimer = true;
                SlotTimerObject.Start();
                if (AverageActivity == "0")
                {
                    CheckIdleTimeFromDb();
                }
                else
                {
                    List<tbl_KeyMouseTrack_Slot_Idle> userIdleTimeList = new List<tbl_KeyMouseTrack_Slot_Idle>();
                    BaseService<tbl_KeyMouseTrack_Slot_Idle> dbServiceT = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                    userIdleTimeList = new List<tbl_KeyMouseTrack_Slot_Idle>(dbServiceT.GetAll());
                    if (userIdleTimeList.Count > 0)
                    {
                        foreach (var idletime in userIdleTimeList)
                        {
                            tbl_KeyMouseTrack_Slot keyMouseTrack_Slot = new tbl_KeyMouseTrack_Slot()
                            {
                                IntervalStratTime = idletime.IntervalStratTime,
                                IntervalEndTime = idletime.IntervalEndTime,
                                Start = idletime.Start,
                                End = idletime.End,
                                Hour = idletime.Hour,
                                keyboardActivity = idletime.keyboardActivity,
                                MouseActivity = idletime.MouseActivity,
                                AverageActivity = idletime.AverageActivity,
                                Id = 0,
                                Latitude = idletime.Latitude,
                                Longitude = idletime.Longitude,
                                ScreenActivity = idletime.ScreenActivity,
                                CreatedDate = idletime.CreatedDate,
                                Entry = idletime.Entry
                            };
                            BaseService<tbl_KeyMouseTrack_Slot> gg = new BaseService<tbl_KeyMouseTrack_Slot>();
                            Common.Storage.SlotID = gg.Add(keyMouseTrack_Slot);
                        }
                        BaseService<tbl_KeyMouseTrack_Slot_Idle> service = new BaseService<tbl_KeyMouseTrack_Slot_Idle>();
                        service.Delete(new tbl_KeyMouseTrack_Slot_Idle());
                        BaseService<tbl_IdleTimeDetails> service1 = new BaseService<tbl_IdleTimeDetails>();
                        service1.Delete(new tbl_IdleTimeDetails());

                    }
                }
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
                SlotTimerObject.Interval = new TimeSpan(0, result, 0);  //new TimeSpan(0, result, 0); //new TimeSpan(0, 2, 0);  //
                SlotTimerObject.Start();
                IsSlotTimer = true;
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

                // App & Url
                List<tbl_AppAndUrl> _appAndUrls = new List<tbl_AppAndUrl>();
                tbl_AppAndUrl appAndUrls;
                //app data
                BaseService<tbl_Apptracking> dbService = new BaseService<tbl_Apptracking>();
                app_Tracking = new List<tbl_Apptracking>(dbService.GetAll());
                if (app_Tracking != null && app_Tracking.Count > 0)
                {
                    var groupApp_TrackingList = app_Tracking.GroupBy(z => z.Activity_Name).ToList();
                    string logtime;
                    string AppOrUrl_Name = "";
                    string startTime = "";
                    foreach (var groupApp_Tracking in groupApp_TrackingList)
                    {
                        foreach (var b in groupApp_Tracking)
                        {
                            AppOrUrl_Name = b.Activity_Name;
                            startTime = b.Start;
                            continue;
                        }
                        //00:00:00
                        int sum_AppUrl = groupApp_Tracking.Sum(a => Convert.ToInt32(a.Activity_TotalRun));
                        TimeSpan _time2 = TimeSpan.FromSeconds(sum_AppUrl);
                        logtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        _time2.Hours,
                        _time2.Minutes,
                        _time2.Seconds);
                        appAndUrls = new tbl_AppAndUrl()
                        {
                            Start = startTime,
                            IsApp = "true",
                            SpendTime = sum_AppUrl.ToStrVal(),
                            Name = AppOrUrl_Name
                        };
                        _appAndUrls.Add(appAndUrls);
                    }
                }
                //url data
                BaseService<tbl_URLTracking> dbService1 = new BaseService<tbl_URLTracking>();
                url_Tracking = new List<tbl_URLTracking>(dbService1.GetAll());
                if (url_Tracking != null && url_Tracking.Count > 0)
                {
                    var groupURL_TrackingList = url_Tracking.GroupBy(z => z.UrlName).ToList();
                    string urlLogtime;
                    string Url_Name = "";
                    string startTime = "";
                    foreach (var groupUrl_Tracking in groupURL_TrackingList)
                    {
                        foreach (var b in groupUrl_Tracking)
                        {
                            Url_Name = b.UrlName;
                            startTime = b.Start;
                            continue;
                        }
                        int sum_Url = groupUrl_Tracking.Sum(a => Convert.ToInt32(a.TotalTimeSpent));
                        TimeSpan _time2 = TimeSpan.FromSeconds(sum_Url);
                        urlLogtime = string.Format("{0:D2}:{1:D2}:{2:D2}",
                        _time2.Hours,
                        _time2.Minutes,
                        _time2.Seconds);
                        appAndUrls = new tbl_AppAndUrl()
                        {
                            Start = startTime,
                            IsApp = "false",
                            SpendTime = sum_Url.ToStrVal(),
                            Name = Url_Name
                        };
                        _appAndUrls.Add(appAndUrls);
                    }
                }
                if (_appAndUrls.Count > 0)
                {
                    BaseService<tbl_AppAndUrl> addAppAndUrl = new BaseService<tbl_AppAndUrl>();
                    addAppAndUrl.AddRange(_appAndUrls);
                }

                BaseService<tbl_Apptracking> service2 = new BaseService<tbl_Apptracking>();
                service2.Delete(new tbl_Apptracking());
                BaseService<tbl_URLTracking> service3 = new BaseService<tbl_URLTracking>();
                service3.Delete(new tbl_URLTracking());

               // LogFile.WriteMessageLog("from Add Slot New app and url" + DateTime.Now);
                //

                tbl_KeyMouseTrack_Slot keyMouseTrack_Slot;
                DateTime oCurrentDate = DateTime.Now;
                if (string.IsNullOrEmpty(Common.Storage.SlotTimerPreviousEndTime))
                {
                    Common.Storage.SlotTimerPreviousEndTime = Common.Storage.SlotTimerStartTime;
                }
                DateTime _IntervalStratTime = Convert.ToDateTime(Common.Storage.SlotTimerPreviousEndTime.ToString());

                //LogFile.WriteMessageLog("1.Slot Added from AddSlot():- \n Start Time -" + _IntervalStratTime.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                //    " End Time -" + oCurrentDate.ToString("yyyy'-'MM'-'dd' 'HH':'mm':'00") +
                //    "\n" + "KeyBoradEventCount : " + Storage.KeyBoradEventCount + " MouseEventCount : " + Storage.MouseEventCount
                //   + " AverageEventCount : " + Storage.AverageEventCount + "\n" + DateTime.Now.ToString("hh:mm:ss"));

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
                    GetProjectsList = GetProjectsList2;
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
                //if (Selectedproject != null)
                //{
                //    GetProjectsList = new List<Organisation_Projects>();
                //    // GetProjectsList2[1] = Selectedproject;
                //    //GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                //    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                //    GetProjectsList = GetProjectsList2;
                //}

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
                    //listproject.SelectedIndex = 0;
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
            if (percentage >= 100)
            {
                percentage = 100;
            }
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
                //App & Url
                AppAndUrl appAndUrls;
                BaseService<tbl_AppAndUrl> dbService = new BaseService<tbl_AppAndUrl>();
                appAndUrl_Tracking = new List<tbl_AppAndUrl>(dbService.GetAllById(startTime, "Start"));
                if (appAndUrl_Tracking != null)
                {
                    if (appAndUrl_Tracking.Count > 0)
                    {
                        foreach (var appAndUrlData in appAndUrl_Tracking)
                        {
                            appAndUrls = new AppAndUrl()
                            {
                                isApp = appAndUrlData.IsApp,
                                spendTime = appAndUrlData.SpendTime,
                                name = appAndUrlData.Name
                            };
                            _appAndUrls.Add(appAndUrls);
                        }
                    }
                }
                //
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
                                //percentage method
                                average = ActivtyPercentage(Convert.ToInt32(slot.AverageActivity), Common.Storage.ActivityIntervel),
                                keyboard = ActivtyPercentage(Convert.ToInt32(slot.keyboardActivity), Common.Storage.ActivityIntervel),
                                mouse = ActivtyPercentage(Convert.ToInt32(slot.MouseActivity), Common.Storage.ActivityIntervel),
                            };
                            intervals = new Intervals()
                            {
                                Entry = slot.Entry,
                                appAndUrls = _appAndUrls,
                                location = _location,
                                screenUrl = slot.ScreenActivity.ToStrVal() == string.Empty ? Common.Storage.ScreenURl : slot.ScreenActivity.ToStrVal(),
                                activityLevel = activityLevel,
                                from = slot.IntervalStratTime.ToStrVal(),
                                to = slot.IntervalEndTime.ToStrVal(),
                                interval_time_db = Common.Storage.ActivityIntervel.ToStrVal()

                            };

                            listofIntervals = new List<Intervals>();
                            listofIntervals.Add(intervals);
                            finalIntervals.AddRange(listofIntervals);
                            //idle time
                            List<Intervals> SortedList = finalIntervals.OrderBy(o => o.Entry).ToList();
                            finalIntervals.Clear();
                            finalIntervals.AddRange(SortedList);
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
                            interval_time = Common.Storage.ActivityIntervel.ToStrVal(),
                            start = item.Start,
                            stop = item.Stop,
                            time_type = item.TimeType,
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
                            // finallist2 = finallist2.OrderByDescending(x => x.intervals.).ToList();
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

                                                // App & Url
                                                BaseService<tbl_AppAndUrl> dbService4 = new BaseService<tbl_AppAndUrl>();
                                                appAndUrl_Tracking = new List<tbl_AppAndUrl>(dbService4.GetAllById(item2.Start, "Start"));
                                                foreach (var item3 in appAndUrl_Tracking)
                                                {
                                                    dbService2.DeleteAppAndURL(item3.Id);
                                                }
                                                //
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
                                    Common.Storage.IsScreenShotCapture = false;
                                    result = false;
                                }
                            }
                            else
                            {
                                Common.Storage.IsScreenShotCapture = false;
                                result = false;
                            }
                            //}
                        }
                    }
                }
                else
                {
                    Common.Storage.IsScreenShotCapture = false;
                    result = false;
                }

            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
                Common.Storage.IsScreenShotCapture = false;
                result = false;
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
                        var data = (List<Organisation_Projects>)listproject.Items;
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
                if (Storage.IsProjectRuning == false)
                {
                    if (isWindows)
                    {

                        activityTracker.KeyBoardActivity(true);
                        activityTracker.MouseActivity(true);
                        LogFile.WriteaActivityLog("HeaderPlay Success");
                    }

                    //App & Url
                    AppandUrlTracking.Interval = TimeSpan.FromSeconds(Convert.ToInt32(10));
                    AppandUrlTracking.Tick += AppandUrlTracking_Tick;
                    AppandUrlTracking.Start();
                    timerStartTime = DateTime.Now;
                    stopWatchAppUrl.Start();
                    stopWatchForUrl.Start();
                    //
                }
                IsAddNoteButtonEnabled = Storage.IsProjectRuning && Storage.IsToDoRuning ? false : true;
                AddNoteButtonOpacity = IsAddNoteButtonEnabled ? 1 : 0.5;
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
                    if (arry.Length > 2)
                    {
                        h2 = Convert.ToInt32(arry[0]);
                        m2 = Convert.ToInt32(arry[1]);
                        s2 = Convert.ToInt32(arry[2]);
                    }

                    ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));

                    HeaderTime = ProjectTime;
                    UpdateProjectList(projectIdSelected);
                    AddUpdateProjectTimeToDB(true);
                    TimeIntervalAddToDB();
                    timerproject.Start();
                    Common.Storage.IsProjectRuning = true;
                    Common.Storage.CurrentProjectId = projectIdSelected.ToInt32();
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
                //App & Url
                timerStartTime = null;
                stopWatchAppUrl.Stop();
                stopWatchForUrl.Stop();
                //

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
                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
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
                //string _time = GetTimeFromProject(projectIdSelected);
                //HeaderTime = _time;
                //BindTotalWorkTime();
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

                        if (isWindows && activityTracker.globalKeyHook == null && activityTracker.globalMouseHook == null)
                        {
                            activityTracker.KeyBoardActivity(true);
                            activityTracker.MouseActivity(true);
                            LogFile.WriteaActivityLog("ProjectPlay success");
                        }

                        //App & Url
                        AppandUrlTracking.Interval = TimeSpan.FromSeconds(Convert.ToInt32(10));
                        AppandUrlTracking.Tick += AppandUrlTracking_Tick;
                        AppandUrlTracking.Start();
                        timerStartTime = DateTime.Now;
                        stopWatchAppUrl.Start();
                        stopWatchForUrl.Start();
                        //
                    }
                    catch (Exception ex)
                    {

                        LogFile.ErrorLog(ex);
                    }
                }

                IsAddNoteButtonEnabled = Storage.IsProjectRuning && Storage.IsToDoRuning ? false : true;
                AddNoteButtonOpacity = IsAddNoteButtonEnabled ? 1 : 0.5;
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
                if (arry.Length > 2)
                {
                    h2 = Convert.ToInt32(arry[0]);
                    m2 = Convert.ToInt32(arry[1]);
                    s2 = Convert.ToInt32(arry[2]);
                }
                ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
                HeaderTime = ProjectTime;
                Storage.IdleProjectTime = ProjectTime;
                UpdateProjectList(projectIdSelected);
                timerproject.Start();
                IsStop = true;
                IsPlaying = false;
                //listproject.ScrollIntoView(listproject.SelectedItem);
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
                //App & Url
                timerStartTime = null;
                stopWatchAppUrl.Stop();
                stopWatchForUrl.Stop();
                // StopTimeIntervalUpdateToDB();
                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
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

                Manualsync();
                //BindTotalWorkTime();
                // AddZebraPatternToProjectList();
                // UpdateProjectList(projectIdSelected);
                // listproject.ScrollIntoView(Selectedproject);
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
                    Storage.IdleProjectTime = ProjectTime;
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
                List<Organisation_Projects> ProjectListFinal = new List<Organisation_Projects>();
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
                    GetProjectsList = new List<Organisation_Projects>();
                    // GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
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
                    GetProjectsList = new List<Organisation_Projects>();
                    //   GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
                    Dispatcher.UIThread.InvokeAsync(new Action(() =>
                    {
                        int index = GetProjectsList.FindIndex(x => x.ProjectId == projectIdSelected);
                        listproject.SelectedIndex = index;
                    }), DispatcherPriority.Background);
                }

                // Initalize TotalTodayWorkTime
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
                    SourceType = Common.Storage.OpreatingSystem,
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
                    SourceType = Common.Storage.OpreatingSystem,
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
                    SourceType = Common.Storage.OpreatingSystem,
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
                    Storage.IdleToDoTime = TODOProjectTime;
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
                        //App & Url
                        AppandUrlTracking.Interval = TimeSpan.FromSeconds(Convert.ToInt32(10));
                        AppandUrlTracking.Tick += AppandUrlTracking_Tick;
                        AppandUrlTracking.Start();
                        timerStartTime = DateTime.Now;
                        stopWatchAppUrl.Start();
                        stopWatchForUrl.Start();
                        //
                    }
                    catch (Exception ex)
                    {

                        LogFile.ErrorLog(ex);
                    }
                }

                IsAddNoteButtonEnabled = Storage.IsProjectRuning && Storage.IsToDoRuning ? false : true;
                AddNoteButtonOpacity = IsAddNoteButtonEnabled ? 1 : 0.5;
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
                Storage.IdleToDoTime = TODOProjectTime;
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
                if (arry.Length > 2)
                {
                    try
                    {
                        h2 = Convert.ToInt32(arry[0]);
                        m2 = Convert.ToInt32(arry[1]);
                        s2 = Convert.ToInt32(arry[2]);
                    }
                    catch
                    {

                    }

                }
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
                IsAddNoteButtonEnabled = false;
                AddNoteButtonOpacity = 0.5;
                SlotTimerObject.Stop();
                timerToDo.Stop();
                timerproject.Stop();
                IsStop = false;
                IsPlaying = true;
                ToDoSelectedID = TodoID;
                Common.Storage.IsProjectRuning = false;
                Common.Storage.IsToDoRuning = false;

                AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);
                bool checkslot = CheckSlotExistNot();
                if (!checkslot)
                {
                    AddSlot();
                }

                TotalSecound = 0;
                TotalSMinute = 0;
                Totalhour = 0;
                Manualsync();

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
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoTimeConsumed = TODOProjectTime;
                            SelectedprojectToDo = item;
                        }
                        else
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                    // GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
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
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                    //GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
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
                GetProjectsList = new List<Organisation_Projects>();
                GetProjectsList2 = new List<Organisation_Projects>();
                Organisation_Projects projects;
                List<tbl_Organisation_Projects> FindUserProjectListFinal = new List<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new List<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));


                foreach (var item in FindUserProjectListFinal)
                {
                    string ProjectTimeConsumed = "";
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
                    if (Common.Storage.IsProjectRuning == true)
                    {
                        if (item.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                        {
                            ProjectTimeConsumed = ProjectTime;
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

                    GetProjectsList2.Add(projects);
                }
                GetProjectsList = GetProjectsList2;
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
                GetProjectsList = new List<Organisation_Projects>();
                GetProjectsList2 = new List<Organisation_Projects>();
                GetProjectsNameList = new List<string>();
                Organisation_Projects projects;
                List<tbl_Organisation_Projects> FindUserProjectListFinal = new List<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new List<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
                foreach (var item in FindUserProjectListFinal)
                {
                    //idle time
                    GetProjectsNameList.Add(item.ProjectName);

                    //
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
                GetProjectsList = new List<Organisation_Projects>(GetProjectsList2);
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
                                //get idle time at first time
                                string _time = GetTimeFromProject(Selectedproject.ProjectId);
                                Storage.IdleProjectTime = _time;
                                //
                            }
                            else
                            {
                                listproject.SelectedIndex = 0;
                                //get idle time at first time
                                //  int index = GetProjectsList.FindIndex(x => x.ProjectId == Selectedproject.ProjectId);
                                if (GetProjectsList != null)
                                {
                                    string _time = GetTimeFromProject(GetProjectsList.FirstOrDefault().ProjectId);
                                    Storage.IdleProjectTime = _time;
                                }
                                //
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
                    if (arry.Length > 2)
                    {
                        try
                        {
                            h2 = Convert.ToInt32(arry[0]);
                            m2 = Convert.ToInt32(arry[1]);
                            s2 = Convert.ToInt32(arry[2]);
                        }
                        catch
                        {

                        }

                    }
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
                ToDoAttachmentListData = new ObservableCollection<tbl_ToDoAttachments>();
                tbl_ServerTodoDetails tbl_ServerTodo;
                GetToDoNameList = new List<string>();
                GetToDoAttachmentsList = new List<string>();
                ObservableCollection<tbl_ToDoAttachments> FindUserToDoAttachmentListFinal = new ObservableCollection<tbl_ToDoAttachments>();
                ObservableCollection<tbl_ServerTodoDetails> FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>();
                FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>(new DashboardSqliteService().GetToDoListData(CurrentProjectId));
                FindUserToDoAttachmentListFinal = new ObservableCollection<tbl_ToDoAttachments>(new DashboardSqliteService().GetToDoAttachmentListData(CurrentProjectId));

                ToDoListData.Clear();
                GetToDoListTemp.Clear();
                ToDoAttachmentListData.Clear();
                foreach (var a in FindUserToDoAttachmentListFinal)
                {
                    a.AttachmentImage = "/Assets/attachment.png";
                    ToDoAttachmentListData.Add(a);
                }
                RaisePropertyChanged("ToDoAttachmentListData");

                foreach (var item in FindUserToDoListFinal)
                {
                    if (Common.Storage.IsToDoRuning)
                    {
                        if (item.Id == ToDoSelectedID)
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoTimeConsumed = TODOProjectTime;
                        }
                        else
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoStopIcon = false;
                            if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                            {
                                item.ToDoTimeConsumed = "00:00:00";
                            }
                        }
                    }
                    else
                    {
                        item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                        item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                        item.ToDoStopIcon = false;
                        if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                        {
                            item.ToDoTimeConsumed = "00:00:00";
                        }
                    }
                    //if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                    //{
                    //    item.ToDoTimeConsumed = "00:00:00";
                    //}
                    item.ToDoName = ExtensionMethod.GetShortDescription(item.ToDoName, 25);

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
                    //foreach (var list in ToDoAttachmentListData)
                    //{
                    //    if (ToDoAttachmentListData.Count >= 1)
                    //    {
                    //        GetToDoAttachmentsList.Add(list.AttachmentImage);
                    //    }
                    //}
                    //item.AttachmentImage = GetToDoAttachmentsList;
                    ToDoListData.Add(item);
                    GetToDoListTemp.Add(item);
                    //idle time
                    GetToDoNameList.Add(item.ToDoName);
                    //
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
                // CheckIdleTimeFromDb();
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
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;

                            item.ToDoPlayIcon = false;
                            item.ToDoStopIcon = item.ToDoCompleteIcon ? false : true;
                        }
                        else
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoStopIcon = false;

                        }
                    }
                    else
                    {
                        item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                        item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                                item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                                item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                    GetProjectsList = new List<Organisation_Projects>();
                    // GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                    //GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);
                    GetProjectsList = GetProjectsList2;
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
                    //StopTimeIntervalUpdateToDB();
                    //AddUpdateProjectTimeToDB(false);
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
                }
                if (Common.Storage.IsToDoRuning)
                {
                    AddUpdateProjectTimeToDBByToDoId(false, projectIdSelected);
                    bool checkslot2 = CheckSlotExistNot();
                    if (!checkslot2)
                    {
                        AddSlot();
                    }
                    else
                    {
                        StopTimeIntervalUpdateToDB();
                    }
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
        //public string GetProjectTempTimeFromDB(int projectID, int OrganizationId, string logTime)
        //{
        //    string strLogTime = "";
        //    try
        //    {
        //        BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
        //        tbl_Temp_SyncTimer p = new tbl_Temp_SyncTimer();
        //        DateTime oCurrentDate = DateTime.Now;
        //        p = service2.Gettbl_ProjectDetailsByIDs(projectID, OrganizationId, oCurrentDate.ToString("dd/MM/yyyy"));
        //        if (p != null)
        //        {
        //            if (!string.IsNullOrEmpty(p.TotalWorkedHours))
        //            {
        //                string[] arryT = p.TotalWorkedHours.Split(':');
        //                if (!string.IsNullOrEmpty(logTime))
        //                {
        //                    string[] arryTlogTime = logTime.Split(':');
        //                    int x, y, z;
        //                    x = arryTlogTime[0].ToInt32();
        //                    y = arryTlogTime[1].ToInt32();
        //                    z = arryTlogTime[2].ToInt32();
        //                    int c = arryT[2].ToInt32();
        //                    z = c;
        //                    if (z >= 60)
        //                    {
        //                        z = 0;
        //                        y += 1;

        //                    }
        //                    if (y == 60)
        //                    {
        //                        y = 0;
        //                        x += 1;
        //                    }
        //                    strLogTime = String.Format("{0}:{1}:{2}", x.ToString().PadLeft(2, '0'), y.ToString().PadLeft(2, '0'), z.ToString().PadLeft(2, '0'));
        //                }
        //                else
        //                {
        //                    strLogTime = String.Format("{0}:{1}:{2}", arryT[0].ToString().PadLeft(2, '0'), arryT[1].ToString().PadLeft(2, '0'), arryT[2].ToString().PadLeft(2, '0'));
        //                }
        //            }
        //            else
        //            {
        //                strLogTime = logTime;
        //            }
        //            if (strLogTime == null)
        //            {
        //                strLogTime = "00:00:00";
        //            }
        //            return strLogTime;
        //        }
        //        else
        //        {
        //            strLogTime = logTime;
        //            if (strLogTime == null)
        //            {
        //                strLogTime = "00:00:00";
        //            }
        //            return strLogTime;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        LogFile.ErrorLog(ex);
        //    }
        //    return strLogTime;
        //}
        public void BindTotalWorkTime()
        {
            try
            {
                if (GetProjectsList2 != null)
                {
                    GetProjectsList = new List<Organisation_Projects>();
                    GetProjectsList = GetProjectsList2;
                }

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
        public void BindUseToDoListforComboBox(string CurrentProjectId)
        {
            try
            {
                ToDoList = new ObservableCollection<tbl_ServerTodoDetails>();
                ObservableCollection<tbl_ServerTodoDetails> FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>();
                FindUserToDoListFinal = new ObservableCollection<tbl_ServerTodoDetails>(new DashboardSqliteService().GetToDoListData(CurrentProjectId.ToInt32()));
                ToDoList = FindUserToDoListFinal;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        void BindUserProjectListforComboBox(string OrganisationId)
        {
            try
            {
                ProjectsList = new List<tbl_Organisation_Projects>();
                Organisation_Projects projects;
                List<tbl_Organisation_Projects> FindUserProjectListFinal = new List<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new List<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
                ProjectsList = FindUserProjectListFinal;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
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
                                item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                                item.ToDoPlayIcon = false;
                                item.ToDoStopIcon = item.ToDoCompleteIcon ? false : true;
                                item.ToDoTimeConsumed = TODOProjectTime;
                            }
                            else
                            {
                                item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                                item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                                item.ToDoStopIcon = false;

                                if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                                {
                                    item.ToDoTimeConsumed = "00:00:00";
                                }

                            }
                        }
                        else
                        {
                            item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                            item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
                            item.ToDoStopIcon = false;
                            if (string.IsNullOrEmpty(item.ToDoTimeConsumed))
                            {
                                item.ToDoTimeConsumed = "00:00:00";
                            }
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
                    GetProjectsList = new List<Organisation_Projects>();
                    List<tbl_Organisation_Projects> FindUserProjectListFinal = new List<tbl_Organisation_Projects>();
                    FindUserProjectListFinal = new List<tbl_Organisation_Projects>(dbService.SearchProjectByString(searchtext, projectid, organization_id));
                    if (FindUserProjectListFinal != null)
                    {
                        GetProjectsList = new List<Organisation_Projects>();
                        GetProjectsList2 = new List<Organisation_Projects>();


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
                            if (Common.Storage.IsProjectRuning == true)
                            {
                                if (item.ProjectId == Convert.ToString(Common.Storage.CurrentProjectId))
                                {
                                    ProjectTimeConsumed = ProjectTime;
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
                            GetProjectsList2.Add(projects);

                        }
                        GetProjectsList = new List<Organisation_Projects>(GetProjectsList2);
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
                        if (organisationListResponse.response.data.Count > 0)
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
                        else
                        {

                            pgrProject.IsVisible = false;
                            pgrToDO.IsVisible = false;
                        }
                    }

                    else
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            organisationListResponse = await _services.GetAsyncData_GetApi(new Get_API_Url().UserOrganizationlist(_baseURL, Common.Storage.LoginId), true, objHeaderModel, organisationListResponse);
                        }
                    }
                }
                BindUserOrganisationListFromLocalDB();
                if (FindOrganisationDetails.Count > 0)
                {
                    dynamic firstOrDefault = FindOrganisationDetails.FirstOrDefault(x => x.OrganizationId == Common.Storage.ServerOrg_Id);
                    if (firstOrDefault == null)
                    {
                        firstOrDefault = FindOrganisationDetails.FirstOrDefault();
                    }
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
                if (userProjectlistResponse.response == null)
                {
                    ChangeOrganizationAPICall(OrganizationID);
                    _baseURL = Configurations.UrlConstant + Configurations.UserProjectlistByOrganizationIDApiConstant;
                    objHeaderModel.SessionID = Common.Storage.TokenId;
                    userProjectlistResponse = _services.GetUserProjectlistByOrganizationIDAsync(new Get_API_Url().UserProjectlistByOrganizationID(_baseURL), true, objHeaderModel, entity);

                }
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
                        BindUserProjectListByResponse(orgdSelectedID);
                    }

                    else
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            userProjectlistResponse = _services.GetUserProjectlistByOrganizationIDAsync(new Get_API_Url().UserProjectlistByOrganizationID(_baseURL), true, objHeaderModel, entity);

                            if (userProjectlistResponse.response != null && userProjectlistResponse.response.code == "200")
                            {
                                BindUserProjectListByResponse(orgdSelectedID);
                            }

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

        public void BindUserProjectListByResponse(string OrganizationID)
        {
            if (userProjectlistResponse.response.data.Count > 0)
            {
                List<tbl_Organisation_Projects> _OrganisationProjects = new List<tbl_Organisation_Projects>();
                BaseService<tbl_Organisation_Projects> dbService = new BaseService<tbl_Organisation_Projects>();
                dbService.Delete(new tbl_Organisation_Projects());

                BaseService<tbl_ServerTodoDetails> dbService2 = new BaseService<tbl_ServerTodoDetails>();
                dbService2.Delete(new tbl_ServerTodoDetails());
                BaseService<tbl_ToDoAttachments> dbService3 = new BaseService<tbl_ToDoAttachments>();
                dbService3.Delete(new tbl_ToDoAttachments());

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
            else
            {
                Common.Storage.CurrentOrganisationId = OrganizationID.ToInt32();
                // HeaderTime = "00:00:00";
                HeaderProjectName = string.Empty;
                pgrProject.IsVisible = false;
                pgrToDO.IsVisible = false;
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
                    memberIds = new List<int>() { Common.Storage.LoginUserID.ToInt32() }
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
                                ToDoTimeConsumed = "00:00:00",// assign default time.
                                Id = item.id
                            };
                            // ToDo attachments
                            foreach (var a in item.todoattachments)
                            {
                                tbl_todoattachment = new tbl_ToDoAttachments()
                                {
                                    Id = a.id,
                                    ProjectId = item.project_id,
                                    OrgId = item.organization_id,
                                    ToDoId = a.todo_id,
                                    Image = a.image,
                                    ImageURL = a.image_url,
                                    AttachmentImage = "/Assets/attachment.png"
                                };
                                new DashboardSqliteService().InsertUserToDoAttachmentList(tbl_todoattachment);
                            }
                            //
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
        public async void GetToDoDetailAPICall(int taskid)
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.ToDoDetailApiConstant;
                objHeaderModel = new HeaderModel();
                getToDoDetailsResponseModel = new GetToDoDetailsResponseModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                var org_id = Storage.CurrentOrganisationId;
                var taskId = taskid;
                getToDoDetailsResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().ToDoDetails(_baseURL, taskid), true, objHeaderModel, getToDoDetailsResponseModel);
                if (getToDoDetailsResponseModel.response != null)
                {
                    if (getToDoDetailsResponseModel.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            getToDoDetailsResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().ToDoDetails(_baseURL, taskid), true, objHeaderModel, getToDoDetailsResponseModel);
                        }
                    }
                    if (getToDoDetailsResponseModel.response.code == "200")
                    {
                        Common.Storage.EdittodoData = getToDoDetailsResponseModel.response.data;


                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }

        }
        public async void AddOrEditPageOpen(int selectedEditToDoId)
        {
            //if (Storage.IsProjectRuning)
            //{
            //    IsAddTaskMode = true;
            //    IsAddTaskModeQuitAlert = true;
            //    return;
            //}
            //else
            //{
            IsAddTaskMode = false;
            IsAddTaskModeQuitAlert = false;
            //  }

            IsToDoDetailsPopUp = false;
            IsStaysOpenToDoDetails = false;
            Common.Storage.EditToDoId = selectedEditToDoId == 0 ? 0 : selectedEditToDoId;
            var dialog = new AddOrEditToDo();
            var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
            await dialog.ShowDialog(mainWindow);
            IsUserOffline = Common.CommonServices.IsConnectedToInternet() ? false : true;


            if (!IsUserOffline)
            {
                BaseService<tbl_ServerTodoDetails> service = new BaseService<tbl_ServerTodoDetails>();
                service.Delete(new tbl_ServerTodoDetails());
            }
            pgrToDO.IsVisible = true;
            //Dispatcher.UIThread.InvokeAsync(new Action(() =>
            //{

            //}), DispatcherPriority.Background);


            // BindUserProjectlistByOrganizationID(Common.Storage.CurrentOrganisationId.ToStrVal());
            List<tbl_Organisation_Projects> FindUserProjectListFinal = new List<tbl_Organisation_Projects>();
            FindUserProjectListFinal = new List<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(Common.Storage.CurrentOrganisationId.ToString()));

            foreach (var item in FindUserProjectListFinal)
            {
                BindUserToDoListFromApi(Convert.ToInt32(item.ProjectId), Common.Storage.CurrentOrganisationId, Common.Storage.CurrentUserId);

            }

            ActivitySyncTimerTODOFromApi();

            //  BindUseToDoListFromLocalDB(Common.Storage.AddOrEditToDoProjectId);


            pgrToDO.IsVisible = true;

            var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == Common.Storage.AddOrEditToDoProjectId.ToStrVal());
            if (data != null)
            {
                Selectedproject = data;
            }

            int indexT = 0;
            if (!string.IsNullOrEmpty(Common.Storage.AddOrEditToDoProjectId.ToStrVal()))
            {
                indexT = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.AddOrEditToDoProjectId.ToStrVal());
            }

            listproject.SelectedIndex = indexT;
            pgrToDO.IsVisible = false;

        }

        public async void AddOrEditPageOpen_ToDoView()
        {
            try
            {
                IsToDoDetailsPopUp = false;
                IsStaysOpenToDoDetails = false;
                Common.Storage.EditToDoId = SelectedToDoItem == 0 ? 0 : SelectedToDoItem;
                var dialog = new AddOrEditToDo();
                var mainWindow = (App.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
                await dialog.ShowDialog(mainWindow);
                IsUserOffline = Common.CommonServices.IsConnectedToInternet() ? false : true;
                if (!IsUserOffline)
                {
                    BaseService<tbl_ServerTodoDetails> service = new BaseService<tbl_ServerTodoDetails>();
                    service.Delete(new tbl_ServerTodoDetails());
                }
                pgrToDO.IsVisible = true;
                //Dispatcher.UIThread.InvokeAsync(new Action(() =>
                //{

                //}), DispatcherPriority.Background);
                BindUserProjectlistByOrganizationID(Common.Storage.CurrentOrganisationId.ToStrVal());
                BindUseToDoListFromLocalDB(Common.Storage.AddOrEditToDoProjectId);
                pgrToDO.IsVisible = true;

                var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == Common.Storage.AddOrEditToDoProjectId.ToStrVal());
                if (data != null)
                {
                    Selectedproject = data;
                }

                int indexT = 0;
                if (!string.IsNullOrEmpty(Common.Storage.AddOrEditToDoProjectId.ToStrVal()))
                {
                    indexT = GetProjectsList.FindIndex(x => x.ProjectId == Common.Storage.AddOrEditToDoProjectId.ToStrVal());
                }

                listproject.SelectedIndex = indexT;
                pgrToDO.IsVisible = false;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }

        }
        public async void DeleteToDoAPICall()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.ToDoDeleteApiConstant;
                getResponseModel = new GetResponseModel();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                var org_id = Storage.CurrentOrganisationId;
                var project_id = Storage.AddOrEditToDoProjectId;
                getResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().GetToDoMarkCompleteorDeleteURL(_baseURL, SelectedToDoItem, org_id), true, objHeaderModel, getResponseModel);
                if (getResponseModel.response != null)
                {
                    if (getResponseModel.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            getResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().GetToDoMarkCompleteorDeleteURL(_baseURL, SelectedToDoItem, org_id), true, objHeaderModel, getResponseModel);
                        }
                    }
                    if (getResponseModel.response.code == "200")
                    {
                        IsToDoDetailsPopUp = false;
                        IsStaysOpenToDoDetails = false;
                        new DashboardSqliteService().DeleteSelectedToDo(SelectedToDoItem);
                        BindUseToDoListFromLocalDB(project_id);
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public async void MarkCompleteToDoAPICall()
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.MarkCompleteApiConstant;
                getResponseModel = new GetResponseModel();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                var org_id = Storage.CurrentOrganisationId;
                var project_id = Storage.AddOrEditToDoProjectId;
                getResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().GetToDoMarkCompleteorDeleteURL(_baseURL, SelectedToDoItem, org_id), true, objHeaderModel, getResponseModel);
                if (getResponseModel.response != null)
                {
                    if (getResponseModel.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            getResponseModel = await _services.GetAsyncData_GetApi(new Get_API_Url().GetToDoMarkCompleteorDeleteURL(_baseURL, SelectedToDoItem, org_id), true, objHeaderModel, getResponseModel);
                        }
                    }
                    if (getResponseModel.response.code == "200")
                    {
                        new DashboardSqliteService().UpdateToDoList(SelectedToDoItem);
                        BindUseToDoListFromLocalDB(project_id);
                        IsToDoDetailsPopUp = false;
                        IsStaysOpenToDoDetails = false;
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        public async void ToDoDetailCall(int toDoId)
        {
            //if (Storage.IsProjectRuning)
            //{
            //    IsAddTaskMode = true;
            //    IsAddTaskModeQuitAlert = true;
            //    return;
            //}
            //else
            //{
            IsAddTaskMode = false;
            IsAddTaskModeQuitAlert = false;
            //}
            SelectedToDoItem = toDoId;
            ToDoDetailData = new ObservableCollection<tbl_ServerTodoDetails>(new DashboardSqliteService().GetToDoData(toDoId));
            if (ToDoDetailData != null)
            {
                foreach (var m in ToDoDetailData)
                {
                    if (m.IsCompleted == 1)
                    {
                        IsMarkComplete = m.IsMarkComplete = false;
                        IsOnlyDeleteVisible = m.IsOnlyDeleteVisible = true;
                    }
                    else
                    {
                        IsMarkComplete = m.IsMarkComplete = true;
                        IsOnlyDeleteVisible = m.IsOnlyDeleteVisible = false;
                    }
                }
            }
            ToDoAttachmentListData.Clear();
            GetToDoAttachmentsList = new List<string>();
            ToDoAttachmentListData = new ObservableCollection<tbl_ToDoAttachments>(new DashboardSqliteService().GetToDoAttachmentsData(toDoId));
            if (ToDoAttachmentListData != null)
            {
                foreach (var m in ToDoAttachmentListData)
                {
                    GetToDoAttachmentsList.Add(m.AttachmentImage);
                }
            }
            IsToDoDetailsPopUp = true;
            IsStaysOpenToDoDetails = true;
            GetToDoDetailAPICall(toDoId);
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
                                        item.ToDoCompleteIcon = item.IsCompleted == 1 ? true : false;
                                        item.ToDoPlayIcon = item.ToDoCompleteIcon ? false : true;
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
                                        ProjectID = Convert.ToInt32(a.projectId == null ? 0 : a.projectId),
                                        timeLog = a.timeLog,
                                        todoId = a.todoId
                                    };
                                    tlst.Add(t);
                                }
                                else
                                {
                                    p = new Plist()
                                    {
                                        ProjectID = Convert.ToInt32(a.projectId == null ? 0 : a.projectId),
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


        public async void ActivitySyncTimerTODOFromApi()
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
                            if (a.projectId > 0)
                            {
                                if (Convert.ToInt32(a.todoId) > 0)
                                {
                                    t = new Tlist()
                                    {
                                        ProjectID = Convert.ToInt32(a.projectId == null ? 0 : a.projectId),
                                        timeLog = a.timeLog,
                                        todoId = a.todoId
                                    };
                                    tlst.Add(t);
                                }
                                else
                                {
                                    p = new Plist()
                                    {
                                        ProjectID = Convert.ToInt32(a.projectId == null ? 0 : a.projectId),
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
                        Common.Storage.IsActivityCall = true;
                        BindUseToDoListFromLocalDB(Common.Storage.CurrentProjectId);
                    }
                }
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
                if (Notes != null && Notes != "")
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
                        time = Common.Storage.ProjectStartTime,
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
                            InfoColor = "Green";
                            AddnoteStatus = "Note added successfully!";
                            Notes = "";
                            IsAddnoteStatus = true;
                            ValidateFormsAndError(5);
                        }
                        else
                        {

                            InfoColor = "Red";
                            AddnoteStatus = "Unable to add a Note!";
                            IsAddnoteStatus = true;
                            ValidateFormsAndError(5);
                        }
                    }
                }
                else
                {

                    InfoColor = "Red";
                    AddnoteStatus = "Please add a Note!";
                    IsAddnoteStatus = true;
                    ValidateFormsAndError(5);
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
                               // Common.Storage.IsScreenShotCapture = true;
                            }
                            else
                            {
                                Common.Storage.ScreenURl = "";
                                Common.Storage.IsScreenShotCapture = false;
                                LogFile.LogApiResponseSceenShot("imageName "+ ScreenshotResponse.response.data.imageName);
                            }
                        }
                        else
                        {
                            Common.Storage.ScreenURl = "";
                            Common.Storage.IsScreenShotCapture = false;
                            LogFile.LogApiResponseSceenShot(ScreenshotResponse.response.code +" "+ ScreenshotResponse.response.message);
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

        public async void SendAfterOfflineScreenShotsToServer(string screenshotFileName)
        {
            try
            {
                _baseURL = Configurations.UrlConstant + Configurations.SendScreenshotsApiConstant;
                ScreenshotResponse = new ScreenShotResponseModel();
                objHeaderModel = new HeaderModel();
                objHeaderModel.SessionID = Common.Storage.TokenId;
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(screenshotFileName));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "screenshot", Path.GetFileName(screenshotFileName));
                form.Add(fileContent, "type", "1");

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
                            if (File.Exists(screenshotFileName))
                            {
                                // If file found, delete it    
                                File.Delete(screenshotFileName);
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
                        SlotInterval = 5;// intrval.ToInt32();//2;
                        Common.Storage.timeIntervel = SlotInterval;
                        Common.Storage.ActivityIntervel = SlotInterval;
                    }
                    else
                    {
                        SlotInterval = 5;
                        Common.Storage.timeIntervel = SlotInterval;
                        Common.Storage.ActivityIntervel = SlotInterval;
                    }
                }
                else
                {
                    SlotInterval = 5;
                    Common.Storage.timeIntervel = SlotInterval;
                    Common.Storage.ActivityIntervel = SlotInterval;
                }
            }
            catch (Exception ex)
            {
                SlotInterval = 5;
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

        public Bitmap LoadEmbeddedResources1(string localFilePath)
        {
            try
            {
                string assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                var rawUri = localFilePath;
                var uri = new Uri($"avares://{assemblyName}{rawUri}");

                var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
                var asset = assets.Open(uri);

                return new Bitmap(asset);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
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