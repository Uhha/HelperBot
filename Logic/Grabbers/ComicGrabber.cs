using DatabaseInteractions;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Logic.Grabbers
{
    static class ComicGrabber
    {

        public static (bool doSend, string alt, string title, string scr) GetOglafPicture(int client)
        {
            WebClient webclient = new WebClient();
            string html = webclient.DownloadString("http://www.oglaf.com");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && node.Attributes["id"]?.Value == "strip"
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return (
                NotSent(client, attrs["alt"]?.Value, Subscription.Oglaf),
                attrs["alt"]?.Value,
                attrs["title"]?.Value,
                attrs["src"]?.Value
                );
        }

        public static (bool doSend, string alt, string title, string scr) GetXKCDPicture(int client)
        {
            WebClient webclient = new WebClient();
            string html = webclient.DownloadString("https://xkcd.com/");


            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            List<HtmlNode> imageNodes = null;
            imageNodes = (from HtmlNode node in doc.DocumentNode.SelectNodes("//img")
                          where node.Name == "img"
                          && !string.IsNullOrEmpty(node.Attributes["title"]?.Value)
                          && node.Attributes["src"].Value.Contains("comics")
                          select node).ToList();

            var attrs = imageNodes[0]?.Attributes;

            return (
                NotSent(client, attrs["alt"]?.Value, Subscription.XKCD),
                attrs["alt"]?.Value,
                attrs["title"]?.Value,
                attrs["src"]?.Value.Substring(2)
                );
        }

        private static bool NotSent(int client, string alt, Subscription subscription)
        {
            using (AlcoDBEntities db = new AlcoDBEntities())
            {
                var lastPostedKey = (from cli in db.Clients
                           join sub in db.Subscriptions on cli.Subscription equals sub.Id
                           where cli.ChatID == client && sub.SubsctiptionType == (int)subscription
                           orderby sub.Id descending
                           select new
                           {
                               LTK = sub.LastPostedKey,
                               SUBID = sub.Id
                           }
                           ).First();

                if (alt.GetHashCode().ToString().Equals(lastPostedKey.LTK)) return false;
                db.Subscriptions.Where(x => x.Id == lastPostedKey.SUBID).First().LastPostedKey = alt.GetHashCode().ToString();
                db.SaveChanges();
            }
            return true;
        }
    }
}
