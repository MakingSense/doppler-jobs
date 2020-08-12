using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossCutting.DopplerSapService;
using CrossCutting.DopplerSapService.Entities;
using Doppler.Currency.Job.DopplerCurrencyService;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Doppler.Currency.Job
{
    public class DopplerCurrencyJob
    {
        private readonly ILogger<DopplerCurrencyJob> _logger;
        private readonly IDopplerCurrencyService _dopplerCurrencyService;
        private readonly IDopplerSapService _dopplerSapService;

        public DopplerCurrencyJob(
            ILogger<DopplerCurrencyJob> logger,
            IDopplerCurrencyService dopplerCurrencyService,
            IDopplerSapService dopplerSapService)
        {
            _logger = logger;
            _dopplerCurrencyService = dopplerCurrencyService;
            _dopplerSapService = dopplerSapService;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 0)]
        public IList<CurrencyResponse> Run() => RunAsync().GetAwaiter().GetResult();

        private async Task<IList<CurrencyResponse>> RunAsync()
        {
            _logger.LogInformation("Getting currency per each code enabled.");
            var currencyDto = await _dopplerCurrencyService.GetCurrencyByCode();

            if (!currencyDto.Any())
            {
                _logger.LogInformation("Non-existent currencies for this date, please check for errors.");
                return new List<CurrencyResponse>();
            }

            _logger.LogInformation("Sending currency data to Doppler SAP system.");
            await _dopplerSapService.SendCurrency(currencyDto);

            _logger.LogInformation("Insert currency data into Doppler Database.");
            await _dopplerCurrencyService.InsertCurrencyIntoDataBase(currencyDto);

            return currencyDto;
        }
    }
}
