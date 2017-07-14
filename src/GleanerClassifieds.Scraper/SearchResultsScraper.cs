using Akka.Actor;
using AngleSharp;
using CSharpVerbalExpressions;
using GleanerClassifieds.Scraper.Messages;
using System;
using System.Configuration;

namespace GleanerClassifieds.Scraper
{
    public class SearchResultsScraper: ReceiveActor
    {
        private double numPages = -1;
        private double numPagesComplete = 0;

        public SearchResultsScraper()
        {         
            Receive<Scrape>(message => Handle(message));
            Receive< TryMessage<Scrape>>(message => Handle(message));
            Receive<GetScrapeStatus>(message => Handle(message));
            Receive<ListPageScraper.ScrapeListPageComplete>(message => Handle(message));            
        }

        public void Handle(Scrape message)
        {
            Self.Tell(new TryMessage<Scrape>(message));
        }

        public void Handle(TryMessage<Scrape> message)
        {
            var msg = message.Message;

            // get the total number of results
            var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
            var pageSize = int.Parse(ConfigurationManager.AppSettings["PageSize"]);
            var url = $"{baseUrl}section_id={msg.SectionId}&category_id={msg.CategoryId}&keyword={msg.Keywords}&page_size={pageSize}&start_rec=";

            var config = AngleSharp.Configuration.Default.WithDefaultLoader();
            var document = BrowsingContext.New(config).OpenAsync(url).Result;

            var totalAdsDiv = document.QuerySelector("div.content > div > section > div");
            var totalNumAds = GetTotalNumAds(totalAdsDiv.TextContent);

            if (totalNumAds > 0)
            {
                numPages = Math.Ceiling((double)totalNumAds / pageSize);

                for (int startRecord = 0; startRecord < totalNumAds; startRecord += pageSize)
                {
                    var listPageUrl = url + startRecord.ToString();
                    var listPageScraper = Context.ActorOf(ListPageScraper.Props(listPageUrl), $"list-page-scraper-{startRecord}");
                    listPageScraper.Tell(new ListPageScraper.ScrapeListPage());
                }
            }
        }

        public void Handle(ListPageScraper.ScrapeListPageComplete msg)
        {
            numPagesComplete += 1;

            if(numPagesComplete == numPages) {
                Sender.Tell(new ScrapeStatus(true));
            }
        }

        public void Handle(GetScrapeStatus msg)
        {
            Sender.Tell(new ScrapeStatus(numPagesComplete == numPages));
        }

        protected override void PreRestart(Exception reason, object message)
        {
            var msg = message as TryMessage<Scrape>;
            if (msg != null)
            {
                if (msg.Retries <= 2)
                {
                    Context.System.Scheduler.ScheduleTellOnce(
                        500, Self, new TryMessage<Scrape>(msg.Message, msg.Retries + 1), Self);
                }
            }

            base.PreRestart(reason, message);
        }

        public static Props Props(int pageSize)
        {
            return Akka.Actor.Props.Create(() => new SearchResultsScraper());
        }

        private static int GetTotalNumAds(string input)
        {
            var regex = new VerbalExpressions()
                .Multiple(@"[A-Za-z\s]", false)
                .Multiple("[0-9]", false)
                .Then(" through ")
                .Multiple("[0-9]", false)
                .Then(" of ")
                .BeginCapture("TotalNumAds")
                .Multiple("[0-9]", false)
                .EndCapture()
                .Multiple(@"[A-Za-z\s]", false)
                .ToRegex();

            return int.TryParse(regex.Match(input)?.Groups["TotalNumAds"]?.Value, out var numAds) ?
                numAds : -1;
        }

        #region Messages

        public class Scrape
        {
            public Scrape(int? categoryId = null, int? sectionId = null, string keywords = null)
            {
                CategoryId = categoryId;
                SectionId = sectionId;
                Keywords = keywords;
            }

            public int? CategoryId { get; private set; }            

            public int? SectionId { get; private set; }
           
            public string Keywords { get; private set; }
        }

        public class ScrapeStatus
        {
            public ScrapeStatus(bool isComplete)
            {
                IsComplete = isComplete;
            }

            public bool IsComplete { get; private set; }
        }

        public class GetScrapeStatus
        {

        }

        #endregion Messages
    }
}
