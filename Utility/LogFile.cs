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
        
        public static bool WriteLog(string strMessage)
        {
            try
            {
                string fullPath = ConfigurationManager.AppSettings["LogPath"].ToString();
                string directoryPath = fullPath;
                string updatedPath = directoryPath.Replace("|", "_");
                updatedPath = directoryPath.Replace(" ", "");
                if (!Directory.Exists(updatedPath))
                {
                    Directory.CreateDirectory(updatedPath);
                }
                string strFileName = DateTime.Now.ToString("yyyyMMdd")+".txt";
                FileStream objFilestream = new FileStream(string.Format("{0}\\{1}", updatedPath, strFileName), FileMode.Append, FileAccess.Write);
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

      
    }
}
