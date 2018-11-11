using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MWPro.FaaS.Models;
using MWPro.FaaS.Models.CoinDesk;
using Newtonsoft.Json;

namespace MWPro.FaaS
{
    public static class EnqueueBitcoinPrice
    {
        private const string BitcoinApiUrl = "https://api.coindesk.com/v1/bpi/currentprice.json";

        [FunctionName("EnqueueBitcoinPrice")]
        [return: Queue(nameof(BitcoinPriceUpdatedEvent))]
        public static async Task<BitcoinPriceUpdatedEvent> Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");

            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Get, BitcoinApiUrl);

                var response = await httpClient.SendAsync(request); 

                if (response.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw new Exception($"Request to CoinDesk failed with {response.StatusCode}");
                }

                var responseString = await response.Content.ReadAsStringAsync();
                var bitcoinData = JsonConvert.DeserializeObject<BitcoinData>(responseString);
                
                var result = new BitcoinPriceUpdatedEvent() {
                    RateUsd = bitcoinData.Bpi["USD"].Rate_float,
                    Timestamp = DateTime.UtcNow
                };

                log.LogInformation($"{result.RateUsd}@{result.Timestamp}");

                return result;
            }
        }
    }
}
