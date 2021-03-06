using Avalonia.Controls;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WorkStatus.Models.ReadDTO;
using WorkStatus.Models.WriteDTO;

namespace WorkStatus.Common
{
  public class Storage
    {
        public static string AppDownload_Link { get; set; }
        public static string ConnectionString { get; set; }
        public static bool IsLogin { get; set; }
        public static string TokenId { get; set; }
        public static int timeIntervel { get; set; }
        public static bool checkTodoApiCallOrNot { get; set; }
        public static string ProjectStartTime { get; set; }
        
        public static bool IsProjectRuning { get; set; }
        public static bool IsToDoRuning { get; set; }
      
        public static string SlotTimerStartTime { get; set; }
        public static string SlotTimerPreviousEndTime { get; set; }
        public static string SlotTimerPreviousStartTime { get; set; }

        public static bool SlotRunning { get; set; }

        public static long  SlotID { get; set; }
        public static long tblTimerSno { get; set; }
        public static long tblProjectSno { get; set; }
        public static long temp_SyncTimerSno { get; set; }
        public static string AppEnvironment { get; set; }
        public static string OpreatingSystem { get; set; }

        public static DateTime? AppTodayStartTM { get; set; }

        public static string CurrentOrganisationName { get; set; }
        public static int CurrentOrganisationId { get; set; }
        public static int CurrentUserId { get; set; }
        public static string CurrentProjectName { get; set; }
        public static int CurrentProjectId { get; set; }
        public static bool IsActivityCall { get; set; }
        public static string GetAppVersion()
        {
            string version = null;
            try
            {
                string v = Assembly.GetEntryAssembly().GetName().Version.ToString();
                string[] s = v.Split(".");
                version = "Version " + s[0] + "." + s[1] + " (909)";
            }
            catch (Exception ex)
            {
                version = "not installed";
            }
            return version;
        }
        public static string ServerOrg_Id { get; set; }
        public static string ServerSd_Token { get; set; }
        public static string ScreenURl { get; set; }
        public static string LoginId { get; set; }
        public static string LoginUserID { get; set; }
        public static int ActivityIntervel { get; set; }
        public static int KeyBoradEventCount { get; set; }
        public static int MouseEventCount { get; set; }
        public static int AverageEventCount { get; set; }
        public static string LastProjectEventCountTime { get; set; }
        public static string LastToDoEventCountTime { get; set; }
        public static int ContinueProjectEventCountTime { get; set; }
        public static string ContinueToDoEventCountTime { get; set; }
        public static string IdleProjectTime { get; set; }
        public static string IdleToDoTime { get; set; }
        public static bool IsScreenShotCapture { get; set; }
        public static int Entry { get; set; }
        public static string StopTimeForDB { get; set; }
        public static string LastStopTimeForDB { get; set; }
        public static int EditToDoId { get; set; }
        public static string AddOrEditToDoProjectName { get; set; }
        public static int AddOrEditToDoProjectId { get; set; }

        public static ToDoDetailsData EdittodoData;
        public static double CurrentWindowHeight { get; set; }
        public static double CurrentWindowWidth { get; set; }

        public static ObservableCollection<AddOrEditToDoAttachments> LocalTODODeleteAttachments = new ObservableCollection<AddOrEditToDoAttachments>();
    }
}
