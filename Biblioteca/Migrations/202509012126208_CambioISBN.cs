namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CambioISBN : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Libros", "ISBN", c => c.String());
            DropColumn("dbo.Libros", "KEY");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Libros", "KEY", c => c.String());
            DropColumn("dbo.Libros", "ISBN");
        }
    }
}
