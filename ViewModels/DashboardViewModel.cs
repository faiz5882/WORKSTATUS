using Avalonia.Collections;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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

        #endregion

        private ObservableCollection<tbl_OrganisationDetails> _findOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
        public ObservableCollection<tbl_OrganisationDetails> FindOrganisationDetails { get => _findOrganisationDetails; set { _findOrganisationDetails = value; } }

        private ObservableCollection<tbl_Organisation_Projects> _findUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
        public ObservableCollection<tbl_Organisation_Projects> FindUserProjectList { get => _findUserProjectList; set { _findUserProjectList = value; } }

       // public ObservableCollection<tbl_OrganisationDetails> OrganisationSelectedItems { get; } = new ObservableCollection<tbl_OrganisationDetails>();

        private string _selectedOrganisationItems;
        public string SelectedOrganisationItems
        {
            get => _selectedOrganisationItems;
            set
            {
                _selectedOrganisationItems = value;
                RaisePropertyChanged("SelectedOrganisationItems");
            }
        }
        #region constructor  
        public DashboardViewModel()
        {
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = "";
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
          // BindUserOrganisationListFromApi();//1st call api and store in localDB            
            BindUserOrganisationListFromLocalDB();//2nd call get from localDB
           // BindUserProjectlistByOrganizationID("245");//3rd call api and sore in localDB
          //  BindUserProjectListFromLocalDB("245");
        }
        #endregion

        #region Methods

        #region OrganisationDetails
        async void BindUserOrganisationListFromApi()
        {
            _baseURL = Configurations.UrlConstant + Configurations.UserOrganisationListApiConstant;            
            organisationListResponse = new UserOrganisationListResponse();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = Common.Storage.TokenId;
            List<tbl_OrganisationDetails> _OrganisationDetails = new List<tbl_OrganisationDetails>();
            organisationListResponse = await _services.GetAsyncData_GetApi(new Get_API_Url().UserOrganizationlist(_baseURL), true, objHeaderModel, organisationListResponse);
            if (organisationListResponse.response.code == "200")
            {
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
        }
        async void BindUserProjectlistByOrganizationID(string OrganizationID)
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
    

    void BindUserOrganisationListFromLocalDB()
    {
        FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
        FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>(new DashboardSqliteService().GetOrganisation());
    }

        void BindUserProjectListFromLocalDB(string OrganisationId)
        {
            FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>();
            FindUserProjectList = new ObservableCollection<tbl_Organisation_Projects>(new DashboardSqliteService().GetProjectsByOrganisationId(OrganisationId));
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
