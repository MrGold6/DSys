using System;

namespace cental_server.Models
{
    public class TaskExpression
    {
        public string TaskId { get; set; }
        public string Expression { get; set; } = "";
        public string Result { get; set; } = "";
        public string TargetClientId { get; set; } = "";
        public DateTime Timestamp { get; set; }

    }

}
