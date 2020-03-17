using System.Linq;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.Logger;
using Hangfire;

namespace Doppler.Sap.Job.Service
{
    public class DopplerSapJob : IRecurringJob
    {
        private readonly ILoggerAdapter<DopplerSapJob> _logger;
        public string Identifier { get; }
        public string IntervalCronExpression { get; }

        private readonly IDopplerCurrencyService _dopplerCurrencyService;

        public DopplerSapJob(
            ILoggerAdapter<DopplerSapJob> logger,
            string intervalCronExpression,
            string identifier,
            IDopplerCurrencyService dopplerCurrencyService)
        {
            _logger = logger;
            IntervalCronExpression = intervalCronExpression;
            Identifier = identifier;
            _dopplerCurrencyService = dopplerCurrencyService;
        }

        [AutomaticRetry(Attempts = 0)]
        public void Run()
        {
            RunAsync().GetAwaiter().GetResult();
        }

        private async Task RunAsync()
        {
            _logger.LogInformation("Getting currency per each code.");
            var currencyDto = await _dopplerCurrencyService.GetCurrencyByCode();
            
            if (currencyDto.Any())
            {
                _logger.LogInformation($"Sending data to Sap system with data: {currencyDto}.");
                //TODO: Add call DopplerSap service
            }
        }
    }
}
