﻿using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using CrossCutting;
using CrossCutting.Authorization;
using Doppler.Currency.Job.DopplerCurrencyService;
using Doppler.Currency.Job.Settings;
using Doppler.Database;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace Doppler.Jobs.Test
{
    [ExcludeFromCodeCoverage]
    public static class CreateSutCurrencyServiceTests
    {
        public static DopplerCurrencyService CreateSut(
            IHttpClientFactory httpClientFactory = null,
            IOptionsMonitor<DopplerCurrencyServiceSettings> dopplerCurrencySettings = null,
            ILogger<DopplerCurrencyService> loggerCurrencyService = null,
            TimeZoneJobConfigurations timeZoneJobConfigurations = null,
            IDbConnectionFactory dbConnectionFactory = null,
            IOptions<JwtOptions> jwtOptions = null,
            SigningCredentials signingCredentials = null,
            JwtSecurityTokenHandler jwtSecurityTokenHandler = null)
        {

            return new DopplerCurrencyService(
                httpClientFactory,
                dopplerCurrencySettings,
                loggerCurrencyService ?? Mock.Of<ILogger<DopplerCurrencyService>>(),
                
                timeZoneJobConfigurations ?? new TimeZoneJobConfigurations
                {
                    TimeZoneJobs = TimeZoneHelper.GetTimeZoneByOperativeSystem("Argentina Standard Time")
                },
                dbConnectionFactory,
                Mock.Of<IJwtTokenGenerator>());
        }
    }
}
