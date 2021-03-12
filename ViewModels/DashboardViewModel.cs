using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.APIServices;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.ViewModels
{
   public class DashboardViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private string _baseURL = string.Empty;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        private ObservableCollection<tbl_OrganisationDetails> _findOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
        public ObservableCollection<tbl_OrganisationDetails> FindOrganisationDetails { get => _findOrganisationDetails; set { _findOrganisationDetails = value; } }
        #region constructor  
        public DashboardViewModel()
        {
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = "";
           
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>() {
            new tbl_OrganisationDetails{OrganizationId="test1",OrganizationName="test 1"},
            new tbl_OrganisationDetails{OrganizationId="test1",OrganizationName="test 1"},
            new tbl_OrganisationDetails{OrganizationId="Google",OrganizationName="Google"},
            };

        }
        #endregion

        #region Methods
        #region OrganisationDetails
       async void BindOrganisationDetails()
        {
            _baseURL = Configurations.UrlConstant + Configurations.LoginApiConstant;
            tbl_OrganisationDetails tbl_Organisation = new tbl_OrganisationDetails() {
            OrganizationId="",
            OrganizationName="Item 1",
            IsOffline=true,
            Sno=1
            };

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
