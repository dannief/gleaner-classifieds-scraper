using System.Data.Entity;
using GleanerClassifieds.Data.Model;

namespace GleanerClassifieds.Data
{
    public interface IGleanerClassifiedsDbContext
    {
        DbSet<Ad> Ads { get; set; }

        DbSet<Category> Categories { get; set; }

        DbSet<NewListingAlert> NewListingAlerts { get; set; }

        DbSet<Section> Sections { get; set; }

        int SaveChanges();
    }
}