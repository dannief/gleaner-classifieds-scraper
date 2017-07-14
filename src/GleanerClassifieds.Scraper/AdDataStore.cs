using Akka.Actor;
using GleanerClassifieds.Data;
using GleanerClassifieds.Data.Model;
using System;
using System.Collections.Immutable;
using System.Linq;

namespace GleanerClassifieds.Scraper
{
    public class AdDataStore: ReceiveActor
    {

        private ImmutableList<Category> Categories;

        public AdDataStore()
        {          
            Receive<SaveAd>(message => Handle(message));
            Receive<GetCategories>(message => Handle(message));
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new AdDataStore());
        }

        public void Handle(SaveAd msg)
        {
            using (var db = new GleanerClassifiedsDbContext())
            {
                // If ad does not already exist ...
                if (!db.Ads.Any(x => x.Id == msg.AdId))
                {
                    // save ad to db
                    var ad = new Ad
                    {
                        Id = msg.AdId,
                        Title = msg.Title,
                        Description = msg.Description,
                        CategoryId = msg.CategoryId,
                        ListedOn = msg.ListedOn,
                        ExpiresOn = msg.ExpiresOn
                    };

                    db.Ads.Add(ad);

                    db.SaveChanges();                    
                }
            }

            Sender.Tell(new AdSaved(msg.AdId));
        }

        public void Handle(GetCategories msg)
        {
            using (var db = new GleanerClassifiedsDbContext())
            {
                if (Categories == null)
                {
                    Categories = db.Categories.ToImmutableList();
                }

                Sender.Tell(new GetCategoriesResult(Categories));
            }
        }

        #region Messages

        public class SaveAd
        {
            public SaveAd(
                int adId,
                int categoryId,
                string title,
                string description,
                DateTime listedOn,
                DateTime expiredOn)
            {
                AdId = adId;
                CategoryId = categoryId;
                Title = title;
                Description = description;
                ListedOn = listedOn;
                ExpiresOn = expiredOn;
            }

            public int AdId { get; private set; }

            public int CategoryId { get; private set; }

            public string Title { get; private set; }

            public string Description { get; private set; }

            public DateTime ListedOn { get; private set; }

            public DateTime ExpiresOn { get; private set; }
        }

        public class AdSaved
        {
            public AdSaved(int adId)
            {
                AdId = adId;
            }

            public int AdId { get; private set; }
        }

        public class GetCategories
        {

        }

        public class GetCategoriesResult
        {
            public GetCategoriesResult(ImmutableList<Category> categories)
            {
                this.Categories = categories;
            }

            public ImmutableList<Category> Categories { get; private set; }
        }

        #endregion Messages
    }
}
