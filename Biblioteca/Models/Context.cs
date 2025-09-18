using System.Data.Entity;

namespace Biblioteca.Models

{

    public class Context : DbContext
    {
        public Context() : base("name=contexto")
        {
        }

        // Definición de los DbSet (tablas)

        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Biblioteca> Bibliotecas { get; set; }
        public DbSet<BibliotecaLibro> BibliotecaLibros { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Libro> Libros { get; set; }
        public DbSet<PrestamoLibro> PrestamoLibros { get; set; }
        public DbSet<RolCliente> RolClientes { get; set; }
        public DbSet<RolUsuario> RolUsuarios { get; set; }
        public DbSet<Tema> Temas { get; set; }
        public DbSet<TipoPrestamo> TipoPrestamos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

    }

}
