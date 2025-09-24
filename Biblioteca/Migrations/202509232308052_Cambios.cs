namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Cambios : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PrestamoLibros", "UsuarioID", c => c.Int(nullable: false));
            AddColumn("dbo.PrestamoLibros", "Devuelto", c => c.Boolean(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaPrestamo", c => c.Long(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaEntrega", c => c.Long(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaEntregaReal", c => c.Long());
            AlterColumn("dbo.PrestamoLibros", "Multa", c => c.Decimal(precision: 18, scale: 2));
            CreateIndex("dbo.PrestamoLibros", "UsuarioID");
            AddForeignKey("dbo.PrestamoLibros", "UsuarioID", "dbo.Usuarios", "ID", cascadeDelete: true);
            DropColumn("dbo.PrestamoLibros", "UsuarioPrestamoID");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PrestamoLibros", "UsuarioPrestamoID", c => c.Int(nullable: false));
            DropForeignKey("dbo.PrestamoLibros", "UsuarioID", "dbo.Usuarios");
            DropIndex("dbo.PrestamoLibros", new[] { "UsuarioID" });
            AlterColumn("dbo.PrestamoLibros", "Multa", c => c.Int(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaEntregaReal", c => c.Int(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaEntrega", c => c.Int(nullable: false));
            AlterColumn("dbo.PrestamoLibros", "FechaPrestamo", c => c.Int(nullable: false));
            DropColumn("dbo.PrestamoLibros", "Devuelto");
            DropColumn("dbo.PrestamoLibros", "UsuarioID");
        }
    }
}
