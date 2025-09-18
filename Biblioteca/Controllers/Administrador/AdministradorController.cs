using Biblioteca.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    public class AdministradorController : Controller
    {
        private Context db = new Context();
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.TotalBibliotecas = db.Bibliotecas.Count();
            ViewBag.TotalClientes = db.Clientes.Count(); 
            ViewBag.TotalLibros = db.Libros.Count();
            ViewBag.Roles = db.RolClientes.ToList();
            ViewBag.Bibliotecas = db.Bibliotecas.ToList();
            return View();
        }

        public ActionResult Bibliotecas()
        {
            int id = 1;
            var NoUsuarios = db.Usuarios.Count(b => b.BibliotecaID == id);
            ViewBag.Biblioteca = new List<Models.Biblioteca>();
            return View("./Bibliotecas");
        }

        public ActionResult Clientes()
        {
            return View("./Clientes");
        }
        public ActionResult Libros()
        {
            return View("./Libros");
        }





    }
}