using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using AvaloniaProgressRing;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive;
using System.Threading.Tasks;
using WorkStatus.APIServices;
using WorkStatus.Configuration;
using WorkStatus.Interfaces;
using WorkStatus.Models;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;
using WorkStatus.Utility;
using WorkStatus.Views;

namespace WorkStatus.ViewModels
{
    public class AddOrEditToDoViewModel : ReactiveObject, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region global properties
        Window _window;
        string[] files = null;
        List<string> Imagelist = new List<string>();
        int fileCount = 0;
        List<HttpContent> lstformDataContent = new List<HttpContent>();
        private AddOrEditToDoAttachments attachments;
        private string _baseURL = string.Empty;
        private AddorEditToDoResponseModel addoreditTodoResponse;
        private HeaderModel objHeaderModel;
        private readonly IDashboard _services;
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        int counter = 0;
        TextBlock tt;
        public ProgressRing Pgrforsavetodo;
        private tbl_ServerTodoDetails tbl_ServerAddTodoDetails;
        private tbl_ToDoAttachments tbl_todoattachment;
        #endregion

        #region Local properties
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                RaisePropertyChanged("Title");
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

        private string _nameAddOrEditTodo;
        public string NameAddOrEditTodo
        {
            get => _nameAddOrEditTodo;
            set
            {
                _nameAddOrEditTodo = value;
                RaisePropertyChanged("NameAddOrEditTodo");
            }
        }
        private string _descriptionAddOrEditTodo;
        public string DescriptionAddOrEditTodo
        {
            get => _descriptionAddOrEditTodo;
            set
            {
                _descriptionAddOrEditTodo = value;
                RaisePropertyChanged("DescriptionAddOrEditTodo");
            }
        }
        private string _startDateAddOrEditTodo;
        public string StartDateAddOrEditTodo
        {
            get => _startDateAddOrEditTodo;
            set
            {
                _startDateAddOrEditTodo = value;
                RaisePropertyChanged("StartDateAddOrEditTodo");
            }
        }
        private string _endDateAddOrEditTodo;
        public string EndDateAddOrEditTodo
        {
            get => _endDateAddOrEditTodo;
            set
            {
                _endDateAddOrEditTodo = value;
                RaisePropertyChanged("EndDateAddOrEditTodo");
            }
        }

        private bool _isPublicCheck;
        public bool IsPublicCheck
        {
            get => _isPublicCheck;
            set
            {
                _isPublicCheck = value;
                RaisePropertyChanged("IsPublicCheck");
            }
        }
        private bool _isPrivateCheck;
        public bool IsPrivateCheck
        {
            get => _isPrivateCheck;
            set
            {
                _isPrivateCheck = value;
                RaisePropertyChanged("IsPrivateCheck");
            }
        }
        private bool _isOnSiteCheck;
        public bool IsOnSiteCheck
        {
            get => _isOnSiteCheck;
            set
            {
                _isOnSiteCheck = value;
                RaisePropertyChanged("IsOnSiteCheck");
            }
        }
        private bool _isOffSiteCheck;
        public bool IsOffSiteCheck
        {
            get => _isOffSiteCheck;
            set
            {
                _isOffSiteCheck = value;
                RaisePropertyChanged("IsOffSiteCheck");
            }
        }
        private string _getImageName;
        public string GetImageName
        {
            get => _getImageName;
            set
            {
                _getImageName = value;
                RaisePropertyChanged("GetImageName");
            }
        }
        private string _textStatusAddOrEditToDo;
        public string TextStatusAddOrEditToDo
        {
            get => _textStatusAddOrEditToDo;
            set
            {
                _textStatusAddOrEditToDo = value;
                RaisePropertyChanged("TextStatusAddOrEditToDo");
            }
        }
        private string _infoColor;
        public string InfoColor
        {
            get => _infoColor;
            set
            {
                _infoColor = value;
                RaisePropertyChanged("InfoColor");
            }
        }
        private bool _isStatus;
        public bool IsStatus
        {
            get => _isStatus;
            set
            {
                _isStatus = value;
                RaisePropertyChanged("IsStatus");
            }
        }
        private bool _isButtonClick;
        public bool IsButtonClick
        {
            get => _isButtonClick;
            set
            {
                _isButtonClick = value;
                RaisePropertyChanged("IsButtonClick");
            }
        }
        #endregion

