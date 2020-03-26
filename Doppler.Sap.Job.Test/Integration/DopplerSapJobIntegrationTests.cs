using System;
using System.Collections.Generic;
using Doppler.Sap.Job.Service;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.DopplerSapService;
using Doppler.Sap.Job.Service.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Doppler.Jobs.Test.Integration
{
    public class DopplerSapJobIntegrationTests : IClassFixture<TestServerFixture>
    {
        private readonly Mock<IDopplerCurrencyService> _dopplerCurrencyServiceMock;
        private readonly Mock<ILogger<DopplerSapJob>> _loggerMock;
        private readonly Mock<IDopplerSapService> _dopplerSapServiceMock;

        public DopplerSapJobIntegrationTests()
        {
            _dopplerCurrencyServiceMock = new Mock<IDopplerCurrencyService>();
            _loggerMock = new Mock<ILogger<DopplerSapJob>>();
            _dopplerSapServiceMock = new Mock<IDopplerSapService>();
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
                _dopplerSapServiceMock.Object);

            job.Run();

            Assert.True(true);

            _loggerMock.VerifyLogger(LogLevel.Information, "Getting currency per each code enabled.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Sending data to Doppler SAP system with data: 0.", Times.Never());
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

            var job = new DopplerSapJob(
                _loggerMock.Object,
                "",
                "",
                _dopplerCurrencyServiceMock.Object,
                _dopplerSapServiceMock.Object);

            job.Run();

            _loggerMock.VerifyLogger(LogLevel.Information, "Getting currency per each code enabled.", Times.Once());
            _loggerMock.VerifyLogger(LogLevel.Information, "Sending data to Doppler SAP system with data: 1.", Times.Once());
        }
    }
}
