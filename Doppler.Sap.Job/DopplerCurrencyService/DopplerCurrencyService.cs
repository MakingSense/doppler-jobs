using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Sap.Job.Service.Dtos;
using Doppler.Sap.Job.Service.Logger;
using Doppler.Sap.Job.Service.Settings;
using Newtonsoft.Json;

namespace Doppler.Sap.Job.Service.DopplerCurrencyService
{
    public class DopplerCurrencyService : IDopplerCurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly DopplerCurrencySettings _dopplerCurrencySettings;
        private readonly ILoggerAdapter<DopplerCurrencyService> _logger;
        private readonly TimeZoneJobConfigurations _jobConfig;

        public DopplerCurrencyService(
            IHttpClientFactory httpClientFactory,
            HttpClientPoliciesSettings httpClientPoliciesSettings,
            DopplerCurrencySettings dopplerCurrencySettings,
            ILoggerAdapter<DopplerCurrencyService> logger,
            TimeZoneJobConfigurations jobConfig)
        {
            _httpClient = httpClientFactory.CreateClient(httpClientPoliciesSettings.ClientName);
            _dopplerCurrencySettings = dopplerCurrencySettings;
            _logger = logger;
            _jobConfig = jobConfig;
        }

        public async Task<IList<CurrencyDto>> GetCurrencyByCode()
        {
            var cstZone = TimeZoneInfo.FindSystemTimeZoneById(_jobConfig.TimeZoneJobs);
            var cstTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, cstZone);
            
            var returnList = new List<CurrencyDto>();
            
            foreach (var currencyCode in _dopplerCurrencySettings.CurrencyCodeList)
            {
                var uri = new Uri(_dopplerCurrencySettings.Url + $"{currencyCode}/{cstTime.Year}-{cstTime.Month}-{cstTime.Day}");

                _logger.LogInformation($"Building http request with url {uri}");
                var httpRequest = new HttpRequestMessage
                {
                    RequestUri = uri,
                    Method = new HttpMethod("GET")
                };

                try
                {
                    _logger.LogInformation("Sending request to Doppler Currency Api.");
                    var httpResponse = await _httpClient.SendAsync(httpRequest).ConfigureAwait(false);

                    if (!httpResponse.IsSuccessStatusCode)
                        continue;

                    var json = await httpResponse.Content.ReadAsStringAsync();

                    var result = JsonConvert.DeserializeObject<dynamic>(json);
                    var date = result["entity"]["date"].ToString();
                    var sale = result["entity"]["saleValue"].ToString();
                    var buy = result["entity"]["buyValue"].ToString();
                    var currencyName = result["entity"]["currencyName"].ToString();

                    var dto = new CurrencyDto
                    {
                        BuyValue = decimal.Parse(buy),
                        CurrencyName = currencyName,
                        Date = date,
                        SaleValue = decimal.Parse(sale),
                        CurrencyCode = currencyCode
                    };

                    returnList.Add(dto);
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
