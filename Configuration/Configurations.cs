using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Configuration
{
    public static class Configurations
    {

        #region ForceUpgardeApp
        public static string ForceUpgardeAppApiConstant
        {
            get
            {
                return "check_version/1234tret1234";
            }
        }
        #endregion
        public static string BaseAppUrlConstant
        {
            get
            {
                return System.Configuration.ConfigurationSettings.AppSettings["ApiBaseLiveUrl"];
            }
        }
        public static string GetConnectionString(string id = "Default")
        {
            return ConfigurationManager.ConnectionStrings[id].ConnectionString;
        }
        public static SolidColorBrush ToSolidColorBrush(this string hex_code)
        {
            return (SolidColorBrush)new BrushConverter().ConvertFromString(hex_code);
        }
        public static string UrlConstant
        {
            get
            {
                return System.Configuration.ConfigurationSettings.AppSettings["ApiLiveUrl"];
            }
        }
        #region Accounts APIEndPoints
        public static string LoginApiConstant
        {
            get
            {
                return "login";
            }
        }
        public static string ForgotPasswordApiConstant
        {
            get
            {
                return "forgotPassword";
            }
        }

        
        #endregion

        #region OrganisationDetails APIEndPoints
        public static string UserOrganisationListApiConstant
        {
            get
            {
                return "user/organization/list";
            }
        }

        public static string ChangeOrganizationApiConstant
        {
            get
            {
                return "change_organization";
            }
        }
        public static string UserProjectlistByOrganizationIDApiConstant
        {
            get
            {
                return "project/list";
            }
        }
        public static string UserToDoListApiConstant
        {
            get
            {
                return "todos/list";
            }
        }

        #endregion
        #region ActivityLog
        public static string UserActivitySyncTimertApiConstant
        {
            get
            {
                return "activity/syncTimer";
            }
        }
        public static string ActivityLogApiConstant
        {
            get
            {
                return "activity/log";
            }
        }
        #endregion

        #region AddNotes
        public static string AddNotesApiConstant
        {
            get
            {
                return "notes/add";
            }
        }
        #endregion
        #region addScreenshot
        public static string SendScreenshotsApiConstant
        {
            get
            {
                return "interval/addScreenshot";
            }
        }
        #endregion

        #region TimeIntervelFromServer
        public static string GetScreenshotIntervalApiConstant
        {
            get
            {
                return "user/screenshotInterval";
            }
        }
        #endregion

        #region RefreshToken
        public static string RenewTokenApiConstant
        {
            get
            {
                return "refreshToken";
            }
        }
        #endregion
    }
}
