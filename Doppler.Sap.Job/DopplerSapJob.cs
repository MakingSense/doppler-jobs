using System.Linq;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.DopplerSapService;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Doppler.Sap.Job.Service
{
    public class DopplerSapJob : IRecurringJob
    {
        private readonly ILogger<DopplerSapJob> _logger;
        public string Identifier { get; }
        public string IntervalCronExpression { get; }

        private readonly IDopplerCurrencyService _dopplerCurrencyService;
        private readonly IDopplerSapService _dopplerSapService;

        public DopplerSapJob(
            ILogger<DopplerSapJob> logger,
            string intervalCronExpression,
            string identifier,
            IDopplerCurrencyService dopplerCurrencyService,
            IDopplerSapService dopplerSapService)
        {
            _logger = logger;
            IntervalCronExpression = intervalCronExpression;
            Identifier = identifier;
            _dopplerCurrencyService = dopplerCurrencyService;
            _dopplerSapService = dopplerSapService;
        }

        [AutomaticRetry(Attempts = 0)]
        public object Run() => RunAsync().GetAwaiter().GetResult();

        private async Task<object> RunAsync()
        {
            _logger.LogInformation("Getting currency per each code enabled.");
            var currencyDto = await _dopplerCurrencyService.GetCurrencyByCode();

            if (!currencyDto.Any()) 
                return "Non-existent Currency for this date, please check if it's a holiday.";

            _logger.LogInformation($"Sending data to Doppler SAP system.");
            await _dopplerSapService.SendCurrency(currencyDto);

            return currencyDto;
        }
    }
}
