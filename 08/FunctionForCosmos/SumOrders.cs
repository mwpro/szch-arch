using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.Documents;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionForCosmos
{
    public static class SumOrders
    {
        [FunctionName("SumOrders")]
        public static void Run([CosmosDBTrigger(
            databaseName: "Store",
            collectionName: "Orders",
            ConnectionStringSetting = "cosmosDbConnection",
            LeaseCollectionName = "leases", CreateLeaseCollectionIfNotExists = true)]IReadOnlyList<Document> input, ILogger log)
        {
            if (input != null && input.Count > 0)
            {
                var completed = input.Where(x => x.GetPropertyValue<string>("state").Equals("completed"));
                var sum = completed.Sum(x => x.GetPropertyValue<decimal>("total"));
                log.LogInformation($"{completed.Count()} orders with sum of {sum}");
            }
            else
            {
                log.LogInformation("No documents in change log");
            }
        }
    }
}
