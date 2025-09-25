using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Biblioteca.Models
{
    [Table("Clientes")]
    public class Cliente
    {
        public int ID { get; set; }
        public string Nombre { get; set; }
        public string Correo { get; set; }
        public string Contrasena { get; set; }
        public int RolID { get; set; }
        public bool Estatus { get; set; } = true;
        public int BibliotecaID { get; set; }
        [ForeignKey("RolID")]
        public virtual RolCliente RolClientes { get; set; }
        [ForeignKey("BibliotecaID")]
        public virtual Biblioteca Biblioteca { get; set; }
    }
}