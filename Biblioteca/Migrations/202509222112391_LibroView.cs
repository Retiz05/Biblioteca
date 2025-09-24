namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LibroView : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.BibliotecaLibroViewModels",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ISBN = c.String(),
                        Materia = c.String(),
                        NumeroEjemplar = c.Int(nullable: false),
                        Clasificacion = c.String(),
                        Estatus = c.Boolean(nullable: false),
                        Autor = c.String(),
                        NumeroAdquisicion = c.Int(),
                        CategoriaNombre = c.String(),
                        PrestamoActivoId = c.Int(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.BibliotecaLibroViewModels");
        }
    }
}
