namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StringKey : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Libros", "KEY", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Libros", "KEY", c => c.Int(nullable: false));
        }
    }
}
