using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.DenseMatrix
{
    public static class DenseMatrixWorker
    {
        public static string Calculate(string json)
        {
            var model = JsonSerializer.Deserialize<DenseMatrixModel>(json);
            var A = model.ARows;
            var B = model.B;

            int cols = B[0].Count;
            var result = new List<List<double>>();

            foreach (var rowA in A)
            {
                var resultRow = new List<double>();
                for (int j = 0; j < cols; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < B.Count; k++)
                    {
                        sum += rowA[k] * B[k][j];
                    }
                    resultRow.Add(sum);
                }
                result.Add(resultRow);
            }

            DenseMatrixPatialResult denseMatrixPatialResult = new DenseMatrixPatialResult();

            denseMatrixPatialResult.RowIndex = model.StartRowIndex;
            denseMatrixPatialResult.Rows = result;
            // повертаємо і результат, і StartRowIndex
            return JsonSerializer.Serialize(denseMatrixPatialResult);
        }
    }

}
