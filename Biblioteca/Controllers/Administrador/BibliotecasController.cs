using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Biblioteca.Models;
using System.Web.UI.WebControls;

namespace Biblioteca.Controllers.Administrador
{
    public class BibliotecasController : Controller
    {
        private Context db = new Context();

        public async Task<ActionResult> Index()
        {
            return RedirectToAction("Index","Administrador");
        }

        public async Task<ActionResult> Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }
            return View(biblioteca);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(Models.Biblioteca biblioteca)
        {
            biblioteca.AdministradorID = 1;
            if (ModelState.IsValid)
            {
                db.Bibliotecas.Add(biblioteca);
                await db.SaveChangesAsync();

                // Redirige a Administrador/Index después de guardar
                return RedirectToAction("Index", "Administrador");
            }

            // Si el modelo NO es válido, vuelve a mostrar la vista Create con el modelo para mostrar errores
            ViewBag.AdministradorID = new SelectList(db.Administradores, "ID", "Nombre", biblioteca.AdministradorID);
            return View("./Index",biblioteca);
        }


        public async Task<ActionResult> Edit(int? id)
        {
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
            return View(biblioteca);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,Nombre,AdministradorID")] Models.Biblioteca biblioteca)
        {
            if (ModelState.IsValid)
            {
                db.Entry(biblioteca).State = EntityState.Modified;
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            ViewBag.AdministradorID = new SelectList(db.Administradores, "ID", "Nombre", biblioteca.AdministradorID);
            return View(biblioteca);
        }

        public async Task<ActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            if (biblioteca == null)
            {
                return HttpNotFound();
            }
            return View(biblioteca);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            Models.Biblioteca biblioteca = await db.Bibliotecas.FindAsync(id);
            db.Bibliotecas.Remove(biblioteca);
            await db.SaveChangesAsync();
            return RedirectToAction("Index");
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
