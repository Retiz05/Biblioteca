using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("PrestamoLibros")]
    public class PrestamoLibro
    {
        public int ID { get; set; }
        public int ClienteID { get; set; }
        public int TipoPrestamoID { get; set; }
        public int BibliotecaLibroID { get; set; }
        public int UsuarioPrestamoID { get; set; }
        public int FechaPrestamo { get; set; }
        public int FechaEntrega { get; set; }
        public int UsuarioRecibeID { get; set; }
        public int FechaEntregaReal { get; set; }
        public int Multa { get; set; }
        public string Observacion { get; set; }
        [ForeignKey("ClienteID")]
        public virtual Cliente Cliente { get; set; }
        [ForeignKey("TipoPrestamoID")]
        public virtual TipoPrestamo TipoPrestamo { get; set; }
        [ForeignKey("BibliotecaLibroID")]
        public virtual BibliotecaLibro BibliotecaLibro { get; set; }
        [ForeignKey("UsuarioPrestamoID")]
        public virtual Usuario Usuario { get; set; }
    }
}