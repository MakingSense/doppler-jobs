using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Sap.Job.Service.Settings;
using Moq;
using Moq.Protected;
using Xunit;

namespace Doppler.Jobs.Test
{
    public class DopplerCurrencyServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;

        public DopplerCurrencyServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeHttpStatusCodeOk_WhenCurrencyCodeIsCorrectAndDateIsNotHoliday()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent($@"{{'entity':{{'date':'2020-03-18','saleValue':65.0000,
                       'buyValue':'20.30','currencyName':'Peso Argentino'}},'success':true,'errors':{{}}}}")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                new DopplerCurrencySettings
                {
                    Url = "https://localhost:5001/Currency/",
                    CurrencyCodeList = new List<string>{ "Ars" }
                }
            );

            var result = await service.GetCurrencyByCode();

            Assert.NotNull(result);
            Assert.Equal("2020-03-18", result[0].Date);
            Assert.Equal(65.0000M, result[0].SaleValue);
            Assert.Equal(20.30M, result[0].BuyValue);
            Assert.Equal("Peso Argentino", result[0].CurrencyName);
            Assert.Equal("Ars", result[0].CurrencyCode);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeEmptyList_WhenCurrencyCodeIsCorrectAndDateIsNotHoliday()
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

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                new DopplerCurrencySettings
                {
                    Url = "https://localhost:5001/Currency/",
                    CurrencyCodeList = new List<string> { "ars" }
                }
            );

            var result = await service.GetCurrencyByCode();

            Assert.Empty(result);
        }
    }
}