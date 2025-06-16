using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyHybridApp.Services.TaskServices.TaskDispatcher.Factorial
{
    public static class ExpressionSplitter
    {
        public static List<string> SplitFactorialByNodes(int number, int nodeCount)
        {
            var allFactors = Enumerable.Range(1, number).ToList();
            int chunkSize = (int)Math.Ceiling((double)allFactors.Count / nodeCount);

            var result = new List<string>();
            for (int i = 0; i < allFactors.Count; i += chunkSize)
            {
                var chunk = allFactors.Skip(i).Take(chunkSize);
                result.Add(string.Join("*", chunk));
            }

            return result;
        }

        public static List<string> AggregateIntermediateResults(List<string> partialResults, int maxFinal = 2)
        {
            var result = new List<string>(partialResults);

            while (result.Count > maxFinal)
            {
                var combined = new List<string>();
                for (int i = 0; i < result.Count; i += 2)
                {
                    if (i + 1 < result.Count)
                        combined.Add($"({result[i]})*({result[i + 1]})");
                    else
                        combined.Add(result[i]);
                }
                result = combined;
            }

            return result;
        }
    }

}
