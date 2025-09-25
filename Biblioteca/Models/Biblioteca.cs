using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("Bibliotecas")]
    public class Biblioteca
    {
        [Key]
        public int ID { get; set; }
        public string Nombre { get; set; }
        public bool Estatus { get; set; } = true;
        public int AdministradorID { get; set; }
        [ForeignKey("AdministradorID")]
        public virtual Administrador Administrador { get; set; }
    }
}