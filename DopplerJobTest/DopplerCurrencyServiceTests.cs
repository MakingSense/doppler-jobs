using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Doppler.Currency.Job.Authorization;
using Doppler.Currency.Job.DopplerCurrencyService;
using Doppler.Currency.Job.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Moq.Protected;
using Xunit;

namespace Doppler.Jobs.Test
{
    [ExcludeFromCodeCoverage]
    public class DopplerCurrencyServiceTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly Mock<IOptionsMonitor<DopplerCurrencyServiceSettings>> _dopplerCurrencyServiceSettingsMock;
        private readonly Mock<IOptions<JwtOptions>> _jwtOptionMock;
        private readonly SigningCredentials _signingCredentials;

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
                    CurrencyCodeList = new List<string> {"ARS"},
                    HolidayRetryCountLimit = 5
                });

            var sKey = new byte[32];
            var sRng = RandomNumberGenerator.Create();
            sRng.GetBytes(sKey);
            var securityKey = new SymmetricSecurityKey(sKey) { KeyId = Guid.NewGuid().ToString() };
            _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            _jwtOptionMock = new Mock<IOptions<JwtOptions>>();
            _jwtOptionMock.Setup(x => x.Value)
                .Returns(new JwtOptions
                {
                    TokenLifeTime = 1
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
                    Content = new StringContent(@"{'date':'2020-03-18','saleValue':65.0000,
                       'buyValue':'20.30','currencyName':'Peso Argentino', 'currencyCode':'ARS'}")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                _dopplerCurrencyServiceSettingsMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                signingCredentials: _signingCredentials);

            var result = await service.GetCurrencyByCode();

            Assert.NotEmpty(result);

            var currency = result[0];
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
                _dopplerCurrencyServiceSettingsMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                signingCredentials: _signingCredentials);

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
                _dopplerCurrencyServiceSettingsMock.Object,
                loggerMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                signingCredentials: _signingCredentials);

            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.GetCurrencyByCode());

            loggerMock.VerifyLogger(LogLevel.Error, "Unexpected error getting currency for ARS.", Times.Once());
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
                _dopplerCurrencyServiceSettingsMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                loggerCurrencyService:loggerMock.Object,
                signingCredentials: _signingCredentials);

            var exception = await Assert.ThrowsAnyAsync<Exception>(() => service.GetCurrencyByCode());

            Assert.Equal("Error parsing boolean value. Path '', line 1, position 1.", exception.Message);
        }

        [Fact]
        public async Task DopplerCurrencyService_ShouldBeEmptyList_WhenDateIsHolidayAndStatusCodeIsBadRequestAfterFiveRetries()
        {
            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"success\":false,\"errors\":{\"No USD for this date\":[\"There are no pending USD currency for that date.\"]}}")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                _dopplerCurrencyServiceSettingsMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                signingCredentials: _signingCredentials);

            var result = await service.GetCurrencyByCode();

            Assert.Empty(result);
            _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(6), ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task DopplerCurrencyService__ShouldBeHttpStatusCodeOk_WhenDateIsHolidayAndStatusCodeIsOkAfterRetry()
        {
            var failedUrlSegment = $"{DateTime.UtcNow.Year}-{DateTime.UtcNow.Month}-{DateTime.UtcNow.Day}";

            var previousDay = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday ? DateTime.UtcNow.AddDays(-3) : DateTime.UtcNow.AddDays(-1);
            var successfulUrlSegment = $"{previousDay.Year}-{previousDay.Month}-{previousDay.Day}";

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsolutePath.EndsWith(failedUrlSegment)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("{\"success\":false,\"errors\":{\"No USD for this date\":[\"There are no pending USD currency for that date.\"]}}")
                });

            _httpMessageHandlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.Is<HttpRequestMessage>(x => x.RequestUri.AbsolutePath.EndsWith(successfulUrlSegment)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(@"{'date':'2020-03-18','saleValue':65.0000,
                       'buyValue':'20.30','currencyName':'Peso Argentino', 'currencyCode':'ARS'}")
                });

            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>()))
                .Returns(_httpClient);

            var service = CreateSutCurrencyServiceTests.CreateSut(
                _httpClientFactoryMock.Object,
                _dopplerCurrencyServiceSettingsMock.Object,
                jwtOptions: _jwtOptionMock.Object,
                signingCredentials: _signingCredentials);

            var result = await service.GetCurrencyByCode();

            Assert.NotEmpty(result);

            var currency = result[0];
            Assert.Equal("2020-03-18", currency.Date);
            Assert.Equal(65.0000M, currency.SaleValue);
            Assert.Equal(20.30M, currency.BuyValue);
            Assert.Equal("Peso Argentino", currency.CurrencyName);
            Assert.Equal("ARS", currency.CurrencyCode);

            _httpMessageHandlerMock.Protected().Verify<Task<HttpResponseMessage>>("SendAsync", Times.Exactly(2), ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>());
        }
    }
}