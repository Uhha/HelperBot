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

            var mockDb = new Mock<IDB>();
            IDB db = mockDb.Object;

            var ms = new SecuritiesService(logger, new HttpClient(), db);
			var result = await ms.GetPricesAsync(1233);
			Assert.IsNotNull(result);
		}

	
	}
}