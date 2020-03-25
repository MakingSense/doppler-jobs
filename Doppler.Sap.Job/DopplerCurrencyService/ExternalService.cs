using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Doppler.Sap.Job.Service.DopplerCurrencyService
{
    public abstract class ExternalService
    {
        protected HttpClient HttpClient;
        protected ILogger<ExternalService> Logger;

        protected ExternalService(HttpClient httpClient, ILogger<ExternalService> logger)
        {
            HttpClient = httpClient;
            Logger = logger;
        }
    }
}
