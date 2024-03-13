using BotApi.Database;
using BotApi.Services;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;
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
            var httpClient = new HttpClient();
            hcp.Setup(factory => factory.CreateClient(It.IsAny<string>())).Returns(httpClient);

            var ms = new SecuritiesService(logger, hcp.Object, db);
			var result = await ms.GetPricesAsync(1233);
			Assert.IsNotNull(result);
		}

	
	}
}