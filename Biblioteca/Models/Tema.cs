using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("Temas")]
    public class Tema
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Clave { get; set; }
        public int CategoriaID { get; set; }
        [ForeignKey("CategoriaID")]
        public virtual Categoria Categoria { get; set; }
    }
}