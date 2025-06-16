using MyHybridApp.Models.TaskModel;
using MyHybridApp.Repository;
using MyHybridApp.Services.TaskServices.TaskDispatcher.DenseMatrix;
using MyHybridApp.Services.TaskServices.TaskDispatcher.EvenNumber;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Factorial;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MinMax;
using MyHybridApp.Services.TaskServices.TaskDispatcher.MultiplicationOfBigNumbers;
using MyHybridApp.Services.TaskServices.TaskDispatcher.ScalarProduct;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Services.TaskServices.TaskDispatcher.Sum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices
{
    public class CalculateResult
    {
        public static string getCalulatedPartialResultByType(TaskExpression task)
        {
            switch (task.Type)
            {
                case MyTaskType.Factorial:
                    {
                        var result = CalculationParser.Evaluate(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.MultiplyBigNumbers:
                    {
                        var result = CalculationParser.Evaluate(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.StdDev:
                    {
                        var result = StdDevWorker.CalculatePartial(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.Min:
                    {
                        string result = MinMaxWorker.Calculate(task.Expression, task.Type);
                        return result.ToString();
                    }
                case MyTaskType.Max:
                    {
                        string result = MinMaxWorker.Calculate(task.Expression, task.Type);
                        return result.ToString();
                    }
                case MyTaskType.Sum:
                    {
                        string result = SumWorker.Calculate(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.EvenFilter:
                    {
                        string result = EvenNumberWorker.Calculate(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.ScalarProduct:
                    {
                        string result = ScalarProductWorker.Calculate(task.Expression);
                        return result.ToString();
                    }
                case MyTaskType.DenseMatrixMultiply:
                    {
                        string result = DenseMatrixWorker.Calculate(task.Expression);
                        return result.ToString();
                    }
                default: return "";

            }
        }

        public static string getCalulatedFinalResultByType(MyTask task, List<TaskExpression> allResults)
        {
            switch (task.Type)
            {
                case MyTaskType.Factorial:
                    {
                        var expression = ExpressionSplitter.AggregateIntermediateResults(allResults.Select(r => r.Result).ToList());
                        var final = CalculationParser.Evaluate(expression.First());
                        return final.ToString();
                    }
                case MyTaskType.MultiplyBigNumbers:
                    {
                        var expression = ExpressionSplitter.AggregateIntermediateResults(allResults.Select(r => r.Result).ToList());
                        var final = CalculationParser.Evaluate(expression.First());
                        return final.ToString();
                    }
                case MyTaskType.StdDev:
                    {
                        var total = allResults.Sum(r => double.Parse(r.Result));

                        var model = JsonSerializer.Deserialize<StdDevExpressionModel>(task.Expression);
                        double final = Math.Sqrt(total / model.TotalCount);

                        return final.ToString();
                    }
                case MyTaskType.Min:
                    {
                        double final = task.Type == MyTaskType.Min
                                            ? allResults.Min(r => double.Parse(r.Result))
                                            : allResults.Max(r => double.Parse(r.Result));
                        return final.ToString();
                    }
                case MyTaskType.Max:
                    {
                        double final = task.Type == MyTaskType.Min
                                            ? allResults.Min(r => double.Parse(r.Result))
                                            : allResults.Max(r => double.Parse(r.Result));
                        return final.ToString();
                    }
                case MyTaskType.Sum:
                    {
                        double final = allResults.Sum(r => double.Parse(r.Result));
                        return final.ToString();
                    }
                case MyTaskType.ScalarProduct:
                    {
                        double final = allResults.Sum(r => double.Parse(r.Result));
                        return final.ToString();
                    }
                case MyTaskType.DenseMatrixMultiply:
                    {
                        int totalRows = allResults.Sum(p => JsonSerializer.Deserialize<DenseMatrixPatialResult>(p.Result).Rows.Count);

                        var C = new List<List<double>>(new List<double>[totalRows]);

                        foreach (var p in allResults)
                        {
                            DenseMatrixPatialResult data = JsonSerializer.Deserialize<DenseMatrixPatialResult>(p.Result);
                            int startIndex = data.RowIndex;
                            var rows = data.Rows;

                            for (int i = 0; i < rows.Count; i++)
                            {
                                C[startIndex + i] = rows[i];
                            }
                        }

                        return JsonSerializer.Serialize(C);
                    }
                case MyTaskType.EvenFilter:
                    {
                        var allEven = new List<long>();

                        foreach (var r in allResults)
                        {
                            var partial = JsonSerializer.Deserialize<List<long>>(r.Result);
                            allEven.AddRange(partial);
                        }

                        return JsonSerializer.Serialize(allEven);
                    }
                default: return "";

            }
        }
    }
}
