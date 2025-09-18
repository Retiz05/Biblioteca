namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnadirAutor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Libros", "Autor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Libros", "Autor");
        }
    }
}
