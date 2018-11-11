using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace MWPro.FaaS.Models
{
    public class BitcoinPriceInfo : TableEntity
    {
        public BitcoinPriceInfo() { }

        public BitcoinPriceInfo(decimal rateUsd, decimal ratePln, DateTime timestamp)
        {
            RatePln = ratePln.ToString();
            RateUsd = rateUsd.ToString();

            PartitionKey = timestamp.ToString("yyyy-MM");
            RowKey = timestamp.Ticks.ToString();
        }

        public string RateUsd { get; set; }
        public string RatePln { get; set; }
    }
}
