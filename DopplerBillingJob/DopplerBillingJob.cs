﻿using System.Linq;
using System.Threading.Tasks;
using CrossCutting.DopplerSapService;
using Doppler.Billing.Job.Database;
using Doppler.Billing.Job.Settings;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Doppler.Billing.Job
{
    public class DopplerBillingJob
    {
        private readonly ILogger<DopplerBillingJob> _logger;
        
        private readonly IDopplerSapService _dopplerSapService;
        private readonly IDopplerRepository _dopplerRepository;
        private readonly IOptionsMonitor<DopplerBillingJobSettings> _dopplerBillingJobSettings;

        public DopplerBillingJob(
            ILogger<DopplerBillingJob> logger,
            IDopplerSapService dopplerSapService,
            IDopplerRepository dopplerRepository,
            IOptionsMonitor<DopplerBillingJobSettings> dopplerBillingJobSettings)
        {
            _logger = logger;
            _dopplerSapService = dopplerSapService;
            _dopplerRepository = dopplerRepository;
            _dopplerBillingJobSettings = dopplerBillingJobSettings;
        }

        [AutomaticRetry(OnAttemptsExceeded = AttemptsExceededAction.Delete, Attempts = 0)]
        public void Run() => RunAsync().GetAwaiter().GetResult();

        private async Task RunAsync()
        {
            _logger.LogInformation("Getting data from Doppler database.");
            var billingData = await _dopplerRepository.GetUserBillingInformation(_dopplerBillingJobSettings.CurrentValue.StoredProcedures);

            if (billingData.Any())
            {
                _logger.LogInformation("Sending Billing data to Doppler SAP with {billingData} user billing.",
                    billingData.Count);
                await _dopplerSapService.SendUserBillings(billingData);
            }
        }
    }
}
