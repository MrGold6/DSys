using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.EvenNumber
{
    public static class EvenNumberWorker
    {
        public static string Calculate(string jsonData)
        {
            var numbers = JsonSerializer.Deserialize<List<long>>(jsonData);
            var evenNumbers = numbers.Where(n => n % 2 == 0).ToList();
            return JsonSerializer.Serialize(evenNumbers);
        }
    }

}
