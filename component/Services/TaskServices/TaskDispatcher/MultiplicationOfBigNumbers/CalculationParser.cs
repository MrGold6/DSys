using MyHybridApp.Helper;
using MyHybridApp.Models.TaskModel;
using MyHybridApp.Services.TaskServices.TaskDispatcher.StdDev;
using MyHybridApp.Storage;
using System;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.MultiplicationOfBigNumbers
{
    public class CalculationParser
    {
        public static BigInteger Evaluate(string expr)
        {
            BigInteger result = 0;

            try
            {
                if (string.IsNullOrWhiteSpace(expr))
                    return result;

                // Вилучаємо числа
                var parts = expr
                    .Replace("(", "")
                    .Replace(")", "")
                    .Split(['+', '-', '*'], StringSplitOptions.RemoveEmptyEntries)
                    .Select(p => BigInteger.Parse(p.Trim()))
                    .ToList();

                if (parts.Count == 0)
                    return result;

                result = parts[0];
                int numberIndex = 1;

                for (int i = 0; i < expr.Length && numberIndex < parts.Count; i++)
                {
                    switch (expr[i])
                    {
                        case '+':
                            result += parts[numberIndex++];
                            break;
                        case '-':
                            result -= parts[numberIndex++];
                            break;
                        case '*':
                            result *= parts[numberIndex++];
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"[CalculationParser ERROR] {ex.Message}");
            }

            return result;
        }

    }
}
