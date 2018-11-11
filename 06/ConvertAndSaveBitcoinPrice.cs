using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MWPro.FaaS.Models;
using MWPro.FaaS.Models.NBP;
using Newtonsoft.Json;

namespace MWPro.FaaS
{
    public static class ConvertAndSaveBitcoinPrice // would be nice to extract conversion/getting usd price to separate func
    {
        private const string NBPRateUrl = "http://api.nbp.pl/api/exchangerates/rates/A/USD/?format=json";

        [FunctionName("ConvertAndSaveBitcoinPrice")]
        [return: Table(nameof(BitcoinPriceInfo))]
        public static async Task<BitcoinPriceInfo> Run([QueueTrigger(nameof(BitcoinPriceUpdatedEvent), Connection = "AzureWebJobsStorage")]BitcoinPriceUpdatedEvent bitcoinPriceUpdatedEvent, ILogger log)
        {
            log.LogInformation($"Consuming price: {bitcoinPriceUpdatedEvent.RateUsd}@{bitcoinPriceUpdatedEvent.Timestamp}");
            var usdToPlnRate = await GetCurrentNbpRate();
            var bitcoinPlnPrice = Decimal.Round(usdToPlnRate * bitcoinPriceUpdatedEvent.RateUsd, 2);
            log.LogInformation($"Saving 1BTC = {bitcoinPlnPrice} PLN at {bitcoinPriceUpdatedEvent.Timestamp}");

            var result = new BitcoinPriceInfo(bitcoinPriceUpdatedEvent.RateUsd, bitcoinPlnPrice, bitcoinPriceUpdatedEvent.Timestamp);

            return result;
        }

        private static async Task<decimal> GetCurrentNbpRate()
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, NBPRateUrl);

                var response = await httpClient.SendAsync(request);

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Request to NBP failed with {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var nbpData = JsonConvert.DeserializeObject<NBPRate>(responseString);

                return nbpData.Rates[0].Mid;
            }
        }
    }
}
