using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Biblioteca.Models
{
    [Table("Libros")]
    public class Libro
    {
        public int ID { get; set; }
        [Column("ISBN")]
        public string ISBN { get; set; }
        public int NumeroEjemplar { get; set; }
        public string Clasificacion { get; set; }
        public bool Estatus { get; set; }
        public string Autor { get; set; }
        public int NumeroAdquisicion { get; set; }

        public int CategoriaID { get; set; }
        [ForeignKey("CategoriaID")]
        public virtual Categoria Categoria { get; set; }
    }
}