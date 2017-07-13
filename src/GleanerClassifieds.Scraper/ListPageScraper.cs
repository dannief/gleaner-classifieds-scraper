using Akka.Actor;
using Akka.Routing;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Globalization;
using System.Threading.Tasks;

namespace GleanerClassifieds.Scraper
{
    public class ListPageScraper: ReceiveActor
    {       
        private readonly IDocument document;       
        private readonly int categoryId;

        private IActorRef adPersister;
        private int numItems;
        private List<int> savedAdIds = new List<int>();
        
        public ListPageScraper(string url, int categoryId)
        {           
            this.categoryId = categoryId;

            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            document = BrowsingContext.New(config).OpenAsync(url).Result;

            Receive<ScrapeListPage>(message => Handle(message));
            Receive<AdPersister.AdSaved>(message => Handle(message));
        }

        public static Props Props(string url, int categoryId)
        {
            return Akka.Actor.Props.Create(() => new ListPageScraper(url, categoryId));
        }

        public void Handle(ScrapeListPage message)
        {            
            var tableRows = document.QuerySelectorAll("div.bcontent section section table tr:nth-child(3n+0)");
            numItems = tableRows.Length;
            for (int i = 0; i < numItems; i++)
            {               
                var tableRow = tableRows[i] as IHtmlTableRowElement;
                var adLink = tableRow.QuerySelector("td:nth-child(3) a") as IHtmlAnchorElement;
                var adListedOnTd = tableRow.QuerySelector("td:nth-child(4)");

                var title = adLink.Text.Replace("...", string.Empty).Replace("\n\t", string.Empty);
                var url = adLink.Href;
                var listedOn = 
                    DateTime.ParseExact(
                        adListedOnTd.TextContent, 
                        ConfigurationManager.AppSettings["DateFormat"], 
                        CultureInfo.InvariantCulture);
                                
                var (expiresOn, description) = ScrapeDetailPage(url);

                var saveAdMsg = new AdPersister.SaveAd(categoryId, title, description, listedOn, expiresOn);

                adPersister.Tell(saveAdMsg);
            }            
        }

        public void Handle(AdPersister.AdSaved msg)
        {
            savedAdIds.Add(msg.AdId);

            if (savedAdIds.Count == numItems)
            {
                Sender.Tell(new ScrapeListPageComplete());
            }
        }

        protected override void PreStart()
        {
            adPersister = Context.ActorOf(AdPersister.Props(), "ad-perister");            
        }

        private (DateTime, string) ScrapeDetailPage(string url)
        {
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var document = BrowsingContext.New(config).OpenAsync(url).Result;

            var dateExpiresTd = document.QuerySelector("#order-info tr:nth-child(2) td:nth-child(2)");
            var dateExpires =
                 DateTime.ParseExact(
                        dateExpiresTd.TextContent,
                        ConfigurationManager.AppSettings["DateFormat"],
                        CultureInfo.InvariantCulture);

            var description = document.QuerySelector("article div.adcontent").OuterHtml;

            return (dateExpires, description);
        }

        #region Messages

        public class ScrapeListPage
        {
        }

        public class ScrapeListPageComplete
        {
        }

        #endregion Messages
    }
}
