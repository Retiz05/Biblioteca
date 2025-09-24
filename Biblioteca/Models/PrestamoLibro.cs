using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Models
{
    [Table("PrestamoLibros")]
    public class PrestamoLibro
    {
        public int ID { get; set; }

        // Cliente (bibliotecario que gestiona el préstamo)
        public int ClienteID { get; set; }

        // Usuario (lector que recibe el préstamo)
        public int UsuarioID { get; set; }

        // Tipo de préstamo
        public int TipoPrestamoID { get; set; }

        // Libro asociado
        public int BibliotecaLibroID { get; set; }

        // Fechas en Unix timestamp
        public long FechaPrestamo { get; set; }
        public long FechaEntrega { get; set; }
        public long? FechaEntregaReal { get; set; }

        public decimal? Multa { get; set; }
        public bool Devuelto { get; set; } = false;
        public string Observacion { get; set; }

        // 🔹 Navegaciones
        [ForeignKey("ClienteID")]
        public virtual Cliente Cliente { get; set; }

        [ForeignKey("UsuarioID")]
        public virtual Usuario Usuario { get; set; }

        [ForeignKey("TipoPrestamoID")]
        public virtual TipoPrestamo TipoPrestamo { get; set; }

        [ForeignKey("BibliotecaLibroID")]
        public virtual BibliotecaLibro BibliotecaLibro { get; set; }
    }
}
