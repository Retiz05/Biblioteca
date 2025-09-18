using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("TipoPrestamos")]
    public class TipoPrestamo
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public int Tiempo { get; set; }
    }
}