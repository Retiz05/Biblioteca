namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CampoEstatusBibliotecaLibro : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BibliotecaLibros", "Estatus", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.BibliotecaLibros", "Estatus");
        }
    }
}
