using System;

namespace MWPro.FaaS.Models
{
    public class BitcoinPriceUpdatedEvent
    {
        public decimal RateUsd { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
