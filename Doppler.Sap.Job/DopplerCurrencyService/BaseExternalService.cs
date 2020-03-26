using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Doppler.Sap.Job.Service.DopplerCurrencyService
{
    public abstract class BaseExternalService
    {
        protected HttpClient HttpClient;
        protected ILogger<BaseExternalService> Logger;

        protected BaseExternalService(HttpClient httpClient, ILogger<BaseExternalService> logger)
        {
            HttpClient = httpClient;
            Logger = logger;
        }
    }
}
