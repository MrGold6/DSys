using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.DenseMatrix
{
    public class DenseMatrixModel
    {
        public List<List<double>> ARows { get; set; } // частина рядків A
        public List<List<double>> B { get; set; }     // повна матриця B
        public int StartRowIndex { get; set; }        // індекс першого рядка ARows
    }

}
