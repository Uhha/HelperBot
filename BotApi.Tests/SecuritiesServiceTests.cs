using BotApi.Database;
using BotApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace BotApi.Tests
{
	[TestClass]
	public class SecuritiesServiceTests
    {
		[TestMethod]
		public async Task GetSecuritiesTest()
		{
			var mock = new Mock<ILogger<SecuritiesService>>();
			ILogger<SecuritiesService> logger = mock.Object;

            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);


            var hcp = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(new HttpClientHandler());
            hcp.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var config = new APIConfig { AlphaVintageAPIKey = "test_key" }; 
            IOptions<APIConfig> options = Options.Create(config);

            var ms = new SecuritiesService(logger, hcp.Object, db, options);
			var result = await ms.GetPricesAsync(123444321);
			Assert.IsNotNull(result);
		}

        [TestMethod]
        public async Task AddSecuritiesTest()
        {
            var mock = new Mock<ILogger<SecuritiesService>>();
            ILogger<SecuritiesService> logger = mock.Object;

            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);


            var hcp = new Mock<IHttpClientFactory>();
            var httpClient = new HttpClient(new HttpClientHandler());
            hcp.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var config = new APIConfig { AlphaVintageAPIKey = "test_key" };
            IOptions<APIConfig> options = Options.Create(config);

            var ms = new SecuritiesService(logger, hcp.Object, db, options);
            var result = ms.AddSecurity(123444321, "APPL");
            Assert.IsNotNull(result);
        }
    }
}