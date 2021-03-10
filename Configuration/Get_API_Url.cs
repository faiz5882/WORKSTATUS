using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace WorkStatus.Configuration
{
   public class Get_API_Url
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="BaseUrl"></param>
        /// <returns></returns>
       
        #region  AccountsAPIEndPoints
        public string LoginApi(string BaseUrl,string Email,string Password)
        {
            return string.Format("{0}?email={1}&password={2}", BaseUrl, Email,Password);
        }
       

        #endregion
    }
}
