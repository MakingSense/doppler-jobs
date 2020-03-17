using Newtonsoft.Json;

namespace Doppler.Sap.Job.Service.Dtos
{
    public class CurrencyDto
    {
        public string Date { get; set; }

        public decimal SaleValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? BuyValue { get; set; }
        public string CurrencyName { get; set; }
        public string CurrencyCode { get; set; }
    }
}