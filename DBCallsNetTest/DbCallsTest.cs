using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseInteractions;
using System.Data.Entity.Core;

namespace DBCallsNetTest
{
    [TestClass]
    public class TestDBCalls
    {
        [TestMethod]
        public void SingleValue()
        {
            var value = DB.GetValue<int>("select top 1 Id from enums");
            Assert.AreEqual(1, value);

            var value2 = DB.GetValue<string>("select top 1 Name from enums");
            Assert.AreEqual("Oglaf", value2);

            var value3 = DB.GetValue<string>("select Name from enums where id = 1000");
            Assert.AreEqual(null, value3);


            //T val = default(T); // TODO: Initialize to an appropriate value  
            //MyLinkedList<T> target = new MyLinkedList<T>(val); // TODO: Initialize to an appropriate value  
            //int expected = 0; // TODO: Initialize to an appropriate value  
            //int actual;
            //actual = target.SizeOfLinkedList();
            //Assert.AreEqual(expected, actual);
            //Assert.Inconclusive("Verify the correctness of this test method.");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void SingleValueExceptions()
        {
            try
            {
                var value4 = DB.GetValue<string>("select Name from enums");
            }
            catch (ArgumentOutOfRangeException)
            {
                throw;
            }
        }

        [TestMethod]
        public void GetMultipleValues()
        {
            var value = DB.GetList<string>("select Name from enums");
            Assert.AreEqual("Oglaf", value[0]);
            Assert.AreEqual("Oglaf", value[1]);
        }

        [TestMethod]
        [ExpectedException(typeof(EntityCommandExecutionException))]
        public void GetMultipleValuesException()
        {
            var value = DB.GetList<string>("select id, Name from enums");
        }


        [TestMethod]
        public void GetTableValues()
        {
            var value = DB.GetTable<TestResult>("select id, Name from enums");
        }

        public class TestResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
