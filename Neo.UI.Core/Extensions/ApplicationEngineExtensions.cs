using System.Linq;
using System.Text;

using Neo.IO.Json;

using Neo.SmartContract;
using Neo.VM;

namespace Neo.UI.Core.Extensions
{
    public static class ApplicationEngineExtensions
    {
        public static string GetInvocationTestResult(this ApplicationEngine applicationEngine)
        {
            var builder = new StringBuilder();
            builder.AppendLine($"VM State: {applicationEngine.State}");
            builder.AppendLine($"Gas Consumed: {applicationEngine.GasConsumed}");
            builder.AppendLine($"Evaluation Stack: {new JArray(applicationEngine.EvaluationStack.Select(p => p.ToParameter().ToJson()))}");

            return builder.ToString();
        }
    }
}
