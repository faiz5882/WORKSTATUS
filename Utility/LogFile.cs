using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Utility
{
   public class LogFile
    {
        static bool isWindows = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);
        static bool isLinux = System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux);
          
        public static bool WriteLog(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath =  ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd")+".txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        public static bool ErrorLog(Exception strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "ErrorLog.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.WriteLine(strMessage.StackTrace);
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);                
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool ErrorLogApiResponse(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "ErrorLog.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);                
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool ActivityAPILogApiResponse(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "APIActivity.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool ActivityTimerDataApiResponse(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "APIActivityTimerData.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool OrgAPILogApiResponse(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "OrganizationAPI.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool WriteMessageLog(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "Timelog.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        public static bool WriteaActivityLog(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "Activity" + ".txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static bool LogApiResponseSceenShot(string strMessage)
        {
            try
            {
                string fullPath = string.Empty;
                if (isLinux)
                { fullPath = "LogWorkStatus"; }
                else
                {
                    fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                }
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd") + "APISceenShot.txt";
                FileStream objFilestream = new FileStream(string.Format("{0}/{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
                StreamWriter objStreamWriter = new StreamWriter((Stream)objFilestream);

                objStreamWriter.WriteLine("-------------------START-------------" + DateTime.Now);
                objStreamWriter.WriteLine(strMessage);
                objStreamWriter.WriteLine("-------------------END-------------" + DateTime.Now);
                objStreamWriter.Close();
                objFilestream.Close();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
