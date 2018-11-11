using System.Collections.Generic;

namespace MWPro.FaaS.Models.NBP
{
    public class NBPRate
    {
        public string Table { get; set; }
        public string Currency { get; set; }
        public string Code { get; set; }
        public List<Rate> Rates { get; set; }
    }
}