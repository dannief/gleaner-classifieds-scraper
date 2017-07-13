namespace GleanerClassifieds.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsActiveFlagToNewListingAlertsTbl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NewListingAlerts", "IsActive", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NewListingAlerts", "IsActive");
        }
    }
}
