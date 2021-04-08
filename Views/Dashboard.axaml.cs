using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
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
        string fullPath = "";
        public DispatcherTimer m_screen = new DispatcherTimer();
        public string screenShotTimeinMinutes;
        public static Stopwatch sw = new Stopwatch();

        public Dashboard()
        {
            InitializeComponent();
            _dashboardVM = new DashboardViewModel(this);
            this.DataContext = _dashboardVM;
            var asyncBox1 = this.FindControl<AutoCompleteBox>("projectlist");
            asyncBox1.AsyncPopulator = PopulateAsyncprojectlist;
            var asyncBox = this.FindControl<AutoCompleteBox>("SearchToDo");
            asyncBox.AsyncPopulator = PopulateAsync;
            Closed += Dashboard_Closed;
           // ListBox lstbox = this.FindControl<ListBox>("LayoutRoot");
            //lstbox.SelectionChanged += Lstbox_SelectionChanged;
            //var themes = this.Find<TextBox>("textOutput");
            // themes.TextInput += Themes_TextInput;

            // var dg1 = this.FindControl<DataGrid>("dataGrid1");
            //  dg1.IsReadOnly = true;
            //  var collectionView1 = new DataGridCollectionView(Countries.All);
            //dg1.Items = collectionView1;

           // lstbox.SelectedIndex = 0;
            int snapshotTime = screenShotTimeinMinutes != null ? Convert.ToInt32(screenShotTimeinMinutes) : 5;
            m_screen.Interval = TimeSpan.FromMinutes(Convert.ToInt32(snapshotTime));
            m_screen.Tick += GetScreenShots;
            m_screen.Start();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void GetScreenShots(object? sender, EventArgs e)
        {
            if (!_dashboardVM.IsPlaying && _dashboardVM.IsStop)
            {
                try
                {
                    Screen sc = Screens.Primary;
                    PixelRect _pr = sc.WorkingArea;
                    int width = _pr.Width;
                    int height = _pr.Height;
                    string serverScreenShotImageName = string.Empty;
                    Bitmap bmpScreenshot = new Bitmap(width, height + 40);
                    Graphics gfxScreenshot = Graphics.FromImage(bmpScreenshot);
                    gfxScreenshot.CopyFromScreen(0, 0, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);
                    var qualityEncoder = Encoder.Quality;
                    var quality = 25;
                    var ratio = new EncoderParameter(qualityEncoder, quality);
                    var codecParams = new EncoderParameters(1);
                    codecParams.Param[0] = ratio;
                    ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
                    var jpegCodecInfo = codecs.Single(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
                    string orgPath = "C:\\Projects\\2021\\CompanyProject\\WorkStatus\\Screenshots\\" + Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";
                    string directoryPath = fullPath + orgPath;
                    string updatedPath = directoryPath.Replace("|", "_");
                    if (!Directory.Exists(updatedPath))
                    {
                        Directory.CreateDirectory(updatedPath);
                    }
                    string filename = updatedPath + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                    bmpScreenshot.Save(filename, jpegCodecInfo, codecParams);
                    var isInternetConnected = true;//BaseDeclaration.IsConnectedToInternet();
                    if (isInternetConnected)
                    {
                        serverScreenShotImageName = filename;
                    }
                    else
                    {
                        serverScreenShotImageName = filename;
                    }
                    gfxScreenshot.Dispose();
                    bmpScreenshot.Dispose();
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    //await MyMessageBox.Show(this, APIExceptionMessage, MessageBoxTitle, MyMessageBox.MessageBoxButtons.Ok);
                }
                finally
                {
                    // GetRelease();
                }
            }
        }
        private void Dashboard_Closed(object? sender, EventArgs e)
        {
            _dashboardVM.ClosedAllTimer();
        }

        private async Task<IEnumerable<object>> PopulateAsyncprojectlist(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                _dashboardVM.SerachProjectDataList(searchText, Common.Storage.CurrentProjectId, Common.Storage.CurrentOrganisationId);
                _dashboardVM.AddZebraPatternToProjectList();
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
                _dashboardVM.SerachToDoDataList(searchText, Common.Storage.CurrentProjectId, Common.Storage.CurrentOrganisationId);
                _dashboardVM.AddZebraPatternToToDoList();
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
            if (lstboxTodo.SelectedItem!=null)
            {
                if(e.AddedItems!=null && e.AddedItems.Count>0)
                {
                    var data = (tbl_ServerTodoDetails)e.AddedItems[0];
                    _dashboardVM.SelectedprojectToDo = data;
                }
                else
                {
                    var data = (tbl_ServerTodoDetails)_dashboardVM.SelectedprojectToDo;
                    _dashboardVM.SelectedprojectToDo = data;                    
                }
            }           
        }
        //private void LstBoxOrganisation_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        //{
        //    Avalonia.Controls.ListBox lstboxTodo = sender as Avalonia.Controls.ListBox;
        //    int indexID = lstboxTodo.SelectedIndex;
        //    if (indexID == -1)
        //    {
        //        lstboxTodo.SelectedItem = _dashboardVM.SelectedOrganisationItems;              
        //    }
        //    if (indexID >= 0)
        //    {
        //        if (e.AddedItems != null && e.AddedItems.Count > 0)
        //        {
        //            var data = (tbl_OrganisationDetails)e.AddedItems[0];
        //            _dashboardVM.SelectedOrganisationItems = data;
        //        }
        //    }
        //}
        private void Lstbox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {

           
            Avalonia.Controls.ListBox lstbox = sender as Avalonia.Controls.ListBox;

            int selectedIndex = lstbox.SelectedIndex;
            if (selectedIndex == -1)
            {

                lstbox.SelectedItem = _dashboardVM.Selectedproject;                
               // lstbox.LayoutUpdated();
            }
            if (lstbox.SelectedItem !=null)
            {                               
                if (e.AddedItems != null && e.AddedItems.Count > 0)
                {
                    var data = (Organisation_Projects)e.AddedItems[0];
                    // _dashboardVM.ProjectStop(data.ProjectId);
                    _dashboardVM.Selectedproject = data;
                }
                else
                {
                    var data = (Organisation_Projects)_dashboardVM.Selectedproject;
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
