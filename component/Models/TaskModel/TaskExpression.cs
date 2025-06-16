using System;

namespace MyHybridApp.Models.TaskModel
{
    public class TaskExpression
    {
        public string TaskId { get; set; }
        public string Expression { get; set; } = "";
        public MyTaskType Type { get; set; }
        public string Result { get; set; } = "";
        public string TargetClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }

    }

}
