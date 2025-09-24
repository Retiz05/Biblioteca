namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CambioPrestamoLibro : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PrestamoLibros", "UsuarioPrestamoID", "dbo.Usuarios");
            DropIndex("dbo.PrestamoLibros", new[] { "UsuarioPrestamoID" });
            DropColumn("dbo.PrestamoLibros", "UsuarioRecibeID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrestamoLibros", "UsuarioRecibeID", c => c.Int(nullable: false));
            CreateIndex("dbo.PrestamoLibros", "UsuarioPrestamoID");
            AddForeignKey("dbo.PrestamoLibros", "UsuarioPrestamoID", "dbo.Usuarios", "ID", cascadeDelete: true);
        }
    }
}
