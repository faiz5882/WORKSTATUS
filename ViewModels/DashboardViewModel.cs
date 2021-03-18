using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Text;
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
    public class DashboardViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private string _baseURL = string.Empty;
        public object DashboardSqliteService;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        private UserOrganisationListResponse organisationListResponse;
        private UserProjectlistByOrganizationIDResponse userProjectlistResponse;
        private tbl_OrganisationDetails tbl_OrganisationDetails;
        private tbl_Organisation_Projects tbl_organisation_Projects;

        #region All ReactiveCommand
        public ReactiveCommand<Unit, Unit> CommandPlay { get; }

        #endregion

        private ObservableCollection<tbl_OrganisationDetails> _findOrganisationDetails;
        public ObservableCollection<tbl_OrganisationDetails> FindOrganisationDetails 
        {
            get => _findOrganisationDetails; 
            set 
            { _findOrganisationDetails = value;
                RaisePropertyChanged("FindOrganisationDetails");
            }
        }

        private ObservableCollection<tbl_Organisation_Projects> _findUserProjectList;// = new ObservableCollection<tbl_Organisation_Projects>();
        public ObservableCollection<tbl_Organisation_Projects> FindUserProjectList
        {
            get => _findUserProjectList;
            set
            {
                _findUserProjectList = value;
                RaisePropertyChanged("FindUserProjectList");
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
                    BindUserProjectListFromLocalDB(SelectedOrganisationItems.OrganizationId);
                }
                // RaisePropertyChanged("SelectedOrganisationItems");
            }
        }

        private Uri _playIcon;// = new ObservableCollection<tbl_Organisation_Projects>();
        public Uri PlayIcon
        {
            get => _playIcon;
            set
            {
                _playIcon = value;
                RaisePropertyChanged("PlayIcon");
            }
        }
        #region constructor  
        public DashboardViewModel()
        {
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            CommandPlay = ReactiveCommand.Create(PlayTimer);
            string pathImage =System.Configuration.ConfigurationSettings.AppSettings["PlayIcon"];
            PlayIcon = new Uri(@"\"+ pathImage + "", UriKind.RelativeOrAbsolute);
            objHeaderModel.SessionID = "";
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
           // BindUserOrganisationListFromApi();//1st call api and store in localDB            
           // BindUserOrganisationListFromLocalDB();//2nd call get from localDB
                                                  // BindUserProjectlistByOrganizationID("245");//3rd call api and sore in localDB
                                                  //BindUserProjectListFromLocalDB("245");
        }
        #endregion

        #region Methods

        #region PlayTimer        
        public void PlayTimer()
        {            
            string pathImage = System.Configuration.ConfigurationSettings.AppSettings["StopIcon"];
            PlayIcon = new Uri(@"\" + pathImage + "", UriKind.RelativeOrAbsolute);
        }
        #endregion
        #region OrganisationDetails
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
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }


       async void BindUserOrganisationListFromLocalDB()
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
                FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
                ObservableCollection<tbl_Organisation_Projects> FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>();
                FindUserProjectListFinal = new ObservableCollection<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
                foreach (var item in FindUserProjectListFinal)
                {
                    item.ProjectId = Convert.ToString("00:00:00");
                    FindUserProjectList.Add(item);
                }
                RaisePropertyChanged("FindUserProjectList");
            }
            catch (Exception ex)
            {

                throw new Exception(ex.Message);
            }
        }
        #endregion
        #endregion
        #region MVVM INotifyPropertyChanged     
        public event PropertyChangedEventHandler? PropertyChanged;
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

        #endregion
    }
}
