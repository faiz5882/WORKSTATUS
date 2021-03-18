﻿using Avalonia.Media;
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
                return System.Configuration.ConfigurationSettings.AppSettings["ApiUrl"];
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
    }
}
