using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Sap.Job.Service.Entities;
using Doppler.Sap.Job.Service.Settings;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TimeZoneConverter;

namespace Doppler.Sap.Job.Service.DopplerCurrencyService
{
    public class DopplerCurrencyService : IDopplerCurrencyService
    {
        private readonly DopplerCurrencyServiceSettings _dopplerCurrencySettings;
        private readonly TimeZoneJobConfigurations _jobConfig;
        private readonly HttpClient _httpClient;
        private readonly ILogger<DopplerCurrencyService> _logger;

        public DopplerCurrencyService(
            IHttpClientFactory httpClientFactory,
            HttpClientPoliciesSettings httpClientPoliciesSettings,
            DopplerCurrencyServiceSettings dopplerCurrencySettings,
            ILogger<DopplerCurrencyService> logger,
            TimeZoneJobConfigurations jobConfig)
        {
            _dopplerCurrencySettings = dopplerCurrencySettings;
            _jobConfig = jobConfig;
            _httpClient = httpClientFactory.CreateClient(httpClientPoliciesSettings.ClientName);
            _logger = logger;
        }

        public async Task<IList<CurrencyResponse>> GetCurrencyByCode()
        {
            var tz = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? _jobConfig.TimeZoneJobs 
                : TZConvert.WindowsToIana(_jobConfig.TimeZoneJobs);

            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
            var cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
            
            var returnList = new List<CurrencyResponse>();
            
            foreach (var currencyCode in _dopplerCurrencySettings.CurrencyCodeList)
            {
                try { 
                    var uri = new Uri(_dopplerCurrencySettings.Url + $"{currencyCode}/{cstTime.Year}-{cstTime.Month}-{cstTime.Day}");
                    
                    _logger.LogInformation($"Building http request with url {uri}"); 
                    var httpRequest = new HttpRequestMessage 
                    {
                        RequestUri = uri, 
                        Method = new HttpMethod("GET")

                    };
                    
                    _logger.LogInformation("Sending request to Doppler Currency Api."); 
                    var httpResponse = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);

                    if (!httpResponse.IsSuccessStatusCode)
                        continue;

                    var json = await httpResponse.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<CurrencyResponse>(json);

                    returnList.Add(result);
                }
                catch (Exception e)
                {
                    _logger.LogError(e,$"Error GetCurrency for {currencyCode}.");
                    throw;
                }
            }

            return returnList;
        }
    }
}
