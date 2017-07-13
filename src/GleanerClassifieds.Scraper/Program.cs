using Akka.Actor;
using Akka.Routing;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
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
                var pageSize = ConfigurationManager.AppSettings["PageSize"];
                var categoryId = 12518;

                var listPageUrl = $"{baseUrl}&category_id={categoryId}&page_size={pageSize}";
                var listPageScraper = system.ActorOf(ListPageScraper.Props(listPageUrl, categoryId), "list-page");

                var result = listPageScraper.Ask(new ListPageScraper.ScrapeListPage()).Result;

                Console.WriteLine("Actor System Shutting Down");
                Console.ReadKey();
            }
        }
    }
}
