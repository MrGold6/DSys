using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.ScalarProduct
{
    public static class ScalarProductWorker
    {
        public static string Calculate(string jsonData)
        {
            var model = JsonSerializer.Deserialize<ScalarInputModel>(jsonData);
            double partialSum = 0;

            for (int i = 0; i < model.VectorA.Count; i++)
            {
                partialSum += model.VectorA[i] * model.VectorB[i];
            }

            return partialSum.ToString();
        }
    }

}
