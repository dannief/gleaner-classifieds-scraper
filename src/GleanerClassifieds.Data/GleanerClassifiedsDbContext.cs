using GleanerClassifieds.Data.Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GleanerClassifieds.Data
{
    public class GleanerClassifiedsDbContext : DbContext, IGleanerClassifiedsDbContext
    {
        public DbSet<Ad> Ads { get; set; }

        public DbSet<Section> Sections { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<NewListingAlert> NewListingAlerts { get; set; }
    }
}
