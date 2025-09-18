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
        public int BibliotecaID { get; set; }
        public int LibroID { get; set; }
        [ForeignKey("LibroID")]
        public virtual Libro Libro { get; set; }
        [ForeignKey("BibliotecaID")]
        public virtual Biblioteca Biblioteca { get; set; }
    }
}