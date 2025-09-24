namespace Biblioteca.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NumeroAdquisicion : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Libros", "NumeroAdquisicion", c => c.Int(nullable: false));
            CreateIndex("dbo.Libros", "NumeroAdquisicion", unique: true);
        }
        
        public override void Down()
        {
            DropIndex("dbo.Libros", new[] { "NumeroAdquisicion" });
            AlterColumn("dbo.Libros", "NumeroAdquisicion", c => c.Int());
        }
    }
}
