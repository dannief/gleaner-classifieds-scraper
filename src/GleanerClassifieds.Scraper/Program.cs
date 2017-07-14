using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GleanerClassifieds.Scraper
{
    public class Program
    {
        static void Main(string[] args)
        {
            using (var system = ActorSystem.Create("gleaner-classifieds-scraper"))
            {                                
                var baseUrl = ConfigurationManager.AppSettings["BaseUrl"];
                var pageSize = int.Parse(ConfigurationManager.AppSettings["PageSize"]);
                var categoryId = 12518;
                
                var searchResultScraper = system.ActorOf(SearchResultsScraper.Props(pageSize), "search-results-scraper");

                searchResultScraper.Tell(new SearchResultsScraper.Scrape(categoryId: categoryId));

                SearchResultsScraper.ScrapeStatus scrapeStatus = new SearchResultsScraper.ScrapeStatus(false);
                do
                {
                    Task.Delay(5000).Wait();                    
                    scrapeStatus =
                        searchResultScraper.Ask(
                            new SearchResultsScraper.GetScrapeStatus()).Result as SearchResultsScraper.ScrapeStatus;
                }
                while (!scrapeStatus.IsComplete);

                Console.WriteLine("Scraping Complete!");

                Console.ReadKey();
            }
        }
    }
}
