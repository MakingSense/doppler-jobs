using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CrossCutting.DopplerSapService.Entities;
using CrossCutting.DopplerSapService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;

namespace CrossCutting.DopplerSapService
{
    public class DopplerSapService : IDopplerSapService
    {
        private readonly DopplerSapConfiguration _dopplerSapServiceSettings;
        private readonly JsonSerializerSettings _serializationSettings;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DopplerSapService> _logger;
        private readonly HttpClientPoliciesSettings _httpClientPoliciesSettings;

        public DopplerSapService(
            IHttpClientFactory httpClientFactory,
            HttpClientPoliciesSettings httpClientPoliciesSettings,
            IOptionsMonitor<DopplerSapConfiguration> dopplerSapServiceSettings,
            ILogger<DopplerSapService> logger)
        {
            _dopplerSapServiceSettings = dopplerSapServiceSettings.CurrentValue;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _httpClientPoliciesSettings = httpClientPoliciesSettings;
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
            _logger.LogInformation("Building http request with url {uri}.", uri);

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
                _logger.LogInformation("Sending request to Doppler SAP Api.");
                var client = _httpClientFactory.CreateClient(_httpClientPoliciesSettings.ClientName);
                httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error occurred trying to send information to SAP return http {code}.", httpResponse.StatusCode);
                throw;
            }

            return httpResponse;
        }

        public async Task<HttpResponseMessage> SendUserBillings(IList<UserBilling> userBillingList)
        {
            var uri = _dopplerSapServiceSettings.Url;
            _logger.LogInformation("Building http request with url {uri}.", uri);

            var httpRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(uri),
                Method = new HttpMethod("POST")
            };
            var requestContent = SafeJsonConvert.SerializeObject(userBillingList, _serializationSettings);
            httpRequest.Content = new StringContent(requestContent, Encoding.UTF8);
            httpRequest.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

            var httpResponse = new HttpResponseMessage();
            try
            {
                _logger.LogInformation("Sending request to Doppler SAP Api.");
                var client = _httpClientFactory.CreateClient(_httpClientPoliciesSettings.ClientName);
                httpResponse = await client.SendAsync(httpRequest).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e,
                    "Error occurred trying to send information to Doppler SAP return http {code}·", httpResponse.StatusCode);
                throw;
            }

            return httpResponse;
        }
    }
}
