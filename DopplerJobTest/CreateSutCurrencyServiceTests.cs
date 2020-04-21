using System.Net.Http;
using CrossCutting;
using Doppler.Currency.Job.DopplerCurrencyService;
using Doppler.Currency.Job.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace Doppler.Jobs.Test
{
    public static class CreateSutCurrencyServiceTests
    {
        public static DopplerCurrencyService CreateSut(
            IHttpClientFactory httpClientFactory = null,
            HttpClientPoliciesSettings httpClientPoliciesSettings = null,
            IOptionsMonitor<DopplerCurrencyServiceSettings> dopplerCurrencySettings = null,
            ILogger<DopplerCurrencyService> loggerCurrencyService = null,
            TimeZoneJobConfigurations timeZoneJobConfigurations = null)
        {

            return new DopplerCurrencyService(
                httpClientFactory,
                httpClientPoliciesSettings,
                dopplerCurrencySettings,
                loggerCurrencyService ?? Mock.Of<ILogger<DopplerCurrencyService>>(),
                timeZoneJobConfigurations ?? new TimeZoneJobConfigurations
                {
                    TimeZoneJobs = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time") 
                });
        }
    }
}
