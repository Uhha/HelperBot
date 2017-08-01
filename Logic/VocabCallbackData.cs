﻿using DatabaseInteractions;
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
        private static WordLookup[] _words;
        private static int _index;
        public static string Message
        {
            get
            {
                var lp = Word;
                return lp.WordText.Bold() + Environment.NewLine + lp.Lookup.Italic();
            }
        }
        public static WordLookup Word
        {
            get
            {
                if (_words == null) SetWords();
                //if (_index < _words.Length) { return _words[_index]; } else { return _words[_index = 0]; }
                return _words[_index];
            }
            set { }
        }

        public static void PrepareNextWord()
        {
            if (_index < _words.Length) { _index++; } else { _index = 0; }
        }

        //public static string CurrentWord
        //{
        //    get
        //    {
        //        if (index == 0) { return _words[_words.Length - 1].WordText; }
        //        return _words[index - 1].WordText;
        //    }
        //}

        private static void SetWords()
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                //IQueryable<(string, string)> wrds = from p in db.Words
                                        //  select ValueTuple.Create(p.Stem, p.Stem);
                var wrds = DB.GetTable<WordLookup>(@"SELECT distinct
                                                w.word as WordText,
                                                STUFF((select ' ' + l2.usage from Lookups l2 
                                                        where l2.WordID = w.wordid for xml path(''), TYPE).value('.', 'varchar(max)'), 1, 1, '') as Lookup
                                                FROM
                                                WORDS w
                                                LEFT JOIN LOOKUPS l
                                                on l.wordID = w.WordID
                                                LEFT JOIN BOOKINFO b
                                                on b.guid = l.bookkey");

                Random rnd = new Random();
                _words = wrds.ToArray().OrderBy(x => rnd.Next()).ToArray();
            }
        }

        public static string GetDefinition(string word)
        {
            string url = "***REMOVED***";
            string appid = "***REMOVED***";
            string appkey = "***REMOVED***";
            string sourcelang = "en";
            //string wordid;
            //if (_words != null && _index != 0) { wordid = _words[_index - 1].WordText; } else { return "No word have chosen"; };
            string urlParameters = sourcelang + "/" + word;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);

            // Add an Accept header for JSON format.
            client.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("app_id", appid);
            client.DefaultRequestHeaders.Add("app_key", appkey);

            // List data response.
            HttpResponseMessage response = client.GetAsync(urlParameters).Result;  // Blocking call!
            if (response.IsSuccessStatusCode)
            {
                // Parse the response body. Blocking!
                var dataObjects = response.Content.ReadAsAsync<Rootobject>().Result;

                StringBuilder sb = new StringBuilder();

                //Word Itself
                sb.Append(word.FirstCap().Bold() + Environment.NewLine);
                //Phonetic Spelling
                sb.Append($"[{dataObjects.results[0]?.lexicalEntries[0]?.pronunciations[0]?.phoneticSpelling}]{Environment.NewLine}");
                foreach (var lentry in dataObjects.results[0].lexicalEntries)
                {
                    if (Array.IndexOf(dataObjects.results[0].lexicalEntries, lentry) != 0)
                    {
                        sb.Append("___________" + Environment.NewLine);
                    }
                    sb.Append(lentry.lexicalCategory + Environment.NewLine);
                    foreach (var entry in lentry.entries)
                    {
                        foreach (var sense in entry.senses)
                        {
                            if (Array.IndexOf(entry.senses, sense) != 0)
                            {
                                sb.Append("..........." + Environment.NewLine); 
                            }

                            for (int i = 0; i < sense.definitions.Length; i++)
                            {
                                sb.Append(sense.definitions[i].FirstCap() + Environment.NewLine);
                                if (sense.examples != null && i < sense.examples.Length)
                                {
                                    sb.Append(sense.examples[i].text.FirstCap().Italic());
                                    if (sense.domains != null && i < sense.domains.Length)
                                    {
                                        sb.Append($" [{sense.domains[i].FirstCap().Italic()}]" + Environment.NewLine);
                                    }
                                    else
                                    {
                                        sb.Append(Environment.NewLine);
                                    }
                                }
                                

                            }

                        }
                    }
                }
                if (sb.Length > 4000) sb.Remove(4000, sb.Length);
                var translation = TranslateText(word, "en|ru");
                sb.Append("rus: " + translation.FirstCap() + Environment.NewLine);
                return sb.ToString();
                
            }
            
            return "No english definition found" + Environment.NewLine + TranslateText(word, "en|ru"); ;
        }

        private static string TranslateText(string input, string languagePair)
        {
            try
            {
                string url = String.Format("http://www.google.com/translate_t?hl=en&ie=UTF8&text={0}&langpair={1}", input, languagePair);
                WebClient webClient = new WebClient();
                webClient.Encoding = Encoding.GetEncoding("windows-1251");
                string result = webClient.DownloadString(url);
                result = result.Substring(result.IndexOf("<span title=\"") + "<span title=\"".Length);
                result = result.Substring(result.IndexOf(">") + 1);
                result = result.Substring(0, result.IndexOf("</span>"));
                return result.Trim();
            }
            catch (Exception)
            {
                return "";
            }

        }

        public class WordLookup
        {
            public string WordText { get; set; }
            public string Lookup { get; set; }
        }

    }


    public static class StringExtensions
    {
        public static string FirstCap(this string value)
        {
            if (String.IsNullOrEmpty(value))
                return "";
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
#region OxfordDic return object

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
#endregion
}
