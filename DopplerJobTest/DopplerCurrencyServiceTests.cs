using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CrossCutting;
using Doppler.Currency.Job.DopplerCurrencyService;
using Doppler.Currency.Job.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly Mock<IOptionsMonitor<DopplerCurrencyServiceSettings>> _dopplerCurrencyServiceSettingsMock;

        public DopplerCurrencyServiceTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            _dopplerCurrencyServiceSettingsMock = new Mock<IOptionsMonitor<DopplerCurrencyServiceSettings>>();
            _dopplerCurrencyServiceSettingsMock.Setup(x => x.CurrentValue)
                .Returns(new DopplerCurrencyServiceSettings
                {
                    Url = "https://localhost:5001/Currency/",
                    CurrencyCodeList = new List<string> {"ARS"}
                });
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeHttpStatusCodeOk_WhenCurrencyCodeIsCorrectAndDateIsNotHoliday()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{'entity':{'date':'2020-03-18','saleValue':65.0000,
                       'buyValue':'20.30','currencyName':'Peso Argentino', 'currencyCode':'ARS'},'success':true,'errors':{}}")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                _dopplerCurrencyServiceSettingsMock.Object
            );

            var result = await service.GetCurrencyByCode();

            Assert.NotEmpty(result);

            var currency = result[0].Entity;
            Assert.Equal("2020-03-18", currency.Date);
            Assert.Equal(65.0000M, currency.SaleValue);
            Assert.Equal(20.30M, currency.BuyValue);
            Assert.Equal("Peso Argentino", currency.CurrencyName);
            Assert.Equal("ARS", currency.CurrencyCode);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeEmptyList_WhenCurrencyCodeIsCorrectAndDateIsNotHoliday()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
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
                _dopplerCurrencyServiceSettingsMock.Object
            );

            var result = await service.GetCurrencyByCode();

            Assert.Empty(result);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeException_WhenUrlIsInvalid()
        {
            var loggerMock = new Mock<ILogger<DopplerCurrencyService>>();

            _dopplerCurrencyServiceSettingsMock.Setup(x => x.CurrentValue)
                .Returns(new DopplerCurrencyServiceSettings
                {
                    CurrencyCodeList = new List<string> { "ARS" }
                });

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                _dopplerCurrencyServiceSettingsMock.Object,
                loggerMock.Object
            );

            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.GetCurrencyByCode());

            loggerMock.VerifyLogger(LogLevel.Error, "Error GetCurrency for ARS.", Times.Once());
            Assert.Equal("Invalid URI: The format of the URI could not be determined.", exception.Message);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeException_WhenJsonContentIsInvalid()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("test")
                });
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var loggerMock = new Mock<ILogger<DopplerCurrencyService>>();

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                new HttpClientPoliciesSettings
                {
                    ClientName = "test"
                },
                _dopplerCurrencyServiceSettingsMock.Object,
                loggerMock.Object
            );

            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.GetCurrencyByCode());

            Assert.Equal("Error parsing boolean value. Path '', line 1, position 1.", exception.Message);
        }
    }
}