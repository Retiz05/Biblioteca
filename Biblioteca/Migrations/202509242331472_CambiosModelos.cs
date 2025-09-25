namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CambiosModelos : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Administradores", "Estatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.Bibliotecas", "Estatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.Clientes", "Estatus", c => c.Boolean(nullable: false));
            AddColumn("dbo.Usuarios", "Estatus", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Usuarios", "Estatus");
            DropColumn("dbo.Clientes", "Estatus");
            DropColumn("dbo.Bibliotecas", "Estatus");
            DropColumn("dbo.Administradores", "Estatus");
        }
    }
}
