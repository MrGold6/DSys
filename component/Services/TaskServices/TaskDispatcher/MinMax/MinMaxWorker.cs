using MyHybridApp.Models.TaskModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax
{
    public static class MinMaxWorker
    {
        public static string Calculate(string data, MyTaskType type)
        {
            var numbers = JsonSerializer.Deserialize<List<double>>(data);
            return type == MyTaskType.Min
                ? numbers.Min().ToString()
                : numbers.Max().ToString();
        }
    }

}
