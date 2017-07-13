using Akka.Actor;
using GleanerClassifieds.Data;
using GleanerClassifieds.Data.Model;
using System;
using System.Linq;

namespace GleanerClassifieds.Scraper
{    
    public class AdPersister: ReceiveActor
    {
        private readonly IGleanerClassifiedsDbContext db;

        public AdPersister(IGleanerClassifiedsDbContext db)
        {
            this.db = db;

            Receive<SaveAd>(message => Handle(message));
        }

        public static Props Props()
        {
            return Akka.Actor.Props.Create(() => new AdPersister(new GleanerClassifiedsDbContext()));
        }

        public void Handle(SaveAd msg)
        {
            // If ad does not already exist ...
            if (!db.Ads.Any(x => x.Title == msg.Title && x.ListedOn == msg.ListedOn))
            {
                // save ad to db
                var ad = new Ad
                {
                    Title = msg.Title,
                    Description = msg.Description,
                    CategoryId = msg.CategoryId,
                    ListedOn = msg.ListedOn,
                    ExpiresOn = msg.ExpiresOn
                };

                db.Ads.Add(ad);

                db.SaveChanges();

                Sender.Tell(new AdSaved(ad.Id));
            }
        }

        #region Messages

        public class SaveAd
        {
            public SaveAd(
                int categoryId,
                string title,
                string description,
                DateTime listedOn,
                DateTime expiredOn)
            {
                CategoryId = categoryId;
                Title = title;
                Description = description;
                ListedOn = listedOn;
                ExpiresOn = expiredOn;
            }

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

        #endregion Messages
    }
}
