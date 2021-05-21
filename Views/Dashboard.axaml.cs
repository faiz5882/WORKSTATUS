using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;
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
        Button btncancel;
        Button btnaddnote;
        ToggleButton tbtn;

        public Dashboard()
        {
            InitializeComponent();
            _dashboardVM = new DashboardViewModel(this);
            this.DataContext = _dashboardVM;
            var asyncBox1 = this.FindControl<AutoCompleteBox>("projectlist");
            asyncBox1.AsyncPopulator = PopulateAsyncprojectlist;
            var asyncBox = this.FindControl<AutoCompleteBox>("SearchToDo");
            asyncBox.AsyncPopulator = PopulateAsync;

            btncancel = this.FindControl<Button>("btnCancel");
            btncancel.Click += Btn_Click;
            btnaddnote = this.FindControl<Button>("btnAddNote");
            btnaddnote.Click += Btnaddnote_Click;
            tbtn = this.FindControl<ToggleButton>("addnotebtn");
            fullPath = ConfigurationManager.AppSettings["WindowsPath"].ToString();
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
            int snapshotTime = screenShotTimeinMinutes != null ? Convert.ToInt32(screenShotTimeinMinutes) : 2;
            m_screen.Interval = TimeSpan.FromMinutes(Convert.ToInt32(snapshotTime));
            m_screen.Tick += GetScreenShots;
            m_screen.Start();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void Btnaddnote_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.AddNotesAPICall();
            tbtn.IsChecked = false;
            _dashboardVM.Notes = "";
        }

        private void Btn_Click(object? sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            _dashboardVM.Notes = "";
        }

        private void GetScreenShots(object? sender, EventArgs e)
        {
            if (!_dashboardVM.IsPlaying && _dashboardVM.IsStop && !Common.Storage.IsScreenShotCapture)
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
                    string orgPath = Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";

                    //C:\Projects\2021\CompanyProject\WorkStatus\Screenshots\\TestNewV3\\20210517070641.jpg
                    string directoryPath = fullPath + orgPath;
                    string updatedPath = directoryPath.Replace("|", "_");
                    updatedPath = directoryPath.Replace(" ", "");
                    if (!Directory.Exists(updatedPath))
                    {
                        Directory.CreateDirectory(updatedPath);
                    }

                    string filename = updatedPath + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                    bmpScreenshot.Save(filename, jpegCodecInfo, codecParams);

                    var isInternetConnected = true;
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
                    byte[] ImageData = System.IO.File.ReadAllBytes(filename);
                    ScreenShotRequestModel model = new ScreenShotRequestModel() { screenshot = filename };
                    _dashboardVM.SendScreenShotsToServer(filename, ImageData);
                    // api call

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

        private async void SignOut_Click(object sender, RoutedEventArgs e)
        {
             _dashboardVM.ClosedAllTimer();
            await _dashboardVM.SendIntervalToServer();
            BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
            service2.Delete(new tbl_Temp_SyncTimer());
            BaseService<tbl_TempSyncTimerTodoDetails> service3 = new BaseService<tbl_TempSyncTimerTodoDetails>();
            service3.Delete(new tbl_TempSyncTimerTodoDetails());
            ChangeDashBoardWindow();
        }
        private async void Quit_Click(object sender, RoutedEventArgs e)
        {
             _dashboardVM.ClosedAllTimer();
            await _dashboardVM.SendIntervalToServer();
            this.Close();
        }

        private void ChangeDashBoardWindow()
        {
            try
            {
                if (Application.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var window = new Login();
                    var prevwindow = desktop.MainWindow;
                    desktop.MainWindow = window;
                    desktop.MainWindow.Show();
                    prevwindow.Close();
                    prevwindow = null;
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        private async Task<IEnumerable<object>> PopulateAsyncprojectlist(string searchText, CancellationToken cancellationToken)
        {
            try
            {
                _dashboardVM.SerachProjectDataList(searchText, _dashboardVM.Selectedproject.ProjectId.ToInt32(), _dashboardVM.Selectedproject.OrganisationId.ToInt32());
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
                // await Task.Delay(TimeSpan.FromSeconds(1.5), cancellationToken);
                
                _dashboardVM.SerachToDoDataList(searchText, _dashboardVM.Selectedproject.ProjectId.ToInt32(), _dashboardVM.Selectedproject.OrganisationId.ToInt32());
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
            if (lstboxTodo.SelectedItem != null)
            {
                if (e.AddedItems != null && e.AddedItems.Count > 0)
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

            //if (_dashboardVM.Selectedproject != null)
            //{
            //    _dashboardVM.listproject.SelectedItem = _dashboardVM.Selectedproject;
            //}
           

            Avalonia.Controls.ListBox lstbox = sender as Avalonia.Controls.ListBox;

            int selectedIndex = lstbox.SelectedIndex;
            int a = _dashboardVM.listproject.SelectedIndex;
            
            if (selectedIndex == -1)
            {

                if (_dashboardVM.projectIdSelected != null)
                {
                    int index = _dashboardVM.GetProjectsList.FindIndex(x => x.ProjectId == _dashboardVM.projectIdSelected);
                    lstbox.SelectedIndex = index;                   
                }
                else
                {
                    //lstbox.SelectedIndex = 0;
                }
                (sender as ListBox).ScrollIntoView(_dashboardVM.Selectedproject);
            }
            else
            {


                if (lstbox.SelectedItem != null)
                {
                    if (e.AddedItems != null && e.AddedItems.Count > 0)
                    {
                        
                        var data = (Organisation_Projects)e.AddedItems[0];                      
                         _dashboardVM.Selectedproject = data;
                        //if (_dashboardVM.projectIdSelected != null)
                        //{
                        //    _dashboardVM.BindUseToDoListFromLocalDB(_dashboardVM.Selectedproject.ProjectId.ToInt32());
                        //}
                        //else
                        //{
                        //     _dashboardVM.Selectedproject = data;
                        //}

                    }
                    else
                    {


                    }

                }

            }



        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
