using MyHybridApp.Models.TaskModel;
using System.Collections.Generic;
using System.Linq;

namespace MyHybridApp.Storage
{
    class TaskStorage
    {
        private static List<TaskExpression> _results = new();

        public static List<TaskExpression> GetResults()
        {
            lock (_results)
            {
                return _results.ToList();
            }
        }
    }
}
