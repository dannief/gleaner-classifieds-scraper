using Akka.Actor;
using Akka.Routing;
using AngleSharp;
using AngleSharp.Dom.Html;
using CSharpVerbalExpressions;
using GleanerClassifieds.Data.Model;
using GleanerClassifieds.Scraper.Messages;
using System;
using System.Collections.Immutable;
using System.Configuration;
using System.Globalization;
using System.Linq;

namespace GleanerClassifieds.Scraper
{
    public class ListPageScraper: ReceiveActor
    {
        private readonly string url;
                
        private IActorRef adDataStore;
        private int numItems = -1;
        private int savedAds = 0;
        private ImmutableList<Category> Categories;
        
        public ListPageScraper(string url)
        {
            this.url = url;
                        
            Receive<ScrapeListPage>(message => Handle(message));
            Receive<TryMessage<ScrapeListPage>>(message => Handle(message));
            Receive<AdDataStore.AdSaved>(message => Handle(message));
        }

        public static Props Props(string url)
        {
            return Akka.Actor.Props.Create(() => new ListPageScraper(url));
        }

        public void Handle(ScrapeListPage message)
        {
            Self.Tell(new TryMessage<ScrapeListPage>(message));          
        }

        public void Handle(TryMessage<ScrapeListPage> message)
        {
            // Get result rows
            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var document = BrowsingContext.New(config).OpenAsync(url).Result;

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
                                
                var (expiresOn, description, categoryId, adId) = ScrapeDetailPage(url);

                var saveAdMsg = new AdDataStore.SaveAd(adId, categoryId, title, description, listedOn, expiresOn);

                adDataStore.Tell(saveAdMsg);
            }            
        }

        public void Handle(AdDataStore.AdSaved msg)
        {
            savedAds += 1;

            if (savedAds == numItems)
            {
                Sender.Tell(new ScrapeListPageComplete());
            }
        }

        protected override void PreStart()
        {
            adDataStore = Context.ActorOf(AdDataStore.Props().WithRouter(new RoundRobinPool(4)), "data-store");

            // TODO: Tell instead of Ask. 
            // Use Become to handle ScrapeListPage after GetCategories result.
            // Stash ScrapeListPage messages until Become
            var result = adDataStore.Ask(new AdDataStore.GetCategories()).Result as AdDataStore.GetCategoriesResult;
            Categories = result.Categories;
        }

        protected override void PreRestart(Exception reason, object message)
        {
            var msg = message as TryMessage<ScrapeListPage>;
            if (msg != null)
            {
                if(msg.Retries <= 2)
                {
                    Context.System.Scheduler.ScheduleTellOnce(
                        500, Self, new TryMessage<ScrapeListPage>(msg.Message, msg.Retries + 1), Self);
                }
            }

            base.PreRestart(reason, message);
        }

        private (DateTime expiresOn, string description, int categoryId, int adId) ScrapeDetailPage(string url)
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

            var titleH2 = document.QuerySelector("#ad-title h2");
            var categoryName = titleH2.TextContent.Split(':')[1].Trim();

                        
            int? categoryId = Categories.SingleOrDefault(c => c.Name.ToLower() == categoryName.ToLower())?.Id;
            int adId = int.Parse(
                new VerbalExpressions()
                .Then("ad_id/")
                .BeginCapture("id")
                .Multiple("[0-9]", false)
                .EndCapture()
                .ToRegex()
                .Match(url)
                .Groups["id"].Value);

            return (dateExpires, description, categoryId.Value, adId);
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
