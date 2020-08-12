using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrossCutting.DopplerSapService;
using CrossCutting.DopplerSapService.Entities;
using CrossCutting.DopplerSapService.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;

namespace Doppler.Jobs.Test
{
    public class DopplerSapServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IOptionsMonitor<DopplerSapConfiguration>> _dopplerSapServiceSettingsMock;

        public DopplerSapServiceTests()
        {
            _dopplerSapServiceSettingsMock = new Mock<IOptionsMonitor<DopplerSapConfiguration>>();
            _dopplerSapServiceSettingsMock.Setup(x => x.CurrentValue)
                .Returns(new DopplerSapConfiguration
                {
                    CurrencyEndpoint = "https://localhost:65100/Billing/SetCurrencyRate",
                    BillingEndpoint = "https://localhost:65100/Billing/CreateBillingRequest"
                });

            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        }

        [Fact]
        public async Task DopplerSapService_ShouldBeReturn_WhenDopplerSapServiceReturnHttpBadRequest()
        {

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = new DopplerSapService(
                _httpClientFactoryMock.Object,
                _dopplerSapServiceSettingsMock.Object, 
                Mock.Of<ILogger<DopplerSapService>>());

            var result = await service.SendCurrency(new List<CurrencyResponse>());

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task DopplerSapService_ShouldBeReturnHttpResponseOk_WhenSapCurrencyServiceReturnOk()
        {

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = new DopplerSapService(
                _httpClientFactoryMock.Object,
                _dopplerSapServiceSettingsMock.Object,
                Mock.Of<ILogger<DopplerSapService>>());

            var result = await service.SendCurrency(new List<CurrencyResponse>());

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
