using Biblioteca.Models;
using Biblioteca.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace Biblioteca.Controllers.Administrador
{
    [Autenticacion("Administrador")]
    public class AdministradorBibliotecasController : Controller
    {
        private Context db = new Context();

        // GET: Administrador/Bibliotecas
        [HttpGet]
        public async Task<ActionResult> Index(string busqueda)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            var bibliotecas = db.Bibliotecas.Where(u => u.Estatus == true).ToList();
            if (!string.IsNullOrEmpty(busqueda))
            {
                bibliotecas = bibliotecas.Where(b => b.Nombre.Contains(busqueda)).ToList();
            }

            var usuariosPorBiblioteca = await db.Usuarios
                .Where(u => u.Estatus == true)
                .GroupBy(u => u.BibliotecaID)
                .ToDictionaryAsync(g => g.Key, g => g.Count());

            ViewBag.UsuariosPorBiblioteca = usuariosPorBiblioteca;
            return View("~/Views/Administrador/Bibliotecas.cshtml", bibliotecas);
        }

        // GET: Administrador/Bibliotecas/Details/5
        [HttpGet]
        public async Task<ActionResult> Details(int? id, string busqueda = "")
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }

            // Consulta de usuarios de la biblioteca
            var usuariosQuery = db.Usuarios.Where(u => u.BibliotecaID == id && u.Estatus == true);
            if (!string.IsNullOrEmpty(busqueda))
            {
                usuariosQuery = usuariosQuery.Where(u => u.Nombre.Contains(busqueda) || u.Correo.Contains(busqueda));
            }

            var usuarios = await usuariosQuery.ToListAsync();
            var administradores = await db.Clientes
                .Where(u => u.BibliotecaID == id && u.Estatus == true)
                .ToListAsync();

            ViewBag.Usuarios = usuarios;
            ViewBag.CantidadUsuarios = usuarios.Count;
            ViewBag.Administradores = administradores;
            ViewBag.Busqueda = busqueda;
            ViewBag.Bibliotecas = await db.Bibliotecas.Where(u => u.Estatus == true).ToListAsync();
            ViewBag.Roles = await db.RolUsuarios.ToListAsync();

            return View("~/Views/Administrador/Details.cshtml", biblioteca);
        }

        // POST: Administrador/Bibliotecas/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Models.Biblioteca biblioteca)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            biblioteca.AdministradorID = 1; // Valor por defecto, puede ajustarse según la lógica
            biblioteca.Estatus = true; // Asegurar que la biblioteca se cree como activa
            if (ModelState.IsValid)
            {
                db.Bibliotecas.Add(biblioteca);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "Administrador");
            }

            ViewBag.AdministradorID = new SelectList(db.Administradores, "ID", "Nombre", biblioteca.AdministradorID);
            return View("~/Views/Administrador/Create.cshtml", biblioteca);
        }

        // GET: Administrador/Bibliotecas/Edit/5
        [HttpGet]
        public async Task<ActionResult> Edit(int? id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }

            ViewBag.AdministradorID = new SelectList(db.Administradores, "ID", "Nombre", biblioteca.AdministradorID);
            return View("~/Views/Administrador/Edit.cshtml", biblioteca);
        }

        // POST: Administrador/Bibliotecas/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Nombre,AdministradorID,Estatus")] Models.Biblioteca biblioteca)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (ModelState.IsValid)
            {
                db.Entry(biblioteca).State = EntityState.Modified;
                // Evitar modificar AdministradorID si no se desea cambiar
                db.Entry(biblioteca).Property(x => x.AdministradorID).IsModified = false;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            ViewBag.AdministradorID = new SelectList(db.Administradores, "ID", "Nombre", biblioteca.AdministradorID);
            return View("~/Views/Administrador/Edit.cshtml", biblioteca);
        }

        // GET: Administrador/Bibliotecas/Delete/5
        [HttpGet]
        public async Task<ActionResult> Delete(int? id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }

            return View("~/Views/Administrador/Delete.cshtml", biblioteca);
        }

        // POST: Administrador/Bibliotecas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var validarResult = Validar();
            if (validarResult != null)
                return validarResult;

            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }

            biblioteca.Estatus = false; // Eliminación lógica
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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