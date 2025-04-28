using client_wpf_app.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace client_wpf_app
{
    public class TaskDTO
    {
        public int Id { get; set; }
        public string Calculation { get; set; }
        public string Result { get; set; }
        public TaskDTOStatus Status { get; set; }
        public int Client_id { get; set; }
    }
}
