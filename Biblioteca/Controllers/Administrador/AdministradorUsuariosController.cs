using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Biblioteca.Models;

namespace Biblioteca.Controllers.Administrador
{
    public class AdministradorUsuariosController : Controller
    {
        private Context db = new Context();

        public ActionResult Index(string busqueda)
        {
            var usuarios = db.Usuarios
                             .Include(u => u.RolUsuario)
                             .Include(u => u.Biblioteca)
                             .Where(u => u.Estatus == true)
                             .ToList();

            if (!string.IsNullOrEmpty(busqueda))
            {
                usuarios = usuarios.Where(u => u.Nombre.Contains(busqueda) || u.Correo.Contains(busqueda))
                                   .ToList();
            }

            ViewBag.Bibliotecas = db.Bibliotecas.Where(u => u.Estatus == true).ToList();
            ViewBag.Roles = db.RolUsuarios.ToList();
            return View(usuarios);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Models.Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                db.Usuarios.Add(usuario);
                await db.SaveChangesAsync();
                return RedirectToAction("Index", "AdministradorUsuarios");
            }

            return View(usuario);
        }

        // GET: Usuarios/Edit/5
        public async Task<ActionResult> Edit(int? id)
        {
            if (id == null)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            Models.Usuario usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            // Pasamos las listas para los select
            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre", usuario.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", usuario.BibliotecaID);

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(Models.Usuario usuario, string redirectTo)
        {
            if (ModelState.IsValid)
            {
                var usuarioDb = await db.Usuarios.FindAsync(usuario.ID);

                if (usuarioDb == null)
                    return HttpNotFound();

                if (usuario.Contrasena != null)
                    usuarioDb.Contrasena = usuario.Contrasena;

                usuarioDb.Nombre = usuario.Nombre;
                usuarioDb.Correo = usuario.Correo;
                usuarioDb.RolID = usuario.RolID;
                usuarioDb.BibliotecaID = usuario.BibliotecaID;

                if (redirectTo == "BibliotecaDetails")
                    return RedirectToAction("Details", "AdministradorBibliotecas", new { id = usuarioDb.BibliotecaID });


                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            // Recargamos selects si el modelo no es válido
            ViewBag.RolID = new SelectList(db.RolUsuarios, "ID", "Nombre", usuario.RolID);
            ViewBag.BibliotecaID = new SelectList(db.Bibliotecas, "ID", "Nombre", usuario.BibliotecaID);

            return View(usuario);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            Models.Usuario usuario = await db.Usuarios.FindAsync(id);
            if (usuario == null)
                return HttpNotFound();

            usuario.Estatus = false;
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
        }
    }
}
