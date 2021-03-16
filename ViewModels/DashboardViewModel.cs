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
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.ViewModels
{
   public class DashboardViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private string _baseURL = string.Empty;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        #region All ReactiveCommand
        
        #endregion
        
        private ObservableCollection<tbl_OrganisationDetails> _findOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
        public ObservableCollection<tbl_OrganisationDetails> FindOrganisationDetails { get => _findOrganisationDetails; set { _findOrganisationDetails = value; } }
        #region constructor  
        public DashboardViewModel()
        {
            _services = new DashboardService();
            objHeaderModel = new HeaderModel();
            objHeaderModel.SessionID = "";
            
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>();
           BindOrganisationDetails();
            //var collectionView1 = new DataGridCollectionView(Countries.All);
            //dataGrid1 = collectionView1;


        }
        #endregion
       
        #region Methods

        #region OrganisationDetails
        async void BindOrganisationDetails()
        {
            _baseURL = Configurations.UrlConstant + Configurations.LoginApiConstant;
            FindOrganisationDetails = new ObservableCollection<tbl_OrganisationDetails>() {
            new tbl_OrganisationDetails{OrganizationId="Ws_P2",OrganizationName="02:21:47"},
            new tbl_OrganisationDetails{OrganizationId="Ws_P1",OrganizationName="00:00:00"},
            new tbl_OrganisationDetails{OrganizationId="Ws_P3",OrganizationName="03:20:50"},
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
