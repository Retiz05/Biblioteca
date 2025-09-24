using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("BibliotecaLibros")]
    public class BibliotecaLibro
    {
        public int ID { get; set; }
        public int BibliotecaID { get; set; }  // Biblioteca propietaria del ejemplar
        public int LibroID { get; set; }       // Libro al que pertenece

        [ForeignKey("LibroID")]
        public virtual Libro Libro { get; set; }

        [ForeignKey("BibliotecaID")]
        public virtual Biblioteca Biblioteca { get; set; }

        // Estado del ejemplar: true = disponible, false = prestado
        public bool Estatus { get; set; } = true;
    }
}