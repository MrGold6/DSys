using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev
{
    public class StdDevExpressionModel
    {
        public List<double> Values { get; set; }
        public double Mean { get; set; }
        public double TotalCount { get; set; }
    }

}
