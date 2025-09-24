namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveMateriaFromLibro : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Libros", new[] { "NumeroAdquisicion" });
            DropColumn("dbo.Libros", "Materia");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Libros", "Materia", c => c.String());
            CreateIndex("dbo.Libros", "NumeroAdquisicion", unique: true);
        }
    }
}
