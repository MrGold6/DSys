using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev
{
    public static class StdDevWorker
    {
        public static double CalculatePartial(string jsonData)
        {
            var model = JsonSerializer.Deserialize<StdDevExpressionModel>(jsonData);
            if (model?.Values == null) return 0;

            double mean = model.Mean;
            return model.Values.Sum(x => Math.Pow(x - mean, 2));
        }
    }
}
