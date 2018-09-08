namespace OCCCPrinting.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class printTrackAddRequiredField : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.PrintTracks", "StudentId", c => c.String(nullable: false));
            AlterColumn("dbo.PrintTracks", "Password", c => c.String(nullable: false));
            AlterColumn("dbo.PrintTracks", "ComputerName", c => c.String(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.PrintTracks", "ComputerName", c => c.String());
            AlterColumn("dbo.PrintTracks", "Password", c => c.String());
            AlterColumn("dbo.PrintTracks", "StudentId", c => c.String());
        }
    }
}
