namespace MWPro.FaaS.Models.CoinDesk
{
    public class Currency
    {
        public string Code { get; set; }
        public string Symbol { get; set; }
        public string Rate { get; set; }
        public string Description { get; set; }
        public decimal Rate_float { get; set; }
    }
}
