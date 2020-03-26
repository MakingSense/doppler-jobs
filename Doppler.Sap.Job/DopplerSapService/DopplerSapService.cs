using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.Entities;
using Doppler.Sap.Job.Service.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace Doppler.Sap.Job.Service.DopplerSapService
{
    public class DopplerSapService : BaseExternalService, IDopplerSapService
    {
        private readonly DopplerSapServiceSettings _dopplerSapServiceSettings;
        private readonly JsonSerializerSettings _serializationSettings;

        public DopplerSapService(
            IHttpClientFactory httpClientFactory, 
            HttpClientPoliciesSettings httpClientPoliciesSettings,
            DopplerSapServiceSettings dopplerSapServiceSettings,
            ILogger<BaseExternalService> logger) 
            : base(httpClientFactory.CreateClient(httpClientPoliciesSettings.ClientName), logger)
        {
            _dopplerSapServiceSettings = dopplerSapServiceSettings;

            _serializationSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                ContractResolver = new ReadOnlyJsonContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new Iso8601TimeSpanConverter()
                }
            };
        }

        public async Task<HttpResponseMessage> SendCurrency(IList<CurrencyResponse> currencyList)
        {
            var uri = _dopplerSapServiceSettings.Url;
            Logger.LogInformation($"Building http request with url {uri}");
            
            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = new HttpMethod("POST")
            };
            var requestContent = SafeJsonConvert.SerializeObject(currencyList, _serializationSettings);
            httpRequest.Content = new StringContent(requestContent, Encoding.UTF8);
            httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var httpResponse = new HttpResponseMessage();
            try
            {
                Logger.LogInformation("Sending request to Doppler SAP Api.");
                httpResponse = await HttpClient.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Logger.LogError(e, $"Error occurred trying to send information to Doppler SAP return http code {httpResponse.StatusCode}.");
                throw;
            }

            return httpResponse;
        }
    }
}
