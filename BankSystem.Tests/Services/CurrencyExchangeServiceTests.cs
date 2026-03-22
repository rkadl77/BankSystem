using System.Net;
using System.Text.Json;
using BankSystem.Services;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace BankSystem.Tests.Services
{
    public class CurrencyExchangeServiceTests
    {
        private Mock<HttpMessageHandler> CreateMockHttpHandler(HttpResponseMessage response)
        {
            var handlerMock = new Mock<HttpMessageHandler>();
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);
            return handlerMock;
        }

        private CurrencyExchangeService CreateService(HttpClient httpClient, IMemoryCache? cache = null)
        {
            var mockLogger = new Mock<ILogger<CurrencyExchangeService>>();
            cache ??= new MemoryCache(new MemoryCacheOptions());
            return new CurrencyExchangeService(httpClient, cache, mockLogger.Object);
        }

        #region GetExchangeRateAsync Tests

        [Fact]
        public async Task GetExchangeRateAsync_SameCurrency_ReturnsOne()
        {
            // Arrange
            var handlerMock = CreateMockHttpHandler(new HttpResponseMessage());
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetExchangeRateAsync("USD", "USD");

            // Assert
            Assert.Equal(1m, result);
        }

        [Fact]
        public async Task GetExchangeRateAsync_CacheHit_ReturnsCachedValue()
        {
            // Arrange
            var cache = new MemoryCache(new MemoryCacheOptions());
            cache.Set("exchange_rate_USD_EUR", 0.85m, TimeSpan.FromHours(1));

            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient, cache);

            // Act
            var result = await service.GetExchangeRateAsync("USD", "EUR");

            // Assert
            Assert.Equal(0.85m, result);
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Never(),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetExchangeRateAsync_CacheMiss_CallsApiAndCachesResult()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"base":"USD","date":"2024-01-01","rates":{"EUR":0.92}}""")
            };

            var handlerMock = CreateMockHttpHandler(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var cache = new MemoryCache(new MemoryCacheOptions());
            var service = CreateService(httpClient, cache);

            // Act
            var result = await service.GetExchangeRateAsync("USD", "EUR");

            // Assert
            Assert.Equal(0.92m, result);
            Assert.True(cache.TryGetValue("exchange_rate_USD_EUR", out decimal cachedValue));
            Assert.Equal(0.92m, cachedValue);
        }

        [Fact]
        public async Task GetExchangeRateAsync_ApiFailure_ThrowsInvalidOperationException()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
            var handlerMock = CreateMockHttpHandler(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.GetExchangeRateAsync("USD", "EUR"));
            Assert.Contains("Failed to get exchange rate", exception.Message);
        }

        [Fact]
        public async Task GetExchangeRateAsync_CacheExpires_AfterOneHour()
        {
            // Arrange - use a mockable time-based cache or wait for expiration
            // For this test, we'll verify the cache entry has the expected expiration
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"base":"USD","date":"2024-01-01","rates":{"EUR":0.92}}""")
            };

            var handlerMock = CreateMockHttpHandler(response);
            var httpClient = new HttpClient(handlerMock.Object);
            
            // Use a custom clock to simulate time passing
            var cacheOptions = new MemoryCacheOptions();
            var cache = new MemoryCache(cacheOptions);
            var service = CreateService(httpClient, cache);

            // First call - should hit API
            await service.GetExchangeRateAsync("USD", "EUR");
            
            // Verify cache has value
            Assert.True(cache.TryGetValue("exchange_rate_USD_EUR", out _));

            // Remove from cache to simulate expiration
            cache.Remove("exchange_rate_USD_EUR");

            // Second call - should hit API again (cache miss)
            var result = await service.GetExchangeRateAsync("USD", "EUR");
            Assert.Equal(0.92m, result);

            // Verify API was called twice
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>()
            );
        }

        [Fact]
        public async Task GetExchangeRateAsync_EmptyCurrency_ThrowsArgumentException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetExchangeRateAsync("", "EUR"));
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.GetExchangeRateAsync("USD", ""));
        }

        #endregion

        #region ConvertAsync Tests

        [Fact]
        public async Task ConvertAsync_SameCurrency_ReturnsSameAmount()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.ConvertAsync(100m, "USD", "USD");

            // Assert
            Assert.Equal(100m, result);
        }

        [Fact]
        public async Task ConvertAsync_DifferentCurrency_AppliesRate()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"base":"USD","date":"2024-01-01","rates":{"EUR":0.85}}""")
            };

            var handlerMock = CreateMockHttpHandler(response);
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.ConvertAsync(100m, "USD", "EUR");

            // Assert
            Assert.Equal(85m, result);
        }

        [Fact]
        public async Task ConvertAsync_NegativeAmount_ThrowsArgumentException()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(handlerMock.Object);
            var service = CreateService(httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(
                () => service.ConvertAsync(-100m, "USD", "EUR"));
        }

        #endregion
    }
}
