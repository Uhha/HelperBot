using DatabaseInteractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Logic
{
    public static class VocabCallbackData
    {
        private static string[] _words;
        private static int index;
        public static string Word
        {
            get
            {
                if (_words == null) SetWords();
                if (index < _words.Length) { return _words[index++]; } else { return _words[index = 0]; }
            }
            set { }
        }

        private static void SetWords()
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                IQueryable<string> wrds = from p in db.Words
                                          select p.Stem;
                Random rnd = new Random();
                _words = wrds.ToArray().OrderBy(x => rnd.Next()).ToArray();
            }
        }

        public static string GetDefinition()
        {
            string url = "***REMOVED***";
            string appid = "***REMOVED***";
            string appkey = "***REMOVED***";
            string sourcelang = "en";
            string wordid;
            if (_words != null && index != 0) { wordid = _words[index - 1]; } else { return "No word have chosen"; };
            string urlParameters = sourcelang + "/" + wordid;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("app_id", appid);
            client.DefaultRequestHeaders.Add("app_key", appkey);

            // List data response.
            string ret = string.Empty;
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<Rootobject>().Result;

                string pronunciation = dataObjects.results[0].lexicalEntries[0].pronunciations[0]?.phoneticSpelling;
                string lexCategory = dataObjects.results[0].lexicalEntries[0].lexicalCategory;
                string definition = dataObjects.results[0].lexicalEntries[0].entries[0].senses[0].definitions[0];
                string example = string.Empty;
                if (dataObjects.results[0].lexicalEntries[0].entries[0].senses[0]?.examples != null)
                {
                    example = dataObjects.results[0].lexicalEntries[0].entries[0].senses[0]?.examples[0]?.text;
                }
                return ret = $"{wordid.FirstCap().Bold()}{Environment.NewLine}" + 
                    $"[{pronunciation}]{Environment.NewLine}" +
                    $"{lexCategory}{Environment.NewLine}" +
                    $"{definition.FirstCap()}{Environment.NewLine}" +
                    $"{example.FirstCap().Italic()}";


                
            }
            

            return "No definition found";
        }
       


    }


    public static class StringExtensions
    {
        public static string FirstCap(this string value)
        {
            if (String.IsNullOrEmpty(value))
                throw new ArgumentException("Can't capitalize null string!");
            return value.First().ToString().ToUpper() + value.Substring(1);
        }
        public static string Bold(this string value)
        {
            return "<b>" + value + "</b>";
        }
        public static string Italic(this string value)
        {
            return "<i>" + value + "</i>";
        }

    }

    public class Rootobject
    {
        public Metadata metadata { get; set; }
        public Result[] results { get; set; }
    }

    public class Metadata
    {
    }

    public class Result
    {
        public string id { get; set; }
        public string language { get; set; }
        public Lexicalentry[] lexicalEntries { get; set; }
        public Pronunciation3[] pronunciations { get; set; }
        public string type { get; set; }
        public string word { get; set; }
    }

    public class Lexicalentry
    {
        public Derivativeof[] derivativeOf { get; set; }
        public Entry[] entries { get; set; }
        public Grammaticalfeature3[] grammaticalFeatures { get; set; }
        public string language { get; set; }
        public string lexicalCategory { get; set; }
        public Note5[] notes { get; set; }
        public Pronunciation2[] pronunciations { get; set; }
        public string text { get; set; }
        public Variantform2[] variantForms { get; set; }
    }

    public class Derivativeof
    {
        public string[] domains { get; set; }
        public string id { get; set; }
        public string language { get; set; }
        public string[] regions { get; set; }
        public string[] registers { get; set; }
        public string text { get; set; }
    }

    public class Entry
    {
        public string[] etymologies { get; set; }
        public Grammaticalfeature[] grammaticalFeatures { get; set; }
        public string homographNumber { get; set; }
        public Note[] notes { get; set; }
        public Pronunciation[] pronunciations { get; set; }
        public Sens[] senses { get; set; }
        public Variantform1[] variantForms { get; set; }
    }

    public class Grammaticalfeature
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Note
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Pronunciation
    {
        public string audioFile { get; set; }
        public string[] dialects { get; set; }
        public string phoneticNotation { get; set; }
        public string phoneticSpelling { get; set; }
        public string[] regions { get; set; }
    }

    public class Sens
    {
        public string[] crossReferenceMarkers { get; set; }
        public Crossreference[] crossReferences { get; set; }
        public string[] definitions { get; set; }
        public string[] domains { get; set; }
        public Example[] examples { get; set; }
        public string id { get; set; }
        public Note3[] notes { get; set; }
        public Pronunciation1[] pronunciations { get; set; }
        public string[] regions { get; set; }
        public string[] registers { get; set; }
        public Subsens[] subsenses { get; set; }
        public Translation1[] translations { get; set; }
        public Variantform[] variantForms { get; set; }
    }

    public class Crossreference
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Example
    {
        public string[] definitions { get; set; }
        public string[] domains { get; set; }
        public Note1[] notes { get; set; }
        public string[] regions { get; set; }
        public string[] registers { get; set; }
        public string[] senseIds { get; set; }
        public string text { get; set; }
        public Translation[] translations { get; set; }
    }

    public class Note1
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Translation
    {
        public string[] domains { get; set; }
        public Grammaticalfeature1[] grammaticalFeatures { get; set; }
        public string language { get; set; }
        public Note2[] notes { get; set; }
        public string[] regions { get; set; }
        public string[] registers { get; set; }
        public string text { get; set; }
    }

    public class Grammaticalfeature1
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Note2
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Note3
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Pronunciation1
    {
        public string audioFile { get; set; }
        public string[] dialects { get; set; }
        public string phoneticNotation { get; set; }
        public string phoneticSpelling { get; set; }
        public string[] regions { get; set; }
    }

    public class Subsens
    {
    }

    public class Translation1
    {
        public string[] domains { get; set; }
        public Grammaticalfeature2[] grammaticalFeatures { get; set; }
        public string language { get; set; }
        public Note4[] notes { get; set; }
        public string[] regions { get; set; }
        public string[] registers { get; set; }
        public string text { get; set; }
    }

    public class Grammaticalfeature2
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Note4
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Variantform
    {
        public string[] regions { get; set; }
        public string text { get; set; }
    }

    public class Variantform1
    {
        public string[] regions { get; set; }
        public string text { get; set; }
    }

    public class Grammaticalfeature3
    {
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Note5
    {
        public string id { get; set; }
        public string text { get; set; }
        public string type { get; set; }
    }

    public class Pronunciation2
    {
        public string audioFile { get; set; }
        public string[] dialects { get; set; }
        public string phoneticNotation { get; set; }
        public string phoneticSpelling { get; set; }
        public string[] regions { get; set; }
    }

    public class Variantform2
    {
        public string[] regions { get; set; }
        public string text { get; set; }
    }

    public class Pronunciation3
    {
        public string audioFile { get; set; }
        public string[] dialects { get; set; }
        public string phoneticNotation { get; set; }
        public string phoneticSpelling { get; set; }
        public string[] regions { get; set; }
    }

}
