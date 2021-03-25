using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class Dashboard : Window
    {
        
        public DashboardViewModel _dashboardVM;
        
        public Dashboard()
        {
            InitializeComponent();
            _dashboardVM = new DashboardViewModel();
            this.DataContext = _dashboardVM;
            var asyncBox1 = this.FindControl<AutoCompleteBox>("projectlist");
            asyncBox1.AsyncPopulator = PopulateAsyncprojectlist;
            var asyncBox = this.FindControl<AutoCompleteBox>("SearchToDo");
            asyncBox.AsyncPopulator = PopulateAsync;

            //lstbox = this.FindControl<ListBox>("LayoutRoot");
            //lstbox.SelectionChanged += Lstbox_SelectionChanged;
            //var themes = this.Find<TextBox>("textOutput");
            // themes.TextInput += Themes_TextInput;

            // var dg1 = this.FindControl<DataGrid>("dataGrid1");
            //  dg1.IsReadOnly = true;
            //  var collectionView1 = new DataGridCollectionView(Countries.All);
            //dg1.Items = collectionView1;
#if DEBUG
            this.AttachDevTools();
#endif
        }
        private async Task<IEnumerable<object>> PopulateAsyncprojectlist(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                _dashboardVM.SerachProjectDataList(searchText, 272, 245);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
        private async Task<IEnumerable<object>> PopulateAsync(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                _dashboardVM.SerachToDoDataList(searchText, 272, 245);
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return null;
        }
        private void LstBoxToDo_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            Avalonia.Controls.ListBox lstboxTodo = sender as Avalonia.Controls.ListBox;
            int indexID = lstboxTodo.SelectedIndex;
            if (indexID == -1)
            {
                lstboxTodo.SelectedItem = _dashboardVM.SelectedprojectToDo;
                //lstboxTodo.LayoutUpdated();
            }
            if (indexID>=0)
            {
                if(e.AddedItems!=null && e.AddedItems.Count>0)
                {
                    var data = (tbl_ServerTodoDetails)e.AddedItems[0];
                    _dashboardVM.SelectedprojectToDo = data;
                }
            }
        }
        private void Lstbox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {

           
            Avalonia.Controls.ListBox lstbox = sender as Avalonia.Controls.ListBox;

            int selectedIndex = lstbox.SelectedIndex;
            if (selectedIndex == -1)
            {

                lstbox.SelectedItem = _dashboardVM.Selectedproject;
               // lstbox.LayoutUpdated();
            }
            if (selectedIndex >= 0)
            {
                _dashboardVM.SelectedIndex = selectedIndex;
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var data = (Organisation_Projects)e.AddedItems[0];
                    // _dashboardVM.ProjectStop(data.ProjectId);
                    _dashboardVM.Selectedproject = data;
                }
            }
            // if(!_dashboardVM.IsProjectRunning)
            //{
            //foreach (var item in e.AddedItems)
            //{
            //    Organisation_Projects projects = new Organisation_Projects();
            //    projects = item as Organisation_Projects;
            //    if (projects.checkTodoApiCallOrNot == false)
            //    {
            //        _dashboardVM.BindUserToDoListFromApi(Convert.ToInt32(projects.ProjectId), Convert.ToInt32(projects.OrganisationId), Convert.ToInt32(projects.UserId));
            //    }
            //}

            //}


        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
