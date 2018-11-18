using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Linq;
using MWPro.FaaS.Models;
using Microsoft.WindowsAzure.Storage.Table;

namespace MWPro.FaaS
{
    public static class GetBitcoinRate
    {
        [FunctionName("GetBitcoinRate")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, 
            [Table(nameof(BitcoinPriceInfo))] CloudTable table,
            ILogger log)
        {
            if (!DateTime.TryParse(req.Query["date"], out var date))
                return new BadRequestResult();

            TableQuery<BitcoinPriceInfo> query = new TableQuery<BitcoinPriceInfo>().Where(TableQuery
                .CombineFilters(
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThanOrEqual, date.Date),
                    TableOperators.And,
                    TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.LessThan, date.Date.AddDays(1))));

            var result = (await table.ExecuteQuerySegmentedAsync(query, null))
                .Select(x => new { x.Timestamp, x.RatePln, x.RateUsd });

            return new OkObjectResult(result);
        }
    }
}
