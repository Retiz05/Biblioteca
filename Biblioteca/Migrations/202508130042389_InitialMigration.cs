namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Administradores",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Correo = c.String(),
                        Contrasena = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.BibliotecaLibros",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        BibliotecaID = c.Int(nullable: false),
                        LibroID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bibliotecas", t => t.BibliotecaID, cascadeDelete: true)
                .ForeignKey("dbo.Libros", t => t.LibroID, cascadeDelete: true)
                .Index(t => t.BibliotecaID)
                .Index(t => t.LibroID);
            
            CreateTable(
                "dbo.Bibliotecas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        AdministradorID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Administradores", t => t.AdministradorID, cascadeDelete: false)
                .Index(t => t.AdministradorID);
            
            CreateTable(
                "dbo.Libros",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        KEY = c.Int(nullable: false),
                        Materia = c.String(),
                        NumeroEjemplar = c.Int(nullable: false),
                        Clasificacion = c.String(),
                        Estatus = c.Boolean(nullable: false),
                        CategoriaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Categorias", t => t.CategoriaID, cascadeDelete: true)
                .Index(t => t.CategoriaID);
            
            CreateTable(
                "dbo.Categorias",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Clave = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Clientes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Correo = c.String(),
                        Contrasena = c.String(),
                        RolID = c.Int(nullable: false),
                        BibliotecaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bibliotecas", t => t.BibliotecaID, cascadeDelete: false)
                .ForeignKey("dbo.RolClientes", t => t.RolID, cascadeDelete: true)
                .Index(t => t.RolID)
                .Index(t => t.BibliotecaID);
            
            CreateTable(
                "dbo.RolClientes",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.PrestamoLibros",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        ClienteID = c.Int(nullable: false),
                        TipoPrestamoID = c.Int(nullable: false),
                        BibliotecaLibroID = c.Int(nullable: false),
                        UsuarioPrestamoID = c.Int(nullable: false),
                        FechaPrestamo = c.Int(nullable: false),
                        FechaEntrega = c.Int(nullable: false),
                        UsuarioRecibeID = c.Int(nullable: false),
                        FechaEntregaReal = c.Int(nullable: false),
                        Multa = c.Int(nullable: false),
                        Observacion = c.String(),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.BibliotecaLibros", t => t.BibliotecaLibroID, cascadeDelete: true)
                .ForeignKey("dbo.Clientes", t => t.ClienteID, cascadeDelete: false)
                .ForeignKey("dbo.TipoPrestamos", t => t.TipoPrestamoID, cascadeDelete: true)
                .ForeignKey("dbo.Usuarios", t => t.UsuarioPrestamoID, cascadeDelete: true)
                .Index(t => t.ClienteID)
                .Index(t => t.TipoPrestamoID)
                .Index(t => t.BibliotecaLibroID)
                .Index(t => t.UsuarioPrestamoID);
            
            CreateTable(
                "dbo.TipoPrestamos",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Tiempo = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Usuarios",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Correo = c.String(),
                        Contrasena = c.String(),
                        RolID = c.Int(nullable: false),
                        BibliotecaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Bibliotecas", t => t.BibliotecaID, cascadeDelete: false)
                .ForeignKey("dbo.RolUsuarios", t => t.RolID, cascadeDelete: true)
                .Index(t => t.RolID)
                .Index(t => t.BibliotecaID);
            
            CreateTable(
                "dbo.RolUsuarios",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                    })
                .PrimaryKey(t => t.ID);
            
            CreateTable(
                "dbo.Temas",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        Nombre = c.String(),
                        Clave = c.String(),
                        CategoriaID = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ID)
                .ForeignKey("dbo.Categorias", t => t.CategoriaID, cascadeDelete: true)
                .Index(t => t.CategoriaID);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Temas", "CategoriaID", "dbo.Categorias");
            DropForeignKey("dbo.PrestamoLibros", "UsuarioPrestamoID", "dbo.Usuarios");
            DropForeignKey("dbo.Usuarios", "RolID", "dbo.RolUsuarios");
            DropForeignKey("dbo.Usuarios", "BibliotecaID", "dbo.Bibliotecas");
            DropForeignKey("dbo.PrestamoLibros", "TipoPrestamoID", "dbo.TipoPrestamos");
            DropForeignKey("dbo.PrestamoLibros", "ClienteID", "dbo.Clientes");
            DropForeignKey("dbo.PrestamoLibros", "BibliotecaLibroID", "dbo.BibliotecaLibros");
            DropForeignKey("dbo.Clientes", "RolID", "dbo.RolClientes");
            DropForeignKey("dbo.Clientes", "BibliotecaID", "dbo.Bibliotecas");
            DropForeignKey("dbo.BibliotecaLibros", "LibroID", "dbo.Libros");
            DropForeignKey("dbo.Libros", "CategoriaID", "dbo.Categorias");
            DropForeignKey("dbo.BibliotecaLibros", "BibliotecaID", "dbo.Bibliotecas");
            DropForeignKey("dbo.Bibliotecas", "AdministradorID", "dbo.Administradores");
            DropIndex("dbo.Temas", new[] { "CategoriaID" });
            DropIndex("dbo.Usuarios", new[] { "BibliotecaID" });
            DropIndex("dbo.Usuarios", new[] { "RolID" });
            DropIndex("dbo.PrestamoLibros", new[] { "UsuarioPrestamoID" });
            DropIndex("dbo.PrestamoLibros", new[] { "BibliotecaLibroID" });
            DropIndex("dbo.PrestamoLibros", new[] { "TipoPrestamoID" });
            DropIndex("dbo.PrestamoLibros", new[] { "ClienteID" });
            DropIndex("dbo.Clientes", new[] { "BibliotecaID" });
            DropIndex("dbo.Clientes", new[] { "RolID" });
            DropIndex("dbo.Libros", new[] { "CategoriaID" });
            DropIndex("dbo.Bibliotecas", new[] { "AdministradorID" });
            DropIndex("dbo.BibliotecaLibros", new[] { "LibroID" });
            DropIndex("dbo.BibliotecaLibros", new[] { "BibliotecaID" });
            DropTable("dbo.Temas");
            DropTable("dbo.RolUsuarios");
            DropTable("dbo.Usuarios");
            DropTable("dbo.TipoPrestamos");
            DropTable("dbo.PrestamoLibros");
            DropTable("dbo.RolClientes");
            DropTable("dbo.Clientes");
            DropTable("dbo.Categorias");
            DropTable("dbo.Libros");
            DropTable("dbo.Bibliotecas");
            DropTable("dbo.BibliotecaLibros");
            DropTable("dbo.Administradores");
        }
    }
}
