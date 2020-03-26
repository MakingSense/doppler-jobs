using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Sap.Job.Service.DopplerCurrencyService;
using Doppler.Sap.Job.Service.DopplerSapService;
using Doppler.Sap.Job.Service.Entities;
using Doppler.Sap.Job.Service.Settings;
using Microsoft.Extensions.Logging;
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

        public DopplerSapServiceTests()
        {
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
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                new DopplerSapServiceSettings()
                {
                    Url = "https://localhost:5001/SetCurrencyRate"
                }, 
                Mock.Of<ILogger<BaseExternalService>>());

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
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                new DopplerSapServiceSettings
                {
                    Url = "https://localhost:5001/SetCurrencyRate"
                },
                Mock.Of<ILogger<BaseExternalService>>());

            var result = await service.SendCurrency(new List<CurrencyResponse>());

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
