using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
//using Notify;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using Image = Avalonia.Controls.Image;
using Button = Avalonia.Controls.Button;
using ComboBox = Avalonia.Controls.ComboBox;
using Application = Avalonia.Application;
using ListBox = Avalonia.Controls.ListBox;

namespace WorkStatus.Views
{

    public class Dashboard : Window
    {
        public DashboardViewModel _dashboardVM;
        public static Stopwatch ProjectStopWatch = new Stopwatch();
        public DispatcherTimer IsInternet = new DispatcherTimer();
        string fullPath = "";
        public DispatcherTimer m_screen = new DispatcherTimer();
        public string screenShotTimeinMinutes;
        public static Stopwatch sw = new Stopwatch();
        public static bool IsChecked = true;
        public static bool IsMiniChecked = true;
        Image imgexpanderButton;
        Button btncancel;
        Button btnaddnote;
        ToggleButton tbtn;
        Button btnUpgardeApp;
        ComboBox ComboToDoFilter;
        Button MinimizeAppbtn;
        Button QuitAppbtn;
        Button CancelAppbtn;
        Button OkConfirmbtn;
        ComboBox ProjectListCombobox;
        ComboBox ToDoListCombobox;
        bool isWindows = false;
        bool isLinux = false;
        Button OkAddTaskbtn;
        public Dashboard()
        {

            this.HandleWindowStateChanged(WindowState);
            this.MaxHeight = 630;
            this.MaxWidth = 1000;
            this.Position = new PixelPoint(3, 0);
            this.SizeToContent = SizeToContent.Manual;
            InitializeComponent();

            _dashboardVM = new DashboardViewModel(this);
            this.DataContext = _dashboardVM;
            var asyncBox1 = this.FindControl<AutoCompleteBox>("projectlist");
            asyncBox1.AsyncPopulator = PopulateAsyncprojectlist;
            var asyncBox = this.FindControl<AutoCompleteBox>("SearchToDo");
            asyncBox.AsyncPopulator = PopulateAsync;
            ComboToDoFilter = this.FindControl<ComboBox>("toDoFilter");
            ComboToDoFilter.SelectionChanged += ComboToDoFilter_SelectionChanged;
            ProjectListCombobox = this.FindControl<ComboBox>("projectdropdown");
            ProjectListCombobox.SelectionChanged += ProjectListCombobox_SelectionChanged;
            ToDoListCombobox = this.FindControl<ComboBox>("tododropdown");
            ToDoListCombobox.SelectionChanged += ToDoListCombobox_SelectionChanged;
            btncancel = this.FindControl<Button>("btnCancel");
            btncancel.Click += Btn_Click;
            btnaddnote = this.FindControl<Button>("btnAddNote");
            btnaddnote.Click += Btnaddnote_Click;
            tbtn = this.FindControl<ToggleButton>("addnotebtn");
            isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
            isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
            if (isWindows)
            {
                fullPath = ConfigurationManager.AppSettings["WindowsPath"].ToString();
                Common.Storage.OpreatingSystem = "4";
                
                //OS = "WIN";
            }

            if (isLinux)
            {
                fullPath = ConfigurationManager.AppSettings["LinuxOSPath"].ToString();
                _dashboardVM.BashNoWait("python keyboardlogger.py");
                _dashboardVM.BashNoWait("python mouselogger.py");
                Common.Storage.OpreatingSystem = "5";
                //OS = "LINUX";
            }

            btnUpgardeApp = this.FindControl<Button>("upgardeapp");
            btnUpgardeApp.Click += BtnUpgardeApp_Click;
            Closing += Dashboard_Closing;
            MinimizeAppbtn = this.FindControl<Button>("minimizeapp");
            MinimizeAppbtn.Click += MinimizeAppbtn_Click;
            QuitAppbtn = this.FindControl<Button>("quitapp");
            QuitAppbtn.Click += QuitAppbtn_Click;
            CancelAppbtn = this.FindControl<Button>("cancelapp");
            CancelAppbtn.Click += CancelAppbtn_Click;
            CancelAppbtn = this.FindControl<Button>("cancelapp");
            CancelAppbtn.Click += CancelAppbtn_Click;
            OkConfirmbtn = this.FindControl<Button>("okConfirmApp");
            OkConfirmbtn.Click += OkConfirmbtn_Click; ;

            OkAddTaskbtn = this.FindControl<Button>("okAddTaskRunning");
            OkAddTaskbtn.Click += OkAddTaskbtn_Click;

            //IdleTimeCheckbox = this.FindControl<CheckBox>("IdleTimeCheckbox");
            //IdleTimeCheckbox.Checked += IdleTimeCheckbox_Checked;
            //IdleTimeCheckbox.Unchecked += IdleTimeCheckbox_Unchecked;

            //ReassignBtn = this.FindControl<Button>("ReassignBtn");
            //ReassignBtn.Click += ReassignBtn_Click;
            //StopBtn = this.FindControl<Button>("StopBtn");
            //StopBtn.Click += StopBtn_Click;
            //ContinueBtn = this.FindControl<Button>("ContinueBtn");
            //ContinueBtn.Click += ContinueBtn_Click;

            //RemeberMeBox = this.FindControl<CheckBox>("remembermecheckbox");
            //RemeberMeBox.Checked += RemeberMeBox_Checked;
            //RemeberMeBox.Unchecked += RemeberMeBox_Unchecked;

            //this.Width = 400;
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

        private void OkConfirmbtn_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.IsSleepMode = false;
            _dashboardVM.IsSleepModeQuitAlert = false;
        }
        private void OkAddTaskbtn_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.IsAddTaskMode = false;
            _dashboardVM.IsAddTaskModeQuitAlert = false;
        }
        private void ProjectListCombobox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                _dashboardVM.IdleSelectedProject = (tbl_Organisation_Projects)e.AddedItems[0];
                _dashboardVM.BindUseToDoListforComboBox(_dashboardVM.IdleSelectedProject.ProjectId);
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }
        private void ToDoListCombobox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                _dashboardVM.IdleSelectedProjectToDo = (tbl_ServerTodoDetails)e.AddedItems[0];
                if (_dashboardVM.IdleSelectedProjectToDo == null)
                    _dashboardVM.IdleSelectedProjectToDo.Id = 0;
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
        }

        #region Quit App PopUp Clicks
        private void RemeberMeBox_Unchecked(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.RemeberMe = false;
        }

        private void RemeberMeBox_Checked(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.RemeberMe = true;
        }

        private void CancelAppbtn_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.IsCancelAppBtn = _dashboardVM.RemeberMe ? true : false;
            _dashboardVM.IsDashBoardClose = false;
            _dashboardVM.IsDashBoardQuitAlert = false;
        }
        private async void QuitAppbtn_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.IsQuitAppBtn = _dashboardVM.RemeberMe ? true : false;
            _dashboardVM.IsDashBoardClose = true;
            // Close();

            /////////////===========new

            _dashboardVM.IsDashBoardClose = false;
            _dashboardVM.IsDashBoardQuitAlert = false;
            _dashboardVM.ClosedAllTimer();
            await _dashboardVM.SendIntervalToServer();
            if (_dashboardVM.IsMiniMizeAppBtn)
            {
                _dashboardVM.IsMiniMizeAppBtn = false;
            }
            if (_dashboardVM.IsQuitAppBtn)
            {
                _dashboardVM.IsQuitAppBtn = false;
            }
            if (_dashboardVM.IsCancelAppBtn)
            {
                _dashboardVM.IsCancelAppBtn = false;
            }

            Environment.Exit(0);

        }
        private void MinimizeAppbtn_Click(object? sender, RoutedEventArgs e)
        {
            _dashboardVM.IsMiniMizeAppBtn = _dashboardVM.RemeberMe ? true : false;
            _dashboardVM.IsDashBoardClose = false;
            _dashboardVM.IsDashBoardQuitAlert = false;
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Minimized;
            }
            else
            {
                WindowState = WindowState.Normal;
                WindowState = WindowState.Minimized;
            }

        }
        private async void Dashboard_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {

                if (!e.Cancel)//&& !_dashboardVM.IsDashBoardClose
                {
                    if (_dashboardVM.IsSignOut)
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                        _dashboardVM.IsDashBoardClose = true;
                        _dashboardVM.IsDashBoardQuitAlert = true;
                    }

                }
                else
                {

                    _dashboardVM.IsDashBoardClose = false;
                    _dashboardVM.IsDashBoardQuitAlert = false;
                    _dashboardVM.ClosedAllTimer();
                    await _dashboardVM.SendIntervalToServer();
                    if (_dashboardVM.IsMiniMizeAppBtn)
                    {
                        _dashboardVM.IsMiniMizeAppBtn = false;
                    }
                    if (_dashboardVM.IsQuitAppBtn)
                    {
                        _dashboardVM.IsQuitAppBtn = false;
                    }
                    if (_dashboardVM.IsCancelAppBtn)
                    {
                        _dashboardVM.IsCancelAppBtn = false;
                    }
                    e.Cancel = false;
                }
            }
            catch (Exception ex)
            {

                LogFile.ErrorLog(ex);
            }
        }

        #endregion

        private void ComboToDoFilter_SelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            try
            {
                var value = ((Avalonia.Controls.ComboBoxItem)(e.AddedItems)[0]).Content;

                if (value.ToString() == "Pending")
                {
                    List<tbl_ServerTodoDetails> GetToDoList = _dashboardVM.GetToDoListTemp;
                    GetToDoList = GetToDoList.FindAll(i => i.IsCompleted == 0);
                    _dashboardVM.ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoList);

                }
                else if (value.ToString() == "Completed")
                {
                    List<tbl_ServerTodoDetails> GetToDoList = _dashboardVM.GetToDoListTemp;
                    GetToDoList = GetToDoList.FindAll(i => i.IsCompleted == 1);
                    _dashboardVM.ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoList);

                }
                else if (value.ToString() == "All Todo's")
                {
                    List<tbl_ServerTodoDetails> GetToDoList = _dashboardVM.GetToDoListTemp;
                    _dashboardVM.ToDoListData = new ObservableCollection<tbl_ServerTodoDetails>(GetToDoList);

                }
                _dashboardVM.AddZebraPatternToToDoList();
            }
            catch (Exception ex)
            {
                LogFile.ErrorLog(ex);
            }
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
            if (!_dashboardVM.IsUserOffline)
            {
                string folderPath = ConfigurationManager.AppSettings["TempWindowsPath"].ToString();
                var files = Directory.GetDirectories(folderPath).ToList();
                if (files != null && files.Count > 0)
                {
                    foreach (var item in files)
                    {
                        var subFiles = Directory.GetFiles(item).ToList();

                        if (subFiles != null && subFiles.Count > 0)
                        {
                            foreach (var subItem in subFiles)
                            {
                                if (subItem != null)
                                {
                                    _dashboardVM.SendAfterOfflineScreenShotsToServer(subItem);
                                }
                                //var subFiles1 = Directory.GetFiles(subItem);
                            }

                        }
                    }

                }
            }

        }
        private void Btnaddnote_Click(object? sender, RoutedEventArgs e)
        {
            try
            {
                _dashboardVM.IsAddnoteStatus = false;
                _dashboardVM.AddnoteStatus = "";
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

        public void GetNotification_Linux(string popupMessage)
        {
            try
            {
                if (isLinux)
                {
                    // Notification linuxPopup = new Notification("WORKSTATUS", popupMessage, 5000, "DotsIcon.png");
                    //  linuxPopup.Show();
                    // Console.WriteLine("WorkStatus Screen Captured");
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        public void GetNotification(string popupMessage)
        {
            try
            {
                if (isWindows)
                {
                    PopupNotifier popup = new PopupNotifier();
                    popup.TitleText = "WORKSTATUS";
                    popup.TitleColor = Color.White;
                    popup.TitleFont = new Font("Tahoma", 13F);
                    popup.BodyColor = Color.FromArgb(33, 26, 35);
                    popup.ContentColor = Color.White;
                    popup.ContentText = popupMessage;
                    popup.Image = LoadEmbeddedResources("/Assets/DotsIcon.png");
                    popup.ImageSize = new System.Drawing.Size(25, 10);
                    popup.ContentFont = new Font("Tahoma", 13F);
                    popup.Size = new System.Drawing.Size(350, 75);
                    popup.ShowGrip = false;
                    popup.HeaderHeight = 2;
                    popup.AnimationDuration = 5000;
                    popup.AnimationInterval = 1;
                    popup.HeaderColor = Color.FromArgb(33, 26, 35);
                    popup.ShowCloseButton = false;
                    System.Media.SystemSounds.Asterisk.Play();
                    popup.Popup();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }

        private void GetScreenShots(object? sender, EventArgs e)
        {
            if (_dashboardVM.IsSlotTimer)
            {
                if (Common.CommonServices.IsConnectedToInternet())
                {
                    if (!_dashboardVM.IsPlaying && _dashboardVM.IsStop && !Common.Storage.IsScreenShotCapture)
                    {
                        try
                        {
                            Avalonia.Platform.Screen sc = Screens.Primary;
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
                            string orgPath = "";// Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";
                            string directoryPath = string.Empty;
                            string updatedPath = string.Empty;
                            string filename = string.Empty;
                            if (isWindows)
                            {

                                orgPath = Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";
                                directoryPath = fullPath + orgPath;
                                updatedPath = directoryPath.Replace("|", "_");
                                updatedPath = directoryPath.Replace(" ", "");

                                if (!Directory.Exists(updatedPath))
                                {
                                    Directory.CreateDirectory(updatedPath);
                                }

                                filename = updatedPath + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                            }
                            if (isLinux)
                            {
                                orgPath = Common.Storage.CurrentOrganisationName + "/" + _dashboardVM.HeaderProjectName;
                                fullPath = ConfigurationManager.AppSettings["LinuxOSPath"].ToString();
                                directoryPath = fullPath + "/" + orgPath;
                                updatedPath = directoryPath.Replace("|", "_");
                                updatedPath = directoryPath.Replace(" ", "");
                                if (!Directory.Exists(updatedPath))
                                {
                                    Directory.CreateDirectory(updatedPath);
                                }

                                filename = updatedPath + "/" + DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                                //OS = "LINUX";	
                            }

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
                            if (Common.Storage.ScreenURl != null && Common.Storage.ScreenURl != "")
                            {
                                if (File.Exists(Path.Combine(directoryPath, filename)))
                                {
                                    // If file found, delete it    
                                    File.Delete(Path.Combine(directoryPath, filename));
                                }
                            }
                            // api call
                            if (isWindows)
                            {
                                GetNotification("WorkStatus\nScreen Captured");
                            }
                            if (isLinux)
                            {
                                GetNotification_Linux("WorkStatus Screen Captured");
                            }
                            //D:\Projects\Workstatus\WorkStatus24062021\WorkStatus\Screenshots\
                        }
                        catch (Exception ex)
                        {
                            var msg = ex.Message;
                            LogFile.ErrorLog(ex);
                        }
                        finally
                        {
                        }
                    }
                }
                else
                {
                    GetOfflineScreenShots();
                }

                _dashboardVM.IsSlotTimer = false;
            }
        }


        private void GetOfflineScreenShots()
        {
            if (!_dashboardVM.IsPlaying && _dashboardVM.IsStop && !Common.Storage.IsScreenShotCapture)
            {
                try
                {
                    Avalonia.Platform.Screen sc = Screens.Primary;
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
                    string orgPath = "";// Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";
                    string directoryPath = string.Empty;
                    string updatedPath = string.Empty;
                    string filename = string.Empty;
                    if (isWindows)
                    {
                        string fullPath = ConfigurationManager.AppSettings["TempWindowsPath"].ToString();
                        orgPath = Common.Storage.CurrentOrganisationName + @"\" + _dashboardVM.HeaderProjectName + @"\";
                        directoryPath = fullPath + orgPath;
                        updatedPath = directoryPath.Replace("|", "_");
                        updatedPath = directoryPath.Replace(" ", "");

                        if (!Directory.Exists(updatedPath))
                        {
                            Directory.CreateDirectory(updatedPath);
                        }

                        string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                        filename = updatedPath + "/" + imageName;
                        //if (imageName != null)
                        //{
                        //    Common.Storage.ScreenURl = imageName;
                        //    //Common.Storage.IsScreenShotCapture = true;
                        //}
                    }
                    if (isLinux)
                    {
                        orgPath = Common.Storage.CurrentOrganisationName + "/" + _dashboardVM.HeaderProjectName;
                        fullPath = ConfigurationManager.AppSettings["LinuxOSPath"].ToString();
                        directoryPath = fullPath + "/" + orgPath;
                        updatedPath = directoryPath.Replace("|", "_");
                        updatedPath = directoryPath.Replace(" ", "");
                        if (!Directory.Exists(updatedPath))
                        {
                            Directory.CreateDirectory(updatedPath);
                        }
                        string imageName = DateTime.Now.ToString("yyyyMMddhhmmss") + ".jpg";
                        filename = updatedPath + "/" + imageName;
                        //OS = "LINUX";	
                    }

                    bmpScreenshot.Save(filename, jpegCodecInfo, codecParams);

                    serverScreenShotImageName = filename;

                    gfxScreenshot.Dispose();
                    bmpScreenshot.Dispose();
                    byte[] ImageData = System.IO.File.ReadAllBytes(filename);
                    ScreenShotRequestModel model = new ScreenShotRequestModel() { screenshot = filename };
                    //_dashboardVM.SendAfterOfflineScreenShotsToServer(filename);
                    //if (Common.Storage.ScreenURl != null && Common.Storage.ScreenURl != "")
                    //{
                    //    if (File.Exists(Path.Combine(directoryPath, filename)))
                    //    {
                    //        // If file found, delete it    
                    //        File.Delete(Path.Combine(directoryPath, filename));
                    //    }
                    //}
                    // api call
                    if (isWindows)
                    {
                        GetNotification("WorkStatus\nScreen Captured");
                    }
                    if (isLinux)
                    {
                        GetNotification_Linux("WorkStatus Screen Captured");
                    }
                    //D:\Projects\Workstatus\WorkStatus24062021\WorkStatus\Screenshots\
                }
                catch (Exception ex)
                {
                    var msg = ex.Message;
                    LogFile.ErrorLog(ex);
                }
                finally
                {
                }
            }
        }
        private async void Dashboard_Closed(object? sender, EventArgs e)
        {
            try
            {
                _dashboardVM.ClosedAllTimer();
                await _dashboardVM.SendIntervalToServer();
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
            var url = Configuration.Configurations.BaseAppMenuUrlConstant + "contact";
            NavigateToBrowser(url);
        }
        private async void Help_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppMenuUrlConstant + "faq";
            NavigateToBrowser(url);
        }
        private async void AboutUs_Click(object sender, RoutedEventArgs e)
        {
            var url = Configuration.Configurations.BaseAppMenuUrlConstant + "about";
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
                BaseService<tbl_UserDetails> service4 = new BaseService<tbl_UserDetails>();
                service4.Delete(new tbl_UserDetails());
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
                    _dashboardVM.IsSignOut = true;
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

        private void LstBoxToDo_SelectionChanged1(object? sender, SelectionChangedEventArgs e)
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
            if (lstbox != null)
            {
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

        }

        #region MiniMize/Maximize App
        protected override void HandleWindowStateChanged(WindowState state)
        {
            if (state == WindowState.Maximized)
            {
                if (this.FindControl<Button>("btntoggle").IsVisible == false)
                {
                    this.Width = 400;
                    this.Height = 250;
                    this.FindControl<Button>("btntoggle").Background = Avalonia.Media.Brushes.Transparent;
                }
                else
                {
                    if (this.FindControl<Button>("btntoggle").Background == Avalonia.Media.Brushes.DodgerBlue || this.FindControl<Button>("btntoggle").Background == Avalonia.Media.Brushes.DarkBlue)
                    {
                        this.Width = 365;
                        this.Height = 630;
                    }
                    else
                    {
                        this.Width = 1000;
                        this.Height = 630;
                    }

                }
            }
            if (state == WindowState.Normal)
            {
                this.SizeToContent = SizeToContent.Manual;
                this.Width = 358;
                this.Height = 630;
            }
        }
        private async void MinimizeItem_click(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Avalonia.Controls.Primitives.ToggleButton)sender).IsChecked = false;
                if (IsMiniChecked == true)
                {
                    IsMiniChecked = false;

                    Avalonia.Media.Imaging.Bitmap AvIrBitmap;
                    var MinImage = LoadEmbeddedResources("/Assets/expandIcon.png");
                    Bitmap PlayPauseToDoButton;
                    PlayPauseToDoButton = MinImage;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        PlayPauseToDoButton.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
                        AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);
                    }

                    imgexpanderButton = (Image)((Avalonia.Controls.ContentControl)sender).Content;
                    imgexpanderButton.Source = AvIrBitmap;
                    this.Height = 250;
                    this.Width = 355;

                    this.FindControl<Grid>("TopLeftGrid").IsVisible = true;
                    this.FindControl<Grid>("MidLeftGrid").IsVisible = false; ;
                    this.FindControl<Grid>("BottomLeftGrid").IsVisible = true;
                    this.FindControl<Grid>("grdCenter").IsVisible = false;
                    this.FindControl<Grid>("grdRight").IsVisible = false;
                    this.FindControl<Button>("btntoggle").IsVisible = false;
                    this.FindControl<Button>("btntoggle1").IsVisible = true;
                    this.FindControl<Grid>("gridLeft").RowDefinitions[0].Height = new GridLength(220, GridUnitType.Pixel);
                    this.FindControl<Grid>("gridLeft").RowDefinitions[1].Height = new GridLength(0, GridUnitType.Pixel);
                    this.FindControl<Grid>("gridLeft").RowDefinitions[2].Height = new GridLength(30, GridUnitType.Pixel);
                }
                else
                {
                    this.Height = 630;
                    this.Width = 358;
                    IsMiniChecked = true;
                    Avalonia.Media.Imaging.Bitmap AvIrBitmap;
                    var maxImage = LoadEmbeddedResources("/Assets/expendCloseIcon.png");
                    Bitmap PlayPauseToDoButton;
                    PlayPauseToDoButton = maxImage;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        PlayPauseToDoButton.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
                        AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);
                    }

                    imgexpanderButton = (Image)((Avalonia.Controls.ContentControl)sender).Content;
                    imgexpanderButton.Source = AvIrBitmap;
                    this.FindControl<Grid>("TopLeftGrid").IsVisible = true;
                    this.FindControl<Grid>("MidLeftGrid").IsVisible = true;
                    this.FindControl<Grid>("BottomLeftGrid").IsVisible = true;
                    this.FindControl<Grid>("grdCenter").IsVisible = true;
                    this.FindControl<Grid>("grdRight").IsVisible = true;
                    this.FindControl<ToggleButton>("btntoggle").IsVisible = true;
                    this.FindControl<ToggleButton>("btntoggle1").IsVisible = true;
                    this.FindControl<Grid>("gridLeft").RowDefinitions[0].Height = new GridLength(200, GridUnitType.Pixel);
                    this.FindControl<Grid>("gridLeft").RowDefinitions[1].Height = new GridLength(400, GridUnitType.Pixel);
                    this.FindControl<Grid>("gridLeft").RowDefinitions[2].Height = new GridLength(30, GridUnitType.Pixel);

                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }


        }
        private async void ExpanderItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.FindControl<Button>("btntoggle").Background = Avalonia.Media.Brushes.Transparent;
                ((Avalonia.Controls.Primitives.ToggleButton)sender).IsChecked = false;
                // if (((Avalonia.Controls.Primitives.ToggleButton)sender).IsChecked == true)
                if (IsChecked == true)
                {
                    IsChecked = false;
                    Avalonia.Media.Imaging.Bitmap AvIrBitmap;
                    var LeftArrowImage = LoadEmbeddedResources("/Assets/arrowhead-left.png");
                    Bitmap PlayPauseToDoButton;
                    PlayPauseToDoButton = LeftArrowImage;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        PlayPauseToDoButton.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
                        AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);
                    }

                    imgexpanderButton = (Image)((Avalonia.Controls.ContentControl)sender).Content;
                    imgexpanderButton.Source = AvIrBitmap;
                    // this.FindControl<Button>("btntoggle").Background = Avalonia.Media.Brushes.Pink;
                    this.Height = 630;
                    this.Width = 1000;
                }
                else
                {

                    Avalonia.Media.Imaging.Bitmap AvIrBitmap;
                    IsChecked = true;
                    var RightArrowImage = LoadEmbeddedResources("/Assets/arrowhead-right.png");
                    Bitmap PlayPauseToDoButton;
                    PlayPauseToDoButton = RightArrowImage;
                    using (MemoryStream memory = new MemoryStream())
                    {
                        PlayPauseToDoButton.Save(memory, ImageFormat.Png);
                        memory.Position = 0;

                        //AvIrBitmap is our new Avalonia compatible image. You can pass this to your view
                        AvIrBitmap = new Avalonia.Media.Imaging.Bitmap(memory);
                    }

                    imgexpanderButton = (Image)((Avalonia.Controls.ContentControl)sender).Content;
                    imgexpanderButton.Source = AvIrBitmap;
                    // this.FindControl<Button>("btntoggle").Background = Avalonia.Media.Brushes.Darklue;
                    this.Height = 630;
                    this.Width = 358;

                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                //await MyMessageBox.Show(this, APIExceptionMessage, MessageBoxTitle, MyMessageBox.MessageBoxButtons.Ok);
            }

        }

        #endregion
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
