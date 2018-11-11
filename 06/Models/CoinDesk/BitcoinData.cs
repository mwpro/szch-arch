using System.Collections.Generic;

namespace MWPro.FaaS.Models.CoinDesk
{
    public class BitcoinData
    {
        public Time Time { get; set; }
        public string ChartName { get; set; }
        public IDictionary<string, Currency> Bpi { get; set; }
    }
}
