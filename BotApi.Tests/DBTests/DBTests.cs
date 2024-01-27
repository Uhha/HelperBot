using BotApi.Database;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BotApi.Tests
{
    [TestClass]
    public class DBTests
    {
        private readonly string tempFilePath = Path.GetTempFileName();
        private string jsonFilePath;
        private long testClientChatId = 1231255L;

        [TestInitialize] public void TestInitialize()
        {

            FieldInfo clientsField = typeof(DB).GetField("JSON_PATH", BindingFlags.NonPublic | BindingFlags.Static);
            var jsonPathValue = (string)clientsField.GetValue(null);

            jsonFilePath = jsonPathValue;
            File.WriteAllText(tempFilePath, "");
        }

        [TestMethod]
        public void AddClient_ShouldAddClient()
        {
            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);
            db.AddClient(testClientChatId);

            Assert.IsNotNull(db.GetClient(testClientChatId));
        }

        [TestMethod]
        public void RemoveClient_ShouldRemoveClient()
        {
            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);

            db.AddClient(testClientChatId);
            db.RemoveClient(testClientChatId);

            Assert.IsNull(db.GetClient(testClientChatId));
        }

        [TestMethod]
        public void AddSubscription_ShouldAddSubscription()
        {
            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);

            db.AddClient(testClientChatId);
            db.AddSubscription(testClientChatId, SubscriptionType.Oglaf);

            var subscription = db.GetSubscription(testClientChatId, SubscriptionType.Oglaf);

            Assert.IsNotNull(subscription);
            Assert.AreEqual(SubscriptionType.Oglaf, subscription.Type);
        }

        [TestMethod]
        public void RemoveSubscription_ShouldRemoveSubscription()
        {
            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);

            db.AddClient(testClientChatId);
            db.AddSubscription(testClientChatId, SubscriptionType.Oglaf);

            db.RemoveSubscription(testClientChatId, SubscriptionType.Oglaf);
            var subscription = db.GetSubscription(testClientChatId, SubscriptionType.Oglaf);

            Assert.IsNull(subscription);
        }

        [TestMethod]
        public void SaveClientsModel_ShouldSaveToFile()
        {
            var loggerMock = new Mock<ILogger<DB>>();
            var db = new DB(loggerMock.Object);

            db.AddClient(testClientChatId);

            // Use reflection or PrivateObject to access private method
            MethodInfo saveClientsModelMethod = typeof(DB).GetMethod("SaveClientsModel", BindingFlags.NonPublic | BindingFlags.Instance);
            saveClientsModelMethod.Invoke(db, new object[] { null });

            // Use reflection to access private field
            FieldInfo clientsField = typeof(DB).GetField("_clients", BindingFlags.NonPublic | BindingFlags.Instance);
            var clientsValue = (ClientsModel)clientsField.GetValue(db);

            var jsonContent = File.ReadAllText(jsonFilePath);
            var expectedJson = JsonSerializer.Serialize(clientsValue, new JsonSerializerOptions { WriteIndented = true });

            Assert.AreEqual(expectedJson, jsonContent);
        }
    }
}
