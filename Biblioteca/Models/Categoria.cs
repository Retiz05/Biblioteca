using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("Categorias")]
    public class Categoria
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Clave {  get; set; }
    }
}