using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Common
{
  public class Storage
    {
        public static string ConnectionString { get; set; }
        public static bool IsLogin { get; set; }
        public static string TokenId { get; set; }
        public static int timeIntervel { get; set; }
        public static bool checkTodoApiCallOrNot { get; set; }
        public static string ProjectStartTime { get; set; }
        public static string TempHeaderTime { get; set; }

        
        public static bool IsProjectRuning { get; set; }
        public static bool IsToDoRuning { get; set; }


        public static string SlotTimerStartTime { get; set; }
        public static long  SlotID { get; set; }
        public static long tblTimerSno { get; set; }
        public static long tblProjectSno { get; set; }
        public static long temp_SyncTimerSno { get; set; }




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
                version = "Version " + s[0] + "." + s[1] + " (895)";
            }
            catch (Exception ex)
            {
                version = "not installed";
            }
            return version;
        }
        
    }
}
