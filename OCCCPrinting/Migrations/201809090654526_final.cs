namespace OCCCPrinting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class final : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrintTracks", "Password", c => c.String(nullable: false));
            AddColumn("dbo.PrintTracks", "ComputerName", c => c.String(nullable: false));
            AlterColumn("dbo.PrintTracks", "StudentId", c => c.String(nullable: false));
            DropColumn("dbo.PrintTracks", "IsCurrentlyPrinting");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrintTracks", "IsCurrentlyPrinting", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PrintTracks", "StudentId", c => c.String());
            DropColumn("dbo.PrintTracks", "ComputerName");
            DropColumn("dbo.PrintTracks", "Password");
        }
    }
}
