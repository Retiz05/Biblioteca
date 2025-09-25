using Biblioteca.Models;
using Biblioteca.Models.ModelosDTO;
using Biblioteca.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Biblioteca.Controllers
{
    [Autenticacion("Administrador")]
    public class UsuariosController : Controller
    {
        private Context db = new Context();

        // GET: Usuarios
        [HttpGet]
        public async Task<ActionResult> Index()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            // Mostrar lista de usuarios para administradores
            if (Session["Rol"]?.ToString() == "Admin")
            {
                var usuarios = await db.Usuarios.Where(u => u.Estatus == true).ToListAsync();
                return View("~/Views/Usuarios/Index.cshtml", usuarios);
            }

            // Mostrar libros con datos de la API para usuarios no administradores
            ViewBag.Categorias = await db.Categorias.ToListAsync();
            var libros = await db.Libros.Where(l => l.Estatus == true).ToListAsync();
            try
            {
                var resultado = await OpenLibraryApi.ConsultarLibrosLibreria(libros);
                if (resultado == null)
                {
                    return View("~/Views/Usuarios/Libros.cshtml");
                }
                return View("~/Views/Usuarios/Libros.cshtml", resultado);
            }
            catch (HttpRequestException httpEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error de conexión HTTP: {httpEx.Message}");
                System.Diagnostics.Debug.WriteLine($"Detalles: {httpEx.InnerException?.Message}");
                return View("~/Views/Usuarios/Libros.cshtml");
            }
            catch (JsonException jsonEx)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error al procesar el JSON: {jsonEx.Message}");
                return View("~/Views/Usuarios/Libros.cshtml");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Error inesperado: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Tipo de error: {ex.GetType().Name}");
                return View("~/Views/Usuarios/Libros.cshtml");
            }
        }

        // GET: Usuarios/Details/5
        [HttpGet]
        public async Task<ActionResult> Details(int? id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            return View("~/Views/Usuarios/Details.cshtml", usuario);
        }

        // GET: Usuarios/Create
        [HttpGet]
        public ActionResult Create()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre");
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas.Where(b => b.Estatus == true), "ID", "Nombre");
            return View("~/Views/Usuarios/Create.cshtml");
        }

        // POST: Usuarios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,Nombre,Correo,Contraseña,RolID,BibliotecaID")] Usuario usuario)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            usuario.Estatus = true; // Asegurar que el usuario se cree como activo
            if (ModelState.IsValid)
            {
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre", usuario.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas.Where(b => b.Estatus == true), "ID", "Nombre", usuario.BibliotecaID);
            return View("~/Views/Usuarios/Create.cshtml", usuario);
        }

        // GET: Usuarios/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre", usuario.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas.Where(b => b.Estatus == true), "ID", "Nombre", usuario.BibliotecaID);
            return View("~/Views/Usuarios/Edit.cshtml", usuario);
        }

        // POST: Usuarios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Nombre,Correo,Contraseña,RolID,BibliotecaID,Estatus")] Usuario usuario)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (ModelState.IsValid)
            {
                db.Entry(usuario).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre", usuario.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas.Where(b => b.Estatus == true), "ID", "Nombre", usuario.BibliotecaID);
            return View("~/Views/Usuarios/Edit.cshtml", usuario);
        }

        // GET: Usuarios/Delete/5
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            return View("~/Views/Usuarios/Delete.cshtml", usuario);
        }

        // POST: Usuarios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            var usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            usuario.Estatus = false; // Eliminación lógica
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        // GET: Usuarios/Prestamos
        [HttpGet]
        public async Task<ActionResult> Prestamos()
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            var usuarioId = (int)(Session["UsuarioID"]);
            var prestamos = await db.PrestamoLibros
                .Where(p => p.UsuarioID == usuarioId && p.Devuelto == true)
                .Include(p => p.BibliotecaLibro.Libro)
                .ToListAsync();
            return View("~/Views/Usuarios/Prestamos.cshtml", prestamos);
        }

        private ActionResult Validar()
        {
            if (Session["UsuarioID"] == null)
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