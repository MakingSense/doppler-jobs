using Newtonsoft.Json;

namespace Doppler.Sap.Job.Service.Entities
{
    public class CurrencyEntity
    {
        public string Date { get; set; }
        public decimal SaleValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? BuyValue { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
    }
}
