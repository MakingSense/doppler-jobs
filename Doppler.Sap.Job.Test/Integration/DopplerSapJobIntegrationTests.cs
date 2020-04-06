using System;
using System.Collections.Generic;
using Doppler.Sap.Job.Service;
using Doppler.Sap.Job.Service.Database;
using Doppler.Sap.Job.Service.Database.Entities;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.DopplerSapService;
using Doppler.Sap.Job.Service.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Doppler.Jobs.Test.Integration
{
    public class DopplerSapJobIntegrationTests
    {
        private readonly Mock<IDopplerCurrencyService> _dopplerCurrencyServiceMock;
        private readonly Mock<ILogger<DopplerSapJob>> _loggerMock;
        private readonly Mock<IDopplerSapService> _dopplerSapServiceMock;
        private readonly Mock<IDopplerRepository> _dopplerRepositoryMock;

        public DopplerSapJobIntegrationTests()
        {
            _dopplerCurrencyServiceMock = new Mock<IDopplerCurrencyService>();
            _loggerMock = new Mock<ILogger<DopplerSapJob>>();
            _dopplerSapServiceMock = new Mock<IDopplerSapService>();
            _dopplerRepositoryMock = new Mock<IDopplerRepository>();
        }

        [Fact]
        public void DopplerSapJob_ShouldBeNoSendDataToSap_WhenListIsEmpty()
        {
            _dopplerCurrencyServiceMock.Setup(x => x.GetCurrencyByCode())
                .ReturnsAsync(new List<CurrencyResponse>());

            var job = new DopplerSapJob(
                _loggerMock.Object,
                "",
                "",
                _dopplerCurrencyServiceMock.Object,
                _dopplerSapServiceMock.Object,
                _dopplerRepositoryMock.Object);

            job.Run();

            Assert.True(true);

            _loggerMock.VerifyLogger(LogLevel.Information, "Getting currency per each code enabled.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Sending data to Doppler SAP system.", Times.Never());
        }

        [Fact]
        public void DopplerSapJob_ShouldBeNoSendDataToSap_WhenListIsHaveOneCurrencyArs()
        {
            var currency = new CurrencyResponse
            {
                Entity = new CurrencyEntity
                {
                    BuyValue = 10.20M,
                    CurrencyName = "Peso Argentino",
                    SaleValue = 30.3333M,
                    CurrencyCode = "Ars",
                    Date = DateTime.UtcNow.ToShortDateString()
                }
            };
            _dopplerCurrencyServiceMock.Setup(x => x.GetCurrencyByCode())
                .ReturnsAsync(new List<CurrencyResponse>
                {
                    currency
                });

            _dopplerRepositoryMock.Setup(x => x.GetUserBillingInformation())
                .ReturnsAsync(new List<UserBilling>());

            var job = new DopplerSapJob(
                _loggerMock.Object,
                "",
                "",
                _dopplerCurrencyServiceMock.Object,
                _dopplerSapServiceMock.Object,
                _dopplerRepositoryMock.Object);

            job.Run();

            _loggerMock.VerifyLogger(LogLevel.Information, "Getting currency per each code enabled.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Sending currency data to Doppler SAP system.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Getting data from Doppler database.", Times.Once());
        }

        [Fact]
        public void DopplerSapJob_ShouldBeSendDataToSap_WhenListIsHaveOneUserBillingCreated()
        {
            var currency = new CurrencyResponse
            {
                Entity = new CurrencyEntity
                {
                    BuyValue = 10.20M,
                    CurrencyName = "Peso Argentino",
                    SaleValue = 30.3333M,
                    CurrencyCode = "Ars",
                    Date = DateTime.UtcNow.ToShortDateString()
                }
            };
            _dopplerCurrencyServiceMock.Setup(x => x.GetCurrencyByCode())
                .ReturnsAsync(new List<CurrencyResponse>
                {
                    currency
                });

            _dopplerRepositoryMock.Setup(x => x.GetUserBillingInformation())
                .ReturnsAsync(new List<UserBilling>
                {
                    new UserBilling
                {
                        Date = DateTime.UtcNow,
                        Amount = 133.212M,
                        CreditsAmount = 1000000,
                        Fee = 0.4M,
                        Description = "Servicio de Email Marketing.",
                        PlanType = "Plan Mensual.",
                        Id = 1,
                        PaymentDate = DateTime.UtcNow.AddMonths(1),
                        TotalAmount = 234.455M,
                        UserId = 10002
                }});

            var job = new DopplerSapJob(
                _loggerMock.Object,
                "",
                "",
                _dopplerCurrencyServiceMock.Object,
                _dopplerSapServiceMock.Object,
                _dopplerRepositoryMock.Object);

            job.Run();

            _loggerMock.VerifyLogger(LogLevel.Information, "Getting data from Doppler database.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Sending Billing data to Doppler SAP system 1.", Times.Once());
        }
    }
}
