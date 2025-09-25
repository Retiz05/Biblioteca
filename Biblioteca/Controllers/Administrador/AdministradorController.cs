using Biblioteca.Models;
using Biblioteca.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    [Autenticacion("Administrador")]
    public class AdministradorController : Controller
    {
        private Context db = new Context();

        // GET: Administrador/Index
        [HttpGet]
        public ActionResult Index()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            // Obtener datos de bibliotecas activas
            ViewBag.TotalBibliotecas = db.Bibliotecas.Where(u => u.Estatus == true).Count();
            ViewBag.Bibliotecas = db.Bibliotecas.Where(u => u.Estatus == true).ToList();
            ViewBag.BibliotecasInactivas = db.Bibliotecas.Where(u => u.Estatus == false).Count();

            // Obtener datos de clientes activos
            ViewBag.TotalClientes = db.Clientes.Where(u => u.Estatus == true).Count();

            // Obtener datos de usuarios activos
            ViewBag.TotalUsuarios = db.Usuarios.Where(u => u.Estatus == true).Count();

            // Obtener datos de libros
            ViewBag.TotalLibros = db.Libros.Where(u => u.Estatus == true).Count();

            // Obtener roles de clientes
            ViewBag.Roles = db.RolClientes.ToList();

            return View();
        }

        // GET: Administrador/Bibliotecas
        [HttpGet]
        public ActionResult Bibliotecas()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            // Obtener conteo de usuarios por biblioteca (usando un ID de ejemplo)
            int id = 1; // Este ID puede ser dinámico según la lógica de tu aplicación
            ViewBag.NoUsuarios = db.Usuarios.Count(b => b.BibliotecaID == id && b.Estatus == true);
            ViewBag.Bibliotecas = db.Bibliotecas.Where(u => u.Estatus == true).ToList();

            return View("~/Views/Administrador/Bibliotecas.cshtml");
        }

        // GET: Administrador/Clientes
        [HttpGet]
        public ActionResult Clientes()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            return View("~/Views/Administrador/Clientes.cshtml");
        }

        // GET: Administrador/Libros
        [HttpGet]
        public ActionResult Libros()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            return View("~/Views/Administrador/Libros.cshtml");
        }

        private ActionResult Validar()
        {
            if (Session["UsuarioID"] == null || Session["Rol"].ToString() != "Admin")
                return RedirectToAction("Index", "Login");
            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}