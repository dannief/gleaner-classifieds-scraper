namespace GleanerClassifieds.Data.Migrations
{
    using GleanerClassifieds.Data.Model;
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<GleanerClassifiedsDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(GleanerClassifiedsDbContext context)
        {
            context.Sections.AddOrUpdate(
                new Section { Id = 10100, Name = "Real Estate" }
            );

            context.Categories.AddOrUpdate(             
                new Category { Id = 12518, Name = "513 Rental Houses/Apts KGN", SectionId = 10100 },
                new Category { Id = 13028, Name = "514 Rental Houses/Apts ST.CTH", SectionId = 10100 }
            );

            context.Database.ExecuteSqlCommand("delete from NewListingAlerts");
            context.NewListingAlerts.AddOrUpdate(                
                new NewListingAlert { CategoryId = 12518, EmailAddress = "debbie.facey@gmail.com", LastAlerted = new System.DateTime(2017, 7, 13), IsActive = true },
                new NewListingAlert { CategoryId = 12518, Keywords = "Stony Hill", EmailAddress = "debbie.facey@gmail.com", LastAlerted = new System.DateTime(2017, 7, 13), IsActive = true },
                new NewListingAlert { CategoryId = 13028, EmailAddress = "shamarra.mais@gmail.com", LastAlerted = new System.DateTime(2017, 7, 13) }
            );
        }
    }
}
