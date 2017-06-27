using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Oglaf
{
    static class OglafGrabber
    {

        public static (bool doSend, string alt, string title, string scr) GetOglafPicture()
        {
            //HtmlDocument doc = new HtmlDocument();
            //doc.Load(yourStream);
            WebClient client = new WebClient();
            string html = client.DownloadString("http://www.oglaf.com");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && node.Attributes["id"]?.Value == "strip"
                          //&& node.Attributes["class"].Value.StartsWith("img_")
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            

            return (
                NotSent(),
                attrs["alt"]?.Value,
                attrs["title"]?.Value,
                attrs["src"]?.Value
                );
            //using (WebClient client = new WebClient())
            //{
            //    client.  //DownloadFile("http://www.example.com/image.jpg", localFilename);
            //}
        }

        private static bool NotSent()
        {

            return true;
        }
    }
}
