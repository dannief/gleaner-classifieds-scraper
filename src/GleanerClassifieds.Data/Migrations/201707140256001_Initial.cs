namespace GleanerClassifieds.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Ads",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        CategoryId = c.Int(nullable: false),
                        Title = c.String(maxLength: 64),
                        Description = c.String(),
                        ListedOn = c.DateTime(nullable: false),
                        ExpiresOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.Categories",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(maxLength: 32),
                        SectionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Sections", t => t.SectionId, cascadeDelete: true)
                .Index(t => t.SectionId);
            
            CreateTable(
                "dbo.Sections",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(maxLength: 32),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NewListingAlerts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EmailAddress = c.String(maxLength: 128),
                        CategoryId = c.Int(),
                        SectionId = c.Int(),
                        Keywords = c.String(maxLength: 128),
                        LastAlerted = c.DateTime(),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Categories", t => t.CategoryId)
                .ForeignKey("dbo.Sections", t => t.SectionId)
                .Index(t => t.CategoryId)
                .Index(t => t.SectionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NewListingAlerts", "SectionId", "dbo.Sections");
            DropForeignKey("dbo.NewListingAlerts", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Ads", "CategoryId", "dbo.Categories");
            DropForeignKey("dbo.Categories", "SectionId", "dbo.Sections");
            DropIndex("dbo.NewListingAlerts", new[] { "SectionId" });
            DropIndex("dbo.NewListingAlerts", new[] { "CategoryId" });
            DropIndex("dbo.Categories", new[] { "SectionId" });
            DropIndex("dbo.Ads", new[] { "CategoryId" });
            DropTable("dbo.NewListingAlerts");
            DropTable("dbo.Sections");
            DropTable("dbo.Categories");
            DropTable("dbo.Ads");
        }
    }
}
