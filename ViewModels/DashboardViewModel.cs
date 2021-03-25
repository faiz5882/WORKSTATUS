using Avalonia.Collections;
using Avalonia.Threading;
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
        DispatcherTimer TimerObject;
        Task[] tasks;
        private string _baseURL = string.Empty;
        public object DashboardSqliteService;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        private UserOrganisationListResponse organisationListResponse;
        private UserProjectlistByOrganizationIDResponse userProjectlistResponse;
        private tbl_OrganisationDetails tbl_OrganisationDetails;
        private tbl_Organisation_Projects tbl_organisation_Projects;
        private ToDoListResponseModel toDoListResponseModel;
        private tbl_AddTodoDetails tbl_AddTodoDetails;
        private string projectIdSelected;
        private long ToDoSelectedID;
        System.Timers.Timer timerHeader;
        System.Timers.Timer timerproject;
        System.Timers.Timer timerToDo;
        ThemeManager themeManager;
        string currentTime = string.Empty;
        int h1, m1, s1, h2, m2, s2, h3, m3, s3;
        #endregion
        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandPlay { get; }
        public ReactiveCommand<Unit, Unit> CommandStop { get; }
        public ReactiveCommand<string, Unit> CommandProjectPlay { get; set; }
        public ReactiveCommand<string, Unit> CommandProjectStop { get; set; }
        public ReactiveCommand<long, Unit> CommandToDoPlay { get; set; }
        public ReactiveCommand<long, Unit> CommandToDoStop { get; set; }

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
            get => _getProjectsList;
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
                    BindUserProjectlistByOrganizationID(SelectedOrganisationItems.OrganizationId);
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
            get => _selectedIndex;
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
        #endregion
        #region constructor  
        public DashboardViewModel()
        {
            themeManager = new ThemeManager();
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            CommandPlay = ReactiveCommand.Create(PlayTimer);
            CommandStop = ReactiveCommand.Create(playStop);
            CommandProjectPlay = ReactiveCommand.Create<string>(ProjectPlay);
            CommandProjectStop = ReactiveCommand.Create<string>(ProjectStop);
            CommandToDoPlay = ReactiveCommand.Create<long>(ToDoPlay);
            CommandToDoStop = ReactiveCommand.Create<long>(ToDoStop);


            // string pathImage =System.Configuration.ConfigurationSettings.AppSettings["PlayIcon"];
            IsPlaying = true;
            IsStop = false;
            objHeaderModel.SessionID = "";
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
            BindUserOrganisationListFromApi();//1st call api and store in localDB            
            //BindUserOrganisationListFromLocalDB();//2nd call get from localDB
            // BindUserProjectlistByOrganizationID("245");//3rd call api and sore in localDB
            //BindUserProjectListFromLocalDB("245");


            timerHeader = new System.Timers.Timer();
            timerproject = new System.Timers.Timer();
            timerToDo = new System.Timers.Timer();
            timerHeader.Interval = 1000;
            timerproject.Interval = 1000;
            timerToDo.Interval = 1000;

            timerproject.Elapsed += Timerproject_Elapsed;
            timerHeader.Elapsed += TimerHeader_Elapsed;
            timerToDo.Elapsed += TimerToDo_Elapsed;

            /////
            ///
            //TimerObject = new DispatcherTimer();
            //TimerObject.Tick += new EventHandler(timer_Elapsed);
            //TimerObject.Interval = new TimeSpan(0, 0, 1);
        }

        

        private void timer_Elapsed(object sender, EventArgs e)
        {
            TimerObject.Stop();

            BackgroundWorker backgroundWorkerObject = new BackgroundWorker();
            backgroundWorkerObject.DoWork += new DoWorkEventHandler(StartThreads);
            backgroundWorkerObject.RunWorkerAsync();
            TimerObject.Start();
        }

        private void StartThreads(object sender, DoWorkEventArgs e)
        {
            tasks = new Task[1];
            tasks[0] = Task.Factory.StartNew(() => DoSomeLongWork());
            // Give the tasks a second to start.
            Thread.Sleep(1000);
        }
        private void DoSomeLongWork()
        {
            s2 += 1;
            if (s2 == 60)
            {
                s2 = 0;
                m2 += 1;
            }
            if (m2 == 60)
            {
                m2 = 0;
                h2 += 1;
            }
            ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
            UpdateProjectList(projectIdSelected);
        }
        private void Timerproject_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {

            s2 += 1;
            if (s2 == 60)
            {
                s2 = 0;
                m2 += 1;
            }
            if (m2 == 60)
            {
                m2 = 0;
                h2 += 1;
            }
            ProjectTime = String.Format("{0}:{1}:{2}", h2.ToString().PadLeft(2, '0'), m2.ToString().PadLeft(2, '0'), s2.ToString().PadLeft(2, '0'));
            UpdateProjectList(projectIdSelected);
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

        }
        #endregion

        #region Methods        
        #region HeaderPlayTimer       
        public void PlayTimer()
        {
            timerHeader.Start();
            IsStop = true;
            IsPlaying = false;
            if (!string.IsNullOrEmpty(projectIdSelected))
            {
                string _time = GetTimeFromProject(projectIdSelected);
                string[] arry = _time.Split(':');
                if (arry[2] == "00")
                {
                    s2 = 0;
                }
                else
                {
                    s2 = Convert.ToInt32(arry[2]);
                }
                timerproject.Start();
            }
            if (ToDoSelectedID > 0)
            {
                string _time1 = GetTimeFromToDoList(ToDoSelectedID);
                string[] arry1 = _time1.Split(':');
                if (arry1[2] == "00")
                {
                    s3 = 0;
                }
                else
                {
                    s3 = Convert.ToInt32(arry1[2]);
                }
                timerToDo.Start();
            }           
        }
        public void playStop()
        {
            timerHeader.Stop();
            timerproject.Stop();
            timerToDo.Stop();
            IsStop = false;
            IsPlaying = true;
            UpdateProjectList(projectIdSelected, "AllStop");
            TODoStopUpdate(ToDoSelectedID);
        }
        #endregion

        #region ProjectPlayTimer
        public void RefreshSelectedItem(string p, bool playStop)
        {
            var data = GetProjectsList.FirstOrDefault(x => x.ProjectId == p);
            if (data != null)
            {
                data.checkTodoApiCallOrNot = playStop;
                Selectedproject = data;
            }

        }
        public void ProjectPlay(string obj)
        {

            projectIdSelected = string.Empty;
            projectIdSelected = obj;
            RefreshSelectedItem(obj, true);
            string _time = GetTimeFromProject(obj);
            string[] arry = _time.Split(':');
            if (arry[2] == "00")
            {
                s2 = 0;
            }
            else
            {
                s2 = Convert.ToInt32(arry[2]);
            }

            timerHeader.Start();
            timerproject.Start();
            // TimerObject.Start();
            IsStop = true;
            IsPlaying = false;
        }
        public void ProjectStop(string obj)
        {
            projectIdSelected = obj;
            // RefreshSelectedItem(obj, false);
            timerHeader.Stop();
            timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            ProjectStopUpdate(projectIdSelected);
            // UpdateProjectList(projectIdSelected, "ProjectStop");
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
                ProjectListFinal.Remove(item);
                ProjectListFinal.Add(item);
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


            ObservableCollection<Organisation_Projects> ProjectListFinal = new ObservableCollection<Organisation_Projects>();
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
                        //if (commandType == "ProjectStop")
                        //{
                        //    item.ProjectPlayIcon = true;
                        //    item.ProjectStopIcon = false;
                        //    item.ProjectTime = ProjectTime;
                        //    item.checkTodoApiCallOrNot = false;
                        //}
                        //else
                        //{
                        //    item.ProjectPlayIcon = false;
                        //    item.ProjectStopIcon = true;
                        //    item.ProjectTime = ProjectTime;
                        //     item.checkTodoApiCallOrNot = true;

                        //}
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


                ProjectListFinal.Remove(item);
                ProjectListFinal.Add(item);

            }

            if (Selectedproject != null)
            {
                // MEMBERSHIP_BENEFIT _selectedBenefit = (MEMBERSHIP_BENEFIT)SelectedBenefitItem;
                GetProjectsList = null;
                GetProjectsList2[GetProjectsList2.FindIndex(i => i.Equals(Selectedproject))] = Selectedproject;
                GetProjectsList = new ObservableCollection<Organisation_Projects>(GetProjectsList2);

            }
            // timerproject.Start();
            // GetProjectsList.Clear();
            /// GetProjectsList = new ObservableCollection<Organisation_Projects>();
            // GetProjectsList = ProjectListFinal;
            //RaisePropertyChanged("GetProjectsList");
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


        #endregion

        #region ToDoPlayTimer
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
            ToDoSelectedID = 0;
            ToDoSelectedID =TodoID;
           // RefreshSelectedItem(obj, true);
            string _time = GetTimeFromToDoList(TodoID);
            string[] arry = _time.Split(':');
            if (arry[2] == "00")
            {
                s3 = 0;
            }
            else
            {
                s3 = Convert.ToInt32(arry[2]);
            }
            timerToDo.Start();
            timerHeader.Start();
            //timerproject.Start();           
            IsStop = true;
            IsPlaying = false;
            
        }
        public void ToDoStop(long TodoID)
        {
            ToDoSelectedID = TodoID;
            timerToDo.Stop();
             timerHeader.Stop();
            //timerproject.Stop();
            IsStop = false;
            IsPlaying = true;
            TODoStopUpdate(ToDoSelectedID);
        }
        public string GetTimeFromToDoList(long todoId)
        {
            string _time = "";
            var t = ToDoListData.FirstOrDefault(x => x.Sno == todoId);
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
            }
            return _time;
        }
        public void UpdateToDoList(long CurrentToDoId)
        {           
            foreach (var item in ToDoListData)
            {
                if(item.Sno==CurrentToDoId)
                {
                    item.ToDoPlayIcon = false;
                    item.ToDoStopIcon = true;
                    item.ToDoTimeConsumed = TODOProjectTime;
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
           
            if (SelectedprojectToDo != null)
            {                
                ToDoListData = null;
                GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
                ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);
            }
        }
        public void TODoStopUpdate(long CurrentToDoId)
        {
            foreach (var item in ToDoListData)
            {
                if (item.Sno == CurrentToDoId)
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

            if (SelectedprojectToDo != null)
            {
                // MEMBERSHIP_BENEFIT _selectedBenefit = (MEMBERSHIP_BENEFIT)SelectedBenefitItem;
                ToDoListData = null;
                GetToDoListTemp[GetToDoListTemp.FindIndex(i => i.Equals(SelectedprojectToDo))] = SelectedprojectToDo;
                ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoListTemp);
            }
        }
        #endregion
        #region OrganisationDetails
        public async void BindUserToDoListFromApi(int projectId, int organizationId, int userId)
        {
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
                        ToDoName = item.name,
                        CurrentUserId = Convert.ToString(item.user_id),
                        CurrentProjectId = Convert.ToString(item.project_id),
                        CurrentOrganisationId = Convert.ToString(item.organization_id),
                        StartDate = Convert.ToString(item.startDate),
                        EndDate = Convert.ToString(item.endDate),
                        EstimatedHours = Convert.ToString(item.estiamtedHours),
                        Description = Convert.ToString(item.description),
                        IsCompleted = item.complete,
                        Privacy = item.privacy,
                        Site = item.site,
                        IsOffline = false,
                        ToDoTimeConsumed = ""
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
                    tbl_ServerTodo = new tbl_ServerTodoDetails();
                    tbl_ServerTodo = item;
                    ToDoListData.Add(tbl_ServerTodo);
                    GetToDoListTemp.Add(tbl_ServerTodo);
                }
               // GetToDoListTemp = new ObservableCollection<tbl_ServerTodoDetails>(ToDoListData);
                RaisePropertyChanged("ToDoListData");
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
                            OrganizationName = item.name
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
                                ProjectName = item.name,
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
                if (GetProjectsList.Count > 0)
                {
                    dynamic firstvalueOfProject = GetProjectsList.FirstOrDefault();
                    if (firstvalueOfProject != null)
                    {
                        Selectedproject = firstvalueOfProject;
                        //int p1 = Convert.ToInt32(Selectedproject.ProjectId);
                        //int o = Convert.ToInt32(Selectedproject.OrganisationId);
                        //int u = Convert.ToInt32(Selectedproject.UserId);
                        //BindUserToDoListFromApi(p1, o, u);
                        //RaisePropertyChanged("Selectedproject");
                    }
                }
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        public void SerachToDoDataList(string searchtext, int projectid = 272, int organization_id = 245)
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
                    projects = new Organisation_Projects() {
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
        #endregion
        #endregion
        #region MVVM INotifyPropertyChanged     
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
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
