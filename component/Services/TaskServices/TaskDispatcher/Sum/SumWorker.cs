using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.Sum
{
    public static class SumWorker
    {
        public static string Calculate(string jsonData)
        {
            var numbers = JsonSerializer.Deserialize<List<double>>(jsonData);
            double sum = numbers.Sum();
            return sum.ToString();
        }
    }

}
