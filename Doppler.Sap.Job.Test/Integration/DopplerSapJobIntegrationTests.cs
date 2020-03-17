using System.Collections.Generic;
using Doppler.Sap.Job.Service;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.Dtos;
using Doppler.Sap.Job.Service.Logger;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Doppler.Jobs.Test.Integration
{
    public class DopplerSapJobIntegrationTests : IClassFixture<TestServerFixture>
    {
        private readonly TestServerFixture _testServer;
        private readonly Mock<IDopplerCurrencyService> _dopplerCurrencyServiceMock;
        private readonly Mock<ILoggerAdapter<DopplerSapJob>> _loggerMock;

        public DopplerSapJobIntegrationTests(TestServerFixture testServerFixture)
        {
            _testServer = testServerFixture;
            _dopplerCurrencyServiceMock = new Mock<IDopplerCurrencyService>();
            _loggerMock = new Mock<ILoggerAdapter<DopplerSapJob>>();
        }

        [Fact]
        public void DopplerSapJob_ShouldBeNoSendDataToSap_WhenListIsEmpty()
        {
            _dopplerCurrencyServiceMock.Setup(x => x.GetCurrencyByCode())
                .ReturnsAsync(new List<CurrencyDto>());

            var job = new DopplerSapJob(
                _loggerMock.Object,
                "",
                "",
                _dopplerCurrencyServiceMock.Object);

            job.Run();

            Assert.True(true);

            _loggerMock.Verify(x => x.LogInformation(
                $"Getting currency per each code."), Times.Once);

            _loggerMock.Verify(x => x.LogInformation(
                "Sending data to Sap system with data: Ars."), Times.Never);
        }
    }
}
