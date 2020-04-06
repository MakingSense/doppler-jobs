using System.Linq;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Sap.Job.Service.Database;
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
        private readonly IDopplerRepository _dopplerRepository;

        public DopplerSapJob(
            ILogger<DopplerSapJob> logger,
            string intervalCronExpression,
            string identifier,
            IDopplerCurrencyService dopplerCurrencyService,
            IDopplerSapService dopplerSapService,
            IDopplerRepository dopplerRepository)
        {
            _logger = logger;
            IntervalCronExpression = intervalCronExpression;
            Identifier = identifier;
            _dopplerCurrencyService = dopplerCurrencyService;
            _dopplerSapService = dopplerSapService;
            _dopplerRepository = dopplerRepository;
        }

        [AutomaticRetry(Attempts = 0)]
        public object Run() => RunAsync().GetAwaiter().GetResult();

        private async Task<object> RunAsync()
        {
            _logger.LogInformation("Getting currency per each code enabled.");
            var currencyDto = await _dopplerCurrencyService.GetCurrencyByCode();

            if (!currencyDto.Any()) 
                return "Non-existent Currency for this date, please check if it's a holiday.";

            _logger.LogInformation("Sending currency data to Doppler SAP system.");
            await _dopplerSapService.SendCurrency(currencyDto);

            _logger.LogInformation("Getting data from Doppler database.");
            var billingData = await _dopplerRepository.GetUserBillingInformation();

            _logger.LogInformation($"Sending Billing data to Doppler SAP system {billingData.Count()}.");
            //TODO: Create a service to send data to SAP system with billingData variable

            return currencyDto;
        }
    }
}