        #region Observable Collection properties
        //attachment
        private ObservableCollection<AddOrEditToDoAttachments> _toDoAttachmentListData;
        public ObservableCollection<AddOrEditToDoAttachments> ToDoAttachmentListData
        {
            get => _toDoAttachmentListData;
            set
            {
                _toDoAttachmentListData = value;
                RaisePropertyChanged("ToDoAttachmentListData");
            }
        }
        private ObservableCollection<AddOrEditToDoAttachments> _listdata;
        public ObservableCollection<AddOrEditToDoAttachments> ListData
        {
            get => _listdata;
            set
            {
                _listdata = value;
                RaisePropertyChanged("ListData");
            }
        }
        //
        #endregion

        #region Commands
        public ReactiveCommand<Unit, Unit> CommandUploadAttachments { get; set; }
        public ReactiveCommand<Unit, Unit> CommandSaveToDo { get; set; }
        public ReactiveCommand<Unit, Unit> CommandCancelToDo { get; set; }
        public ReactiveCommand<string, Unit> CommandDeleteAttachment { get; set; }

        #endregion

        #region Constructor
        public AddOrEditToDoViewModel(Window window)
        {
            _window = window;
            _services = new DashboardService();
            CheckAddOrEditToDo();
            HeaderProjectName = Common.Storage.AddOrEditToDoProjectName;
            CommandUploadAttachments = ReactiveCommand.Create(UploadAttachments);
            CommandSaveToDo = ReactiveCommand.Create(SaveToDo);
            CommandCancelToDo = ReactiveCommand.Create(CancelToDo);
            CommandDeleteAttachment = ReactiveCommand.Create<string>(DeleteAttachment);
            Pgrforsavetodo = _window.FindControl<ProgressRing>("addoredittodopgr");
            Pgrforsavetodo.IsVisible = false;
            StartDateAddOrEditTodo = "Start Date";
            EndDateAddOrEditTodo = "End Date";
            tt = _window.FindControl<TextBlock>("errorStatus"); 
            IsButtonClick = true;
        }
        #endregion

