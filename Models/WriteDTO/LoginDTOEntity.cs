using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.WriteDTO
{
   public class LoginDTOEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public object Email_verified_at { get; set; }
        public DateTime? Created_at { get; set; }
        public DateTime? Updated_at { get; set; }
        public object Profile { get; set; }
        public object Timezone { get; set; }
        public object Phone { get; set; }
        public object Skype { get; set; }
        public object DeviceId { get; set; }
        public object DeviceType { get; set; }
        public object OsVersion { get; set; }
        public object Lat { get; set; }
        public object Lng { get; set; }
        public string Confirmation_token { get; set; }
        public int Is_confirmed { get; set; }
        public object Invitation_token { get; set; }
        public object Invitation_status { get; set; }
        public object Address { get; set; }
        public int Client_status { get; set; }
        public object Firebase_token { get; set; }
        public int Deactivate_user { get; set; }
        public object Activation_token { get; set; }
        public object Phone_country_code_id { get; set; }
        public int Invite_member_status { get; set; }
        public bool Is_organiztion { get; set; }
        public string Token { get; set; }
        public string Org_Id { get; set; }
        public string Sd_Token { get; set; }

    }
    public class LoginRequestDTOEntity
    {
        public string email { get; set; }
        public string password { get; set; }
        public string deviceType { get; set; }
        public string deviceId { get; set; }
      

    }
}
