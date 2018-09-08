namespace OCCCPrinting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class printTrackAddComputerName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrintTracks", "Password", c => c.String());
            AddColumn("dbo.PrintTracks", "ComputerName", c => c.String());
            DropColumn("dbo.PrintTracks", "IsCurrentlyPrinting");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrintTracks", "IsCurrentlyPrinting", c => c.Boolean(nullable: false));
            DropColumn("dbo.PrintTracks", "ComputerName");
            DropColumn("dbo.PrintTracks", "Password");
        }
    }
}
