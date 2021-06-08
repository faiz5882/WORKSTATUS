using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class ToDoListResponseModel
    {
        public ToDoList Response { get; set; }
    }
    public class ToDoList
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public List<ResponseData> Data { get; set; }
    }
    public class Project
    {
        public int id { get; set; }
        public int user_id { get; set; }
        public string name { get; set; }
        public bool billable { get; set; }
        public int organization_id { get; set; }
        public int added_by { get; set; }
        public string added_time { get; set; }
        public int updated_by { get; set; }
        public string updated_time { get; set; }
        public int deleted_by { get; set; }
        public object deleted_time { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public object deleted_at { get; set; }
        public int archieved { get; set; }
    }

    public class Pivot
    {
        public int todo_id { get; set; }
        public int user_id { get; set; }
    }

    public class Member
    {
        public int id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public object email_verified_at { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public object profile { get; set; }
        public object timezone { get; set; }
        public object phone { get; set; }
        public object skype { get; set; }
        public object deviceId { get; set; }
        public object deviceType { get; set; }
        public object osVersion { get; set; }
        public object lat { get; set; }
        public object lng { get; set; }
        public string confirmation_token { get; set; }
        public int? is_confirmed { get; set; }
        public object invitation_token { get; set; }
        public object invitation_status { get; set; }
        public object address { get; set; }
        public string firebase_token { get; set; }
        public int deactivate_user { get; set; }
        public object activation_token { get; set; }
        public object phone_country_code_id { get; set; }
        public int invite_member_status { get; set; }
        public int client_status { get; set; }
        public Pivot pivot { get; set; }
    }

    public class Todoattachment
    {
        public int id { get; set; }
        public int todo_id { get; set; }
        public string image { get; set; }
        public string image_url { get; set; }
    }

    public class ResponseData
    {
        public int id { get; set; }
        public int organization_id { get; set; }
        public int user_id { get; set; }
        public int project_id { get; set; }
        public string name { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public string estiamtedHours { get; set; }
        public string description { get; set; }
        public int complete { get; set; }
        public string privacy { get; set; }
        public string site { get; set; }
        public DateTime? created_at { get; set; }
        public DateTime? updated_at { get; set; }
        public Project project { get; set; }
        public List<Member> member { get; set; }
        public object repeat { get; set; }
        public string repeat_until { get; set; }
        public int archieved { get; set; }
        public List<Todoattachment> todoattachments { get; set; }
    }
}
