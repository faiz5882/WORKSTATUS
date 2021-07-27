using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class GetToDoDetailsResponseModel
    {
        public GetToDoDetailsResponse response { get; set; }
    }
    public class GetToDoDetailsResponse
    {
        public string code { get; set; }
        public string message { get; set; }
        public ToDoDetailsData data { get; set; }
    }
    public class ToDoDetailsData
    {
        public int id { get; set; }
        public int organization_id { get; set; }
        public int user_id { get; set; }
        public int project_id { get; set; }
        public string name { get; set; }
        public string? startDate { get; set; }
        public string endDate { get; set; }
        public object estiamtedHours { get; set; }
        public string description { get; set; }
        public int complete { get; set; }
        public int updated_by { get; set; }
        public int deleted_by { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public object deleted_at { get; set; }
        public string privacy { get; set; }
        public string site { get; set; }
        public object repeat { get; set; }
        public object repeat_until { get; set; }
        public object schedule_date { get; set; }
        public int archieved { get; set; }
        public object once_every { get; set; }
        public object end_after { get; set; }
        public int recurrence_status { get; set; }
        public object old_todo_id { get; set; }
        public int toggle_status { get; set; }
        public int todo_type { get; set; }
        public int recurrence_count { get; set; }
        public List<Attachment> attachment { get; set; }
    }
    public class Attachment
    {
        public int id { get; set; }
        public string url { get; set; }
        public string name { get; set; }
    }
}