        #region Methods
        public bool CheckAddOrEditToDo()
        {            
            if (Common.Storage.EditToDoId != 0)
            {
                if (Common.Storage.EdittodoData != null)
                {
                    Common.Storage.LocalTODODeleteAttachments.Clear();
                    Title = "Edit- " + Common.Storage.EdittodoData.name;
                    NameAddOrEditTodo = Common.Storage.EdittodoData.name;
                    DescriptionAddOrEditTodo = Common.Storage.EdittodoData.description != null ? Common.Storage.EdittodoData.description : "";
                    IsPublicCheck = Common.Storage.EdittodoData.privacy == "Public" ? true : false;
                    IsPrivateCheck = Common.Storage.EdittodoData.privacy == "Private" ? true : false;
                    IsOnSiteCheck = Common.Storage.EdittodoData.site == "OnSite" ? true : false;
                    IsOffSiteCheck = Common.Storage.EdittodoData.site == "OffSite" ? true : false;
                    if (Common.Storage.EdittodoData.attachment != null)
                    {
                        int b = Common.Storage.EdittodoData.attachment.Count;
                        files = new string[b];
                        for (int i = 0; i < b; i++)
                        {
                            files[i] = Common.Storage.EdittodoData.attachment[i].url;
                        }
                        foreach (string file in files)
                        {
                            var webClient = new WebClient();
                            byte[] imgData = webClient.DownloadData(file);
                            Stream stream = new MemoryStream(imgData);
                            HttpContent fileStreamContent = new StreamContent(stream);
                            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "attachments[" + fileCount + "]",
                                FileName = file
                            };
                            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                            lstformDataContent.Add(fileStreamContent);

                            fileCount++;
                        }
                        ToDoAttachmentListData = new ObservableCollection<AddOrEditToDoAttachments>();
                        ToDoAttachmentListData.Clear();
                        ListData = new ObservableCollection<AddOrEditToDoAttachments>();
                        ListData.Clear();
                        foreach (var a in Common.Storage.EdittodoData.attachment)
                        {
                            attachments = new AddOrEditToDoAttachments()
                            {
                                ImageIcon = "/Assets/attachment.png",
                                CloseIcon = "/Assets/close.png",
                                ImageName = a.name,
                                Url = a.url
                                
                            };
                            
                            ToDoAttachmentListData.Add(attachments);
                            ListData.Add(attachments);
                            Imagelist.Add(a.name);
                            
                        }


                     
                    }
                }
                return true;
            }
            else
            {
                Title = "AddTodo";
                return false;
            }
        }
        public async void UploadAttachments()
        {
            try
            {
                ListData = new ObservableCollection<AddOrEditToDoAttachments>();
                ListData.Clear();
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.Title = "Save file";
                //OFD.Filters = GetFilters();
                OFD.AllowMultiple = true;
                var result = await OFD.ShowAsync(AddOrEditToDo.AddorEditToDoInstance);
                if (result != null)
                {
                    files = result;
                    try
                    {
                        foreach (string file in files)
                        {
                            string filename = Path.GetFullPath(file);
                            if (!filename.Contains(".jpeg") && !filename.Contains(".png") && !filename.Contains(".pdf") && !filename.Contains(".jpg") && !filename.Contains(".PNG") && !filename.Contains(".PDF") && !filename.Contains(".JPEG") && !filename.Contains(".JPG"))
                            {
                                InfoColor = "Red";
                                ValidateFormsAndError("Please select only image and pdf file.", 5);
                                return;
                            }
                            string[] image = filename.Split("\\");
                            foreach (var i in image)
                            {
                                if (i.Contains(".jpeg") || i.Contains(".png") || i.Contains(".pdf") || i.Contains(".jpg") || i.Contains(".PNG") || i.Contains(".PDF") || i.Contains(".JPEG") || i.Contains(".JPG"))
                                {
                                    GetImageName = i;
                                }
                            }
                            Imagelist.Add(GetImageName);
                            AddOrEditRequestModel addRequest = new AddOrEditRequestModel();
                            string localFilePath = Path.GetFullPath(file);
                            byte[] imgData = System.IO.File.ReadAllBytes(filename);
                            Stream stream = new MemoryStream(imgData);
                            HttpContent fileStreamContent = new StreamContent(stream);
                            fileStreamContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
                            {
                                Name = "attachments[" + fileCount + "]",
                                FileName = filename
                            };
                            fileStreamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data");
                            lstformDataContent.Add(fileStreamContent);

                            fileCount++;
                        }

                        fileCount = 0;
                        ToDoAttachmentListData = new ObservableCollection<AddOrEditToDoAttachments>();
                        ToDoAttachmentListData.Clear();
                        foreach (var a in Imagelist)
                        {
                            attachments = new AddOrEditToDoAttachments()
                            {
                                ImageIcon = "/Assets/attachment.png",
                                CloseIcon = "/Assets/close.png",
                                ImageName = a
                            };
                            ToDoAttachmentListData.Add(attachments);
                            ListData.Add(attachments);
                        }
                    }
                    catch (Exception ex)
                    {
                        string msg = ex.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
        }
        private List<FileDialogFilter> GetFilters()
        {
            return new List<FileDialogFilter>
                                {
                                  new FileDialogFilter
                                 {
                                    Name = "Jpeg files ",
                                    Extensions = new List<string> {"jpeg"}
                                 },
                                   new FileDialogFilter
                                 {
                                    Name = "Png files",
                                    Extensions = new List<string> {"png"}
                                   },
                                    new FileDialogFilter
                                 {
                                    Name = "Pdf files",
                                    Extensions = new List<string> {"pdf"}
                                   }
                                };
        }
        public void DeleteAttachment(string imageName)
        {

            foreach (var a in ListData)
            {
                int listDataindex = ListData.FindIndex(x => x.ImageName == imageName);
                AddOrEditToDoAttachments DeletedToDoAttachment = ToDoAttachmentListData.Where(x => x.ImageName == imageName).FirstOrDefault();

                if (DeletedToDoAttachment != null)
                {
                    Common.Storage.LocalTODODeleteAttachments.Add(DeletedToDoAttachment);
                }

                int ToDoAttachmentindex = ToDoAttachmentListData.FindIndex(x => x.ImageName == imageName);
                if (listDataindex == ToDoAttachmentindex)
                {
                    ToDoAttachmentListData.RemoveAt(ToDoAttachmentindex);
                    Imagelist.RemoveAt(ToDoAttachmentindex);
                }
            }
        }
        public async void SaveToDo()
        {
            try
            {
                IsButtonClick = false;
                if (string.IsNullOrEmpty(NameAddOrEditTodo))
                {
                    ValidateFormsAndError("Todo name is required!", 5);
                    InfoColor = "Red";
                    IsButtonClick = true;
                    return;
                }
                if ((StartDateAddOrEditTodo != "Start Date") && (EndDateAddOrEditTodo != "End Date"))
                {
                    if (Convert.ToDateTime(EndDateAddOrEditTodo) < Convert.ToDateTime(StartDateAddOrEditTodo))
                    {
                        ValidateFormsAndError("The End Date must be greater than Start Date!", 5);
                        InfoColor = "Red";
                        IsButtonClick = true;
                        return;
                    }
                }
                Pgrforsavetodo.IsVisible = true;
                _baseURL = Common.Storage.EditToDoId == 0 ? Configurations.UrlConstant + Configurations.AddTodoApiConstant :
                           Configurations.UrlConstant + Configurations.EditTodoApiConstant + "/" + Common.Storage.EditToDoId;
                addoreditTodoResponse = new AddorEditToDoResponseModel();
                objHeaderModel = new HeaderModel();
                AddOrEditRequestModel entity = new AddOrEditRequestModel()
                {
                    organization_id = Common.Storage.CurrentOrganisationId.ToStrVal(),
                    project_id = Common.Storage.AddOrEditToDoProjectId.ToStrVal(),
                    name = NameAddOrEditTodo,
                    description = DescriptionAddOrEditTodo != null ? DescriptionAddOrEditTodo : "",
                    startDate = StartDateAddOrEditTodo == "Start Date" ? "" : StartDateAddOrEditTodo,
                    endDate = EndDateAddOrEditTodo == "End Date" ? "" : EndDateAddOrEditTodo,
                    privacy = IsPublicCheck == true ? "Public" : "Private",
                    site = IsOnSiteCheck == true ? "OnSite" : "OffSite",
                    attachments = lstformDataContent,
                    memberIds = new List<string>() { Common.Storage.LoginUserID }
                    //toggle_status = 1,
                    //repeat = 1,
                    //once_every = 7,
                    //end_after = 7,
                    //recurrence_status = 1
                };

                if (Common.Storage.LocalTODODeleteAttachments != null && Common.Storage.LocalTODODeleteAttachments.Count > 0)
                {
                    foreach (AddOrEditToDoAttachments item in Common.Storage.LocalTODODeleteAttachments.ToList())
                    {
               int ToDoAttachmentindex = entity.attachments.FindIndex(x => x.Headers.ContentDisposition.FileName.Contains(item.ImageName));
                        if (ToDoAttachmentindex > 0)
                        {
                            entity.attachments.RemoveAt(ToDoAttachmentindex);
                        }
                    }
                }
                objHeaderModel.SessionID = Common.Storage.TokenId;
                addoreditTodoResponse = await _services.AddorEditToDoApiCall(_baseURL, objHeaderModel, entity);
                if (addoreditTodoResponse.response != null)
                {
                    if (addoreditTodoResponse.response.code == "1001")
                    {
                        RefreshTokenRequest refreshTokenRequest = new RefreshTokenRequest() { token = Common.Storage.TokenId };
                        bool renewtoken = _services.RenewTokenAPI(true, objHeaderModel, refreshTokenRequest).Result;
                        if (renewtoken)
                        {
                            objHeaderModel.SessionID = Common.Storage.TokenId;
                            addoreditTodoResponse = await _services.AddorEditToDoApiCall(_baseURL, objHeaderModel, entity);
                        }
                    }
                    if (addoreditTodoResponse.response.code == "200")
                    {
                        InfoColor = "Green";
                        string successMsg = addoreditTodoResponse.response.message;
                        ValidateFormsAndError(successMsg, 5);
                        Pgrforsavetodo.IsVisible = false;
                        if (addoreditTodoResponse.response.message.Contains("successfully"))
                        {
                            Title = Common.Storage.EditToDoId == 0 ? "AddTodo" : "EditTodo";
                            NameAddOrEditTodo = string.Empty;
                            DescriptionAddOrEditTodo = string.Empty;
                            StartDateAddOrEditTodo = "";
                            EndDateAddOrEditTodo = "";
                            IsPublicCheck = false;
                            IsPrivateCheck = false;
                            IsOnSiteCheck = false;
                            IsOffSiteCheck = false;
                            ToDoAttachmentListData = new ObservableCollection<AddOrEditToDoAttachments>();
                            ToDoAttachmentListData.Clear();
                            ListData = new ObservableCollection<AddOrEditToDoAttachments>();
                            ListData.Clear();
                            Imagelist = new List<string>();
                            Imagelist.Clear();
                            await Task.Delay(800);
                            _window.Close();
                        }
                    }
                    else
                    {

                        InfoColor = "Red";
                        ValidateFormsAndError(addoreditTodoResponse.response.message, 5);
                    }
                }
                else
                {

                    InfoColor = "Red";
                    ValidateFormsAndError("No response", 5);
                }
                IsButtonClick = true;
            }
            catch (Exception ex)
            {

                var msg = ex.Message;
            }
        }
        public void CancelToDo()
        {
            NameAddOrEditTodo = string.Empty;
            DescriptionAddOrEditTodo = string.Empty;
            StartDateAddOrEditTodo = "";
            EndDateAddOrEditTodo = "";
            IsPublicCheck = false;
            IsPrivateCheck = false;
            IsOnSiteCheck = false;
            IsOffSiteCheck = false;
            ToDoAttachmentListData = new ObservableCollection<AddOrEditToDoAttachments>();
            ToDoAttachmentListData.Clear();
            ListData = new ObservableCollection<AddOrEditToDoAttachments>();
            ListData.Clear();
            Imagelist = new List<string>();
            Imagelist.Clear();
            _window.Close();
        }
        public void ValidateFormsAndError(string strMessage, int timeOut)
        {
            DateTime oCurrentDate2 = DateTime.Now;
            int s = oCurrentDate2.Second;
            int result = s + timeOut;
            if (result >= 60)
            {
                s = 0;
                result = s + timeOut;
            }
            counter = result;
            TextStatusAddOrEditToDo = strMessage;
            tt.IsVisible = true;
            dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
            dispatcherTimer.Start();
        }
        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            DateTime oCurrentDate = DateTime.Now;
            int s = oCurrentDate.Second;

            if (counter == s)
            {

                tt.IsVisible = false;
                dispatcherTimer.Stop();
            }
        }
        //public void UpdateToDolistDatainLocalDB()
        //{
        //    tbl_ServerAddTodoDetails = new tbl_ServerTodoDetails()
        //    {
        //        ToDoName = NameAddOrEditTodo,
        //        CurrentUserId = Common.Storage.LoginUserID,
        //        CurrentProjectId = Common.Storage.AddOrEditToDoProjectId.ToStrVal(),
        //        CurrentOrganisationId = Common.Storage.CurrentOrganisationId.ToStrVal(),
        //        StartDate = StartDateAddOrEditTodo,
        //        EndDate = EndDateAddOrEditTodo,
        //        EstimatedHours = "",
        //        Description = DescriptionAddOrEditTodo,
        //        IsCompleted = ,
        //        Privacy = item.privacy,
        //        Site = item.site,
        //        IsOffline = false,
        //        ToDoTimeConsumed = "00:00:00",// assign default time.
        //        Id = item.id
        //    };
        //    // ToDo attachments
        //    foreach (var a in item.todoattachments)
        //    {
        //        tbl_todoattachment = new tbl_ToDoAttachments()
        //        {
        //            Id = a.id,
        //            ProjectId = item.project_id,
        //            OrgId = item.organization_id,
        //            ToDoId = a.todo_id,
        //            Image = a.image,
        //            ImageURL = a.image_url,
        //            AttachmentImage = "/Assets/attachment.png"
        //        };
        //        new DashboardSqliteService().InsertUserToDoAttachmentList(tbl_todoattachment);
        //    }
        //    //
        //    // _OrganisationDetails.Add(tbl_OrganisationDetails);
        //    new DashboardSqliteService().InsertUserToDoList(tbl_ServerAddTodoDetails);
        //}
        #endregion

        #region MVVM INotifyPropertyChanged     
        public event PropertyChangedEventHandler? PropertyChanged;
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            try
            {
                if (!string.IsNullOrEmpty(propertyName))
                {
                    if (this.PropertyChanged != null)
                    {
                        this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
                    }
                }
            }
            catch
            {

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
