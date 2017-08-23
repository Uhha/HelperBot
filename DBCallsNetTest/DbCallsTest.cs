using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DatabaseInteractions;
using System.Data.Entity.Core;
using System.Linq;

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
            Assert.AreEqual("XKCD", value[1]);
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

            var wrds = DB.GetTable<WordLookup>(@"SELECT distinct
                                                w.word as WordText,
                                                STUFF((select '\n' + l2.usage from Lookups l2 
                                                        where l2.WordID = w.wordid for xml path(''), TYPE).value('.', 'varchar(max)'), 1, 2, '') as Lookup
                                                FROM
                                                WORDS w
                                                LEFT JOIN LOOKUPS l
                                                on l.wordID = w.WordID
                                                LEFT JOIN BOOKINFO b
                                                on b.guid = l.bookkey");


        }

        public class TestResult
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class WordLookup
        {
            public string WordText { get; set; }
            public string Lookup { get; set; }
        }

        [TestMethod]
        public void SimpleTest()
        {
            var worddef = "/definition=Theword";
            var callback = "/vocabNewWord";
            var callbackPrefix = worddef.Substring(worddef.IndexOf('=') + 1);
            var callbackPrefix2 = callback.Substring(callback.IndexOf('=') + 1);
            var wordsp = "/define someword";
            var callbackPrefix3 = wordsp.Substring(0, wordsp.IndexOf(' '));

        }
    }
}
