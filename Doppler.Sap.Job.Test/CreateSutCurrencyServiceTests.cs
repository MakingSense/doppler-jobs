using System.Net.Http;
using CrossCutting;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.Settings;
using Microsoft.Extensions.Logging;
using Moq;

namespace Doppler.Jobs.Test
{
    public static class CreateSutCurrencyServiceTests
    {
        public static DopplerCurrencyService CreateSut(
            IHttpClientFactory httpClientFactory = null,
            HttpClientPoliciesSettings httpClientPoliciesSettings = null,
            DopplerCurrencySettings dopplerCurrencySettings = null,
            ILogger<BaseExternalService> logger = null,
            TimeZoneJobConfigurations timeZoneJobConfigurations = null)
        {

            return new DopplerCurrencyService(
                httpClientFactory,
                httpClientPoliciesSettings,
                dopplerCurrencySettings,
                logger ?? Mock.Of<ILogger<BaseExternalService>>(),
                timeZoneJobConfigurations ?? new TimeZoneJobConfigurations
                {
                    TimeZoneJobs = "Argentina Standard Time"
                });
        }
    }
}
