using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WorkStatus.Models.ReadDTO
{
    public class AddNotesResponseModel
    {
        public NotesResponse response { get; set; }
    }
    public class NotesResponse
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}
