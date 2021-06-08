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
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Tulpep.NotificationWindow;
using WorkStatus.Models;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;
using WorkStatus.ViewModels;

namespace WorkStatus.Views
{
    public class Dashboard : Window
    {

        public DashboardViewModel _dashboardVM;
        public DispatcherTimer IsInternet = new DispatcherTimer();
        string fullPath = "";
        public DispatcherTimer m_screen = new DispatcherTimer();
        public string screenShotTimeinMinutes;
        public static Stopwatch sw = new Stopwatch();
        Button btncancel;
        Button btnaddnote;
        ToggleButton tbtn;
        Button btnUpgardeApp;
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
            btnUpgardeApp = this.FindControl<Button>("upgardeapp");
            btnUpgardeApp.Click += BtnUpgardeApp_Click;
            Closed += Dashboard_Closed;

            IsInternet.Interval = TimeSpan.FromMinutes(Convert.ToInt32(1));
            IsInternet.Tick += IsInternet_Tick;
            IsInternet.Start();
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
        private void BtnUpgardeApp_Click(object? sender, RoutedEventArgs e)
        {
            var url = Common.Storage.AppDownload_Link;
            NavigateToBrowser(url);
            _dashboardVM.IsStayOpen = true;
        }
        private void IsInternet_Tick(object? sender, EventArgs e)
        {
            _dashboardVM.IsUserOffline = Common.CommonServices.IsConnectedToInternet() ? false : true;
        }
        private void Btnaddnote_Click(object? sender, RoutedEventArgs e)
        {
            try
            {


                _dashboardVM.AddNotesAPICall();
                tbtn.IsChecked = false;
                _dashboardVM.Notes = "";
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        private void Btn_Click(object? sender, RoutedEventArgs e)
        {
            tbtn.IsChecked = false;
            _dashboardVM.Notes = "";
        }
        public Bitmap LoadEmbeddedResources(string localFilePath)
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
        public void GetNotification(string popupMessage)
        {
            try
            {
                PopupNotifier popup = new PopupNotifier();
                popup.TitleText = "          WorkStatus";
                popup.TitleColor = System.Drawing.Color.White;
                popup.TitleFont = new System.Drawing.Font("Tahoma", 14F);
                popup.BodyColor = Color.FromArgb(33, 26, 35);
                popup.ContentColor = Color.White;
                popup.ContentText = popupMessage;
                popup.Image = LoadEmbeddedResources("/Assets/LogoSmall.ico");
                popup.ImageSize = new System.Drawing.Size(42, 42);
                popup.ContentFont = new System.Drawing.Font("Tahoma", 11F);
                popup.Size = new System.Drawing.Size(350, 75);
                popup.ShowGrip = false;
                popup.HeaderHeight = 2;
                popup.AnimationDuration = 2000;
                popup.AnimationInterval = 1;
                popup.HeaderColor = Color.FromArgb(33, 26, 35);
                popup.ShowCloseButton = false;
                popup.Popup();
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
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
                    GetNotification("     Screenshot taken");
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    LogFile.ErrorLog(ex);
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
            try
            {
                _dashboardVM.ClosedAllTimer();
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        #region Menu Item Click
        private async void OpenDashboard_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppUrlConstant + "dashboard/analytics";
            NavigateToBrowser(url);
        }
        private async void AddEditTime_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppUrlConstant + "dashboard/timesheets/view-edit-timesheets";
            NavigateToBrowser(url);
        }
        private async void ReportError_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppUrlConstant + "contact";
            NavigateToBrowser(url);
        }
        private async void Help_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppUrlConstant + "faq";
            NavigateToBrowser(url);
        }
        private async void AboutUs_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppUrlConstant + "about";
            NavigateToBrowser(url);
        }

        private async void SignOut_Click(object sender, RoutedEventArgs e)
        {
            try
            {


                _dashboardVM.ClosedAllTimer();
                await _dashboardVM.SendIntervalToServer();
                BaseService<tbl_Temp_SyncTimer> service2 = new BaseService<tbl_Temp_SyncTimer>();
                service2.Delete(new tbl_Temp_SyncTimer());
                BaseService<tbl_TempSyncTimerTodoDetails> service3 = new BaseService<tbl_TempSyncTimerTodoDetails>();
                service3.Delete(new tbl_TempSyncTimerTodoDetails());
                ChangeDashBoardWindow();
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }
        private async void Quit_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                _dashboardVM.ClosedAllTimer();
                await _dashboardVM.SendIntervalToServer();
                this.Close();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }

        private void NavigateToBrowser(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? url : "open",
                CreateNoWindow = true,
                UseShellExecute = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            });
        }

        #endregion

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
                LogFile.ErrorLog(ex);
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
                LogFile.ErrorLog(ex);
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
                LogFile.ErrorLog(ex);
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
