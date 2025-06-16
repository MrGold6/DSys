using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Models.TaskModel
{
    public class MyTask
    {
        public string TaskId { get; set; }
        public string Expression { get; set; } = "";
        public int CountOfSubTasks { get; set; }
        public string Result { get; set; } = "";
        public MyTaskStatus Status { get; set; }
        public MyTaskType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public void setStatus(MyTaskStatus status) { 
            this.Status = status;
        }
    }

}
